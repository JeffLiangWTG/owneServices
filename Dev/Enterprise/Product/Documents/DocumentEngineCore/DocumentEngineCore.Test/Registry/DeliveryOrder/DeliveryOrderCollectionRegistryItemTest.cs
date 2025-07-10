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
	[TestedType(typeof(DeliveryOrderCollectionRegistryItem))]
	public class DeliveryOrderCollectionRegistryItemTest : StronglyTypedRegistryItemTestCase<DeliveryOrderCollection>
	{
		#region TestFindDeliveryOrderForPrincipal

		public void TestFindDeliveryOrderForPrincipal()
		{
			ZGuid principalPK1 = CreatePrincipal("Principal1");
			ZGuid principalPK2 = CreatePrincipal("Principal2");

			DeliveryOrderCollection collection = new DeliveryOrderCollection();

			DeliveryOrder dO1 = collection.AddNew();
			dO1.PrincipalPK = principalPK1;
			dO1.PrintParameter = DeliveryOrder.PrintConstants.Code.PDO;
			dO1.Image = new Bitmap(10, 10);

			DeliveryOrder dO2 = collection.AddNew();
			dO2.PrincipalPK = principalPK2;
			dO2.PrintParameter = DeliveryOrder.PrintConstants.Code.PDO;
			dO2.Image = new Bitmap(15, 15);

			Item.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);
			AssertEquals("Find DO1", dO1.PrincipalPK, Item.FindDeliveryOrderForPrincipal(principalPK1).PrincipalPK);
			AssertEquals("Find DO2", dO2.PrincipalPK, Item.FindDeliveryOrderForPrincipal(principalPK2).PrincipalPK);
			AssertEquals("Find nothing", null, Item.FindDeliveryOrderForPrincipal(ZGuid.NewZGuid()));
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

		protected new DeliveryOrderCollectionRegistryItem Item
		{
			get { return (DeliveryOrderCollectionRegistryItem)base.Item; }
		}

		protected override StronglyTypedRegistryItem<DeliveryOrderCollection, DeliveryOrderCollection> GetNewRegistryItem()
		{
			return new DeliveryOrderCollectionRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.Company, RegistryOptions.Default);
		}

		#endregion
	}
}
