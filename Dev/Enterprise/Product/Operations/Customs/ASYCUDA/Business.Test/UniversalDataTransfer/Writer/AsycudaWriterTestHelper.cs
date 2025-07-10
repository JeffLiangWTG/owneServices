using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing
{
	public class AsycudaWriterTestHelper : OrganizationAddressTestHelper
	{
		public void PrepareCusCodeDataForTesting()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory.BOFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "PackageTypes");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "VU", "Vanuatu", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "SG", "Singapore", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Port");
			helper.CreateCusCodeList("SG", "PORT", "AUSDY", "desc1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList("SG", "PORT", "SGVLI", "desc2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var vuVAIR = helper.CreateNewOrGetExistingCusCodeList("VU", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "VAIR", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(vuVAIR.PK, "AIR", "VUVLI");
			var sbHIRA = helper.CreateNewOrGetExistingCusCodeList("SG", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "HIRA", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sbHIRA.PK, "AIR", "SGVLI");

			var sgRegistry = ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>();
			sgRegistry.ACCESSEnable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Factory.SaveForTesting();
		}

		public T SetupManifestHeader<T>(ZString countryCode, ZString manifestType, ZString transportMode, ZString manifestNumber)
			where T : AsycudaManifestHeader
		{
			var manifestHeader = (T)AsycudaManifestHeaderHelper.CreateNew(Factory.BOFactory, countryCode, manifestType);
			manifestHeader.AMA_RN_NKCountry = countryCode;
			manifestHeader.AMA_TransportMode = transportMode;
			manifestHeader.AMA_MasterBill = manifestNumber;
			return manifestHeader;
		}

		public static void AssertAddressData(List<OrganizationAddress> organizationAddressCollection, ZString docAddressType, string traderName, OrgAddress orgAddress)
		{
			var addressData = organizationAddressCollection.FirstOrDefault(x => x.AddressType.Value == docAddressType);
			AssertEquals(traderName, addressData.CompanyName);
			AssertEquals(orgAddress.Address1, addressData.Address1);
			AssertEquals(orgAddress.Address2, addressData.Address2);
			AssertEquals(orgAddress.City, addressData.City);
			AssertEquals(orgAddress.Country?.Code, addressData.Country?.Code);
		}
	}
}
