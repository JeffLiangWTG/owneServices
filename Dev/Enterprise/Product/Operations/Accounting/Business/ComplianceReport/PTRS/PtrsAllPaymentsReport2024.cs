using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Accounting.Business.ComplianceReport.PTRS
{
	public class PtrsAllPaymentsReport2024 : PtrsReportBase
	{
		public PtrsAllPaymentsReport2024(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Report based summaries

		[ResourceStringData("e738c63d-afbc-4639-8aa4-682b1c039f23", Caption = "Small Business Partial Payments")]
		[DecimalPlaces(2)]
		public ZDecimal SmallBusinessPartialPaymentAmount { get => smallBusinessPartialPaymentAmount.Value; private set => smallBusinessPartialPaymentAmount.Value = value; }

		[ResourceStringData("397f1e02-8775-45e6-b30c-83e1c3369056", Caption = "Small Business Full Payments")]
		[DecimalPlaces(2)]
		public ZDecimal SmallBusinessFullPaymentAmount { get => smallBusinessFullPaymentAmount.Value; private set => smallBusinessFullPaymentAmount.Value = value; }

		[ResourceStringData("d0d1e074-bfdc-4112-856f-092c1a57d7bc", Caption = "Other Partial Payments")]
		[DecimalPlaces(2)]
		public ZDecimal OthersPartialPaymentAmount { get => othersPartialPaymentAmount.Value; private set => othersPartialPaymentAmount.Value = value; }

		[ResourceStringData("dc6c44d3-2b3b-4133-a4f1-07e2996743bb", Caption = "Other Full Payments")]
		[DecimalPlaces(2)]
		public ZDecimal OthersFullPaymentAmount { get => othersFullPaymentAmount.Value; private set => othersFullPaymentAmount.Value = value; }

		[ResourceStringData("0d044fca-7eae-44f8-87c4-9cea272413cf", Caption = "All Payments Amount")]
		[DecimalPlaces(2)]
		public ZDecimal AllInvoicesPaid => SmallBusinessPartialPaymentAmount + SmallBusinessFullPaymentAmount + OthersPartialPaymentAmount + OthersFullPaymentAmount;

		#endregion

		#region Columns to store summaries

		PtrsReportAmountColumn smallBusinessPartialPaymentAmount => this.GetOrCreateColumn<PtrsReportAmountColumn>(nameof(SmallBusinessPartialPaymentAmount)); 

		PtrsReportAmountColumn smallBusinessFullPaymentAmount => this.GetOrCreateColumn<PtrsReportAmountColumn>(nameof(SmallBusinessFullPaymentAmount));

		PtrsReportAmountColumn othersPartialPaymentAmount => this.GetOrCreateColumn<PtrsReportAmountColumn>(nameof(OthersPartialPaymentAmount));

		PtrsReportAmountColumn othersFullPaymentAmount => this.GetOrCreateColumn<PtrsReportAmountColumn>(nameof(OthersFullPaymentAmount));

		#endregion

		#region Get data

		protected override void GetDataFromComplianceReportLines()
		{
			if (ComplianceReport != null && !IsSubmitted)
			{
				var helper = Factory.ServiceContainer.GetService<IPtrsReport2024Helper>();
				if (helper != null)
				{
					var data = helper.CalculatePtrsAllPaymentsReport2024Data(ComplianceReport);

					SmallBusinessPartialPaymentAmount = data.SmallBusinessPartialPaymentAmount;
					SmallBusinessFullPaymentAmount = data.SmallBusinessFullPaymentAmount;
					OthersPartialPaymentAmount = data.OthersPartialPaymentAmount;
					OthersFullPaymentAmount = data.OthersFullPaymentAmount;
				}
			}
		}

		#endregion
	}
}
