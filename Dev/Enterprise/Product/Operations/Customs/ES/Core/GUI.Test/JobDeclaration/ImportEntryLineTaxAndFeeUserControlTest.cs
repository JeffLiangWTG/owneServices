using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Test
{
	sealed class ImportEntryLineTaxAndFeeUserControlTest : TestCaseWithFactory
	{
		public void TestEntryLineDutyAndTaxGroupBoxCaption()
		{
			AssertEquals("Duty And Tax", importEntryLineTaxAndFeeUserControl.EntryLineDutyAndTaxGroupBox.CaptionResourceString.Caption);
		}

		public void TestEntryLineTaxAndConfirmedFeeUserControl_ChargeTypeGroup()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryheader = declaration.ActiveEntryHeaders.AddNew();
			using (var form = new ZForm(entryheader.AllEntryLines))
			{
				form.Controls.Add(importEntryLineTaxAndFeeUserControl);
				form.Show();
				var dutyAndTaxGrid = importEntryLineTaxAndFeeUserControl.EntryLineDutyAndTaxGrid;
				CombineAssertions(() =>
				{
					AssertEquals("Charge Type Group", "329CF68A-F0C3-4BE6-BB05-352F13B5CBF3", dutyAndTaxGrid.Columns[Customs.Business.AutoCusEntryLineFee.Schema.CF_ChargeType].GroupName.Key);
					AssertEquals("Charge Description Group", "329CF68A-F0C3-4BE6-BB05-352F13B5CBF3", dutyAndTaxGrid.Columns[nameof(EU.Business.Declaration.CusEntryLineFee.ChargeTypeDescription)].GroupName.Key);
				});
			}
		}

		public void TestEntryLineTaxAndConfirmedFeeUserControl_Type()
		{
			AssertColumnStyle<ZDropEditColumnStyleInfo>(Customs.Business.AutoCusEntryLineFee.Schema.CF_ChargeType, "Type", 80);
		}

		public void TestEntryLineTaxAndConfirmedFeeUserControl_TypeDescription()
		{
			var columnName = nameof(EU.Business.Declaration.CusEntryLineFee.ChargeTypeDescription);
			AssertColumnStyle<ZTextBoxColumnStyleInfo>(columnName, "Description", 150);
			var columnStyle = importEntryLineTaxAndFeeUserControl.EntryLineDutyAndTaxGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);
			AssertEquals("IsReadOnly", true, columnStyle.IsReadOnly);
		}

		public void TestEntryLineTaxAndConfirmedFeeUserControl_Action()
		{
			AssertColumnStyle<ZDropEditColumnStyleInfo>(Customs.Business.AutoCusEntryLineFee.Schema.CF_RateOverrideReasonCode, "Action", 140);
		}

		public void TestEntryLineTaxAndConfirmedFeeUserControl_BaseAmount()
		{
			AssertColumnStyle<ZCalcEditColumnStyleInfo>(Customs.Business.AutoCusEntryLineFee.Schema.CF_BaseValue, "Base Amount", 80);
		}

		public void TestEntryLineTaxAndConfirmedFeeUserControl_MethodOfCalculation()
		{
			AssertColumnStyle<ZDropEditColumnStyleInfo>(Customs.Business.AutoCusEntryLineFee.Schema.CF_MethodOfCalculation, "Method of Calculation", 150);
		}

		public void TestEntryLineTaxAndConfirmedFeeUserControl_TaxRate()
		{
			var columnName = Customs.Business.AutoCusEntryLineFee.Schema.CF_Rate;
			AssertColumnStyle<ZCalcEditColumnStyleInfo>(columnName, "Tax Rate", 80);
			var columnStyle = (ZCalcEditColumnStyleInfo)importEntryLineTaxAndFeeUserControl.EntryLineDutyAndTaxGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);
			AssertEquals("TextAlign", HorizontalAlignment.Right, columnStyle.TextAlign);
		}

		public void TestEntryLineTaxAndConfirmedFeeUserControl_TotalAmount()
		{
			var columnName = Customs.Business.AutoCusEntryLineFee.Schema.CF_ChargeAmount;
			AssertColumnStyle<ZCalcEditColumnStyleInfo>(columnName, "Total Amount", 80);
			var columnStyle = (ZCalcEditColumnStyleInfo)importEntryLineTaxAndFeeUserControl.EntryLineDutyAndTaxGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);
			AssertEquals("TextAlign", HorizontalAlignment.Right, columnStyle.TextAlign);
		}

		public void TestEntryLineTaxAndConfirmedFeeUserControl_MethodOfPayment()
		{
			AssertColumnStyle<ZDropEditColumnStyleInfo>(Customs.Business.AutoCusEntryLineFee.Schema.CF_MethodOfPayment, "Method of Payment", 120);
		}

		public void TestEntryLineTaxAndConfirmedFeeUserControl_MaxMin()
		{
			AssertColumnStyle<ZTextBoxColumnStyleInfo>(CusEntryLineFee.Schema.MaxMin, "Max-Min", 60);
		}

		protected override void SetUp()
		{
			base.SetUp();
			importEntryLineTaxAndFeeUserControl = new ImportEntryLineTaxAndFeeUserControl();
		}
		ImportEntryLineTaxAndFeeUserControl importEntryLineTaxAndFeeUserControl;

		protected override void TearDown()
		{
			importEntryLineTaxAndFeeUserControl?.Dispose();
			base.TearDown();
		}

		void AssertColumnStyle<T>(string columnName, string caption, int width) where T : ZGridColumnInfo
		{
			var columnStyle = importEntryLineTaxAndFeeUserControl.EntryLineDutyAndTaxGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);

			CombineAssertions(() =>
			{
				AssertType<T>(columnStyle);
				AssertEquals("Caption", caption, columnStyle.CaptionResourceString.Caption);
				AssertEquals("Width", width, columnStyle.Width);
			});
		}
	}
}
