using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Accounting.Business.ComplianceReport.PTRS
{
	public class PtrsAllPaymentsReport : PtrsReportBase
	{
		public PtrsAllPaymentsReport(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void GetDataFromComplianceReportLines()
		{
			if (ComplianceReport != null
				&& (ComplianceReport.ACR_Status == AccComplianceReport.Status.ReportGenerated || ComplianceReport.ACR_IsFinalised))
			{
				AllInvoicesPaid = ComplianceReport.ReportLines.OfType<AccComplianceReportLine>().Where(x => !x.PreCalculatedAmount.IsEmpty).Sum(x => x.PreCalculatedAmount);
				if (!this.IsInDatabase)
				{
					// Force creating AccTaxReturnColumn which will keep the Amount used by PTR report
					AllInvoicesPaidWithOverride = AllInvoicesPaid;
				}
			}
		}

		[ResourceStringData("d1c9cdc1-ed1f-45e2-aaf4-94faf83d79b8", Caption = "All Payments Amount")]
		[DecimalPlaces(2)]
		public ZDecimal AllInvoicesPaid { get; private set; }

		[ResourceStringData("04a7d4c7-297f-4f37-9d50-7dc5d352e588", Caption = "Adjusted Amount")]
		[DecimalPlaces(2)]
		public ZDecimal AllInvoicesPaidWithOverride
		{
			get => this.GetOverriddenOrCalculatedAmount(AllInvoicesPaidColumnName, AllInvoicesPaid, NeedReasonToOverride);
			set
			{
				this.SetOverriddenAmount(AllInvoicesPaidColumnName, value, valueChanged: null, commentRequiredCheck: NeedReasonToOverride);
				RefreshBindingIncludingChildren();
			}
		}

		[ReadOnlyMember(nameof(ReasonToOverrideAllInvoicesPaid_ReadOnly))]
		[ResourceStringData("5AF124F7-C4DC-4327-9107-D870C9FB4CF0", Caption = "Adjustment Reason")]
		public ZString ReasonToOverrideAllInvoicesPaid
		{
			get => this.GetOverrideReason(AllInvoicesPaidColumnName, NeedReasonToOverride);
			set
			{
				this.SetOverrideReason(AllInvoicesPaidColumnName, value, NeedReasonToOverride);
				RefreshBindingIncludingChildren();
			}
		}

		public ZPropertyInfo ReasonToOverrideAllInvoicesPaidInfo
			=> GetWrappedZPropertyInfo(nameof(ReasonToOverrideAllInvoicesPaid), _ => this.GetOverrideReasonInfo(AllInvoicesPaidColumnName, commentRequiredCheck: () => NeedReasonToOverride(), AllInvoicesPaid));

		bool NeedReasonToOverride() => AllInvoicesPaid != AllInvoicesPaidWithOverride;

		bool ReasonToOverrideAllInvoicesPaid_ReadOnly => !NeedReasonToOverride() && ReasonToOverrideAllInvoicesPaid.IsEmpty;

		const string AllInvoicesPaidColumnName = nameof(AllInvoicesPaid);
	}
}
