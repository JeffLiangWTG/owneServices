using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class EntryLineTaxAndFeeUserControlTest : TestCaseWithFactory
	{
		public void TestCF_IsLandedCostOnly_IsAvailable_IsReadonly()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryheader = declaration.ActiveEntryHeaders.AddNew();
			using (var form = new ZForm(entryheader.AllEntryLines))
			{
				form.Controls.Add(entryLineTaxAndFeeUserControl);
				form.Show();
				var feesGrid = entryLineTaxAndFeeUserControl.EntryLineDutyAndTaxGrid;
				var column = feesGrid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x =>
					!x.IsUnavailable && x.ColumnName == CusEntryLineFee.Schema.CF_IsLandedCostOnly);
				AssertNotNull("Column is available", column);
				CombineAssertions(() =>
				{
					AssertEquals("Column is readonly", true, column.IsReadOnly);
					AssertEquals("Column not visible by default", false, column.IsVisible);
				});
			}
		}

		[RequiresSTA]
		public void TestTestEntryLineDutyAndTaxGrid_ChargeTypeGroup()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryheader = declaration.ActiveEntryHeaders.AddNew();
			using (var form = new ZForm(entryheader.AllEntryLines))
			{
				form.Controls.Add(entryLineTaxAndFeeUserControl);
				form.Show();
				var dutyAndTaxGrid = entryLineTaxAndFeeUserControl.EntryLineDutyAndTaxGrid;
				CombineAssertions(() =>
				{
					AssertEquals("Charge Type Group", "7116D434-32A6-495D-8CC2-31609E454702", dutyAndTaxGrid.Columns[Customs.Business.AutoCusEntryLineFee.Schema.CF_ChargeType].GroupName.Key);
					AssertEquals("Charge Description Group", "7116D434-32A6-495D-8CC2-31609E454702", dutyAndTaxGrid.Columns[nameof(CusEntryLineFee.ChargeTypeDescription)].GroupName.Key);
				});
			}
		}

		public void TestEntryLineDutyAndTaxGrid_Type()
		{
			AssertColumnStyle<ZDropEditColumnStyleInfo>(Customs.Business.AutoCusEntryLineFee.Schema.CF_ChargeType, "Type", 80);
		}

		public void TestEntryLineDutyAndTaxGrid_TypeDescription()
		{
			var columnName = nameof(CusEntryLineFee.ChargeTypeDescription);
			AssertColumnStyle<ZTextBoxColumnStyleInfo>(columnName, "Description", 150);
			var columnStyle = entryLineTaxAndFeeUserControl.EntryLineDutyAndTaxGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);
			AssertEquals("IsReadOnly", true, columnStyle.IsReadOnly);
		}

		public void TestEntryLineDutyAndTaxGrid_Action()
		{
			AssertColumnStyle<ZDropEditColumnStyleInfo>(Customs.Business.AutoCusEntryLineFee.Schema.CF_RateOverrideReasonCode, "Action", 140);
		}

		public void TestEntryLineDutyAndTaxGrid_BaseAmount()
		{
			AssertColumnStyle<ZCalcEditColumnStyleInfo>(Customs.Business.AutoCusEntryLineFee.Schema.CF_BaseValue, "Base Amount", 80);
		}

		public void TestEntryLineDutyAndTaxGrid_MethodOfCalculation()
		{
			AssertColumnStyle<ZDropEditColumnStyleInfo>(Customs.Business.AutoCusEntryLineFee.Schema.CF_MethodOfCalculation, "Method of Calculation", 150);
		}

		public void TestEntryLineDutyAndTaxGrid_TaxRate()
		{
			var columnName = Customs.Business.AutoCusEntryLineFee.Schema.CF_Rate;
			AssertColumnStyle<ZCalcEditColumnStyleInfo>(columnName, "Tax Rate", 80);
			var columnStyle = (ZCalcEditColumnStyleInfo)entryLineTaxAndFeeUserControl.EntryLineDutyAndTaxGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);
			AssertEquals("TextAlign", HorizontalAlignment.Right, columnStyle.TextAlign);
		}

		public void TestEntryLineDutyAndTaxGrid_TotalAmount()
		{
			var columnName = Customs.Business.AutoCusEntryLineFee.Schema.CF_ChargeAmount;
			AssertColumnStyle<ZCalcEditColumnStyleInfo>(columnName, "Total Amount", 80);
			var columnStyle = (ZCalcEditColumnStyleInfo)entryLineTaxAndFeeUserControl.EntryLineDutyAndTaxGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);
			AssertEquals("TextAlign", HorizontalAlignment.Right, columnStyle.TextAlign);
		}

		public void TestEntryLineDutyAndTaxGrid_MethodOfPayment()
		{
			AssertColumnStyle<ZDropEditColumnStyleInfo>(Customs.Business.AutoCusEntryLineFee.Schema.CF_MethodOfPayment, "Method of Payment", 120);
		}

		protected override void SetUp()
		{
			base.SetUp();
			entryLineTaxAndFeeUserControl = new EntryLineTaxAndFeeUserControl();
		}
		EntryLineTaxAndFeeUserControl entryLineTaxAndFeeUserControl;

		protected override void TearDown()
		{
			entryLineTaxAndFeeUserControl?.Dispose();
			base.TearDown();
		}

		void AssertColumnStyle<T>(string columnName, string caption, int width) where T : ZGridColumnInfo
		{
			var columnStyle = entryLineTaxAndFeeUserControl.EntryLineDutyAndTaxGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);

			CombineAssertions(() =>
			{
				AssertType<T>(columnStyle);
				AssertEquals("Caption", caption, columnStyle.CaptionResourceString.Caption);
				AssertEquals("Width", width, columnStyle.Width);
			});
		}
	}
}
