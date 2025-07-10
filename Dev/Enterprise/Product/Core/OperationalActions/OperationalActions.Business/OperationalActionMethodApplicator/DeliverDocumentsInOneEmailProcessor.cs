using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using static Enterprise.Core.Constants;
using static Enterprise.DocumentEngine.DeliveryMethods.FactoryStrategy;

namespace Enterprise.Services.OperationalActions.Business
{
	class DeliverDocumentsInOneEmailProcessor : BaseDocumentProcessor
	{
		public DeliverDocumentsInOneEmailProcessor(OperationalActionRunner runner) : base(runner)
		{
		}

		public override void ProcessDocument()
		{
			Log.SetSectionProgressMax(1);

			var firstTargetPK = TargetsPKs[0];
			if (Runner.RunOnAllMatchingRecords && Runner.Selection is ModuleSelection selection && selection.Module.GridCollection.Count > 0 && TargetsPKs.Contains(((BusinessObject)selection.Module.GridCollection[0]).PK))
			{
				firstTargetPK = ((BusinessObject)selection.Module.GridCollection[0]).PK;
			}

			var factory = GetNewFactory();
			List<IDocumentSupportable> targets;
			if (typeof(NonPersistentBusinessObject).IsAssignableFrom(TargetType))
			{
				targets = new List<IDocumentSupportable>();
				foreach (var pk in TargetsPKs)
				{
					var target = factory.Load(TargetType, pk);
					if (target is IDocumentSupportable supportable)
					{
						targets.Add(supportable);
					}
				}
			}
			else
			{
				var query = new ZQuery(BusinessObjectFactory.GetTableSchemaFromType(TargetType).PK, SQLComparisonOperator.Equal, TargetsPKs);
				targets = factory.Load(TargetType, query)?.OfType<IDocumentSupportable>().ToList();
			}
			if (targets != null && targets.Any())
			{
				var commands = GetTargetCommands();
				if (commands == null)
				{
					Log.Notify(OperationalActionLogErrorLevel.Informational, Res.GetString("0EC73C6E-83AD-4790-A004-AB80A99FCC83", "There is no document that needs to be sent."));
					return;
				}
				var targetCommand = commands[0];
				UpdateTargetsIfNeeded(targets, firstTargetPK);

				if (CheckAutoDeliveryContactsAndGenerateDocumentPacks(targets, commands, out var recipients, out var documentPacks, out var dummyDocumentEvents, out var dummyDocumentCommands))
				{
					if (documentPacks.Any())
					{
						Log.Notify(OperationalActionLogErrorLevel.Informational, Res.GetString("816A9C07-4060-4081-866F-269707E7D0A5", "Generating documents."));

						var factoryForSaving = new BusinessObjectFactory { NameForDebugging = "DocumentPseudoApplicator.ApplyDeliverDocumentsInOneEmail" };
						var instructions = CreateDeliveryInstructions(targetCommand.SU_DraftOption, new PopulateButDoNotSave(factoryForSaving));
						var isSingleAttachment = Runner.AttachmentOptions == OperationalActionRunnerLookups.AttachmentOptionsCodes.SingleAttachment;
						using (instructions.EnableOperationalActionDeliverDocumentsInOneEmail(isSingleAttachment, targetCommand))
						using (instructions.Recipients.SuspendListChanged())
						{
							var bulkDeliveryMethod = Runner.Lookups.BulkDeliveryMethod_List[Runner.BulkDeliveryMethod];
							bulkDeliveryMethod.SetRecipients(instructions, recipients);
							SetDeliveryInstructionsCoverNoteIfNeeded(instructions);

							using (var set = new DocumentPrintSet(targetCommand))
							{
								if (isSingleAttachment)
								{
									recipients.OfType<DocDeliveryContact>().ForEach(x => x.AttachmentType = OrgConstants.AttachmentType.PDFC);

									var firstPack = documentPacks[0];
									if (documentPacks.Count > 1)
									{
										documentPacks.Skip(1).ForEach(p => firstPack.AddRange(p));
									}

									set.Add(firstPack);
								}
								else
								{
									set.AddRange(documentPacks);
								}

								set.DocumentPrePrinted += (sender, e) =>
								{
									dummyDocumentEvents.ForEach(item =>
									{
										dummyDocumentCommands[item.Key].ForEach(command =>
										{
											command.Parent = item.Key;
											item.Value.OnDocumentPrePrinted(item.Key, new DocumentPrintedEventArgs(instructions.Destination, command));
										});
									});

									var firstTarget = targets.FirstOrDefault(t => t.DocumentSupporter.PK == firstTargetPK);
									targetCommand.Parent = firstTarget;
								};

								set.Run(instructions);

								if (instructions.ContinuedWhenDocumentsFileSizeGreaterThanSizeLimit)
								{
									ZExceptionReporting.ProcessWithSaveExceptionHandling(factoryForSaving.Save, null, reportErrorsOnly: true);

									dummyDocumentEvents.ForEach(item =>
									{
										dummyDocumentCommands[item.Key].ForEach(command =>
										{
											command.Parent = item.Key;
											item.Value.OnDocumentPrinted(item.Key, new DocumentPrintedEventArgs(instructions.Destination, command));
										});
									});
								}
							}
						}
					}
				}
			}

			Log.BumpSectionProgress();
		}

		void UpdateTargetsIfNeeded(List<IDocumentSupportable> targets, ZGuid firstTargetPK)
		{
			if (targets[0].DocumentSupporter.PK != firstTargetPK)
			{
				var topTarget = targets.FirstOrDefault(t => t.DocumentSupporter.PK == firstTargetPK);
				if (topTarget != null)
				{
					var index = targets.IndexOf(topTarget);
					targets.RemoveAt(index);
					targets.Insert(0, topTarget);
				}
			}
		}

		List<DocumentCommand> GetTargetCommands()
		{
			var results = Action.DocumentPivots.Select(p => p.Document).ToList();
			if (results.Any())
			{
				var primaryCommand = results.FirstOrDefault(d => !d.SU_PrimaryDocPackItemId.IsEmpty);
				if (primaryCommand != null)
				{
					results.RemoveAt(results.IndexOf(primaryCommand));
					results.Insert(0, primaryCommand);
				}
				return results;
			}

			return null;
		}

		bool CheckAutoDeliveryContactsAndGenerateDocumentPacks(IEnumerable<IDocumentSupportable> targets, List<DocumentCommand> commands, out DocDeliveryContactCollection recipients, out List<DocumentPack> documentPacks, out Dictionary<IDocumentSupportable, DummyDocumentEvents> dummyDocumentEvents, out Dictionary<IDocumentSupportable, List<DocumentCommand>> dummyDocumentCommands)
		{
			recipients = null;
			documentPacks = new List<DocumentPack>();
			dummyDocumentEvents = new Dictionary<IDocumentSupportable, DummyDocumentEvents>();
			dummyDocumentCommands = new Dictionary<IDocumentSupportable, List<DocumentCommand>>();
			var result = true;
			var orgPk = ZGuid.Empty;

			foreach (var target in targets)
			{
				var dummyCommands = new List<DocumentCommand>();
				var events = CreateDummyDocumentEvents(target);
				foreach (var currentCommand in commands)
				{
					currentCommand.Parent = target;
					DocDeliveryContactCollection currentContacts;
					if (Runner.OverrideRecipientEmail)
					{
						currentContacts = new DocDeliveryContactCollection(null);
						var overriddenContact = currentContacts.AddNew();
						overriddenContact.OrgHeaderPK = GlbCompany.CurrentCompany.OrgProxy.PK;
						overriddenContact.DeliveryMethod = ContactNotifyModes.Email;
						overriddenContact.Email = Runner.RecipientEmail;
					}
					else
					{
						var autoDelivery = new DocAutoDelivery();
						currentContacts = autoDelivery.GetDeliveryContacts(currentCommand, target.DocumentSupporter);
					}

					if (currentContacts == null || currentContacts.Count == 0 || !currentContacts.OfType<DocDeliveryContact>().Any(c => !c.OrgHeaderPK.IsEmpty && c.DeliveryMethod == ContactNotifyModes.Email))
					{
						Log.Notify(OperationalActionLogErrorLevel.Error, Res.GetString("50C5E674-D939-4EC5-AAB9-41C156CBBE89", "There is no valid recipient for [{0}].", ((BusinessObject)target).HumanReadableName));
						result = false;
						break;
					}

					var contacts = currentContacts.OfType<DocDeliveryContact>().ToArray();
					if (orgPk.IsEmpty)
					{
						Log.Notify(OperationalActionLogErrorLevel.Informational, Res.GetString("F17AA7C1-CFD1-4665-B6BA-B663AC338EE1", "Found the following recipient email address. \r\n{0}", string.Join("\r\n", contacts.Where(x => x.DeliveryMethod == ContactNotifyModes.Email).Select(x => x.DeliveryAddress))));
						orgPk = contacts.FirstOrDefault().OrgHeaderPK;
						recipients = currentContacts;
					}
					else if (contacts.Any(c => c.OrgHeaderPK != orgPk))
					{
						Log.Notify(OperationalActionLogErrorLevel.Error, Res.GetString("F5CEAFAB-0CD9-4AA2-9BC7-4B2CE4AD8939", "The Deliver Documents In One Email option only supports sending documents to contacts in a single organization, delivering documents to multiple organizations is not supported."));
						result = false;
						break;
					}

					var dataState = target.DocumentSupporter.GetDataStateBeforeRun(currentCommand);
					if (dataState != null && !dataState.IsValid)
					{
						result = false;
						Log.Notify(OperationalActionLogErrorLevel.Error, dataState.ErrorMessage + " (" + ((BusinessObject)target).HumanReadableName + ", " + currentCommand.DocumentId + ")");
						break;
					}

					var providerList = InitializeProviderList((BusinessObject)target);
					var printSet = new DocumentPrintSet(currentCommand, providerList);

					bool cancel = events.OnDocumentPrintRequested(target, new DocumentCancelEventArgs(printSet.ParentMenuCommand));
					if (!cancel)
					{
						documentPacks.AddRange(printSet.GetDocumentPacks());
						dummyCommands.Add(currentCommand);
					}
				}

				if (!result)
				{
					break;
				}

				dummyDocumentEvents.Add(target, events);
				dummyDocumentCommands.Add(target, dummyCommands);
			}

			return result;
		}
	}
}
