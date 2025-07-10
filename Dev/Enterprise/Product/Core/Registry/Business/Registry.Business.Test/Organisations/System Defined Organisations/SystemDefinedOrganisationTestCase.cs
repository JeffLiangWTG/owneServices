using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business.Testing
{
	abstract class SystemDefinedOrganisationTestCase : RegistryBusinessObjectTemplateTestCase
	{
		public void TestIsEnabled()
		{
			AssertEquals("IsEnabled", false, BizObj.IsEnabled);
		}

		public void TestOrganisation()
		{
			BusinessObject systemOrg = GetSystemOrgHeader();
			SystemDefinedOrganisation bizObj = (SystemDefinedOrganisation)GetNewBusinessObject();
			int loadCount = Factory.DatabaseLoadCount;
			AssertEquals("Organisation", systemOrg.PK, bizObj.Organisation);
			AssertEquals("Organisation", systemOrg.PK, bizObj.Organisation);
			Assert("Value should be cached.", Factory.DatabaseLoadCount <= loadCount + 1);
			AssertEquals("organisation Code", systemOrg[OrgHeaderSchema.OH_Code], bizObj.Code);
		}

		public void TestLastSavedOrganisation()
		{
			ZGuid systemOrgPK = GetSystemOrgHeader().PK;
			IRegistryDataType dataType = new DummyNonPersistentBusinessObjectRegistryDataType(ExpectedBusinessObjectType);
			SystemDefinedOrganisation bizObj = (SystemDefinedOrganisation)dataType.Deserialise(dataType.Serialise(GetNewBusinessObject()));
			AssertEquals("LastSavedOrganisation", systemOrgPK, bizObj.LastSavedOrganisation);
		}

		public void TestPrimaryOfficeAddress()
		{
			BusinessObject orgHeader = GetSystemOrgHeader();

			ZDBOnlyQuery filter = new ZDBOnlyQuery(typeof(IOrgAddress));
			filter.AddToFilter(OrgAddressSchema.OA_OH, orgHeader.PK);

			ZDBOnlySubQuery capabilitySubQuery = new ZDBOnlySubQuery(ObjectFactory.GetType<Enterprise.MasterFiles.Integration.IOrgAddressCapability>(), OrgAddressCapabilitySchema.PZ_OA);
			capabilitySubQuery.AddToFilter(OrgAddressCapabilitySchema.PZ_IsMainAddress, ZBool.True);
			capabilitySubQuery.AddToFilter(OrgAddressCapabilitySchema.PZ_AddressType, OrgConstants.AddressType.Office);

			filter.AddSubQuery(capabilitySubQuery, JoinCondition.And);

			BusinessObject orgAddress = (BusinessObject)Factory.LoadTop1<IOrgAddress>(filter);

			if (orgAddress == null)
			{
				orgAddress = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IOrgAddress)));
				orgAddress[OrgAddressSchema.Constants.OA_OH] = orgHeader.PK;

				BusinessObject orgAddressCapability = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.MasterFiles.Integration.IOrgAddressCapability)));
				orgAddressCapability[OrgAddressCapabilitySchema.PZ_AddressType] = (ZString)OrgConstants.AddressType.Office;
				orgAddressCapability[OrgAddressCapabilitySchema.PZ_IsMainAddress] = ZBool.True;
				orgAddressCapability[OrgAddressCapabilitySchema.PZ_OA] = orgAddress.PK;

				Factory.Save();
			}

			int loadCount = Factory.DatabaseLoadCount;
			AssertEquals("PrimaryOfficeAddress", orgAddress.PK, BizObj.PrimaryOfficeAddress);
			AssertEquals("PrimaryOfficeAddress", orgAddress.PK, BizObj.PrimaryOfficeAddress);
			Assert("Value should be cached.", Factory.DatabaseLoadCount <= loadCount + 1);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			SystemDefinedOrganisation result = (SystemDefinedOrganisation)GetNewBusinessObject();
			result.IsEnabled = true;
			result.SetOrganisation(ZGuid.NewZGuid());
			result.SetLastSavedOrganisation(result.Organisation);
			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected new SystemDefinedOrganisation BizObj
		{
			get { return (SystemDefinedOrganisation)base.BizObj; }
		}

		protected BusinessObject GetSystemOrgHeader()
		{
			ZQuery filter = new ZQuery(OrgHeaderSchema.OH_Code, OrgCodeForTesting);
			BusinessObject systemOrg = (BusinessObject)Factory.LoadTop1<IOrgHeader>(filter);

			if (systemOrg == null)
			{
				systemOrg = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IOrgHeader)));
				systemOrg[OrgHeaderSchema.Constants.OH_Code] = OrgCodeForTesting;
				Factory.Save();
			}

			return systemOrg;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return (BusinessObject)Activator.CreateInstance(ExpectedBusinessObjectType, new object[] { Factory });
		}

		protected override void CheckAllPropertiesAreEqual(RegistryBusinessObjectTemplate originalBusinessObject, RegistryBusinessObjectTemplate newBusinessObject, bool isClone)
		{
			base.CheckAllPropertiesAreEqual(originalBusinessObject, newBusinessObject, isClone);
			AssertEquals("LastSavedOrganisation", ((SystemDefinedOrganisation)originalBusinessObject).LastSavedOrganisation, ((SystemDefinedOrganisation)newBusinessObject).LastSavedOrganisation);
		}

		protected override void TearDown()
		{
			base.TearDown();
			SystemDefinedOrganisation.ClearCache();
		}

		protected override void SetUp()
		{
			base.SetUp();
			SystemDefinedOrganisation.ClearCache();
		}

		protected abstract string OrgCodeForTesting { get; }

		#endregion
	}
}
