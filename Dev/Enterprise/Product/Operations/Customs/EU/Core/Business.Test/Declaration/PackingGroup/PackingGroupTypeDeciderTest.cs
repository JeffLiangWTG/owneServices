using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	sealed class PackingGroupTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForBindingForNonEUCountry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var typeDecider = new PackingGroupTypeDecider();
				var expectedType = typeof(PackingGroup);
				AssertEquals(expectedType, typeDecider.GetTypeForBinding());
			}
		}

		public void TestGetTypeForNewForNonEUCountry()
		{
			var typeDeciderContextMock = new Mock<ITypeDeciderContext>();
			typeDeciderContextMock.Setup(k => k.Country).Returns(Core.Constants.CountryCodes.Australia);

			var typeDecider = new PackingGroupTypeDecider();
			var expectedType = typeof(PackingGroup);
			AssertEquals(expectedType, typeDecider.GetTypeForNew(typeDeciderContextMock.Object));
		}

		public void TestTypeDecider_EU()
		{
			AssertNoExceptionWhenLoading<Integration.Customs.EU.IPackingGroup>(Core.Constants.CountryCodes.Latvia);
		}

		public void TestTypeDecider_Poland()
		{
			AssertNoExceptionWhenLoading<Integration.Customs.PL.IPackingGroup>(Core.Constants.CountryCodes.Poland);
		}

		public void TestGetTypeForNew_ITypeDeciderContext()
		{
			var packingGroup = Factory.New<PackingGroup>();
			AssertEquals("Enterprise.Customs.EU.Business.Declaration.PackingGroup", packingGroup.GetType().FullName);

			var typeDeciderContextMock = new Mock<ITypeDeciderContext>();
			typeDeciderContextMock.Setup(k => k.Country).Returns("PL");
			packingGroup = Factory.New<PackingGroup>(typeDeciderContextMock.Object);
			AssertEquals("Enterprise.Customs.PL.Business.Declaration.PackingGroup", packingGroup.GetType().FullName);
		}

		BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>(TestBusinessObjectKind.MinimumRequiredToSave);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONT123456";
			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "12345678";

			var packGroup = (houseBill.PackingGroups.Count > 0) ? houseBill.PackingGroups[0] : houseBill.PackingGroups.AddNew();
			packGroup.CR_CO_Container = container.PK;
			packGroup.CR_CU_HouseBill = houseBill.PK;

			return packGroup;
		}

		void AssertNoExceptionWhenLoading<T>(string country)
			where T : class
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
			{
				var jobDeclaration = Factory.New<JobDeclaration>();
				var packingGroup = GetNewBusinessObjectForLoadTest();
				Factory.Save();
				var newFactory = new BusinessObjectFactory();
				AssertNoExceptionThrown($"The reload as {jobDeclaration.CountryCode}.CEH for {country} of an object previously loaded and cached as EU.CEH did not explode", () =>
				{
					newFactory.Load<PackingGroup>(packingGroup.PK);
					newFactory.Load<T>(packingGroup.PK);
				});
			}
		}
	}
}
