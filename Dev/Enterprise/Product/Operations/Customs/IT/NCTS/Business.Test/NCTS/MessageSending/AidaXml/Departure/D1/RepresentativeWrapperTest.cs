using System;
using CargoWise.Common.Collections;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using OrgAddress = Enterprise.MasterFiles.Business.OrgAddress;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class RepresentativeWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when carrier is null", () => new RepresentativeWrapper(null));
		AssertNoExceptionThrown(() => new RepresentativeWrapper(representativeHeader));
	}

	public void TestIdentificationNumber()
	{
		var wrapper = GetWrapper();
		AssertNullOrEmpty(nameof(IHolderOfTransitProcedure.IdentificationNumber), wrapper.IdentificationNumber);

		representativeAddress.CustomsCodes.AddNew("EOR", "385040449", "US");
		representativeAddress.CustomsCodes.AddNew("TCU", "ABB3332", "US");

		wrapper = GetWrapper();
		AssertEquals("When organisation has both EOR and TCU, IdentificationNumber", "US385040449", wrapper.IdentificationNumber);

		representativeAddress.CustomsCodes.RemoveAll(x => x.OK_CodeType == "EOR");
		wrapper = GetWrapper();
		AssertEquals("When organisation has only TCU, IdentificationNumber", "USABB3332", wrapper.IdentificationNumber);
	}

	public void TestStatus()
	{
		var wrapper = GetWrapper();
		AssertEquals(2, wrapper.Status);
	}

	protected override void SetUp()
	{
		base.SetUp();
		var factory = Factory;

		representativeHeader = factory.New<OrgHeader>();
		representativeAddress = representativeHeader.Addresses.AddNew();
		representativeAddress.CustomsCodes.AddNew("DEP", "IT12343");
	}

	IRepresentative GetWrapper() => new RepresentativeWrapper(representativeHeader);

	OrgHeader representativeHeader;
	OrgAddress representativeAddress;
}
