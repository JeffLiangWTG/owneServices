using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

[TestedType(typeof(CGMAsycudaContainer))]
sealed class CGMAsycudaContainerTest : EnterpriseBusinessObjectTestCase
{
	public void TestIAsycudaContainer()
	{
		var bizObj = GetNewBusinessObject();
		Factory.Save();
		AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.INManifest.ICGMAsycudaContainer>(bizObj.PK).GetType());
	}

	public void TestHeader()
	{
		var container = (CGMAsycudaContainer)GetNewBusinessObject();
		AssertType<CGMAsycudaManifestHeader>(container.Header);
	}

	public void TestACN_IsShipperOwnedCaptions()
	{
		var cont = Factory.New<CGMAsycudaContainer>();
		var resData = DataBoundResourceStrings.GetDataForProperty(cont.ACN_IsShipperOwnedInfo);
		CombineAssertions(() =>
		{
			AssertNotNull("Res string data", resData);
			AssertEquals("Caption", "Shipper's Own Container", resData?.Caption);
			AssertEquals("MediumCaption", "Shipper's Own Cont.", resData?.MediumCaption);
			AssertEquals("ShortCaption", "SOC", resData?.ShortCaption);
		});
	}

	public void TestDocAddresses()
	{
		var container = (CGMAsycudaContainer)GetNewBusinessObject();
		AssertType<JobDocAddressDependentCollection>(container.DocAddresses);
	}

	public void TestContainerAgentCodeDocAddress()
	{
		var container = (CGMAsycudaContainer)GetNewBusinessObject();
		AssertEquals(DocAddressType.ContainerAgentCodeAddress, container.ContainerAgentCodeDocAddress.DocAddressType);
	}

	public void TestContainerAgentCode()
	{
		CombineAssertions(() =>
		{
			var container = (CGMAsycudaContainer)GetNewBusinessObject();
			AssertEquals("ContainerAgentCode - default value", ZGuid.Empty, container.ContainerAgentCode);
			AssertEquals("ContainerAgentCode_ZAddress.OrgPk - default value", ZGuid.Empty, container.ContainerAgentCodeDocAddress.OrganisationPK);
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			container.ContainerAgentCodeOrgPK = orgHeader.PK;
			container.ContainerAgentCode = orgHeader.MainAddress.PK;
			AssertEquals("Setting the ContainerAgentCodeOrgPk loads the correct OrgHeader", orgHeader, container.ContainerAgentCodeDocAddress.Organisation);
			AssertEquals("The DeliveryDestination address PK is returned", orgHeader.MainAddress.PK, container.ContainerAgentCode);
			Factory.Save();
			var jobDocAddresses = Factory.Load<JobDocAddress>(new ZQuery(ZArchitecture.Schema.JobDocAddressSchema.E2_ParentID, container.PK));
			AssertEquals(1, jobDocAddresses.Length);
			var jobDocAddress = jobDocAddresses[0];
			AssertEquals("CAC", jobDocAddress.E2_AddressType);
			AssertEquals(orgHeader, jobDocAddress.Organisation);
			AssertEquals(orgHeader.MainAddress, jobDocAddress.Address);
		});
	}

	public void TestContainerAgentCodeCaptions()
	{
		var cont = Factory.New<CGMAsycudaContainer>();
		var containerAgentCodeResData = DataBoundResourceStrings.GetDataForProperty(cont.ContainerAgentCodeInfo);
		CombineAssertions(() =>
		{
			AssertNotNull("Res string data", containerAgentCodeResData);
			AssertEquals("Caption", "Container Agent Code Address", containerAgentCodeResData?.Caption);
			AssertEquals("MediumCaption", "Cont. Agent Code Address", containerAgentCodeResData?.MediumCaption);
			AssertEquals("ShortCaption", "Cont. Agt. Cd. Address", containerAgentCodeResData?.ShortCaption);
		});
	}

	public void TestContainerAgentCodeOrgPKCaptions()
	{
		var cont = Factory.New<CGMAsycudaContainer>();
		var containerAgentCodeOrgPKResData = DataBoundResourceStrings.GetDataForProperty(cont.ContainerAgentCodeOrgPKInfo);
		CombineAssertions(() =>
		{
			AssertNotNull("Res string data", containerAgentCodeOrgPKResData);
			AssertEquals("Caption", "Container Agent Code", containerAgentCodeOrgPKResData?.Caption);
			AssertEquals("MediumCaption", "Cont. Agent Code", containerAgentCodeOrgPKResData?.MediumCaption);
			AssertEquals("ShortCaption", "Cont. Agt. Cd.", containerAgentCodeOrgPKResData?.ShortCaption);
		});
	}

	public void TestContainerAgentPAN()
	{
		var orgHeader = Factory.New<OrgHeader>();
		CombineAssertions(() =>
		{
			var container = (CGMAsycudaContainer)GetNewBusinessObject();
			AssertNullOrEmpty("ContainerAgentCode is empty", container.ContainerAgentPAN);

			container.ContainerAgentCodeOrgPK = orgHeader.PK;
			container.ContainerAgentCode = orgHeader.MainAddress.PK;
			AssertNullOrEmpty("ContainerAgentCode PAN is missing", container.ContainerAgentPAN);

			CreateCusCodeForType(IndiaOrgCusCodeInfo.OrgCusCodes.PAN);
			CreateCusCodeForType(IndiaOrgCusCodeInfo.OrgCusCodes.TAN);
			AssertEquals("ContainerAgentCode PAN is set", "PAN_1234", container.ContainerAgentPAN);
		});

		void CreateCusCodeForType(string codeType)
		{
			var code = orgHeader.CustomsCodes.AddNew();
			code.OK_CodeType = codeType;
			code.OK_RN_NKCodeCountry = CountryCodes.India;
			code.OK_CustomsRegNo = codeType + "_1234";
		}
	}

	public void TestACN_GoodsWeight()
	{
		var cont = Factory.New<CGMAsycudaContainer>();
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(cont.ACN_GoodsWeightInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Container Weight", resourceStringData.Caption);
			AssertEquals("Medium Caption", "Cont. Weight", resourceStringData.MediumCaption);
			AssertEquals("Short Caption", "Cont. Wt.", resourceStringData.ShortCaption);
		});
	}

	public void TestContainerWeightInTonnes()
	{
		CombineAssertions(() =>
		{
			var container = (CGMAsycudaContainer)GetNewBusinessObject();
			AssertEquals("Container Weight is empty as no weight is set", 0m, container.ContainerWeightInTonnes);

			container.ACN_GoodsWeight = 1234m;
			container.ACN_GoodsWeightUQ = "XYZ";
			AssertEquals("Container Weight is empty as invalid weight unit is set", 0m, container.ContainerWeightInTonnes);

			container.ACN_GoodsWeightUQ = Core.Constants.Weight.Kilograms;
			AssertEquals("Container Weight is set in Tonnes", 1.234m, container.ContainerWeightInTonnes);
		});
	}

	public void TestISOCode()
	{
		var container = Factory.New<CGMAsycudaContainer>();
		CombineAssertions(() =>
		{
			container.ISOCode = "2000";
			AssertEquals("2000", container.GetSystemDefinedValue<ZString>(CGMAsycudaContainer.Schema.ISOCode));

			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(container.ISOCodeInfo);
			AssertEquals("Caption", "Customs Container Code", resourceStringData.Caption);
			AssertEquals("Medium Caption", "Container Code", resourceStringData.MediumCaption);
			AssertEquals("Short Caption", "Cont. Code", resourceStringData.ShortCaption);

			AssertEquals(CGMAsycudaContainer.Schema.ISOCodeMaxLength, container.ISOCodeInfo.MaxLength);
		});
	}

	public void TestISOCode_ReadOnly()
	{
		var container = Factory.New<CGMAsycudaContainer>();
		CombineAssertions(() =>
		{
			Assert("Should not be readonly when no refContainer", !container.ISOCode_ReadOnly);

			container.ACN_RC_ContainerType = CreateRefContainer(Core.Constants.CountryCodes.Australia, "1000").PK;
			Assert("Should not be readonly when no IN container code map", !container.ISOCode_ReadOnly);

			container.ACN_RC_ContainerType = CreateRefContainer(Core.Constants.CountryCodes.India, "2000").PK;
			Assert("Should be readonly when has IN container code map", container.ISOCode_ReadOnly);
		});
	}

	public void TestISOCode_Defaulting()
	{
		var container = Factory.New<CGMAsycudaContainer>();

		container.ISOCode = "1234";
		container.ACN_RC_ContainerType = CreateRefContainer(Core.Constants.CountryCodes.India, "2000").PK;
		AssertEquals("Default to IN container code map", "2000", container.ISOCode);
		container.ACN_RC_ContainerType = CreateRefContainer(Core.Constants.CountryCodes.India, "3000").PK;
		AssertEquals("Update when ACN_RC_ContainerType changed", "3000", container.ISOCode);
		container.ACN_RC_ContainerType = CreateRefContainer(Core.Constants.CountryCodes.Australia, "1000").PK;
		AssertEquals("Not update when no IN container code map", "3000", container.ISOCode);
	}

	RefContainer CreateRefContainer(ZString codeMapCountryCode, ZString codeMapCode)
	{
		var refContainer = Factory.New<RefContainer>();
		refContainer.RC_Code = "40GP";
		refContainer.RC_ISOType = "40GP";
		var containerMap = refContainer.CodeMapCollection.AddNew();
		containerMap.RCM_RN_NKCountry = codeMapCountryCode;
		containerMap.RCM_Code = codeMapCode;
		return refContainer;
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var header = factory.New<CGMAsycudaManifestHeader>();
		header.SuspendCheckBusinessObjectType();
		var cont = header.Containers.AddNew();
		cont.ACN_ContainerNumber = "Test";
		return cont;
	}
}
