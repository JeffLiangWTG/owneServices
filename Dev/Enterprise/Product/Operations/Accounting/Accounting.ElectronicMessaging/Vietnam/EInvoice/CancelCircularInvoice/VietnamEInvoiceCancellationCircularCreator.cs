using System;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Vietnam
{
	class VietnamEInvoiceCancellationCircularCreator
	{
		public VietnamEInvoiceCancellationCircularCreator(AdditionalTransactionInfoForVietnamEInvoice additionalTransactionInfo)
		{
			this.additionalTransactionInfo = additionalTransactionInfo;
		}

		readonly AdditionalTransactionInfoForVietnamEInvoice additionalTransactionInfo;

		#region SuppressResourceStringsCheckRegion

		public VietnamEInvoiceCancellationCircular Create()
		{
			var companyPk = additionalTransactionInfo.CompanyPK.ToGuid();
			var branchPk = additionalTransactionInfo.BranchPK.ToGuid();

			var eInvoice = new VietnamEInvoiceCancellationCircular()
			{
				Language = AccountingConfigurationRegistry.Instance.VietnamEInvoicingErrorMessageLanguage.GetFallBackValueAtAllLevels(companyPk, branchPk, Guid.Empty),
				User = { Username = "", Password = "" },
				Wrongnotice =
				{
					Stax = VietnamEInvoiceHelper.GetVietnamRegistrationNumber(companyPk, branchPk),
					Taxtype = "1",
					Taxnum = ZString.Empty,
					Taxdt = ZString.Empty,
					BudgetRelationid = ZString.Empty,
					Place = VietnamEInvoiceHelper.GetVietnamProxyOrgCityName(companyPk, branchPk),
				},
			};

			var item = new ItemForCancelCir78
			{
				Form = "1",
				Serial = additionalTransactionInfo.ComplianceSequenceInfo.OriginalSeriesPrefix,
				Seq = additionalTransactionInfo.OriginalSequenceNumber,
				Idt = additionalTransactionInfo.ComplianceDocumentDate.ToString("yyyy-MM-dd HH:mm"),
				TypeRef = 1,
				NotiType = "1",
				Rea = additionalTransactionInfo.RectifyReason
			};

			eInvoice.Wrongnotice.Items.Add(item);

			return eInvoice;
		}

		#endregion
	}
}
