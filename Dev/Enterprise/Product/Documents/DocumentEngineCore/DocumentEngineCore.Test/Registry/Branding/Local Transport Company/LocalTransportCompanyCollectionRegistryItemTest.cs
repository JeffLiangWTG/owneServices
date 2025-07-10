using System;
using System.Drawing;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(LocalTransportCompanyBrandingCollectionRegistryItem))]
	public class LocalTransportCompanyCollectionRegistryItemTest : StronglyTypedRegistryItemTestCase<LocalTransportCompanyBrandingCollection>
	{
		#region FindBrandingForLocalTransportCompany

		public void TestFindBrandingForLocalTransportCompany()
		{
			var factory = new BusinessObjectFactory();

			var company1 = factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IOrgHeader)));
			company1[OrgHeaderSchema.Constants.OH_Code] = "COMPANY1";
			company1[OrgHeaderSchema.Constants.OH_IsLocalTransport] = true;
			company1[OrgHeaderSchema.Constants.OH_IsShippingProvider] = true;

			var company2 = factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IOrgHeader)));
			company2[OrgHeaderSchema.Constants.OH_Code] = "COMPANY2";
			company2[OrgHeaderSchema.Constants.OH_IsLocalTransport] = true;
			company2[OrgHeaderSchema.Constants.OH_IsShippingProvider] = true;

			factory.Save();

			var collection = new LocalTransportCompanyBrandingCollection();

			var branding1 = collection.AddNew();
			branding1.LocalTransportCompanyPK = company1.PK;
			branding1.Image = new Bitmap(10, 10);

			var branding2 = collection.AddNew();
			branding2.LocalTransportCompanyPK = company2.PK;
			branding2.Code = "PB2";
			branding2.Image = new Bitmap(15, 15);

			Item.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);
			AssertEquals("Find branding1", branding1.LocalTransportCompanyPK, Item.FindBrandingForLocalTransportCompany(company1.PK).LocalTransportCompanyPK);
			AssertEquals("Find branding2", branding2.LocalTransportCompanyPK, Item.FindBrandingForLocalTransportCompany(company2.PK).LocalTransportCompanyPK);
			AssertEquals("Find nothing", null, Item.FindBrandingForLocalTransportCompany(ZGuid.NewZGuid()));
		}

		#endregion

		#region Implementation

		protected new LocalTransportCompanyBrandingCollectionRegistryItem Item
		{
			get { return (LocalTransportCompanyBrandingCollectionRegistryItem)base.Item; }
		}

		protected override StronglyTypedRegistryItem<LocalTransportCompanyBrandingCollection, LocalTransportCompanyBrandingCollection> GetNewRegistryItem()
		{
			return new LocalTransportCompanyBrandingCollectionRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.Company, RegistryOptions.Default);
		}

		#endregion
	}
}
