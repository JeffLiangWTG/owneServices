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
	[TestedType(typeof(PrincipalBrandingCollectionRegistryItem))]
	public class PrincipalBrandingCollectionRegistryItemTest : StronglyTypedRegistryItemTestCase<PrincipalBrandingCollection>
	{
		#region TestFindBrandingForPrincipal

		public void TestFindBrandingForPrincipal()
		{
			ZGuid principalPK1 = CreatePrincipal("Principal1");
			ZGuid principalPK2 = CreatePrincipal("Principal2");

			PrincipalBrandingCollection collection = new PrincipalBrandingCollection();

			PrincipalBranding branding1 = collection.AddNew();
			branding1.PrincipalPK = principalPK1;
			branding1.Code = "PB1";
			branding1.BrandName = "Brand 1";
			branding1.BrandEmailAddress = "bob@blah.com";
			branding1.Image = new Bitmap(10, 10);

			PrincipalBranding branding2 = collection.AddNew();
			branding2.PrincipalPK = principalPK2;
			branding2.Code = "PB2";
			branding2.BrandName = "Brand 2";
			branding2.BrandEmailAddress = "bob@blah.net";
			branding2.Image = new Bitmap(15, 15);

			Item.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);
			AssertEquals("Find branding1", branding1.PrincipalPK, Item.FindBrandingForPrincipal(principalPK1).PrincipalPK);
			AssertEquals("Find branding2", branding2.PrincipalPK, Item.FindBrandingForPrincipal(principalPK2).PrincipalPK);
			AssertEquals("Find nothing", null, Item.FindBrandingForPrincipal(ZGuid.NewZGuid()));
		}

		#endregion

		#region Implementation

		#region Principal

		ZGuid CreatePrincipal(ZString code)
		{
			BusinessObjectFactory dirtyFactory = new BusinessObjectFactory();
			BusinessObject principal = dirtyFactory.NewWithValidTestData(ObjectFactory.GetType(typeof(IOrgHeader)));
			principal[OrgHeaderSchema.Constants.OH_Code] = code;
			principal[OrgHeaderSchema.Constants.OH_IsShippingProvider] = true;

			BusinessObject companyData = (BusinessObject)principal["CompanyData"];
			companyData[OrgCompanyDataSchema.Constants.OB_CRIsShipsAgencyPrincipal] = true;
			dirtyFactory.Save();

			return principal.PK;
		}

		#endregion

		protected new PrincipalBrandingCollectionRegistryItem Item
		{
			get { return (PrincipalBrandingCollectionRegistryItem)base.Item; }
		}

		protected override StronglyTypedRegistryItem<PrincipalBrandingCollection, PrincipalBrandingCollection> GetNewRegistryItem()
		{
			return new PrincipalBrandingCollectionRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.Company);
		}

		#endregion
	}
}
