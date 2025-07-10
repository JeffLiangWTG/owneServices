using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;

namespace Enterprise.Customs.AE.Business.Testing;

public class ManifestLineFormatTest : FlatFileFormatTestCase
{
	public void TestConvertToString()
	{
		ManifestLineFormat format = new ManifestLineFormat();
		ManifestLine line = new ManifestLineTest.MockManifestLine(4);
		line.SetField(1, "ab");
		line.SetField(2, "cd");
		line.SetField(3, "ef");
		ZString expected = "\"MOCK\",\"ab\",\"cd\",\"ef\"";
		AssertEquals("Did not format a correct Manifest line from ManifestLine", expected, format.ConvertToLine(line));
	}

	public void TestConvertToManifestLine()
	{
		ZString line = "\"ab\",\"cd\",\"ef\"";
		ManifestLineFormat format = new ManifestLineFormat();
		ManifestLine manifestLine = new ManifestLineTest.MockManifestLine(format.ConvertToRow(line));
		AssertEquals("ab", manifestLine.GetField(0));
		AssertEquals("cd", manifestLine.GetField(1));
		AssertEquals("ef", manifestLine.GetField(2));
	}

	protected override FlatFileFormat GetFlatFileFormat()
	{
		return new ManifestLineFormat();
	}
}
