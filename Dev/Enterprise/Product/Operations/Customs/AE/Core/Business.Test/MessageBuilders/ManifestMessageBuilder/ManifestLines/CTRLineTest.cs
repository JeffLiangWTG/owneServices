using System;
using CargoWise.Types;

namespace Enterprise.Customs.AE.Business.Testing;

public class CTRLineTest : ManifestLineTest.ManifestLineTesting
{
	public void TestProperties()
	{
		CTRLine line = (CTRLine)GetNewManifestLine();
		line.ContainerNumber = Pad("CTR001");
		line.CheckDigit = Pad("3123");
		line.TareWeightInMT = 135.2343m;
		line.SealNumber = Pad("SEALNUM");
		AssertEquals("CTR", line.Identifier);
		AssertEquals("CTR001    ", line.ContainerNumber);
		AssertEquals("3", line.CheckDigit);
		AssertEquals(new ZDecimal(135.2), line.TareWeightInMT);
		AssertEquals("SEALNUM   ", line.SealNumber);
	}

	#region override
	protected override ZString TestingCountry
	{
		get
		{
			return null;
		}
	}

	protected override ManifestLine GetNewManifestLine()
	{
		return new CTRLine();
	}

	protected override int FieldCount
	{
		get
		{
			return 5;
		}
	}

	protected override Type ExpectedType
	{
		get
		{
			return typeof(CTRLine);
		}
	}
	#endregion
}
