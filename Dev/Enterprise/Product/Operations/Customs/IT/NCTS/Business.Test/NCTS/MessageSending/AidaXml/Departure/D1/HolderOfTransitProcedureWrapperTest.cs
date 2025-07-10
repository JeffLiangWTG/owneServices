using System;
using CargoWise.Common.Collections;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Enterprise.MasterFiles.Business;
using OrgAddress = Enterprise.MasterFiles.Business.OrgAddress;
using OrgHeader = Enterprise.MasterFiles.Business.OrgHeader;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class HolderOfTransitProcedureWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when carrier is null", () => new HolderOfTransitProcedureWrapper(null, null));
		AssertNoExceptionThrown(() => new HolderOfTransitProcedureWrapper(jobDocAddress, null));
		AssertNotNull(jobDocAddress.Organisation);
	}

	public void TestIdentificationNumber()
	{
		var wrapper = GetWrapper(null);
		AssertNullOrEmpty(nameof(IHolderOfTransitProcedure.IdentificationNumber), wrapper.IdentificationNumber);

		principalAddress.CustomsCodes.AddNew("EOR", "385040449", "US");
		principalAddress.CustomsCodes.AddNew("TCU", "ABB3332", "US");

		wrapper = GetWrapper(null);
		AssertEquals("When organisation has both EOR and TCU, IdentificationNumber", "US385040449", wrapper.IdentificationNumber);

		principalAddress.CustomsCodes.RemoveAll(x => x.OK_CodeType == "EOR");
		wrapper = GetWrapper(null);
		AssertEquals("When organisation has only TCU, IdentificationNumber", "USABB3332", wrapper.IdentificationNumber);
	}

	public void TestTirHolderIdentificationNumber()
	{
		var wrapper = GetWrapper(null);
		AssertNullOrEmpty(nameof(IHolderOfTransitProcedure.TirHolderIdentificationNumber), wrapper.TirHolderIdentificationNumber);

		CombineAssertions("when BM_InBondEntryType = 'TIR'", () =>
		{
			principalAddress.CustomsCodes.AddNew("TIR", "ITA/001/1011001", "IT");
			principalAddress.CustomsCodes.AddNew("EOR", "385040451", "US");
			principalAddress.CustomsCodes.AddNew("TCU", "385040452", "US");

			wrapper = GetWrapper(NctsPhase5DeclarationTypeList.Codes.TIR);
			AssertEquals("and exist TIR", "ITA/001/1011001", wrapper.TirHolderIdentificationNumber);

			principalAddress.CustomsCodes.RemoveAll(x => x.OK_CodeType == "TIR");

			wrapper = GetWrapper(NctsPhase5DeclarationTypeList.Codes.TIR);
			AssertEquals("and not exist TIR, and exist EOR", "US385040451", wrapper.TirHolderIdentificationNumber);
		});
	}

	public void TestAddress()
	{
		principalAddress.OA_RN_NKCountryCode = "IT";
		var wrapper = GetWrapper(null);
		var address = wrapper.Address;

		AssertNotNull(nameof(IHolderOfTransitProcedure.Address), address);
		AssertType<AddressWrapper>(address);
		AssertSame(nameof(IHolderOfTransitProcedure.Address), address, wrapper.Address);
		AssertEquals("IT", address.Country);
	}

	protected override void SetUp()
	{
		base.SetUp();
		var factory = Factory;

		principalHeader = factory.New<OrgHeader>();
		principalAddress = principalHeader.Addresses.AddNew();
		principalAddress.CustomsCodes.AddNew("DEP", "IT12343");
		jobDocAddress = factory.New<JobDocAddress>();
		jobDocAddress.OrganisationPK = principalHeader.PK;
	}

	IHolderOfTransitProcedure GetWrapper(string declarationType) => new HolderOfTransitProcedureWrapper(jobDocAddress, declarationType);

	OrgHeader principalHeader;
	OrgAddress principalAddress;
	JobDocAddress jobDocAddress;
}
