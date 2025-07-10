using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.DataImport.Level1FileFormat.Testing
{
	public class _400000LineTest : TestCase
	{
		public void TestAccountNumber()
		{
			AssertEquals("A15V04", _400000Line.AccountNumber);
		}

		public void TestName()
		{
			AssertEquals("DAVID TASKER", _400000Line.Name);
		}

		public void TestStreet1()
		{
			AssertEquals("18 WENDY CRESENT", _400000Line.Street1);
		}

		public void TestStreet2()
		{
			AssertEquals("XYZ", _400000Line.Street2);
		}

		public void TestCity()
		{
			AssertEquals("QUEENSLAND", _400000Line.City);
		}

		public void TestCountry()
		{
			AssertEquals("AU", _400000Line.Country);
		}

		public void TestState()
		{
			AssertEquals("VI", _400000Line.State);
		}

		public void TestPostCode()
		{
			AssertEquals("4019", _400000Line.PostCode);
		}

		public void TestPhone()
		{
			AssertEquals("0738831842", _400000Line.Phone);
		}

		public void TestFax()
		{
			AssertEquals("999", _400000Line.Fax);
		}

		public void TestContactName()
		{
			AssertEquals("DAVID TASKER", _400000Line.ContactName);
		}

		public void TestReferenceNumber2()
		{
			AssertEquals("REFERENCE2REFERENCE2REFERENCE2REF22", _400000Line.ReferenceNumber2);
		}

		protected override void SetUp()
		{
			base.SetUp();
			_400000Line = new _400000Line("US4196AU9639000626              DA15V04FXKC7400000        0000A15V04DAVID TASKER                       DAVID TASKER             18 WENDY CRESENT                   XYZ                                QUEENSLAND                                             VI4019     AU 0738831842    999                       REFERENCE2REFERENCE2REFERENCE2REF22                   000           ");
		}

		_400000Line _400000Line;
	}
}
