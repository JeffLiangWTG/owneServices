using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(Package))]
	class PackageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestValidation()
		{
			var package = (Package)GetNewBusinessObject();
			AssertType<PackageValidation>(package.Validation);
		}

		public void TestLoad()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONT123456";
			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "12345678";

			var packGroup = houseBill.PackingGroups[0];
			packGroup.CR_CO_Container = container.PK;
			packGroup.CR_CU_HouseBill = houseBill.PK;

			var package = packGroup.Packages[0];
			package.CW_PackQty = 10;
			package.CW_PackType = "PT";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var query = new ZQuery(CusDecHouseContainerPackSchema.CW_CR_HouseContainer, packGroup.PK);
			newFactory.Load<BasePackage>(query);
			AssertNoExceptionThrown("Attempted to return a Enterprise.Customs.EU.Business.Declaration.Package when a Enterprise.Customs.DE.Business.Declaration.Package was requested.", () => newFactory.Load<Package>(query));
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MasterBill = "MB1";
			declaration.JE_TotalNoOfPacks = 123;
			declaration.JE_TotalNoOfPacksPackType = "AE";
			var pack = declaration.Packages[0];
			return pack;
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
	}
}
