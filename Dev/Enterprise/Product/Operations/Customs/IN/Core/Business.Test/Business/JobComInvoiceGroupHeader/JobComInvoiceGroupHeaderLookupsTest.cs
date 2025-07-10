using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(JobComInvoiceGroupHeaderLookups))]
sealed class JobComInvoiceGroupHeaderLookupsTest : Customs.Business.Testing.JobComInvoiceHeaderLookupsTest
{
	public override void TestExporters()
	{
		var header = Factory.New<JobComInvoiceHeader>();
		var exporters = header.Lookups.Exporters;
		var propertyName = "Country/Region:Property";
		CombineAssertions(() =>
		{
			AssertEquals(typeof(ConsignorCollection), exporters.GetType());
			Assert("Filter", exporters.FilterBusinessObjectDefaults.ContainsDefaultFor(propertyName));
			AssertEquals("Filter Value", Core.Constants.CountryCodes.India, (ZString)exporters.FilterBusinessObjectDefaults[propertyName].Value);
		});
	}
}
