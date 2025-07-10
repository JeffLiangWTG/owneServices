using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.Declaration.Testing
{
	sealed class EntryLineTaxAndConfirmedFeeUserControlTest : TestCaseWithFactory
	{
		public void TestEntryLineDutyAndTaxGroupBoxCaption()
		{
			AssertEquals("Calculated Duty And Tax", entryLineTaxAndConfirmedFeeUserControl.EntryLineDutyAndTaxGroupBox.CaptionResourceString.Caption);
		}

		public void TestEntryLineDutyAndTaxGrid_ChargeTypeGroup()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryheader = declaration.ActiveEntryHeaders.AddNew();
			using (var form = new ZForm(entryheader.AllEntryLines))
			{
				form.Controls.Add(entryLineTaxAndConfirmedFeeUserControl);
				form.Show();
				var dutyAndTaxGrid = entryLineTaxAndConfirmedFeeUserControl.EntryLineDutyAndTaxGrid;
				CombineAssertions(() =>
				{
					AssertEquals("Charge Type Group", "321D28C7-371B-4933-9416-555BBA2266C9", dutyAndTaxGrid.Columns[Customs.Business.AutoCusEntryLineFee.Schema.CF_ChargeType].GroupName.Key);
					AssertEquals("Charge Description Group", "321D28C7-371B-4933-9416-555BBA2266C9", dutyAndTaxGrid.Columns[nameof(CusEntryLineFee.ChargeTypeDescription)].GroupName.Key);
				});
			}
		}

		public void TestEntryLineDutyAndTaxGrid_Type()
		{
			AssertColumnStyle<ZDropEditColumnStyleInfo>(entryLineTaxAndConfirmedFeeUserControl.EntryLineDutyAndTaxGrid, Customs.Business.AutoCusEntryLineFee.Schema.CF_ChargeType, "Type", 47);
		}

		public void TestEntryLineDutyAndTaxGrid_TypeDescription()
		{
			var columnName = nameof(CusEntryLineFee.ChargeTypeDescription);
			AssertColumnStyle<ZTextBoxColumnStyleInfo>(entryLineTaxAndConfirmedFeeUserControl.EntryLineDutyAndTaxGrid, columnName, "Description", 183);
			var columnStyle = entryLineTaxAndConfirmedFeeUserControl.EntryLineDutyAndTaxGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);
			AssertEquals("IsReadOnly", true, columnStyle.IsReadOnly);
		}

		public void TestEntryLineDutyAndTaxGrid_Action()
		{
			AssertColumnStyle<ZDropEditColumnStyleInfo>(entryLineTaxAndConfirmedFeeUserControl.EntryLineDutyAndTaxGrid, Customs.Business.AutoCusEntryLineFee.Schema.CF_RateOverrideReasonCode, "Action", 53);
		}

		public void TestEntryLineDutyAndTaxGrid_BaseAmount()
		{
			AssertColumnStyle<ZCalcEditColumnStyleInfo>(entryLineTaxAndConfirmedFeeUserControl.EntryLineDutyAndTaxGrid, Customs.Business.AutoCusEntryLineFee.Schema.CF_BaseValue, "Base Amount", 115);
		}

		public void TestEntryLineDutyAndTaxGrid_MethodOfCalculation()
		{
			AssertColumnStyle<ZDropEditColumnStyleInfo>(entryLineTaxAndConfirmedFeeUserControl.EntryLineDutyAndTaxGrid, Customs.Business.AutoCusEntryLineFee.Schema.CF_MethodOfCalculation, "Method of Calculation", 128);
		}

		public void TestEntryLineDutyAndTaxGrid_TaxRate()
		{
			var columnName = Customs.Business.AutoCusEntryLineFee.Schema.CF_Rate;
			AssertColumnStyle<ZCalcEditColumnStyleInfo>(entryLineTaxAndConfirmedFeeUserControl.EntryLineDutyAndTaxGrid, columnName, "Tax Rate", 126);
			var columnStyle = (ZCalcEditColumnStyleInfo)entryLineTaxAndConfirmedFeeUserControl.EntryLineDutyAndTaxGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);
			AssertEquals("TextAlign", HorizontalAlignment.Right, columnStyle.TextAlign);
		}

		public void TestEntryLineDutyAndTaxGrid_TotalAmount()
		{
			var columnName = Customs.Business.AutoCusEntryLineFee.Schema.CF_ChargeAmount;
			AssertColumnStyle<ZCalcEditColumnStyleInfo>(entryLineTaxAndConfirmedFeeUserControl.EntryLineDutyAndTaxGrid, columnName, "Total Amount", 142);
			var columnStyle = (ZCalcEditColumnStyleInfo)entryLineTaxAndConfirmedFeeUserControl.EntryLineDutyAndTaxGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);
			AssertEquals("TextAlign", HorizontalAlignment.Right, columnStyle.TextAlign);
		}

		public void TestEntryLineDutyAndTaxGrid_MethodOfPayment()
		{
			AssertColumnStyle<ZDropEditColumnStyleInfo>(entryLineTaxAndConfirmedFeeUserControl.EntryLineDutyAndTaxGrid, Customs.Business.AutoCusEntryLineFee.Schema.CF_MethodOfPayment, "Method of Payment", 118);
		}

		public void TestEntryLineConfirmedDutyAndTaxGroupBoxCaption()
		{
			AssertEquals("Confirmed Duty And Tax", entryLineTaxAndConfirmedFeeUserControl.EntryLineConfirmedDutyAndTaxGroupBox.CaptionResourceString.Caption);
		}

		public void TestEntryLineTaxAndConfirmedFeeUserControl_ChargeTypeGroup()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryheader = declaration.ActiveEntryHeaders.AddNew();
			using (var form = new ZForm(entryheader.AllEntryLines))
			{
				form.Controls.Add(entryLineTaxAndConfirmedFeeUserControl);
				form.Show();
				var dutyAndTaxGrid = entryLineTaxAndConfirmedFeeUserControl.EntryLineConfirmedDutyAndTaxGrid;
				CombineAssertions(() =>
				{
					AssertEquals("Charge Type Group", "B8800F7B-E130-4A8C-B9A2-116051DC0C59", dutyAndTaxGrid.Columns[Customs.Business.AutoCusEntryLineFee.Schema.CF_ChargeType].GroupName.Key);
					AssertEquals("Charge Description Group", "B8800F7B-E130-4A8C-B9A2-116051DC0C59", dutyAndTaxGrid.Columns[nameof(EU.Business.Declaration.CusEntryLineFee.ChargeTypeDescription)].GroupName.Key);
				});
			}
		}

		public void TestEntryLineTaxAndConfirmedFeeUserControl_Type()
		{
			AssertColumnStyle<ZTextBoxColumnStyleInfo>(entryLineTaxAndConfirmedFeeUserControl.EntryLineConfirmedDutyAndTaxGrid, Customs.Business.AutoCusEntryLineFee.Schema.CF_ChargeType, "Type", 47);
		}

		public void TestEntryLineTaxAndConfirmedFeeUserControl_TypeDescription()
		{
			var columnName = nameof(EU.Business.Declaration.CusEntryLineFee.ChargeTypeDescription);
			AssertColumnStyle<ZTextBoxColumnStyleInfo>(entryLineTaxAndConfirmedFeeUserControl.EntryLineConfirmedDutyAndTaxGrid, columnName, "Description", 183);
			var columnStyle = entryLineTaxAndConfirmedFeeUserControl.EntryLineConfirmedDutyAndTaxGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);
			AssertEquals("IsReadOnly", true, columnStyle.IsReadOnly);
		}

		public void TestEntryLineTaxAndConfirmedFeeUserControl_Action()
		{
			AssertColumnStyle<ZTextBoxColumnStyleInfo>(entryLineTaxAndConfirmedFeeUserControl.EntryLineConfirmedDutyAndTaxGrid, Customs.Business.AutoCusEntryLineFee.Schema.CF_RateOverrideReasonCode, "Action", 53);
		}

		public void TestEntryLineTaxAndConfirmedFeeUserControl_BaseAmount()
		{
			AssertColumnStyle<ZCalcEditColumnStyleInfo>(entryLineTaxAndConfirmedFeeUserControl.EntryLineConfirmedDutyAndTaxGrid, Customs.Business.AutoCusEntryLineFee.Schema.CF_BaseValue, "Base Amount", 115);
		}

		public void TestEntryLineTaxAndConfirmedFeeUserControl_MethodOfCalculation()
		{
			AssertColumnStyle<ZTextBoxColumnStyleInfo>(entryLineTaxAndConfirmedFeeUserControl.EntryLineConfirmedDutyAndTaxGrid, Customs.Business.AutoCusEntryLineFee.Schema.CF_MethodOfCalculation, "Method of Calculation", 128);
		}

		public void TestEntryLineTaxAndConfirmedFeeUserControl_TaxRate()
		{
			var columnName = Customs.Business.AutoCusEntryLineFee.Schema.CF_Rate;
			AssertColumnStyle<ZCalcEditColumnStyleInfo>(entryLineTaxAndConfirmedFeeUserControl.EntryLineConfirmedDutyAndTaxGrid, columnName, "Tax Rate", 126);
			var columnStyle = (ZCalcEditColumnStyleInfo)entryLineTaxAndConfirmedFeeUserControl.EntryLineConfirmedDutyAndTaxGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);
			AssertEquals("TextAlign", HorizontalAlignment.Right, columnStyle.TextAlign);
		}

		public void TestEntryLineTaxAndConfirmedFeeUserControl_TotalAmount()
		{
			var columnName = Customs.Business.AutoCusEntryLineFee.Schema.CF_ChargeAmount;
			AssertColumnStyle<ZCalcEditColumnStyleInfo>(entryLineTaxAndConfirmedFeeUserControl.EntryLineConfirmedDutyAndTaxGrid, columnName, "Total Amount", 142);
			var columnStyle = (ZCalcEditColumnStyleInfo)entryLineTaxAndConfirmedFeeUserControl.EntryLineConfirmedDutyAndTaxGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);
			AssertEquals("TextAlign", HorizontalAlignment.Right, columnStyle.TextAlign);
		}

		public void TestEntryLineTaxAndConfirmedFeeUserControl_MethodOfPayment()
		{
			AssertColumnStyle<ZTextBoxColumnStyleInfo>(entryLineTaxAndConfirmedFeeUserControl.EntryLineConfirmedDutyAndTaxGrid, Customs.Business.AutoCusEntryLineFee.Schema.CF_MethodOfPayment, "Method of Payment", 118);
		}

		public void TestEntryLineDutyAndTaxGrid_NationalFeeTypeCode()
		{
			AssertColumnStyle<ZTextBoxColumnStyleInfo>(entryLineTaxAndConfirmedFeeUserControl.EntryLineConfirmedDutyAndTaxGrid, CusEntryLineFee.Schema.NationalFeeTypeCode, "National Type", 90);
		}

		protected override void SetUp()
		{
			base.SetUp();
			entryLineTaxAndConfirmedFeeUserControl = new EntryLineTaxAndConfirmedFeeUserControl();
		}
		EntryLineTaxAndConfirmedFeeUserControl entryLineTaxAndConfirmedFeeUserControl;

		protected override void TearDown()
		{
			entryLineTaxAndConfirmedFeeUserControl?.Dispose();
			base.TearDown();
		}

		void AssertColumnStyle<T>(ZGrid grid, string columnName, string caption, int width) where T : ZGridColumnInfo
		{
			var columnStyle = grid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);

			CombineAssertions(() =>
			{
				AssertType<T>(columnStyle);
				AssertEquals("Caption", caption, columnStyle.CaptionResourceString.Caption);
				AssertEquals("Width", width, columnStyle.Width);
			});
		}
	}
}
