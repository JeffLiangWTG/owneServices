using System.Drawing;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.DocumentEngineCore.Registry.DeliveryOrderCollectionRegistryItem;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(DeliveryOrderRegistryDataType))]
	class DeliveryOrderRegistryDataTypeNonPersistentTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DeliveryOrderRegistryDataType>
	{
		protected override DeliveryOrderRegistryDataType GetNewDataType()
		{
			return new DeliveryOrderRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new DeliveryOrderCollection();
			var order = collection.AddNew();
			order.PrintParameter = DeliveryOrder.PrintConstants.Code.PCT;
			order.Image = new Bitmap(3, 4);
			order.PrincipalPK = principalPK1;

			var collection2 = new DeliveryOrderCollection();
			var order2 = collection2.AddNew();
			order2.PrintParameter = DeliveryOrder.PrintConstants.Code.PDO;
			order2.Image = new Bitmap(3, 5);
			order2.PrincipalPK = principalPK2;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, new DeliveryOrderRegistryDataType().Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(collection2, new DeliveryOrderRegistryDataType().Serialise(collection2))
			};
		}

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

		protected override void SetUp()
		{
			base.SetUp();

			principalPK1 = CreatePrincipal("Test1");
			principalPK2 = CreatePrincipal("Test2");
		}

		ZGuid principalPK1;
		ZGuid principalPK2;

		protected override string ExpectedEditorName => "DeliveryOrderRegistryItemEditor";
	}
}
