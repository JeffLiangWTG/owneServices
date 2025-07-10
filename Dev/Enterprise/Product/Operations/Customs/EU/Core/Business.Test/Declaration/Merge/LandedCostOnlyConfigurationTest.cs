using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Moq.Protected;

namespace Enterprise.Customs.EU.Business.Testing.Declaration.Merge
{
	public class LandedCostOnlyConfigurationTest : TestCaseWithFactory
	{
		public void TestGetIsLandedCostingOnlyFuncForEntryLine()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var refCusProcedure = Factory.New<RefCusProcedure>();

			var mockLine = Factory.NewMoq<JobComInvoiceLine>();
			mockLine.Protected().Setup<RefCusProcedure>("GetCusProcedure", ItExpr.IsAny<ZString>()).Returns(refCusProcedure);
			var line = mockLine.Object;
			entryLine.InvoiceLines.Add(line);

			var landedCostOnlyConfiguration = new LandedCostOnlyConfiguration();

			CombineAssertions(() =>
			{
				refCusProcedure.ZZ6_CalculateDuty = false;
				refCusProcedure.ZZ6_LandedCost = true;
				Assert("IsLandedCosting is true", landedCostOnlyConfiguration.GetIsLandedCostingOnlyFuncForEntryLine().Invoke(entryLine, ""));

				refCusProcedure.ZZ6_CalculateDuty = true;
				refCusProcedure.ZZ6_LandedCost = true;
				Assert("IsLandedCosting is false as CalculateDuty is true in ref table", !landedCostOnlyConfiguration.GetIsLandedCostingOnlyFuncForEntryLine().Invoke(entryLine, ""));

				refCusProcedure.ZZ6_CalculateDuty = false;
				refCusProcedure.ZZ6_LandedCost = false;
				Assert("IsLandedCosting is false as LandedCost is false in ref table", !landedCostOnlyConfiguration.GetIsLandedCostingOnlyFuncForEntryLine().Invoke(entryLine, ""));
			});
		}

		public void TestGetIsLandedCostingOnlyFuncForEntryHeader()
		{
			var entryHeader = Factory.New<Customs.Business.CusEntryHeader>();
			var landedCostOnlyConfiguration = new LandedCostOnlyConfiguration();

			Assert("IsLandedCosting is false", !landedCostOnlyConfiguration.GetIsLandedCostingOnlyFuncForEntryHeader().Invoke(entryHeader, ""));
		}
	}
}
