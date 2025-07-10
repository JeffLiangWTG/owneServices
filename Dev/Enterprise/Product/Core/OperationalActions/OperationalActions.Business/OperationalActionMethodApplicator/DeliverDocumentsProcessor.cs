using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;

namespace Enterprise.Services.OperationalActions.Business
{
	class DeliverDocumentsProcessor : BaseDocumentProcessor
	{
		public DeliverDocumentsProcessor(OperationalActionRunner runner) : base(runner)
		{
		}

		public override void ProcessDocument()
		{
			Log.SetSectionProgressMax(TargetsPKs.Length * Action.DocumentPivots.Count);

			BusinessObjectFactory currentFactory = null;
			int targetsProcessed = 0;
			const int factoryTargetLimit = 10; // this is how many targets will be loaded per factory, so that GC can periodically clean up the unused factories

			Dictionary<ZGuid, (DocDeliveryPrintDetails, bool)> overridePrinterDetails = null;

			foreach (var pk in TargetsPKs)
			{
				if (targetsProcessed % factoryTargetLimit == 0)
				{
					currentFactory = GetNewFactory();
				}

				var target = currentFactory.Load(TargetType, pk);
				if (target == null)
				{
					Log.Notify(OperationalActionLogErrorLevel.Warning, Res.GetString("DocumentPseudoApplicator|NoTargetFound", "Unable to find the target file with type as {0} and PK as {1}", TargetType, pk));
					Log.BumpSectionProgress();
				}
				else
				{
					var dummyDocumentEvents = CreateDummyDocumentEvents((IDocumentSupportable)target);
					UserControlProviderList providerList = null;
					foreach (OperationalActionDocumentPivot pivot in Action.DocumentPivots)
					{
						DocumentCommand command = currentFactory.Load<DocumentCommand>(pivot.SF_SU_Outward);
						command.Parent = (IDocumentSupportable)command.Factory.Load(target.GetType(), target.PK);

						bool hasError = false;
						DocumentSupporterDataState dataState = command.Parent.DocumentSupporter.GetDataStateBeforeRun(command);
						if (dataState != null)
						{
							if (!dataState.IsValid)
							{
								Log.Notify(OperationalActionLogErrorLevel.Warning, dataState.ErrorMessage + " (" + target.HumanReadableName + ", " + command.DocumentId + ")");
								hasError = true;
							}
						}

						if (!hasError)
						{
							Log.NotifyFormat(OperationalActionLogErrorLevel.Debug, Res.GetString(
									"DocumentPseudoApplicator|Delivering",
									"Delivering '{0}' for '{1}'",
									command.SU_MenuName, target.HumanReadableName));

							var autoDelivery = new DocAutoDelivery();
							var instructions = CreateDeliveryInstructions(command.SU_DraftOption, null);
							using (instructions.Recipients.SuspendListChanged())
							{
								BulkDeliveryMethod.SetRecipients(instructions, autoDelivery.GetDeliveryContacts(command, ((IDocumentSupportable)target).DocumentSupporter));
								if (instructions.Recipients.Count > 0)
								{
									if (instructions.Recipients.Cast<DocDeliveryContact>().Any(recipient => recipient.DeliveryMethod == ContactNotifyModes.Email && recipient.EmailToRecipients.IsNullOrEmpty()))
									{
										Log.Notify(OperationalActionLogErrorLevel.Warning, Res.GetString(
												"DocumentPseudoApplicator|NoEmailToRecipientsFound",
												"There is no Email Recipient found when delivering document '{0}'\r\n('{1}')",
												command.SU_MenuName, target.HumanReadableName));
									}
									else
									{
										SetDeliveryInstructionsCoverNoteIfNeeded(instructions);

										Log.Notify(OperationalActionLogErrorLevel.Debug, Res.GetString(
												"DocumentPseudoApplicator|ContactsFound",
												"... found {0} contact(s).",
												instructions.Recipients.Count));

										providerList = providerList ?? InitializeProviderList(target);

										using (var set = new DocumentPrintSet(command, providerList))
										{
											bool cancel = dummyDocumentEvents.OnDocumentPrintRequested(target, new DocumentCancelEventArgs(set.ParentMenuCommand));

											if (!cancel)
											{
												DocumentPrintedEventHandler notifyPrePrinted = (sender, e) =>
													dummyDocumentEvents.OnDocumentPrePrinted(target, new DocumentPrintedEventArgs(instructions.Destination, set.ParentMenuCommand));
												set.DocumentPrePrinted += notifyPrePrinted;

												if (set.Count > 0)
												{
													instructions.DocPack = set.GetFirstDocumentPack();
												}
#if DEBUG
												if (Globals.IsTest)
												{
													BeginRunningPrintSet?.Invoke(this, new BeggingRunningPrintSetArg(target.PK, instructions));
												}
#endif

												var allowOverridePrintDetails = BulkDeliveryMethod.AllowOverridePrintDetails && set.Count > 0;
												RunDocumentPrintSet(set, instructions, allowOverridePrintDetails, ref overridePrinterDetails);

												dummyDocumentEvents.OnDocumentPrinted(target, new DocumentPrintedEventArgs(instructions.Destination, set.ParentMenuCommand));

												if (notifyPrePrinted != null)
												{
													set.DocumentPrePrinted -= notifyPrePrinted;
												}
											}
										}
									}
								}
								else
								{
									Log.Notify(OperationalActionLogErrorLevel.Warning, Res.GetString(
											"DocumentPseudoApplicator|NoContactsFound",
											"There are no contacts found that can be used to deliver '{0}'\r\n('{1}')",
											command.SU_MenuName, target.HumanReadableName));
								}
							}
						}
						Log.BumpSectionProgress();
					}
				}
				targetsProcessed++;
			}
		}

		void RunDocumentPrintSet(PrintTask set, DeliveryInstructions instructions, bool overridePrinter, ref Dictionary<ZGuid, (DocDeliveryPrintDetails PrintDetails, bool IncludedInPrint)> overrideDetails)
		{
			if (overridePrinter)
			{
				var documents = instructions.DocumentsToBeDelivered.OfType<IDeliverable>().Where(d => !d.MenuTemplatePivotPK.IsEmpty);

				var needToPopupDeliveryForm = false;
				if (overrideDetails != null)
				{
					foreach (var document in documents)
					{
						if (overrideDetails.TryGetValue(document.MenuTemplatePivotPK, out var overrideDetail))
						{
							document.PrinterDetails.PrintQueuePK = overrideDetail.PrintDetails.PrintQueuePK;
							document.PrinterDetails.NumberOfCopies = overrideDetail.PrintDetails.NumberOfCopies;
							document.IncludedInPrint = overrideDetail.IncludedInPrint;
						}
						else
						{
							needToPopupDeliveryForm = true;
						}
					}
				}

				if (overrideDetails == null || needToPopupDeliveryForm)
				{
					instructions.DeliveryOptions = AllowedDeliveryOptions.OverridePrintDetails;
					set.ShowOverridePrintDetailsDeliveryForm(instructions, Env.Security.None);

					if (instructions.Destination != DeliveryInstructionDestination.UserCancelled)
					{
						if (overrideDetails == null)
						{
							overrideDetails = new Dictionary<ZGuid, (DocDeliveryPrintDetails, bool)>();
						}

						foreach (var document in instructions.DocumentsToBeDelivered.OfType<IDeliverable>())
						{
							if (!overrideDetails.ContainsKey(document.MenuTemplatePivotPK))
							{
								overrideDetails.Add(document.MenuTemplatePivotPK, (document.PrinterDetails, document.IncludedInPrint));
							}
						}
					}
				}
			}

			set.Run(instructions);
		}

#if DEBUG
		internal event EventHandler<BeggingRunningPrintSetArg> BeginRunningPrintSet;

		public class BeggingRunningPrintSetArg : EventArgs
		{
			public BeggingRunningPrintSetArg(ZGuid dataSourcePK, DeliveryInstructions instructions)
			{
				DataSourcePK = dataSourcePK;
				Instructions = instructions;
			}

			public readonly ZGuid DataSourcePK;
			public readonly DeliveryInstructions Instructions;
		}
#endif
	}
}
