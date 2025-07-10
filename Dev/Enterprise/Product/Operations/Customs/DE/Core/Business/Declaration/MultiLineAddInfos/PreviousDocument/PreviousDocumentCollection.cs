using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class PreviousDocumentCollection : EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection
	{
		public PreviousDocumentCollection(BusinessObject parent, bool isProcedure)
			: base(parent)
		{
			this.isProcedure = isProcedure;
			EnableMaxCountValidation();
		}
		readonly bool isProcedure;

		public new PreviousDocument this[int i] => (PreviousDocument)base[i];

		public new PreviousDocument AddNew() => (PreviousDocument)base.AddNew();

		protected override bool AllowNewCore
		{
			get
			{
				bool allowNew = base.AllowNewCore;
				if (Master is CusEntryInstruction entryInstruction)
				{
					allowNew = allowNew && !PreviousDocumentHelper.PreviousProceduresRequiringReference.Contains(entryInstruction.PreviousDocumentMaster.CSI_Procedure);
				}
				return allowNew;
			}
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = base.CreateAdditionalFilter();
			if (Master is IPreviousDocumentParentProvider master && master.PreviousDocumentMaster.IsExport)
			{
				if (isProcedure)
				{
					result.AddToFilter(CusSupportingInfoSchema.CSI_Procedure, new[] { PreviousProcedureList.Codes._ATAV, PreviousProcedureList.Codes._ATZL });
				}
				else
				{
					result.AddToFilter(CusSupportingInfoSchema.CSI_Procedure, SQLComparisonOperator.NotEqual, new[] { PreviousProcedureList.Codes._ATAV, PreviousProcedureList.Codes._ATZL });
				}
			}
			return result;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			using (child.SuspendSettingHasChanges())
			{
				if (Master is IPreviousDocumentParentProvider master)
				{
					var previousDocument = (PreviousDocument)child;
					var prevDocMaster = master.PreviousDocumentMaster;
					if (prevDocMaster.IsImport)
					{
						previousDocument.CSI_Procedure = prevDocMaster.CSI_Procedure;
						previousDocument.CSI_Status = PreviousStatusList.Codes.Yes;

						if (previousDocument.IsProcedureATAV)
						{
							previousDocument.AuthorizationNumber = prevDocMaster.AuthorizationNumber;
							previousDocument.CSI_CustomsOffice = prevDocMaster.CSI_CustomsOffice;
							previousDocument.SimplifiedGrantAuthorizationFlag = prevDocMaster.SimplifiedGrantAuthorizationFlag;
						}
						if (previousDocument.IsProcedureATZL)
						{
							previousDocument.AuthorizationNumber = prevDocMaster.AuthorizationNumber;
							previousDocument.CSI_ReferenceNumber2 = prevDocMaster.CSI_ReferenceNumber2;
						}
					}
					else if (isProcedure && prevDocMaster.IsExport && Master is JobComInvoiceLine invoiceLine)
					{
						previousDocument.CSI_Procedure = invoiceLine.PreviousProcedureMaster.CSI_Procedure;
						previousDocument.CSI_Status = PreviousStatusList.Codes.Yes;
					}
				}
			}
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			if (Count == 0)
			{
				//Ensures grid is made invisible, and Previous Procedure group box value is cleared.
				if (Master is JobComInvoiceHeader invoiceHeader)
				{
					var previousDocumentMaster = invoiceHeader.PreviousDocumentMaster;
					previousDocumentMaster.CSI_Procedure_ValueChanged?.Invoke(previousDocumentMaster, null);
					previousDocumentMaster.CSI_ProcedureInfo.RefreshBinding();
				}
				else if (Master is CusEntryInstruction entryInstruction)
				{
					var previousDocumentMaster = entryInstruction.PreviousDocumentMaster;
					previousDocumentMaster.CSI_Procedure_ValueChanged?.Invoke(previousDocumentMaster, null);
					previousDocumentMaster.CSI_ProcedureInfo.RefreshBinding();
				}
				else if (isProcedure && Master is JobComInvoiceLine invoiceLine)
				{
					var previousProcedureMaster = invoiceLine.PreviousProcedureMaster;
					previousProcedureMaster.CSI_Procedure_ValueChanged?.Invoke(previousProcedureMaster, null);
					previousProcedureMaster.CSI_ProcedureInfo.RefreshBinding();
				}
			}
		}

		void EnableMaxCountValidation()
		{
			if (!isProcedure
				&& Master is IPreviousDocumentParentProvider master
				&& master.PreviousDocumentMaster.IsExport
				&& (master is JobComInvoiceHeader || master is JobComInvoiceLine))
			{
				var maxCount = master.JobDeclaration.IsTransitionPeriodAES30 ? PreviousDocumentHelper.MaximumAllowedItemsForExportInvoiceHeaderAndInvoiceLineDuringTransitionPeriod : PreviousDocumentHelper.MaximumAllowedItemsForExportInvoiceHeaderAndInvoiceLine;
				MaxCountValidationEnable(maxCount, GetMaxCountErrorMessage(maxCount));
			}
		}

		string GetMaxCountErrorMessage(int maxCount) => Res.GetString("CD1D2AB1-3D1D-4331-8412-D1A1485B5664", "You may enter a maximum of {0} Previous Documents", maxCount);
	}
}
