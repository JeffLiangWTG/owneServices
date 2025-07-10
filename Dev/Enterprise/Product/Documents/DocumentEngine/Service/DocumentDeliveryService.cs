using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Shared;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Service
{
	public class DocumentDeliveryService : IDocumentDeliveryService
	{
		public Action<LogType, string> LogAction { get; set; }

		public ActionResult DeliverDocument(Guid documentCommandPk, string tablePrefix, Guid businessObjectPk, DeliveryInstructionsBase deliveryInstructions, DocumentDetail[] documents)
		{
			var factory = new BusinessObjectFactory() { NameForDebugging = "DocumentDelivery WebService Document Delivery" };
			var documentCommand = factory.Load<DocumentCommand>(documentCommandPk);
			var businessObject = factory.Load(tablePrefix, businessObjectPk);

			var result = ValidateForDelivery(businessObject, documentCommand, deliveryInstructions);

			if (!result.Success)
			{
				return result;
			}

			var documentSuppressionList = new List<Guid>();
			if (documents != null)
			{
				var documentsToHide =
					from item in documents
					where !item.ShouldInclude
					select item.Id;
				documentSuppressionList.AddRange(documentsToHide);
			}

			using (var documentPrintSet = new DocumentPrintSet(documentCommand, null, documentSuppressionList.ToArray()))
			{
				var adapter = new DeliveryInstructionsAdapter(deliveryInstructions);

				if (adapter.BackgroundDelivery)
				{
					documentPrintSet.CreateStmDocumentDelivery(adapter);
				}
				else
				{
					documentPrintSet.Run(adapter);
				}

				if (adapter.HasPrintedDocuments)
				{
					SaveDefaultPrinter(deliveryInstructions.PrinterId, documentCommandPk);
				}

				ShowOnlyPrintersUserCanPrintTo = deliveryInstructions.ShowOnlyPrintersUserCanPrintTo;
			}

			return result;
		}

		ActionResult ValidateForDelivery(BusinessObject businessObject, DocumentCommand documentCommand, DeliveryInstructionsBase deliveryInstructions)
		{
			var dataState = CheckDataState(documentCommand, businessObject);
			if (dataState is { IsValid: false })
			{
				LogAction?.Invoke(LogType.Error, FormattableString.Invariant($"Unable to run this document, the error reason is {dataState.ErrorMessage}"));
				return new ActionResult(false, dataState.ErrorMessage);
			}

			DocDeliveryPrintDetails.PrintQueuePK = deliveryInstructions.PrinterId;
			var printQueueError = DocDeliveryPrintDetails.PrintQueuePKInfo.GetErrors().LastOrDefault();
			if (printQueueError != null)
			{
				return new ActionResult(false, printQueueError.Message);
			}

			return new ActionResult(true, "");
		}

		public RecipientDetail[] GetDeliveryRecipients(Guid documentCommandPk, string tablePrefix, Guid businessObjectPk)
		{
			var result = new List<RecipientDetail>();
			var factory = new BusinessObjectFactory() { NameForDebugging = "DocumentDelivery WebService Document Auto-Delivery Contract Retrieval" };
			var documentCommand = factory.Load<DocumentCommand>(documentCommandPk);
			var documentSupportable = factory.Load(tablePrefix, businessObjectPk) as IDocumentSupportable;
			if (documentCommand != null && documentSupportable != null)
			{
				documentCommand.Parent = documentSupportable;

				var documentSupporter = documentSupportable.DocumentSupporter;
				if (documentSupporter != null)
				{
					IEnumerable<DocDeliveryContact> deliveryContacts;
					if (documentCommand.SU_PreventAutoDelivery || documentCommand.SU_ContactType == ContactType.NoContactType.Code)
					{
						var docDeliveryContact = new DocDeliveryContact(factory);
						docDeliveryContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
						deliveryContacts = new[] { docDeliveryContact };
					}
					else
					{
						var docAutoDelivery = new DocAutoDelivery();
						deliveryContacts = docAutoDelivery.GetDeliveryContacts(documentCommand, documentSupporter).OfType<DocDeliveryContact>();
					}

					foreach (var deliveryContact in deliveryContacts)
					{
						var recipientDetail = new RecipientDetail
						{
							Name = deliveryContact.Name,
							AttachmentType = deliveryContact.AttachmentType,
							Address = deliveryContact.DeliveryAddress,
							DeliveryMethod = deliveryContact.DeliveryMethod,
							CC = deliveryContact.EmailCarbonCopyRecipientsAsString,
							BCC = deliveryContact.EmailBlindCarbonCopyRecipientsAsString,
						};

						var orgHeader = deliveryContact.OrgHeader;
						if (orgHeader != null)
						{
							recipientDetail.OrganizationID = orgHeader.PK.ToGuid();
							recipientDetail.Organization = orgHeader.OH_Code;
						}

						result.Add(recipientDetail);
					}
				}
			}

			return result.ToArray();
		}

		public Guid? GetDefaultPrinterKey(Guid documentCommandPK)
		{
			var factory = new BusinessObjectFactory { NameForDebugging = "DocumentDelivery WebSerivce Default Printer Retrieval" };
			return StmDefaultPrinter.LoadDefaultPrinterUnsafe(factory, Env.CurrentUserPK, documentCommandPK, null)?.SDP_SQ_Printer.ToGuid();
		}

		static void SaveDefaultPrinter(Guid printerId, Guid documentCommandPK)
		{
			if (printerId != Guid.Empty && documentCommandPK != Guid.Empty)
			{
				var factory = new BusinessObjectFactory { NameForDebugging = "DocumentDelivery WebService Default Printer Save" };
				var defaultPrinter = StmDefaultPrinter.LoadOrCreateDefaultPrinterUnsafe(factory, Env.CurrentUserPK, GlbStaffSchema.Constants.Prefix, documentCommandPK, null); // Env.CurrentUser is always a staff
				defaultPrinter.SDP_SQ_Printer = printerId;
				factory.Save();
			}
		}

		public bool ShowOnlyPrintersUserCanPrintTo
		{
			get => DocDeliveryPrintDetails.ShowOnlyPrintersUserCanPrintTo;
			set => DocDeliveryPrintDetails.ShowOnlyPrintersUserCanPrintTo = value;
		}

		public PrinterDetail[] GetPrinters()
		{
			var result = new List<PrinterDetail>();

			foreach (StmPrintQueue printQueue in DocDeliveryPrintDetails.GetOnlinePrintersRegardlessOfAllowable())
			{
				var printerDetail = new PrinterDetail()
				{
					ID = printQueue.PK.ToGuid(),
					Name = printQueue.SQ_DisplayName,
					Location = printQueue.SQ_ServerName,
					IsPrintAllowed = printQueue.IsPrintAllowed
				};

				result.Add(printerDetail);
			}

			return result.ToArray();
		}

		DocDeliveryPrintDetails DocDeliveryPrintDetails => docDeliveryPrintDetails ??= new DocDeliveryPrintDetails(new BusinessObjectFactory() { NameForDebugging = "DocumentDelivery WebService Print Details Retrieval" });
		DocDeliveryPrintDetails docDeliveryPrintDetails;

		public DocumentDetail[] GetDocuments(Guid documentCommandPk)
		{
			var result = new List<DocumentDetail>();
			var factory = new BusinessObjectFactory() { NameForDebugging = "DocumentDelivery WebService Document Retrieval" };
			var documentCommand = factory.Load<DocumentCommand>(documentCommandPk);
			if (documentCommand != null)
			{
				AddDocumentsToList(result, documentCommand);

				foreach (StmMenuMenuPivotBase childDocumentPivot in documentCommand.ChildMenus)
				{
					var childCommand = factory.Load<DocumentCommand>(childDocumentPivot.SF_SU_Outward);
					AddDocumentsToList(result, childCommand);
				}
			}

			return result.ToArray();
		}

		public bool CanPreview(Guid documentCommandPk)
		{
			var factory = new BusinessObjectFactory { NameForDebugging = "DocumentDelivery WebSerivce Document Preview Security Right Retrieval" };
			var documentCommand = factory.Load<DocumentCommand>(documentCommandPk);

			if (documentCommand == null)
			{
				return false;
			}
			else
			{
				var documentPack = new DocumentPack(documentCommand);
				var deliveryInstructions = documentPack.DeliveryInstructions;

				return deliveryInstructions.AllowPreviewIfDocument;
			}
		}

		DocumentSupporterDataState CanRunDocument(DocumentCommand documentCommand)
		{
			var canRunDocument = true;
			var message = ZString.Empty;

			if (documentCommand.SU_IsPublished)
			{
				var securityCheckpoint = StmMenuItemBaseCheckpointFinder.FindSecurityCheckpointByPK(documentCommand.PK);
				if (securityCheckpoint != null && !securityCheckpoint.IsAllowed)
				{
					canRunDocument = false;
					message = Env.Security.GetErrorMessageForNotAllowedInGLOW((Security.SecurityCheckpoint)securityCheckpoint);
				}
			}

			return new DocumentSupporterDataState(canRunDocument, message);
		}

		public DocumentSupporterDataState CheckDataState(Guid documentCommandPk, string tablePrefix, Guid businessObjectPk)
		{
			var factory = new BusinessObjectFactory() { NameForDebugging = "DocumentDelivery WebService Document Data State Check" };
			var documentCommand = factory.Load<DocumentCommand>(documentCommandPk);

			var businessObject = factory.Load(tablePrefix, businessObjectPk);
			return CheckDataState(documentCommand, businessObject);
		}

		DocumentSupporterDataState CheckDataState(DocumentCommand documentCommand, BusinessObject businessObject)
		{
			if (documentCommand != null && businessObject is IDocumentSupportable documentSupportable)
			{
				var canRunDocument = CanRunDocument(documentCommand);
				if (!canRunDocument.IsValid)
				{
					return canRunDocument;
				}
				documentCommand.Parent = documentSupportable;
				return documentSupportable.DocumentSupporter.GetDataStateBeforeRun(documentCommand);
			}

			return new DocumentSupporterDataState(false, Res.GetString("d333eaec-52e8-4dd0-b688-545490b96b79", "No Document Menu or Object found."));
		}

		static void AddDocumentsToList(List<DocumentDetail> result, DocumentCommand documentCommand)
		{
			foreach (StmMenuTemplatePivot document in documentCommand.Documents.ToArray()) //Use ToArray() to prevent changing collection during iteration.
			{
				var documentDetail = new DocumentDetail
				{
					Id = document.PK.ToGuid(),
					Name = document.SI_DocumentTitle,
					Mode = document.SI_PrintCopyType,
					ShouldInclude = true
				};

				result.Add(documentDetail);
			}
		}
	}

	#region Types

	public class DocumentDetail
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public string Mode { get; set; }
		public bool ShouldInclude { get; set; }
	}

	public class PrinterDetail
	{
		public Guid ID { get; set; }
		public string Name { get; set; }
		public string Location { get; set; }
		public bool IsPrintAllowed { get; set; }
	}

	public class RecipientDetail
	{
		public Guid OrganizationID { get; set; }
		public string Organization { get; set; }
		public string Name { get; set; }
		public string Address { get; set; }
		public string CC { get; set; }
		public string BCC { get; set; }
		public string DeliveryMethod { get; set; }
		public string AttachmentType { get; set; }
	}

	public class DeliveryInstructionDetail
	{
		public RecipientDetail[] Recipients { get; set; }
		public DocumentDetail[] Documents { get; set; }
		public PrinterDetail[] Printers { get; set; }
		public Guid? PrinterPK { get; set; }
		public bool ShowOnlyPrintersUserCanPrintTo { get; set; }
		public bool CanPreview { get; set; }
	}

	public class ActionResult
	{
		public ActionResult(bool isSuccess, string message)
		{
			Success = isSuccess;
			Message = message;
		}
		public bool Success { get; }
		public string Message { get; }
	}
	#endregion
}
