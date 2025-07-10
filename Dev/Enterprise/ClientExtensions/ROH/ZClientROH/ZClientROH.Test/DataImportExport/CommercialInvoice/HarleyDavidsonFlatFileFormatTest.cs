using NUnit.Framework;

namespace Enterprise.Client.Rohlig.HarleyDavidson.Testing
{
	public class HarleyDavidsonFlatFileFormatTest : TestCase
	{
		public void TestConvertToRow()
		{
			HarleyDavidsonFlatFileFormat format = new HarleyDavidsonFlatFileFormat();
			AssertNull("Row should be null, Header row should be ignored", format.ConvertToRow("PDLITM,PDDSC1,PDUORG,COSTAUS,PDCNID"));
			AssertNotNull("Row should not be null, this is not a header row", format.ConvertToRow("N0521.02A8,\"FOOTPEG ASSY, RIDER, RH\",4,12.51,CPSU4026351"));
		}
	}
}
