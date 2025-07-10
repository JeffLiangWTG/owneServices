using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IN.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(InvoiceLinePartiesUserControl))]
sealed class InvoiceLinePartiesUserControlTest : TestCaseWithFactory
{
	public void TestDataSourceType()
	{
		using var control = new InvoiceLinePartiesUserControl();
		AssertEquals("DataSourceType", typeof(JobDeclaration), control.DataSourceType);
	}

	public void TestManufacturerAddressControlBindToOrgList()
	{
		using var control = new InvoiceLinePartiesUserControl();
		var addressControl = control.ManufacturerAddressControl;
		AssertEquals("Bind to OrgList", "FilteredInvoiceLines.Lookups.OrganizationList", addressControl.BindToOrgList);
	}
}
