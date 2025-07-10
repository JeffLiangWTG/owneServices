using System.Drawing;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.DocumentEngineCore.Registry.LocalTransportCompanyBrandingCollectionRegistryItem;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(LocalTransportCompanyBrandingRegistryDataType))]
	class LocalTransportCompanyBrandingRegistryDataTypeFallBackMergedTest : FallbackMergedRegistryBusinessObjectCollectionDataTypeTestCase
	{
		protected override string ExpectedEditorName => "LocalTransportCompanyBrandingRegistryItemEditor";

		protected override IRegistryDataType GetNewDataType()
		{
			return new LocalTransportCompanyBrandingRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new LocalTransportCompanyBrandingCollection();
			var collection2 = new LocalTransportCompanyBrandingCollection();

			var branding1 = collection.AddNew();
			branding1.LocalTransportCompanyPK = company1PK;
			branding1.Code = "PB1";
			branding1.Image = new Bitmap(10, 10);

			var branding2 = collection2.AddNew();
			branding2.LocalTransportCompanyPK = company2PK;
			branding2.Code = "PB2";
			branding2.Image = new Bitmap(15, 15);

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, new LocalTransportCompanyBrandingRegistryDataType().Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(collection2, new LocalTransportCompanyBrandingRegistryDataType().Serialise(collection2))
			};
		}

		protected override void SetUp()
		{
			base.SetUp();

			var factory = new BusinessObjectFactory();

			var company1 = factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IOrgHeader)));
			company1[OrgHeaderSchema.Constants.OH_Code] = "PB1";
			company1[OrgHeaderSchema.Constants.OH_IsLocalTransport] = true;
			company1[OrgHeaderSchema.Constants.OH_IsShippingProvider] = true;

			var company2 = factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IOrgHeader)));
			company2[OrgHeaderSchema.Constants.OH_Code] = "PB2";
			company2[OrgHeaderSchema.Constants.OH_IsLocalTransport] = true;
			company2[OrgHeaderSchema.Constants.OH_IsShippingProvider] = true;

			factory.Save();

			company1PK = company1.PK;
			company2PK = company2.PK;
		}

		ZGuid company1PK;
		ZGuid company2PK;
	}
}
