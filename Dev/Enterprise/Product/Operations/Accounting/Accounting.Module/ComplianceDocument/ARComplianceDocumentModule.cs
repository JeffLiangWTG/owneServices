using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Module
{
	public partial class ARComplianceDocumentModule : ComplianceDocumentModule
	{
		#region message

		static string AllocatingCaption => Res.GetString("47073b0c-14d4-4742-abcb-6786978b903f", "Allocate Compliance Sequence Number");

		#endregion

		#region show message

		bool ConfirmToPrintDocument(string message)
		{
			return Globals.Message.Show(message, ComplianceDocumentHelper.PrintingCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
		}

		void ShowPrintingResult(string message, bool isError)
		{
			Globals.Message.Show(message, ComplianceDocumentHelper.PrintingCaption, MessageBoxButtons.OK, isError ? MessageBoxIcon.Error : MessageBoxIcon.Warning);
		}

		#endregion

		public override ModuleIdentifier ID => ModuleIDs.ARComplianceDocument;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.ReceivablesComplianceDocuments;

		public override string WorkflowType => WorkflowDescriptors.ARComplianceDocumentCode;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.ARComplianceDocument);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ARComplianceDocumentFilterStripBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			var controller = new ComplianceDocumentFilterStripControl(GridCollection, (ARComplianceDocumentFilterStripBusinessObject)FilterBusinessObject);
			controller.FilteredGrid.ColorContextKey = "ARComplianceDocumentFilterStripControl";
			return controller;
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ARComplianceDocumentHeaderCollection(Factory);
		}

		#region GetNewAdditionalMenuItems

		protected override MenuItem[] GetNewAdditionalMenuItems()
		{
			List<MenuItem> menus = new List<MenuItem>(base.GetNewAdditionalMenuItems());

			menus.Insert(0, new ZMenuItem(ResString.GetMultilingualString("86578a06-6fa4-4672-a437-99f5437803ca", "Print"), new EventHandler(HandlePrint)));

			return menus.ToArray();
		}

		#endregion

		#region GetNewMenuItems

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var menuItems = new List<MenuItem>(base.GetNewActionMenuItems());

			if (GlbCompany.CurrentCompany.Country.HasAccComplianceSequence)
			{
				menuItems.Add(new ZMenuItem("-"));
				menuItems.Add(new ZMenuItem(Res.GetString("abadc9f5-91a1-4276-bfd1-ef741bb69aae", "Lock Counter Compliance Book"), new EventHandler(HandleLockComplianceBook)));
				menuItems.Add(new ZMenuItem(Res.GetString("6e2c5d8e-cfdc-4fa9-83b0-2d9dd99275ea", "Release Counter Compliance Book"), new EventHandler(HandleReleaseComplianceBook)));
			}

			menuItems.Add(new ZMenuItem("-"));
			menuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Accounting.ARComplianceDocument.Allocate", "Allocate Compliance Sequence Number"), new EventHandler(HandleAllocate)));

			if (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value)
			{
				menuItems.Add(new ZMenuItem("-"));
				menuItems.Add(new ZMenuItem(Res.GetString("3F39A6C8-3B82-4BC4-81C1-FA95EF8EF198", "Reset Status to Queued"), new EventHandler(HandleResetStatusToQueued)));
			}

			return menuItems.ToArray();
		}

		#endregion

		#region EventHandlers

		protected void HandlePrint(object sender, EventArgs e)
		{
			if (!Env.Security.PrintReceivablesComplianceDocuments.IsAllowed)
			{
				Env.Security.PrintReceivablesComplianceDocuments.ShowError();
				return;
			}

			if (SelectedBusinessObjects.Length == 0)
			{
				ShowNoSelectedMessage();
				return;
			}

			var selectedDocumentHeaders = SelectedBusinessObjects.Cast<ARComplianceDocumentHeader>();
			ComplianceDocumentHelper.PromptAndPrintComplianceDocument(ConfirmToPrintDocument, ShowPrintingResult, selectedDocumentHeaders, string.Empty, true);
		}

		#region Allocate

		#region Allocate Messages

		#region Message Headers

		static string AllocateSucceed => Res.GetString("9491B213-8BA9-4810-A72A-FA85E9C02E36", "Allocate Compliance Sequence Number successfully.");
		static string AllocateAllFailed => Res.GetString("747A1551-5013-41E6-AD12-4C36170860D5", "No compliance document allocated due to one of the following reasons:");
		static string AllocateSomeFailed => Res.GetString("9FEB5672-5F3E-42A2-BD9F-F2A55F86DB1D", "Some of the compliance document records that you have selected cannot be allocated due to one of the following reasons:");
		static string AllocateException => Res.GetString("44CF346F-5D5E-48D7-B7E4-3A4AA05EEE6F", "The compliance document cannot be allocated.");

		#endregion

		#region Error Reasons

		static string Voided => Res.GetString("FF52305C-7668-4802-8063-A366E66C641E", "The compliance document(s) record that you have selected already has been voided.");
		static string HaveComplianceNumber => Res.GetString("C7608150-8022-463E-AE3F-BF79E9C8F9CC", "The compliance document(s) record that you have selected already has a compliance number allocated.");
		static string NoMatchingComplianceInvoiceBookFixed => Res.GetString("E5F99B74-6DB5-4DF8-A692-C42A5FA634AE", "No matching compliance invoice book.");
		static string NoMatchingComplianceInvoiceBook => Res.GetString("8801BB2F-E8B7-47A9-8E07-C6694031C1E4", "The compliance document(s) you selected no matching compliance invoice book found.");
		static string ComplianceNumberAllocated => Res.GetString("E1522712-03B0-4C5E-84EA-75D379F7F66E", "Compliance number has already been allocated.");
		static string ComplianceNumberVoided => Res.GetString("284A8AF4-6B11-47C6-B7E9-EA797B75F3E1", "Compliance number has already been voided.");
		static string CreditNoteNotAllowedByRegistry => Res.GetString("2809E54C-9535-491E-BF86-06A1D5D94D94", @"Allocation of compliance sequence number to Credit Note compliance document is not allowed.
The Credit Note compliance document’s number must reference an existing Invoice compliance document as per system configuration.");
		static string TXENotAllowedByRegistry => Res.GetString("71946629-FF2E-4544-B305-821427880225", "Compliance document number cannot be allocated to TXE compliance document as 'Enable E-Reporting Functionality' registry is set to 'No'.");

		#endregion

		static List<string> FixedPossibleReasons => new List<string>() {
				NoMatchingComplianceInvoiceBookFixed,
				ComplianceNumberAllocated,
				ComplianceNumberVoided
			};

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected void HandleAllocate(object sender, EventArgs e)
		{
			if (!Env.Security.AllocateComplianceSequenceNumber.IsAllowed)
			{
				Env.Security.AllocateComplianceSequenceNumber.ShowError();
				return;
			}

			if (SelectedBusinessObjects.Length == 0)
			{
				ShowNoSelectedMessage();
				return;
			}

			var selectedComplianceDocuments = SelectedBusinessObjects.Cast<ARComplianceDocumentHeader>();

			if (selectedComplianceDocuments.All(x => x.IsVoided))
			{
				Globals.Message.Show(Voided, AllocatingCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			if (selectedComplianceDocuments.All(x => x.IsAllocated) && !selectedComplianceDocuments.Any(x => x.IsVoided))
			{
				Globals.Message.Show(HaveComplianceNumber, AllocatingCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			var newFactory = new BusinessObjectFactory();
			var complianceDocumentsToAllocate = newFactory.Load<ARComplianceDocumentHeader>(new ZQuery(AccComplianceDocumentHeaderSchema.PK, selectedComplianceDocuments.Select(x => x.PK))).ToList();
			var countOfAllocatedBefore = complianceDocumentsToAllocate.Count(x => x.IsAllocated);

			var possiblereasonsForNotAllocating = FixedPossibleReasons;

			if (!FilterByTransactionType(selectedComplianceDocuments, possiblereasonsForNotAllocating, complianceDocumentsToAllocate))
			{
				return;
			}

			if (!FilterByComplianceSubType(selectedComplianceDocuments, possiblereasonsForNotAllocating, complianceDocumentsToAllocate))
			{
				return;
			}

			var countryFactory = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(complianceDocumentsToAllocate.FirstOrDefault().Company.Country.Code);
			var complianceDocumentNumberProvider = (countryFactory as IInstanceProvider<IComplianceDocumentNumberProvider>)?.Get();
			var allocateComplianceDocumentNumberErrorMessage = complianceDocumentNumberProvider?.AllocateComplianceDocumentNumberErrorMessage(selectedComplianceDocuments);

			if (!string.IsNullOrEmpty(allocateComplianceDocumentNumberErrorMessage))
			{
				Globals.Message.Show(allocateComplianceDocumentNumberErrorMessage, AllocatingCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			complianceDocumentNumberProvider?.CheckCanAllocateComplianceDocumentNumberForSomeComplianceDocument(possiblereasonsForNotAllocating, complianceDocumentsToAllocate);

			ZString[] complianceBooks;
			try
			{
				complianceBooks = ComplianceDocumentHelper.AllocateComplianceDocuments(newFactory, complianceDocumentsToAllocate.ToArray());
				newFactory.Save();
			}
			catch (ComplianceSequenceRelatedException ex) when
			(ex is FailedToFindComplianceSequenceException
			|| ex is AllocationComplianceSequenceFullException
			|| ex is AllocationComplianceSequenceBusyException)
			{
				Globals.Message.Show($"{AllocateException}\r\n{ex.UserFriendlyMessage}", AllocatingCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			if (complianceBooks.Length > 0)
			{
				var notSynchroniseMessage = ComplianceDocumentHelper.GetNotSyncErrorMessage(complianceBooks);
				possiblereasonsForNotAllocating.Add(notSynchroniseMessage);
			}

			var countOfAllocatedAfter = newFactory.Load<ARComplianceDocumentHeader>(new ZQuery(AccComplianceDocumentHeaderSchema.PK, complianceDocumentsToAllocate.Select(x => x.PK))).Count(x => x.IsAllocated);

			if (selectedComplianceDocuments.All(x => !x.IsAllocated && x.ADH_XD_ComplianceBook.IsEmpty))
			{
				Globals.Message.Show(NoMatchingComplianceInvoiceBook, AllocatingCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			else
			{
				if (countOfAllocatedAfter == selectedComplianceDocuments.Count())
				{
					ShowAllocateAllSuccessfullyMessage();
				}
				else
				{
					var allCanNotAllocated = countOfAllocatedAfter == countOfAllocatedBefore;
					ShowNotAllocateMessage(possiblereasonsForNotAllocating, allCanNotAllocated);
				}
			}
		}

		bool FilterByTransactionType(IEnumerable<ARComplianceDocumentHeader> selectedComplianceDocuments, List<string> possiblereasonsForNotAllocatings, List<ARComplianceDocumentHeader> complianceDocumentsToAllocate)
		{
			if (AccountingMasterFilesRegistry.Instance.CreditNoteComplianceDocumentConfiguration.Value)
			{
				if (selectedComplianceDocuments.All(x => x.ADH_TransactionType == TransactionTypes.CreditNote))
				{
					Globals.Message.Show(CreditNoteNotAllowedByRegistry, AllocatingCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
					return false;
				}

				complianceDocumentsToAllocate.RemoveAll(n => n.ADH_TransactionType != TransactionTypes.Invoice);

				possiblereasonsForNotAllocatings.Add(CreditNoteNotAllowedByRegistry);
			}

			return true;
		}

		bool FilterByComplianceSubType(IEnumerable<ARComplianceDocumentHeader> selectedComplianceDocuments, List<string> possiblereasonsForNotAllocatings, List<ARComplianceDocumentHeader> complianceDocumentsToAllocate)
		{
			if (!AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value)
			{
				Func<ARComplianceDocumentHeader, bool> subTypeCodeIsTXEPredicate = n => n.ComplianceSubType == TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE;

				if (selectedComplianceDocuments.All(subTypeCodeIsTXEPredicate))
				{
					Globals.Message.Show(TXENotAllowedByRegistry, AllocatingCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
					return false;
				}

				if (complianceDocumentsToAllocate.Any(subTypeCodeIsTXEPredicate))
				{
					complianceDocumentsToAllocate.RemoveAll(n => subTypeCodeIsTXEPredicate(n));
					possiblereasonsForNotAllocatings.Add(TXENotAllowedByRegistry);
				}
			}

			return true;
		}

		void ShowAllocateAllSuccessfullyMessage()
		{
			Globals.Message.Show(AllocateSucceed, AllocatingCaption, MessageBoxButtons.OK, MessageBoxIcon.None);
		}

		void ShowNotAllocateMessage(List<string> possiblereasonsForNotAllocating, bool allCanNotAllocated)
		{
			var possiblereasonsForNotAllocatingMessage = ComplianceDocumentHelper.JoinAndAttachLineNumber(possiblereasonsForNotAllocating);
			if (allCanNotAllocated)
			{
				var errorMessageForAllCanNotAllocated = AllocateAllFailed + System.Environment.NewLine + possiblereasonsForNotAllocatingMessage;
				Globals.Message.Show(errorMessageForAllCanNotAllocated, AllocatingCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			else
			{
				var errorMessageForSomeOfCanNotAllocated = AllocateSomeFailed + System.Environment.NewLine + possiblereasonsForNotAllocatingMessage;
				Globals.Message.Show(errorMessageForSomeOfCanNotAllocated, AllocatingCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		#endregion

		protected void HandleResetStatusToQueued(object sender, EventArgs e)
		{
			if (SelectedBusinessObjects != null && SelectedBusinessObjects.Length > 0)
			{
				var pivotStatusesEligibleForRequeuing = new List<ZString>() { EInvoicingPivotState.BatchedWithError, EInvoicingPivotState.Failed };
				var requeueFactory = new BusinessObjectFactory();
				var documentPivots = requeueFactory.Load<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, SelectedBusinessObjects.Select(x => x.PK)).AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ParentTableCode, AccComplianceDocumentHeaderSchema.Constants.Prefix));
				var documentPivotsToQueue = documentPivots.Where(x => pivotStatusesEligibleForRequeuing.Contains(x.AIP_Status) || (x.AIP_Status == EInvoicingPivotState.Batched && x.Batch?.AIB_Status.ToString() == EInvoicingBatchState.Discarded)).ToList();
				var userMessageBuilder = new ZStringBuilder();

				if (documentPivotsToQueue.Count < SelectedBusinessObjects.Length)
				{
					userMessageBuilder.Append(Res.GetString("007A87C8-65E1-4810-98F7-A7A074FEC0FE", "You can only reset compliance documents where E-Reporting pivot status is 'FAL' - Fail or 'BER' - Batched with errors or 'BCH' - Batched and batch status is 'DCD' - Discarded."));
				}

				if (documentPivotsToQueue.Any())
				{
					if (!userMessageBuilder.IsEmpty)
					{
						userMessageBuilder.Append(Res.GetString("81FB89F2-0BDB-45B8-8642-70B739F66EDE", "Only compliance documents that satisfy this criteria will be reset."));
						Globals.Message.ShowWarning(userMessageBuilder.ToStringWithNewLineBetweenAppends());
					}
					HandleResetStatusToQueuedCore(documentPivotsToQueue);
				}
				else
				{
					userMessageBuilder.Append(Res.GetString("010E9EF8-927D-4E47-9D91-F05C9F7C6D3F", "No compliance documents will be reset."));
					Globals.Message.ShowError(userMessageBuilder.ToStringWithNewLineBetweenAppends());
				}
			}
			else
			{
				ShowNoSelectedMessage();
			}
		}

		protected virtual void HandleResetStatusToQueuedCore(List<AccEInvoicingTransactionPivot> documentPivotsToQueue)
		{
			try
			{
				var parentDocumentPKs = documentPivotsToQueue.Select(x => x.AIP_ParentID);
				var parentDocuments = SelectedBusinessObjects.Where(x => parentDocumentPKs.Contains(x.PK)).ToList();
				documentPivotsToQueue.ForEach(x => x.Requeue());
				documentPivotsToQueue.First().Factory.Save();
				parentDocuments.ForEach(x => x.Refresh());
				Globals.Message.ShowInformation(Res.GetString("D9656023-B34B-4AF4-9993-0CA23DBEB4A7", "Compliance Documents were successfully reset."));
			}
			catch (ZSaveConcurrencyException)
			{
				Globals.Message.ShowError(Res.GetString("791F478E-DCF4-4DB4-AA11-ADB9B445BEB3", "While you were working, another user has modified these compliance documents. Please refresh the grid and try again."));
			}
		}

		protected void HandleLockComplianceBook(object sender, EventArgs e)
		{
			if (Env.Security.ComplianceSequencesModifyLockRelease.IsAllowed)
			{
				ZController lockComplianceBookController = (MasterFiles.Module.LockComplianceBookController)ZControllerFactory.Create(ControllerIDs.LockComplianceBook);

				var lockForm = (MasterFiles.GUI.LockReleaseComplianceBookForm)lockComplianceBookController.ShowNewForm();

#if DEBUG
				LastShownComplianceDocumentForm_ForTest = lockForm;
#endif
			}
			else
			{
				Env.Security.ComplianceSequencesModifyLockRelease.ShowError();
			}
		}

		protected void HandleReleaseComplianceBook(object sender, EventArgs e)
		{
			if (Env.Security.ComplianceSequencesModifyLockRelease.IsAllowed || Env.Security.ComplianceSequencesModifyReleaseOtherStaff.IsAllowed)
			{
				ZController releaseComplianceBookController = (MasterFiles.Module.ReleaseComplianceBookController)ZControllerFactory.Create(ControllerIDs.ReleaseComplianceBook);
#if DEBUG
				LastShownComplianceDocumentForm_ForTest = (ZForm)releaseComplianceBookController.ShowNewForm();
#else
				releaseComplianceBookController.ShowNewForm();
#endif
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("36c9383c-ac8e-4fca-820f-b4a77d273e92", @"You do not have the appropriate security rights to run this function. 

If you require access to lock/release ‘CTR’ Allocation Level Compliance Invoice Book, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

{0}

If you require access to release ‘CTR’ Allocation Level Compliance Invoice Book locked by other staff, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

{1}", Env.Security.ComplianceSequencesModifyLockRelease.DisplayTextPathToSecurityRight, Env.Security.ComplianceSequencesModifyReleaseOtherStaff.DisplayTextPathToSecurityRight));
			}
		}

		#endregion
	}
}
