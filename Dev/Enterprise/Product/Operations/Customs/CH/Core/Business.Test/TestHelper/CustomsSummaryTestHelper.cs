using CargoWise.EntityFramework;

namespace Enterprise.Customs.CH.Business.Testing;

public static class CustomsSummaryTestHelper
{
	public static CustomsSummaryLine CreateSummaryLine(BusinessObjectFactory factory)
	{
		var header = factory.New<CustomsSummaryHeader>();
		var line = factory.New<CustomsSummaryLine>();
		line.B3_B2 = header.PK;
		var lineCharge = factory.New<CustomsSummaryLineCharge>();
		lineCharge.B4_B3 = line.PK;
		return line;
	}
}
