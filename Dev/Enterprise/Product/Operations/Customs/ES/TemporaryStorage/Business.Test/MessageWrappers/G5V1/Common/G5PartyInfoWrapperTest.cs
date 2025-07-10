using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.MessageWrappers.Testing
{
	public class G5PartyInfoWrapperTest : WrapperHelperTest<G5PartyInfoWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertNull("OrgAddress null", PartyNameWrapper.New((OrgAddress)null));
				var address = Factory.New<OrgAddress>();
				AssertNull("OrgAddress with null OrgHeader null", GetWrapper(address));

				var org = Factory.New<OrgHeader>();
				address.OA_OH = org.PK;
				AssertNotNull("OrgAddress not null", GetWrapper(address));
			});
		}

		public void TestId()
		{
			CombineAssertions("Declarant and Representative", () =>
			{
				AssertEquals("Expected empty Id", ZString.Empty, wrapperForDeclarantAndRepresentative.Id);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB333333333", "GB");
				AssertEquals("Expected PAS Id with country code when no NIF or EORI declared", "GB333333333", wrapperForDeclarantAndRepresentative.Id);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				AssertEquals("Expected NIF Id", "NIF22222222", wrapperForDeclarantAndRepresentative.Id);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeader.OH_Category = OrgConstants.Category.Government;
				AssertEquals("Expected Country+NIF for non NAT Organizations", "ESNIF22222222", wrapperForDeclarantAndRepresentative.Id);

				var eoriCusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
				AssertEquals("Expected EORI Id with country code", "FR22222222", wrapperForDeclarantAndRepresentative.Id);

				eoriCusCode.OK_CustomsRegNo = "ES22222222";
				eoriCusCode.OK_RN_NKCodeCountry = "ES";
				AssertEquals("Expected EORI Id with country code not repeated", "ES22222222", wrapperForDeclarantAndRepresentative.Id);
			});

			orgHeader.CustomsCodes.RemoveAndDeleteAll();

			CombineAssertions("Consignor And Consignee", () =>
			{
				AssertEquals("Expected empty Id", ZString.Empty, wrapperForConsignorAndConsignee.Id);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB333333333", "GB");
				AssertEquals("Expected PAS Id with country code when no NIF or EORI declared", "GB333333333", wrapperForConsignorAndConsignee.Id);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				AssertEquals("Expected NIF Id", "NIF22222222", wrapperForConsignorAndConsignee.Id);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeader.OH_Category = OrgConstants.Category.Government;
				AssertEquals("Expected Country+NIF for non NAT Organizations", "ESNIF22222222", wrapperForConsignorAndConsignee.Id);

				var eoriCusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
				AssertEquals("Expected EORI Id with country code", "FR22222222", wrapperForConsignorAndConsignee.Id);

				eoriCusCode.OK_CustomsRegNo = "ES22222222";
				eoriCusCode.OK_RN_NKCodeCountry = "ES";
				AssertEquals("Expected EORI Id with country code not repeated", "ES22222222", wrapperForConsignorAndConsignee.Id);

				wrapperForConsignorAndConsignee = GetWrapper(null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				AssertEquals("Expected empty Id when address is null and id is empty", ZString.Empty, wrapperForConsignorAndConsignee.Id);

				wrapperForConsignorAndConsignee = GetWrapper(null, "id", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				AssertEquals("Expected filled Id with data from argument when address is null but id is not empty", "id", wrapperForConsignorAndConsignee.Id);
			});
		}

		public void TestName()
		{
			CombineAssertions(() =>
			{
				orgHeader.OH_FullName = OrgHeaderData.Name;
				AssertEquals("Expected filled Name with data from orgHeader (ForDeclarantAndRepresentative)", OrgHeaderData.Name, wrapperForDeclarantAndRepresentative.Name);

				AssertEquals("Expected filled Name with data from argument (ForConsignorAndConsignee)", "name", wrapperForConsignorAndConsignee.Name);

				wrapperForConsignorAndConsignee = GetWrapper(null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				AssertEquals("Expected empty Name with data from argument (ForConsignorAndConsignee) when given empty", ZString.Empty, wrapperForConsignorAndConsignee.Name);
			});
		}

		public void TestType()
		{
			CombineAssertions(() =>
			{
				orgHeader.OH_Category = OrgConstants.Category.Business;
				wrapperForDeclarantAndRepresentative = GetWrapper(orgAddress);
				AssertEquals("Expected filled Type with 2 when Category is BUS (ForDeclarantAndRepresentative)", "2", wrapperForDeclarantAndRepresentative.Type);

				orgHeader.OH_Category = OrgConstants.Category.Government;
				wrapperForDeclarantAndRepresentative = GetWrapper(orgAddress);
				AssertEquals("Expected filled Type with 3 when Category is GOV (ForDeclarantAndRepresentative)", "3", wrapperForDeclarantAndRepresentative.Type);

				orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				wrapperForDeclarantAndRepresentative = GetWrapper(orgAddress);
				AssertEquals("Expected filled Type with 1 when Category is NAT (ForDeclarantAndRepresentative)", "1", wrapperForDeclarantAndRepresentative.Type);

				orgHeader.OH_Category = OrgConstants.Category.NonGovernmentOrganisation;
				wrapperForDeclarantAndRepresentative = GetWrapper(orgAddress);
				AssertEquals("Expected filled Type with 3 when Category is NGO (ForDeclarantAndRepresentative)", "3", wrapperForDeclarantAndRepresentative.Type);

				AssertEquals("Expected filled Type with data from argument (ForConsignorAndConsignee)", "5", wrapperForConsignorAndConsignee.Type);

				wrapperForConsignorAndConsignee = GetWrapper(null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				AssertEquals("Expected empty Type with data from argument (ForConsignorAndConsignee) when given empty", ZString.Empty, wrapperForConsignorAndConsignee.Type);
			});
		}

		public void TestStreet()
		{
			CombineAssertions(() =>
			{
				orgAddress.OA_Address1 = "Test Street N1234";
				wrapperForDeclarantAndRepresentative = GetWrapper(orgAddress);
				AssertEquals("Expected filled Street (ForDeclarantAndRepresentative)", "Test Street N", wrapperForDeclarantAndRepresentative.Street);

				AssertEquals("Expected filled Street with data from argument (ForConsignorAndConsignee)", "street", wrapperForConsignorAndConsignee.Street);

				wrapperForConsignorAndConsignee = GetWrapper(null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				AssertEquals("Expected empty Street with data from argument (ForConsignorAndConsignee) when given empty", ZString.Empty, wrapperForConsignorAndConsignee.Street);
			});
		}

		public void TestStreetAddLine()
		{
			CombineAssertions(() =>
			{
				orgAddress.OA_Address2 = "1234 Test Extra Street";
				wrapperForDeclarantAndRepresentative = GetWrapper(orgAddress);
				AssertEquals("Expected filled StreetAddLine (ForDeclarantAndRepresentative)", "1234 Test Extra Street", wrapperForDeclarantAndRepresentative.StreetAddLine);

				AssertEquals("Expected filled StreetAddLine with data from argument (ForConsignorAndConsignee)", "extra street n", wrapperForConsignorAndConsignee.StreetAddLine);

				wrapperForConsignorAndConsignee = GetWrapper(null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				AssertEquals("Expected empty StreetAddLine with data from argument (ForConsignorAndConsignee) when given empty", ZString.Empty, wrapperForConsignorAndConsignee.StreetAddLine);
			});
		}

		public void TestNumber()
		{
			CombineAssertions(() =>
			{
				orgAddress.OA_Address1 = "Test Extra Street 567";
				wrapperForDeclarantAndRepresentative = GetWrapper(orgAddress);
				AssertEquals("Expected filled Number with Address2 when it ends in numbers and Address1 doesn't (ForDeclarantAndRepresentative)", "567", wrapperForDeclarantAndRepresentative.Number);

				orgAddress.OA_Address1 = "Test Street N1234";
				wrapperForDeclarantAndRepresentative = GetWrapper(orgAddress);
				AssertEquals("Expected filled Number with Address1 when it ends in numbers, even if Address2 does as well (ForDeclarantAndRepresentative)", "1234", wrapperForDeclarantAndRepresentative.Number);

				AssertEquals("Expected filled Number with data from streetAddLine when it ends in numbers and street doesn't (ForConsignorAndConsignee)", "9876", wrapperForConsignorAndConsignee.Number);

				wrapperForConsignorAndConsignee = GetWrapper(null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				AssertEquals("Expected empty Number with data from argument (ForConsignorAndConsignee) when given empty street and streetAddLine", ZString.Empty, wrapperForConsignorAndConsignee.Number);

				wrapperForConsignorAndConsignee = GetWrapper(orgAddress, "id", "name", "5", "street 147", "extra street n9876", "state", "ES", "28010", "madrid", "123456789");
				AssertEquals("Expected filled Number with data from street when it ends in numbers, even if streetAddLine does as well (ForConsignorAndConsignee)", "147", wrapperForConsignorAndConsignee.Number);

				wrapperForConsignorAndConsignee = GetWrapper(orgAddress, "id", "name", "5", "147 street", "n9876 extra street", "state", "ES", "28010", "madrid", "123456789");
				AssertEquals("Expected empty Number when neither street nor streetAddLine end in numbers (ForConsignorAndConsignee)", ZString.Empty, wrapperForConsignorAndConsignee.Number);
			});
		}

		public void TestPOBox()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty POBox always (ForDeclarantAndRepresentative)", ZString.Empty, wrapperForDeclarantAndRepresentative.POBox);

				AssertEquals("Expected empty POBox always (ForConsignorAndConsignee)", ZString.Empty, wrapperForConsignorAndConsignee.POBox);
			});
		}

		public void TestState()
		{
			CombineAssertions(() =>
			{
				orgAddress.OA_State = "M";
				AssertEquals("Expected filled State (ForDeclarantAndRepresentative)", "MADRID", wrapperForDeclarantAndRepresentative.State);

				AssertEquals("Expected filled State with data from argument (ForConsignorAndConsignee)", "state", wrapperForConsignorAndConsignee.State);

				wrapperForConsignorAndConsignee = GetWrapper(null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				AssertEquals("Expected empty State with data from argument (ForConsignorAndConsignee) when given empty", ZString.Empty, wrapperForConsignorAndConsignee.State);
			});
		}

		public void TestCountry()
		{
			CombineAssertions(() =>
			{
				orgAddress.OA_RN_NKCountryCode = "FR";
				AssertEquals("Expected filled Country (ForDeclarantAndRepresentative)", "FR", wrapperForDeclarantAndRepresentative.Country);

				AssertEquals("Expected filled Country with data from argument (ForConsignorAndConsignee)", "ES", wrapperForConsignorAndConsignee.Country);

				wrapperForConsignorAndConsignee = GetWrapper(null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				AssertEquals("Expected empty Country with data from argument (ForConsignorAndConsignee) when given empty", ZString.Empty, wrapperForConsignorAndConsignee.Country);
			});
		}

		public void TestPostCode()
		{
			CombineAssertions(() =>
			{
				orgAddress.OA_PostCode = "10001";
				AssertEquals("Expected filled PostCode (ForDeclarantAndRepresentative)", "10001", wrapperForDeclarantAndRepresentative.PostCode);

				AssertEquals("Expected filled PostCode with data from argument (ForConsignorAndConsignee)", "28010", wrapperForConsignorAndConsignee.PostCode);

				wrapperForConsignorAndConsignee = GetWrapper(null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				AssertEquals("Expected empty PostCode with data from argument (ForConsignorAndConsignee) when given empty", ZString.Empty, wrapperForConsignorAndConsignee.PostCode);
			});
		}

		public void TestCity()
		{
			CombineAssertions(() =>
			{
				orgAddress.OA_City = "barcelona";
				AssertEquals("Expected filled City (ForDeclarantAndRepresentative)", "barcelona", wrapperForDeclarantAndRepresentative.City);

				AssertEquals("Expected filled City with data from argument (ForConsignorAndConsignee)", "madrid", wrapperForConsignorAndConsignee.City);

				wrapperForConsignorAndConsignee = GetWrapper(null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				AssertEquals("Expected empty City with data from argument (ForConsignorAndConsignee) when given empty", ZString.Empty, wrapperForConsignorAndConsignee.City);
			});
		}

		public void TestCommunicationType()
		{
			CombineAssertions(() =>
			{
				orgAddress.OA_Phone = "PhoneNum";
				wrapperForDeclarantAndRepresentative = GetWrapper(orgAddress);
				AssertEquals("Expected filled CommunicationType with TE when OA_Phone is declared and OA_Email is not (ForDeclarantAndRepresentative)", "TE", wrapperForDeclarantAndRepresentative.CommunicationType);

				orgAddress.OA_Email = "mail.mail@mail.com";
				wrapperForDeclarantAndRepresentative = GetWrapper(orgAddress);
				AssertEquals("Expected filled CommunicationType with EM when OA_Email is declared, even if OA_Phone is declared (ForDeclarantAndRepresentative)", "EM", wrapperForDeclarantAndRepresentative.CommunicationType);

				wrapperForConsignorAndConsignee = GetWrapper(null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "PhoneNum");
				AssertEquals("Expected filled CommunicationType with TE when address is null and communicationId given has no @ (ForConsignorAndConsignee)", "TE", wrapperForConsignorAndConsignee.CommunicationType);

				wrapperForConsignorAndConsignee = GetWrapper(null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "mail.mail@mail.com");
				AssertEquals("Expected filled CommunicationType with EM when address is null and communicationId given has @ (ForConsignorAndConsignee)", "EM", wrapperForConsignorAndConsignee.CommunicationType);

				orgAddress.OA_Email = ZString.Empty;
				orgAddress.OA_Phone = "PhoneNum";
				wrapperForConsignorAndConsignee = GetWrapper(orgAddress, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "mail.mail@mail.com");
				AssertEquals("Expected filled CommunicationType with TE when address is not null and OA_Phone is declared and OA_Email is not, even if communicationId is declared (ForConsignorAndConsignee)", "TE", wrapperForConsignorAndConsignee.CommunicationType);

				orgAddress.OA_Email = "mail.mail@mail.com";
				wrapperForConsignorAndConsignee = GetWrapper(orgAddress, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "PhoneNum");
				AssertEquals("Expected filled CommunicationType with EM when address is not null and OA_Email is declared, even if OA_Phone is declared, even if communicationId is declared (ForConsignorAndConsignee)", "EM", wrapperForConsignorAndConsignee.CommunicationType);

				wrapperForConsignorAndConsignee = GetWrapper(null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				AssertEquals("Expected empty CommunicationType with data from argument (ForConsignorAndConsignee) when given empty and address is null", ZString.Empty, wrapperForConsignorAndConsignee.CommunicationType);
			});
		}

		public void TestCommunicationId()
		{
			CombineAssertions(() =>
			{
				orgAddress.OA_Phone = "PhoneNum";
				wrapperForDeclarantAndRepresentative = GetWrapper(orgAddress);
				AssertEquals("Expected filled CommunicationId with OA_Phone when declared and OA_Email is not (ForDeclarantAndRepresentative)", "PhoneNum", wrapperForDeclarantAndRepresentative.CommunicationId);

				orgAddress.OA_Email = "mail.mail@mail.com";
				wrapperForDeclarantAndRepresentative = GetWrapper(orgAddress);
				AssertEquals("Expected filled CommunicationId with OA_Email when declared, even if OA_Phone is declared (ForDeclarantAndRepresentative)", "mail.mail@mail.com", wrapperForDeclarantAndRepresentative.CommunicationId);

				orgAddress.OA_Email = ZString.Empty;
				orgAddress.OA_Phone = "PhoneNum";
				wrapperForConsignorAndConsignee = GetWrapper(orgAddress, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "mail.mail@mail.com");
				AssertEquals("Expected filled CommunicationId with OA_Phone when declared and declared and OA_Email is not, even if communicationId is declared (ForConsignorAndConsignee)", "PhoneNum", wrapperForConsignorAndConsignee.CommunicationId);

				orgAddress.OA_Email = "mail.mail@mail.com";
				wrapperForConsignorAndConsignee = GetWrapper(orgAddress, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "PhoneNum");
				AssertEquals("Expected filled CommunicationId with OA_Email when declared and address is not null, even if OA_Phone is declared and communicationId is declared (ForConsignorAndConsignee)", "mail.mail@mail.com", wrapperForConsignorAndConsignee.CommunicationId);

				wrapperForConsignorAndConsignee = GetWrapper(null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "123456789");
				AssertEquals("Expected filled CommunicationId with data from argument when address is null (ForConsignorAndConsignee)", "123456789", wrapperForConsignorAndConsignee.CommunicationId);

				wrapperForConsignorAndConsignee = GetWrapper(null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				AssertEquals("Expected empty CommunicationId with data from argument (ForConsignorAndConsignee) when given empty and address is null", ZString.Empty, wrapperForConsignorAndConsignee.CommunicationId);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			orgHeader = Factory.New<OrgHeader>();
			orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;

			wrapperForDeclarantAndRepresentative = GetWrapper(orgAddress);
			wrapperForConsignorAndConsignee = GetWrapper(orgAddress, "id", "name", "5", "street", "extra street n9876", "state", "ES", "28010", "madrid", "123456789");
		}

		OrgHeader orgHeader;
		OrgAddress orgAddress;
		G5PartyInfoWrapper wrapperForDeclarantAndRepresentative;
		G5PartyInfoWrapper wrapperForConsignorAndConsignee;

		G5PartyInfoWrapper GetWrapper(OrgAddress address) => G5PartyInfoWrapper.New(address);
		G5PartyInfoWrapper GetWrapper(OrgAddress address, ZString id, ZString name, ZString type, ZString street, ZString streetAddLine, ZString state, ZString country, ZString postCode, ZString city, ZString communicationId)
			=> new G5PartyInfoWrapper(address, id, name, type, street, streetAddLine, state, country, postCode, city, communicationId);

		protected override G5PartyInfoWrapper GetProvider() => wrapperForDeclarantAndRepresentative;
	}
}
