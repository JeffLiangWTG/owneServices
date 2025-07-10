using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class T2LPOUSCommonPersonReqPresWithAddressWrapperTest : WrapperHelperTest<T2LPOUSCommonPersonReqPresWithAddressWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				OrgAddress address = null;
				AssertNull("OrgAddress null", GetWrapper(address));
				address = Factory.New<OrgAddress>();
				AssertNull("OrgAddress with null OrgHeader null", GetWrapper(address));

				var org = Factory.New<OrgHeader>();
				address.OA_OH = org.PK;
				AssertNotNull("OrgAddress not null", GetWrapper(address));
			});
		}

		public void TestAddress()
		{
			CombineAssertions(() =>
			{
				AssertNull("Expected null Address when no id declared", wrapper.Address);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "33333333", "FR");
				var cusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "ABC123456", "ES");
				orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				orgHeader.MainAddress.OA_Address1 = "MainRoad";
				orgAddress.OA_Address1 = "Road";
				wrapper = GetWrapper(orgAddress);
				var address = wrapper.Address;
				AssertNotNull("Expected filled Address when Id PAS (category is NAT)", wrapper.Address);
				AssertSame("Cached Address", wrapper.Address, address);
				AssertEquals("Expected filled Address the one passed, not Main Address", orgAddress.OA_Address1, wrapper.Address.Address);

				var cusCode2 = orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				wrapper = GetWrapper(orgAddress);
				AssertNull("Expected null Address when Id is NIF", wrapper.Address);

				orgHeader.CustomsCodes.Remove(cusCode2);
				cusCode.OK_CustomsRegNo = "";
				wrapper = GetWrapper(orgAddress);
				AssertNull("Expected null Address when id declared is empty", wrapper.Address);

				orgHeader.OH_Category = OrgConstants.Category.Business;
				wrapper = GetWrapper(orgAddress);
				AssertNull("Expected null Address when Id is not the PAS (EORI when category is not NAT)", wrapper.Address);
			});
		}

		public void TestId()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty Id when no id declared", ZString.Empty, wrapper.Id);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "ABC123456");
				AssertEquals("Expected PAS Id when category is not NAT and EORI is empty", "ABC123456", wrapper.Id);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
				AssertEquals("Expected EORI Id with country code when category is not NAT and EORI is not empty", "FR22222222", wrapper.Id);

				orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				AssertEquals("Expected PAS Id when category is NAT and NIF is empty", "ABC123456", wrapper.Id);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				AssertEquals("Expected NIF Id when category is NAT", "NIF22222222", wrapper.Id);
			});
		}

		public void TestContactPerson()
		{
			CombineAssertions(() =>
			{
				var contactPerson = wrapper.ContactPerson;
				AssertNotNull("Expected filled ContactPerson", contactPerson);
				AssertSame("Cached ContactPerson", wrapper.ContactPerson, contactPerson);
				AssertEquals("Expected correct PhoneNumber from address entered, not main", "22222222", contactPerson.PhoneNumber);

				wrapper = GetWrapper(orgAddress, isRepresentativeDeclared: true);
				AssertNull("Expected empty ContactPerson when flag isRepresentativeDeclared is true", wrapper.ContactPerson);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			orgHeader = Factory.New<OrgHeader>();

			var orgAddressMain = orgHeader.MainAddress;
			orgAddressMain.OA_Phone = "11111111";

			orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Phone = "22222222";

			wrapper = GetWrapper(orgAddress);
		}
		OrgAddress orgAddress;
		OrgHeader orgHeader;
		T2LPOUSCommonPersonReqPresWithAddressWrapper wrapper;

		T2LPOUSCommonPersonReqPresWithAddressWrapper GetWrapper(OrgAddress orgAddress, bool isRepresentativeDeclared = false) => T2LPOUSCommonPersonReqPresWithAddressWrapper.New(orgAddress, ZString.Empty, isRepresentativeDeclared);

		protected override T2LPOUSCommonPersonReqPresWithAddressWrapper GetProvider() => wrapper;
	}
}
