using System;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Edec.GoodsDeclarations;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business.Testing;

class EdecPermitDataProviderTest : TestCaseWithFactory
{
	public void TestNew()
	{
		CombineAssertions(() =>
		{
			AssertNull("Argument == null", EdecPermitDataProvider.New(null));
			AssertNotNull("Argument != null", EdecPermitDataProvider.New(permit));
		});
	}

	public void TestPermitType()
	{
		CombineAssertions(() =>
		{
			permit.CSI_Code = "12";
			AssertEquals("When not empty", "12", dataProvider.PermitType);

			permit.CSI_Code = ZString.Empty;
			AssertEquals("When empty", "0", dataProvider.PermitType);
		});
	}

	public void TestPermitAuthority()
	{
		CombineAssertions(() =>
		{
			permit.CSI_IssuerType = "13";
			AssertEquals("When not empty", "13", dataProvider.PermitAuthority);

			permit.CSI_IssuerType = ZString.Empty;
			AssertEquals("When empty", "0", dataProvider.PermitAuthority);
		});
	}

	public void TestPermitNumber()
	{
		CombineAssertions(() =>
		{
			permit.CSI_ReferenceNumber = "REF45678901234567";
			AssertEquals("When not empty", "REF45678901234567", dataProvider.PermitNumber);

			permit.CSI_ReferenceNumber = ZString.Empty;
			AssertEquals("When empty", string.Empty, dataProvider.PermitNumber);
		});
	}

	public void TestIssueDate()
	{
		CombineAssertions(() =>
		{
			permit.CSI_DateOfIssue = new ZDateTime(2022, 2, 28);
			AssertEquals("When not empty", new DateTime(2022, 2, 28), dataProvider.IssueDate);

			permit.CSI_DateOfIssue = ZDateTime.Empty;
			AssertEquals("When empty", null, dataProvider.IssueDate);
		});
	}

	public void TestAdditionalInformation()
	{
		CombineAssertions(() =>
		{
			permit.CSI_Description = "ADD4567890123456789012345678901234567890123456789012345678901234567890";
			AssertEquals("When not empty", "ADD4567890123456789012345678901234567890123456789012345678901234567890", dataProvider.AdditionalInformation);

			permit.CSI_Description = ZString.Empty;
			AssertEquals("When empty", null, dataProvider.AdditionalInformation);
		});
	}

	public void TestPermitItemDetails()
	{
		CombineAssertions(() =>
		{
			var detailsDataProviders = dataProvider.PermitItemDetails;
			AssertEquals("No details - count", 0, detailsDataProviders.Count());

			var permitItemDetail1 = permit.PermitItemDetails.AddNew();
			permitItemDetail1.CY_Code = "12";
			var permitItemDetail2 = permit.PermitItemDetails.AddNew();
			permitItemDetail2.CY_Code = "13";

			dataProvider = EdecPermitDataProvider.New(permit);
			detailsDataProviders = dataProvider.PermitItemDetails;
			AssertEquals("Count", 2, detailsDataProviders.Count());
			AssertEquals($"{nameof(IEdecGoodsItemPermitDetail.Key)} 1", "12", detailsDataProviders.ElementAt(0).Key);
			AssertEquals($"{nameof(IEdecGoodsItemPermitDetail.Key)} 2", "13", detailsDataProviders.ElementAt(1).Key);

			AssertSame("Cached", dataProvider.PermitItemDetails, dataProvider.PermitItemDetails);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		permit = invoiceLine.Permits.AddNew();
		dataProvider = EdecPermitDataProvider.New(permit);
	}
	Permit permit;
	EdecPermitDataProvider dataProvider;
}
