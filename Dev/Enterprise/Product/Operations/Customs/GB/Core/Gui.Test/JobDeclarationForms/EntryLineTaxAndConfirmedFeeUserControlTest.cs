using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Testing
{
	sealed class EntryLineTaxAndConfirmedFeeUserControlTest : TestCaseWithFactory
	{
		public void TestEntryLineDutyAndTaxGroupBoxCaption()
		{
			AssertEquals("Calculated Duty And Tax", entryLineTaxAndConfirmedFeeUserControl.EntryLineCalculatedDutyAndTaxUserControl.EntryLineDutyAndTaxGroupBox.CaptionResourceString.Caption);
		}

		public void TestEntryLineCalculatedDutyAndTaxUserControlType()
		{
			AssertType<EU.GUI.EntryLineTaxAndFeeUserControl>(entryLineTaxAndConfirmedFeeUserControl.EntryLineCalculatedDutyAndTaxUserControl);
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
					AssertEquals("Charge Type Group", "bd973848-1bc3-4082-910d-fe1005e9b5a7", dutyAndTaxGrid.Columns[Customs.Business.AutoCusEntryLineFee.Schema.CF_ChargeType].GroupName.Key);
					AssertEquals("Charge Description Group", "bd973848-1bc3-4082-910d-fe1005e9b5a7", dutyAndTaxGrid.Columns[nameof(EU.Business.Declaration.CusEntryLineFee.ChargeTypeDescription)].GroupName.Key);
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
			var columnStyle = entryLineTaxAndConfirmedFeeUserControl.EntryLineConfirmedDutyAndTaxGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);
			AssertEquals("IsReadOnly", true, columnStyle.IsReadOnly);
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
			var columnStyle = (ZCalcEditColumnStyleInfo)entryLineTaxAndConfirmedFeeUserControl.EntryLineConfirmedDutyAndTaxGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);
			AssertEquals("TextAlign", HorizontalAlignment.Right, columnStyle.TextAlign);
		}

		public void TestEntryLineTaxAndConfirmedFeeUserControl_TotalAmount()
		{
			var columnName = Customs.Business.AutoCusEntryLineFee.Schema.CF_ChargeAmount;
			AssertColumnStyle<ZCalcEditColumnStyleInfo>(columnName, "Total Amount", 80);
			var columnStyle = (ZCalcEditColumnStyleInfo)entryLineTaxAndConfirmedFeeUserControl.EntryLineConfirmedDutyAndTaxGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);
			AssertEquals("TextAlign", HorizontalAlignment.Right, columnStyle.TextAlign);
		}

		public void TestEntryLineTaxAndConfirmedFeeUserControl_MethodOfPayment()
		{
			AssertColumnStyle<ZDropEditColumnStyleInfo>(Customs.Business.AutoCusEntryLineFee.Schema.CF_MethodOfPayment, "Method of Payment", 120);
		}

		public void TestEntryLineTaxAndConfirmedFeeUserControl_IsLandedCostOnly()
		{
			AssertColumnStyle<ZCheckBoxColumnStyleInfo>(Customs.Business.AutoCusEntryLineFee.Schema.CF_IsLandedCostOnly, "Is Landed Cost Only", 120);
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

		void AssertColumnStyle<T>(string columnName, string caption, int width) where T : ZGridColumnInfo
		{
			var columnStyle = entryLineTaxAndConfirmedFeeUserControl.EntryLineConfirmedDutyAndTaxGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);

			CombineAssertions(() =>
			{
				AssertType<T>(columnStyle);
				AssertEquals("Caption", caption, columnStyle.CaptionResourceString.Caption);
				AssertEquals("Width", width, columnStyle.Width);
			});
		}
	}
}
