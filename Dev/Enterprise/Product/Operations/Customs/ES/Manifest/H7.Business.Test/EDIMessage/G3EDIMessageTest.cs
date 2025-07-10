using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	[TestedType(typeof(G3EDIMessage))]
	sealed class G3EDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHeader()
		{
			var g3Declaration = Factory.New<G3EDIMessage>();
			var header = Factory.New<AsycudaManifestHeader>();
			g3Declaration.EM_LinkedObject = header;

			AssertEquals(header, g3Declaration.Header);
		}

		public void TestBillsCount()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.Bills.AddNew();

			var messageWithHeader = Factory.New<G3EDIMessage>();
			messageWithHeader.EM_LinkedObject = header;

			var messageWithNoHeadeer = Factory.New<G3EDIMessage>();

			CombineAssertions("Bills count", () =>
			{
				AssertEquals("BillsCount returns the number of bills in the linked header", 1, messageWithHeader.BillsCount);
				AssertEquals("BillsCount returns 0 when there is no linked header", 0, messageWithNoHeadeer.BillsCount);
			});
		}
	}
}
