using System;

namespace Enterprise.Customs.AE.Business.Testing;

public class ENDLineTest : ManifestLineTest.ManifestLineTesting
{
	public void TestProperties()
	{
		ENDLine line = (ENDLine)GetNewManifestLine();
		line.NoOfContainerRelatedBOL = 123456789;
		line.NoOfOtherBOL = 12345679;
		line.Remarks = "REMARKS";
		AssertEquals("END", line.Identifier);
		AssertEquals(1234, line.NoOfContainerRelatedBOL);
		AssertEquals(1234, line.NoOfOtherBOL);
		AssertEquals("REMARKS", line.Remarks);
	}

	#region Override
	protected override int FieldCount
	{
		get
		{
			return 4;
		}
	}

	protected override Type ExpectedType
	{
		get
		{
			return typeof(ENDLine);
		}
	}

	protected override ManifestLine GetNewManifestLine()
	{
		return new ENDLine();
	}
	#endregion
}
