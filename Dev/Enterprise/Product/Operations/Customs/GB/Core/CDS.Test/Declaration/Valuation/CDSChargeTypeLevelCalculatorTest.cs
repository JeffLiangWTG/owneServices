using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.CDS.Declaration;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	public class CDSChargeTypeLevelCalculatorTest : TestCaseWithFactory
	{
		readonly ZString[] itemChargeTypes = new ZString[]
		{
			"AB", "AC", "AD", "AE", "AF", "AX", "AG", "AH", "AZ", "AI", "AM", "AJ", "AL", "AN", "AO",
			"AT", "AU", "BB", "BC", "BD" , "BL", "BE" , "BM", "BF" , "BG", "BK", "BH", "BI" , "BT"
		};

		readonly ZString[] headerChargeTypes = new ZString[]
		{
			"AK", "AP", "AQ", "AR", "AS", "AV", "AW", "BA", "BU", "BR", "BS"
		};

		public void TestIsItemLevel()
		{
			foreach (var chargeType in headerChargeTypes)
			{
				var cdsChargeTypeLevelCalculator = new CDSChargeTypeLevelCalculator(chargeType);
				Assert(chargeType, !cdsChargeTypeLevelCalculator.IsItemLevel);
			}

			foreach (var chargeType in itemChargeTypes)
			{
				var cdsChargeTypeLevelCalculator = new CDSChargeTypeLevelCalculator(chargeType);
				Assert(chargeType, cdsChargeTypeLevelCalculator.IsItemLevel);
			}
		}
	}
}
