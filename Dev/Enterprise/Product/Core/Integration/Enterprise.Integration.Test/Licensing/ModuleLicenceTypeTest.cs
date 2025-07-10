using NUnit.Framework;

namespace Enterprise.Integration.Licensing.Test
{
	sealed class ModuleLicenceTypeTest : TestCase
	{
		public void TestIntegerValues()
		{
			AssertEquals("Do NOT change this", 13, (int)ModuleLicenceType.NON);
			AssertEquals("Do NOT change this", 23, (int)ModuleLicenceType.PUR);
			AssertEquals("Do NOT change this", 33, (int)ModuleLicenceType.REN);
			AssertEquals("Do NOT change this", 43, (int)ModuleLicenceType.TRI);
			AssertEquals("Do NOT change this", 53, (int)ModuleLicenceType.OTM);
			AssertEquals("Do NOT change this", 63, (int)ModuleLicenceType.OPN);
			AssertEquals("Do NOT change this", 73, (int)ModuleLicenceType.SRU);
			AssertEquals("Do NOT change this", 93, (int)ModuleLicenceType.ODM);
		}
	}
}
