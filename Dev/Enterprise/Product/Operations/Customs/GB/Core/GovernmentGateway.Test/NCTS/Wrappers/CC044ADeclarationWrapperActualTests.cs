using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.GB.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GovernmentGateway.GatewayApplications.CTC.Messaging.Testing
{
	[TestedType(typeof(CC044ADeclarationWrapperActual))]
	class CC044ADeclarationWrapperActualTests : EU.NCTS.Business.Testing.DeclarationWrapperAbstractTest<CC044ADeclarationWrapperActual>
	{
		public void TestUnloadingDate()
		{
			header.UnloadingRemark.G9_UnloadingDate = ZDateTime.BrettsBirthday;
			AssertEquals("19710918", wrapper.UnloadingRemark.UnloadingDate);
		}

		public void TestUnloadedGoodsItems()
		{
			unloadingMovementHeader.GoodsItems.AddNew();
			unloadingMovementHeader.GoodsItems.AddNew();
			unloadingMovementHeader.GoodsItems.AddNew();
			AssertEquals(3, wrapper.UnloadedGoodsItems.Count);
		}

		public void TestConforms()
		{
			header.UnloadingRemark.G9_Conform = "Y";
			AssertEquals("Y", wrapper.UnloadingRemark.Conform);
		}

		public void TestStateOfSeals()
		{
			header.UnloadingRemark.G9_StateOfSealsOk = "X";
			AssertEquals("X", wrapper.UnloadingRemark.StateOfSealsOk);
		}

		public void TestCompleted()
		{
			header.UnloadingRemark.G9_UnloadingCompletion = "Z";
			AssertEquals("Z", wrapper.UnloadingRemark.UnloadingCompletion);
		}

		public void TestSeals()
		{
			var seals = header.ArrivalMovementHeader.Seals;
			seals.AddNew().CY_Data = "S1";
			seals.AddNew().CY_Data = "S2";
			AssertContainsExactElementsInAnyOrder(new[] { "S1", "S2" }, wrapper.Seals.Select(s => s.SealIdentity));
		}

		public void TestNumberOfSeals()
		{
			var seals = header.ArrivalMovementHeader.Seals;
			seals.AddNew().CY_Data = "S1";
			seals.AddNew().CY_Data = "S2";
			AssertEquals(2, wrapper.NumberOfSeals);
		}

		public void TestRemarks()
		{
			header.HeaderUnloadingNotes = "A-OK";
			AssertEquals("A-OK", wrapper.HeaderUnloadingNotes);
		}

		public void TestIdentityOfMeansOfTransportAtDeparture()
		{
			header.UnloadedMeansOfTransportAtDepartureIdentity = "LCK999";
			AssertEquals("LCK999", wrapper.IdentityOfMeansOfTransportAtDeparture);
		}

		public void TestIdentityOfMeansOfTransportAtDepartureLanguage()
		{
			AssertEquals("", wrapper.IdentityOfMeansOfTransportAtDepartureLanguage);
		}

		public void TestNationalityOfMeansOfTransportAtDeparture()
		{
			header.UnloadedMeansOfTransportAtDepartureNationality = "LC";
			AssertEquals("LC", wrapper.NationalityOfMeansOfTransportAtDeparture);
		}

		public void TestTotalNumberOfItems()
		{
			unloadingMovementHeader.GoodsItems.AddNew();
			unloadingMovementHeader.GoodsItems.AddNew();
			unloadingMovementHeader.GoodsItems.AddNew();
			AssertEquals(3, wrapper.TotalNumberOfItems);
		}

		public void TestTotalNumberOfPackages()
		{
			var gi1 = unloadingMovementHeader.GoodsItems.AddNew();
			var p11 = gi1.Packages.AddNew();
			p11.B5_UnitCount = 60;
			var p12 = gi1.Packages.AddNew();
			p12.B5_UnitCount = 1;

			var gi2 = unloadingMovementHeader.GoodsItems.AddNew();
			var p21 = gi2.Packages.AddNew();
			p21.B5_UnitCount = 5;
			var p22 = gi2.Packages.AddNew();
			p22.B5_UnitCount = 3;

			AssertEquals(69, wrapper.TotalNumberOfPackages);
		}

		public void TestTotalGrossMass()
		{
			unloadingMovementHeader.BM_GrossWeight = 123.45m;
			var gi1 = unloadingMovementHeader.GoodsItems.AddNew();
			gi1.BY_GrossWeight = 680m;
			gi1.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;

			var gi2 = unloadingMovementHeader.GoodsItems.AddNew();
			gi2.BY_GrossWeight = 22.05m;
			gi2.BY_GrossWeightUnit = Core.Constants.Weight.Pounds;

			AssertEquals(123.45m, wrapper.TotalGrossMass);
		}

		protected override CC044ADeclarationWrapperActual GetProvider() => wrapperCore;

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			wrapperCore = new CC044ADeclarationWrapperActual(header);
			wrapper = wrapperCore;
			unloadingMovementHeader = header.UnloadingMovementHeader;
		}
		CC044ADeclarationWrapperActual wrapperCore;
		ICC044ADeclaration wrapper;
		NctsHeader header;
		EU.NCTS.Business.NctsUnloadingMovementHeader unloadingMovementHeader;
	}
}
