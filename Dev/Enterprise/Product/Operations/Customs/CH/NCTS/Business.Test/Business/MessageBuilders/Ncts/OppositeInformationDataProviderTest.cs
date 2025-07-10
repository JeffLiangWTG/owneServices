using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(OppositeInformationDataProvider))]
sealed class OppositeInformationDataProviderTest : TestCaseWithFactory
{
	public void TestNew() => CombineAssertions(() =>
	{
		AssertNull("NctsHeader==null", OppositeInformationDataProvider.New(null));

		var nctsHeader = Factory.New<NctsHeader>();
		AssertNull("MovementHeader==null", OppositeInformationDataProvider.New(nctsHeader));

		nctsHeader.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
		AssertNotNull("MovementHeader != null", OppositeInformationDataProvider.New(nctsHeader));
	});

	public void TestNew_ReferenceNumberInput() => CombineAssertions(() =>
	{
		var messageIdentification = Guid.NewGuid().ToString();
		var dataProvider = OppositeInformationDataProvider.New(NctsHeader, messageIdentification);

		AssertEquals("ReferenceNumber is Message Identification", messageIdentification, dataProvider.ReferenceNumber);
	});

	public void TestReferenceNumber()
	{
		NctsHeader.MovementHeader.BM_PaperlessInbondNum = "2367";
		AssertEquals("ReferenceNumber", "2367", DataProvider.ReferenceNumber);
	}

	public void TestText()
	{
		AssertEquals($"CW-{GlbCompany.CurrentCompany.LicenceKeyIdentifier}", DataProvider.Text);
	}

	public void TestUnusedProperties()
	{
		AssertNull("Detail", DataProvider.Detail);
	}

	NctsHeader NctsHeader => nctsHeader ??= CreateNctsHeader();
	NctsHeader nctsHeader;

	OppositeInformationDataProvider DataProvider => dataProvider ??= OppositeInformationDataProvider.New(NctsHeader);
	OppositeInformationDataProvider dataProvider;

	NctsHeader CreateNctsHeader()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
		return nctsHeader;
	}
}
