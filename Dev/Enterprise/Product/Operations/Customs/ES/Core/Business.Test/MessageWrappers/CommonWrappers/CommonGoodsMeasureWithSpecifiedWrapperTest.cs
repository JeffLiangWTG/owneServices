using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing;

public class CommonGoodsMeasureWithSpecifiedWrapperTest : WrapperHelperTest<CommonGoodsMeasureWithSpecifiedWrapper>
{
	public void TestGrossWeight()
	{
		CombineAssertions(() =>
		{
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Grams;
			invoiceLine.JI_Weight = 1.123m;

			using (TemporarilyClearDeclarationConfigurationAndThenSetIsTransitionPeriodAES30(declaration, true))
			{
				declaration.JE_MessageType = "EXP";
				wrapper = GetWrapper(entryLine);
				AssertEquals("TransitionalPeriod EXP: Expected 3 decimals", 0.001m, wrapper.GrossWeight);

				declaration.JE_MessageType = "IMP";
				wrapper = GetWrapper(entryLine);
				AssertEquals("TransitionalPeriod IMP: Expected 3 decimals", 0.001m, wrapper.GrossWeight);
			}

			using (TemporarilyClearDeclarationConfigurationAndThenSetIsTransitionPeriodAES30(declaration, false))
			{
				declaration.JE_MessageType = "EXP";
				wrapper = GetWrapper(entryLine);
				AssertEquals("FinalPeriod EXP: Expected 6 decimals", 0.001123m, wrapper.GrossWeight);

				declaration.JE_MessageType = "IMP";
				wrapper = GetWrapper(entryLine);
				AssertEquals("FinalPeriod IMP: Expected 3 decimals", 0.001m, wrapper.GrossWeight);
			}
		});
	}

	public void TestGrossWeightSpecified()
	{
		CombineAssertions(() =>
		{
			wrapper = GetWrapper(entryLine);
			AssertEquals("Expected true GrossWeightSpecified when constructor used is the one with CusEntryLine", true, wrapper.GrossWeightSpecified);

			wrapper = new CommonGoodsMeasureWithSpecifiedWrapper(0m, 20.987654m);
			AssertEquals("Expected false GrossWeightSpecified when constructor used is the one with direct masses and gross mass is 0", false, wrapper.GrossWeightSpecified);

			wrapper = new CommonGoodsMeasureWithSpecifiedWrapper(40.123456m, 20.987654m);
			AssertEquals("Expected true GrossWeightSpecified when constructor used is the one with direct masses and gross mass is not 0", true, wrapper.GrossWeightSpecified);
		});
	}

	public void TestNetWeight()
	{
		CombineAssertions(() =>
		{
			invoiceLine.JI_CustomsUnitQty = Core.Constants.Weight.Grams;
			invoiceLine.JI_CustomsQuantity = 1.123m;

			using (TemporarilyClearDeclarationConfigurationAndThenSetIsTransitionPeriodAES30(declaration, true))
			{
				declaration.JE_MessageType = "EXP";
				wrapper = GetWrapper(entryLine);
				AssertEquals("TransitionalPeriod EXP: Expected 3 decimals", 0.001m, wrapper.NetWeight);

				declaration.JE_MessageType = "IMP";
				wrapper = GetWrapper(entryLine);
				AssertEquals("TransitionalPeriod IMP: Expected 3 decimals", 0.001m, wrapper.NetWeight);
			}

			using (TemporarilyClearDeclarationConfigurationAndThenSetIsTransitionPeriodAES30(declaration, false))
			{
				declaration.JE_MessageType = "EXP";
				wrapper = GetWrapper(entryLine);
				AssertEquals("FinalPeriod EXP: Expected 6 decimals", 0.001123m, wrapper.NetWeight);

				declaration.JE_MessageType = "IMP";
				wrapper = GetWrapper(entryLine);
				AssertEquals("FinalPeriod IMP: Expected 3 decimals", 0.001m, wrapper.NetWeight);
			}
		});
	}

	public void TestNetWeightSpecified()
	{
		CombineAssertions(() =>
		{
			wrapper = GetWrapper(entryLine);
			AssertEquals("Expected true NetWeightSpecified when constructor used is the one with CusEntryLine", true, wrapper.GrossWeightSpecified);

			wrapper = new CommonGoodsMeasureWithSpecifiedWrapper(40.123456m, 0m);
			AssertEquals("Expected false NetWeightSpecified when constructor used is the one with direct masses and net mass is 0", false, wrapper.NetWeightSpecified);

			wrapper = new CommonGoodsMeasureWithSpecifiedWrapper(40.123456m, 20.987654m);
			AssertEquals("Expected true NetWeightSpecified when constructor used is the one with direct masses and net mass is not 0", true, wrapper.NetWeightSpecified);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		var invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();

		AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

		entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

		wrapper = GetWrapper(entryLine);
	}
	JobDeclaration declaration;
	JobComInvoiceLine invoiceLine;
	CusEntryLine entryLine;
	CommonGoodsMeasureWithSpecifiedWrapper wrapper;

	CommonGoodsMeasureWithSpecifiedWrapper GetWrapper(CusEntryLine entryLine) => new CommonGoodsMeasureWithSpecifiedWrapper(entryLine);

	protected override CommonGoodsMeasureWithSpecifiedWrapper GetProvider() => wrapper;
}
