using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.DataImport.Level1FileFormat.Testing
{
	public class _300000LineTest : TestCase
	{
		public void TestAccountNumber()
		{
			AssertEquals("A15V04", _300000Line.AccountNumber);
		}

		public void TestName()
		{
			AssertEquals("AVM SOFTWARE", _300000Line.Name);
		}

		public void TestStreet1()
		{
			AssertEquals("213WEST 35TH STREET,", _300000Line.Street1);
		}

		public void TestStreet2()
		{
			AssertEquals("402", _300000Line.Street2);
		}

		public void TestCity()
		{
			AssertEquals("NEW YORK", _300000Line.City);
		}

		public void TestCountry()
		{
			AssertEquals("US", _300000Line.Country);
		}

		public void TestState()
		{
			AssertEquals("NY", _300000Line.State);
		}

		public void TestPostCode()
		{
			AssertEquals("10001", _300000Line.PostCode);
		}

		public void TestPhone()
		{
			AssertEquals("12125649997", _300000Line.Phone);
		}

		public void TestFax()
		{
			AssertEquals("12125630422", _300000Line.Fax);
		}

		public void TestContactName()
		{
			AssertEquals("NALENI MCGA", _300000Line.ContactName);
		}

		public void TestReferenceNumber1()
		{
			AssertEquals("REFERENCE1REFERENCE1REFERENCE1REF11", _300000Line.ReferenceNumber1);
		}

		protected override void SetUp()
		{
			base.SetUp();
			_300000Line = new _300000Line("US4196AU9639000626              DA15V04FXKC730000007201004A15V04    AVM SOFTWARE                       213WEST 35TH STREET,               402                                NEW YORK                                               NY10001    US 12125649997   12125630422                                                                 REFERENCE1REFERENCE1REFERENCE1REF11NALENI MCGA");
		}

		_300000Line _300000Line;
	}
}
