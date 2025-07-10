using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.DE.GUI.Testing
{
	public class InvoiceLineChargesUserControlTest : TestCaseWithFactory
	{
		public void TestFixedRateColumnDetails()
		{
			using (var control = new InvoiceLineChargesUserControl())
			{
				var apportionedChargesGrid = control.ApportionedChargesGrid;
				var fixedRateColumnStyle = apportionedChargesGrid.GetColumnStyle(InvoiceLineCharge.Schema.IsJ7_ExchangeRateIATA);
				CombineAssertions(() =>
				{
					AssertEquals("Width", 47, fixedRateColumnStyle.Width);
					AssertEquals("Mandatory", true, fixedRateColumnStyle.IsMandatory);
				});
			}
		}

		public void TestExchangeRateDateColumn()
		{
			using (var control = new InvoiceLineChargesUserControl())
			{
				var apportionedChargesGrid = control.ApportionedChargesGrid;
				var exchangeRateColumnStyle = (ZDateEditColumnStyleInfo)apportionedChargesGrid.GetColumnStyle(InvoiceLineCharge.Schema.J7_ExchangeRateDate);
				CombineAssertions(() =>
				{
					AssertEquals("Width", 121, exchangeRateColumnStyle.Width);
					AssertEquals("Mandatory", true, exchangeRateColumnStyle.IsMandatory);
					AssertEquals("Format", ZArchitecture.Core.ZDateTimePickerFormat.Short, exchangeRateColumnStyle.DateTimeFormat);
					AssertEquals("Caption", "Exchange Rate Date", exchangeRateColumnStyle.CaptionResourceString.Caption);
				});
			}
		}

		public void TestChargesGridColumns()
		{
			using (var control = new InvoiceLineChargesUserControl())
			{
				var chargesGrid = control.ChargesGrid;
				CombineAssertions(() =>
				{
					AssertEquals("ChargeCodeDescription", true, chargesGrid.GetColumnStyle(InvoiceLineCharge.Schema.ChargeCodeDescription).IsUnavailable);
					AssertEquals("IsJ7_ExchangeRateIATA", false, chargesGrid.GetColumnStyle(InvoiceLineCharge.Schema.IsJ7_ExchangeRateIATA).IsUnavailable);
					AssertEquals("J7_ExchangeRateDate", false, chargesGrid.GetColumnStyle(InvoiceLineCharge.Schema.J7_ExchangeRateDate).IsUnavailable);
					AssertEquals("J7_ChargeDescription", false, chargesGrid.GetColumnStyle(InvoiceLineCharge.Schema.J7_ChargeDescription).IsUnavailable);
				});
			}
		}

		public void TestDescriptionColumn()
		{
			using (var control = new InvoiceLineChargesUserControl())
			{
				var chargesGrid = control.ChargesGrid;
				var descriptionColumnStyle = chargesGrid.GetColumnStyle(InvoiceLineCharge.Schema.J7_ChargeDescription);
				CombineAssertions(() =>
				{
					AssertEquals("Caption", "Description", descriptionColumnStyle.CaptionResourceString.Caption);
					AssertEquals("Mandatory", true, descriptionColumnStyle.IsMandatory);
					AssertEquals("Width", 184, descriptionColumnStyle.Width);
					AssertEquals("CharacterCasing", CharacterCasing.Normal, descriptionColumnStyle.CharacterCasing);
				});
			}
		}
	}
}
