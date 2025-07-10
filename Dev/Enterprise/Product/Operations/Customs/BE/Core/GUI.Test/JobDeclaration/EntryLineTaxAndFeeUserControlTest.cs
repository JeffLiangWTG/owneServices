using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI.Testing;

sealed class EntryLineTaxAndFeeUserControlTest : TestCaseWithFactory
{
	public void TestSetAllEntryLinesReadOnly()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.ZG_ManualDeclaration = false;
		var entryHeader = declaration.ActiveEntryHeaders.AddNew() as CusEntryHeader;
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryHeader.MovementReferenceNumberSetter("TESTMRN456");
		var line = entryHeader.AllEntryLines.AddNew();
		line.Taxes.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("Lines where just added so not readonly", false, entryHeader.AllEntryLines.ReadOnly);
			using (var form = new ZForm())
			{
				form.Controls.Add(entryLineTaxAndFeeUserControl);
				entryLineTaxAndFeeUserControl.SetDataBinding(entryHeader.AllEntryLines, "");
				form.Show();
				AssertEquals("The binding has fired the check for the read only", true, entryHeader.AllEntryLines.ReadOnly);
			}
		});
	}

	public void TestEntryLineDutyAndTaxGroupBoxCaption()
	{
		AssertEquals("Duty And Tax", entryLineTaxAndFeeUserControl.EntryLineDutyAndTaxGroupBox.CaptionResourceString.Caption);
	}

	public void TestEntryLineTaxAndConfirmedFeeUserControl_ChargeTypeGroup()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.ActiveEntryHeaders.AddNew();
		using (var form = new ZForm(entryHeader.AllEntryLines))
		{
			form.Controls.Add(entryLineTaxAndFeeUserControl);
			form.Show();
			var dutyAndTaxGrid = entryLineTaxAndFeeUserControl.EntryLineDutyAndTaxGrid;
			CombineAssertions(() =>
			{
				AssertEquals("Charge Type Group", "EC73ED11-9936-4600-86A1-4E96FCD36448", dutyAndTaxGrid.Columns[Customs.Business.AutoCusEntryLineFee.Schema.CF_ChargeType].GroupName.Key);
				AssertEquals("Charge Description Group", "EC73ED11-9936-4600-86A1-4E96FCD36448", dutyAndTaxGrid.Columns[nameof(EU.Business.Declaration.CusEntryLineFee.ChargeTypeDescription)].GroupName.Key);
			});
		}
	}

	public void TestEntryLineTaxAndConfirmedFeeUserControl_Type()
	{
		var columnName = Customs.Business.AutoCusEntryLineFee.Schema.CF_ChargeType;
		var columnStyle = entryLineTaxAndFeeUserControl.EntryLineDutyAndTaxGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);
		AssertEquals("Width", 100, columnStyle.Width);
	}

	public void TestEntryLineTaxAndConfirmedFeeUserControl_TypeDescription()
	{
		var columnName = nameof(EU.Business.Declaration.CusEntryLineFee.ChargeTypeDescription);
		AssertColumnStyle<ZTextBoxColumnStyleInfo>(columnName, "Description", 150);
		var columnStyle = entryLineTaxAndFeeUserControl.EntryLineDutyAndTaxGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);
		AssertEquals("IsReadOnly", true, columnStyle.IsReadOnly);
		AssertEquals("Width", 150, columnStyle.Width);
	}

	public void TestEntryLineTaxAndConfirmedFeeUserControl_Action()
	{
		var columnName = Customs.Business.AutoCusEntryLineFee.Schema.CF_RateOverrideReasonCode;
		var columnStyle = (ZDropEditColumnStyleInfo)entryLineTaxAndFeeUserControl.EntryLineDutyAndTaxGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);
		AssertEquals("Width", 140, columnStyle.Width);
	}

	public void TestEntryLineTaxAndConfirmedFeeUserControl_BaseAmount()
	{
		var columnName = Customs.Business.AutoCusEntryLineFee.Schema.CF_BaseValue;
		var columnStyle = (ZCalcEditColumnStyleInfo)entryLineTaxAndFeeUserControl.EntryLineDutyAndTaxGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);
		AssertEquals("Width", 80, columnStyle.Width);
	}

	public void TestEntryLineTaxAndConfirmedFeeUserControl_MethodOfCalculation()
	{
		var columnName = Customs.Business.AutoCusEntryLineFee.Schema.CF_MethodOfCalculation;
		var columnStyle = (ZDropEditColumnStyleInfo)entryLineTaxAndFeeUserControl.EntryLineDutyAndTaxGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);
		AssertEquals("Width", 150, columnStyle.Width);
	}

	public void TestEntryLineTaxAndConfirmedFeeUserControl_TaxRate()
	{
		var columnName = Customs.Business.AutoCusEntryLineFee.Schema.CF_Rate;
		var columnStyle = (ZCalcEditColumnStyleInfo)entryLineTaxAndFeeUserControl.EntryLineDutyAndTaxGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);
		AssertEquals("TextAlign", HorizontalAlignment.Right, columnStyle.TextAlign);
		AssertEquals("Width", 80, columnStyle.Width);
	}

	public void TestEntryLineTaxAndConfirmedFeeUserControl_TotalAmount()
	{
		var columnName = Customs.Business.AutoCusEntryLineFee.Schema.CF_ChargeAmount;
		var columnStyle = (ZCalcEditColumnStyleInfo)entryLineTaxAndFeeUserControl.EntryLineDutyAndTaxGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);
		AssertEquals("TextAlign", HorizontalAlignment.Right, columnStyle.TextAlign);
		AssertEquals("Width", 120, columnStyle.Width);
	}

	public void TestEntryLineTaxAndConfirmedFeeUserControl_MethodOfPayment()
	{
		var columnName = Customs.Business.AutoCusEntryLineFee.Schema.CF_MethodOfPayment;
		var columnStyle = (ZDropEditColumnStyleInfo)entryLineTaxAndFeeUserControl.EntryLineDutyAndTaxGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);
		AssertEquals("Width", 120, columnStyle.Width);
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
