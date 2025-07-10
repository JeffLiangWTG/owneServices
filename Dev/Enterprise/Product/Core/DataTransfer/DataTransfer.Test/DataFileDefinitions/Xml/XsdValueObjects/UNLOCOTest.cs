using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.UNLOCO))]
	sealed class UNLOCOTest : ValueObjectTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.UNLOCO value = null;
			value = new Xsd.UNLOCOCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}

		public void TestIsSpecified()
		{
			Xsd.UNLOCO unloco = new Xsd.UNLOCO();
			AssertEquals("Should not be specified by default", false, unloco.IsSpecified);
			unloco.Value = "splaty";
			AssertEquals("Should be specified when the port is set", true, unloco.IsSpecified);
			unloco.IsSpecified = false;
			AssertEquals("IsSpecified should be false when set explicitly to false", false, unloco.IsSpecified);
		}

		public void TestFromPort()
		{
			RefUNLOCO port = Factory.LoadFromNaturalKey<RefUNLOCO>(ZArchitecture.Schema.RefUNLOCOSchema.RL_Code, "AUSYD");
			Xsd.UNLOCO unlocoValue = Xsd.UNLOCO.FromPort(port);
			AssertEquals("City", "SYDNEY", unlocoValue.City.ToUpper());
			AssertEquals("Country", "AUSTRALIA", unlocoValue.Country.ToUpper());
			AssertEquals("Code", "AUSYD", unlocoValue.Value.ToUpper());

			AssertEquals("No value should return null", null, Xsd.UNLOCO.FromPort(null));
		}

		public void TestFromPortCode()
		{
			Xsd.UNLOCO unlocoValue = Xsd.UNLOCO.FromPortCode(Factory, "AUSYD");
			AssertEquals("City", "SYDNEY", unlocoValue.City.ToUpper());
			AssertEquals("Country", "AUSTRALIA", unlocoValue.Country.ToUpper());
			AssertEquals("Code", "AUSYD", unlocoValue.Value.ToUpper());

			unlocoValue = Xsd.UNLOCO.FromPortCode(Factory, "ZZYYY");
			AssertEquals("City", true, unlocoValue.City.IsEmpty);
			AssertEquals("Country", true, unlocoValue.Country.IsEmpty);
			AssertEquals("Code", "ZZYYY", unlocoValue.Value.ToUpper());

			AssertEquals("No value should return null", null, Xsd.UNLOCO.FromPortCode(Factory, null));
		}

		public void TestFromIATACode()
		{
			Xsd.UNLOCO unlocoValue = Xsd.UNLOCO.FromIATACode(Factory, "SYD");
			AssertEquals("City", "SYDNEY", unlocoValue.City.ToUpper());
			AssertEquals("Country", "AUSTRALIA", unlocoValue.Country.ToUpper());
			AssertEquals("Code", "AUSYD", unlocoValue.Value.ToUpper());

			unlocoValue = Xsd.UNLOCO.FromIATACode(Factory, "ZZZ");
			AssertEquals("UnlocoValue", null, unlocoValue);

			unlocoValue = Xsd.UNLOCO.FromIATACode(Factory, "AAE");
			AssertEquals("City", "ANNABA (EX BONE)", unlocoValue.City.ToUpper());
			AssertEquals("Country", "ALGERIA", unlocoValue.Country.ToUpper());
			AssertEquals("Code", "DZAAE", unlocoValue.Value.ToUpper());
		}
	}
}
