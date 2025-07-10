using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(JobComInvoiceHeader))]
sealed class JobComInvoiceHeaderTest : Customs.Business.Testing.BaseJobComInvoiceHeaderTest<JobDeclaration, JobComInvoiceHeader, JobComInvoiceLine>
{
	public override string GetLocalCurrencyCode() => Core.Constants.CurrencyCodes.UnitedArabEmirates;

	public void TestTypeDecider()
	{
		Assert("Update BaseJobComInvoiceHeaderTypeDecider to include a decider for this class", Factory.New(typeof(BaseJobComInvoiceHeader)).GetType() == typeof(JobComInvoiceHeader));
	}

	public void TestDefaultDataGroupingCode()
	{
		var header = Factory.New<JobComInvoiceHeader>();
		AssertEquals("Data grouping", AEConstants.DefaultDataGroupingForTariffs, header.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff));
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var declaration = Factory.New<JobDeclaration>();
		JobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];
		return groupHeader.JobComInvoiceHeaders.AddNew();
	}

	protected override bool RatesAreReciprocal => true;
}
