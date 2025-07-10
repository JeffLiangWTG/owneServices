using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage.Testing
{
	sealed class TS313AndTS315DeclarationTypeProviderBaseOnlyTest : TS313AndTS315DeclarationTypeProviderTest<TS313AndTS315DeclarationTypeProvider>
	{
		protected override TS313AndTS315DeclarationTypeProvider GetProvider(TemporaryStorageHeader header) => new TS313AndTS315DeclarationTypeProvider(header);
	}

	abstract class TS313AndTS315DeclarationTypeProviderTest<T> : DataProviderTestCase<T>
		where T : TS313AndTS315DeclarationTypeProvider
	{
		public void TestMessageType()
		{
			(var header, var provider) = GetHeaderAndProvider();
			header.AMA_MessageType = PNTSMessageTypeList.Codes.CombinedTemporaryStorage;
			AssertEquals("MessageType G4G3", TemporaryStorageDeclarationTypeList.Codes.DeclarationAndPresentationNotification, provider.MessageType);

			header.AMA_MessageType = PNTSMessageTypeList.Codes.PreLodgedTempStorage;
			AssertEquals("MessageType G4", TemporaryStorageDeclarationTypeList.Codes.Declaration, provider.MessageType);
		}

		public void TestLRN()
		{
			(var header, var provider) = GetHeaderAndProvider();
			header.LRN = "123";
			AssertEquals("LRN", "123", provider.LRN);
			header.LRN = "456";
			AssertEquals("LRN", "456", provider.LRN);
		}

		public void TestCustomsOffices()
		{
			(var header, var provider) = GetHeaderAndProvider();
			AssertSame(provider, provider.CustomsOffices);
		}

		public void TestSupervisingCustomsOffice()
		{
			(var header, var provider) = GetHeaderAndProvider();
			header.AMA_CustomsOffice = "IE123";
			AssertEquals("IE123", provider.SupervisingCustomsOffice);
			header.AMA_CustomsOffice = "IE456";
			AssertEquals("IE456", provider.SupervisingCustomsOffice);
		}

		public void TestCustomsOfficeLodgement()
		{
			(var header, var provider) = GetHeaderAndProvider();
			header.CustomsOfficeOfLodgement = "IE456";
			AssertEquals("IE456", provider.CustomsOfficeLodgement);
			header.CustomsOfficeOfLodgement = "IE123";
			AssertEquals("IE123", provider.CustomsOfficeLodgement);
		}

		public void TestParties()
		{
			(var header, var provider) = GetHeaderAndProvider();
			AssertSame(provider, provider.Parties);
		}

		public void TestDeclarant()
		{
			(var header, var provider) = GetHeaderAndProvider();
			header.AMA_OA_Declarant = ZGuid.Empty;
			AssertNull("No Declarant added to header", provider.Declarant);

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "E009");
			var customsContact = orgHeader.Contacts.AddNew();
			customsContact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;
			customsContact.OC_Phone = "+35312345678";
			var orgAddress = orgHeader.MainAddress;
			orgAddress.CompanyName = "Rep1";
			orgAddress.OA_Address1 = "No1";
			orgAddress.OA_Address2 = "Sandy Cove";
			orgAddress.OA_City = "Dublin";
			orgAddress.OA_PostCode = "D12654";
			orgAddress.OA_RN_NKCountryCode = "IE";
			header.AMA_OA_Declarant = orgAddress.PK;

			provider = GetProvider(header);
			AssertEquals("Declarant", "IEE009", provider.Declarant);
		}

		public void TestRepresentative()
		{
			(var header, var provider) = GetHeaderAndProvider();
			header.AMA_OA_Representative = ZGuid.Empty;
			AssertNull("No representative added to header", provider.Representative);

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "E007");
			var customsContact = orgHeader.Contacts.AddNew();
			customsContact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;
			customsContact.OC_Phone = "+35312345678";
			var orgAddress = orgHeader.MainAddress;
			orgAddress.CompanyName = "Rep1";
			orgAddress.OA_Address1 = "No1";
			orgAddress.OA_Address2 = "Sandy Cove";
			orgAddress.OA_City = "Dublin";
			orgAddress.OA_PostCode = "D12654";
			orgAddress.OA_RN_NKCountryCode = "IE";
			header.AMA_OA_Representative = orgAddress.PK;
			header.AMA_AgentType = RepresentativeStatusCodeList.Codes._2;

			provider = GetProvider(header);
			var representative = provider.Representative;
			CombineAssertions(() =>
			{
				AssertType<TSRepresentativeProvider>("Representative Type", representative);
				AssertEquals("Status", "2", representative.Status);
				AssertEquals("Id", "IEE007", representative.ID);
			});
		}

		protected (TemporaryStorageHeader header, T provider) GetHeaderAndProvider()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			return (header, GetProvider(header));
		}

		protected abstract T GetProvider(TemporaryStorageHeader header);

		protected override T GetProvider() => GetHeaderAndProvider().provider;
	}
}
