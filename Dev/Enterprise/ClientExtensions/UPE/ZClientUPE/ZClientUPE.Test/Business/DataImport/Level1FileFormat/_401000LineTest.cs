using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.DataImport.Level1FileFormat.Testing
{
	public class _401000LineTest : TestCase
	{
		public void TestAccountNumber()
		{
			AssertEquals("8AU0596227", _401000Line.AccountNumber);
		}

		public void TestName()
		{
			AssertEquals("WENDY PATERSON", _401000Line.Name);
		}

		public void TestStreet1()
		{
			AssertEquals("GPO BOX 1609", _401000Line.Street1);
		}

		public void TestStreet2()
		{
			AssertEquals("ABC", _401000Line.Street2);
		}

		public void TestCity()
		{
			AssertEquals("SYDNEY", _401000Line.City);
		}

		public void TestCountry()
		{
			AssertEquals("AU", _401000Line.Country);
		}

		public void TestState()
		{
			AssertEquals("NS", _401000Line.State);
		}

		public void TestPostCode()
		{
			AssertEquals("2001", _401000Line.PostCode);
		}

		public void TestPhone()
		{
			AssertEquals("123456789", _401000Line.Phone);
		}

		public void TestFax()
		{
			AssertEquals("987654321", _401000Line.Fax);
		}

		public void TestContactName()
		{
			AssertEquals("ContactName", _401000Line.ContactName);
		}

		public void TestLeadTrackingNumberForGCCShipment()
		{
			AssertEquals("09576XP334C", _401000Line.LeadTrackingNumberForGCCShipment);
		}

		public void TestTotalNumberOfShipmentsForGCCShipment()
		{
			AssertEquals(169, _401000Line.TotalNumberOfShipmentsForGCCShipment);
		}

		public void TestTotalPackageCountForGCCShipment()
		{
			AssertEquals(173, _401000Line.TotalPackageCountForGCCShipment);
		}

		public void TestWeightUnitOfQuantityForGCCShipment()
		{
			AssertEquals(Core.Constants.Weight.Pounds, _401000Line.WeightUnitOfQuantityForGCCShipment);
		}

		public void TestTotalWeightForGCCShipment()
		{
			AssertEquals(466m, _401000Line.TotalWeightForGCCShipment);
		}

		protected override void SetUp()
		{
			base.SetUp();
			_401000Line = new _401000Line("US2795AU9639040422              DA4W82133K3G401000        8AU0596227WENDY PATERSON                     ContactName              GPO BOX 1609                       ABC                                SYDNEY                                                 NS2001     AU 123456789        987654321  09576XP334CAU09639169   173    LBS466                                237           ");
		}

		_401000Line _401000Line;
	}
}
