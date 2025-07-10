using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging.AES;
using Enterprise.Customs.IE.Messaging.Testing;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	sealed class IM460ProviderTest : TestCaseWithFactory
	{
		public void TestLRN()
		{
			AssertEquals("LRN000213", provider.LocalReferenceNumber);
		}

		public void TestCustomsRegistrationNumber()
		{
			AssertEquals("23IECUSREG000009R1", provider.CustomsRegistrationNumber);
		}

		public void TestMRN()
		{
			AssertEquals("MRN2382385", provider.MovementReferenceNumber);
		}

		public void TestNotificationDate()
		{
			AssertEquals(new ZDate(2023, 09, 14), provider.NotificationDate);
		}

		public void TestNotificationType()
		{
			AssertEquals("4", provider.NotificationType);
		}

		public void TestAnticipatedControlDate()
		{
			AssertEquals(new ZDate(2023, 09, 21), provider.AnticipatedControlDate);
		}

		public void TestText()
		{
			AssertEquals("Text", provider.Text);
		}

		public void TestOverallControlTypeCode()
		{
			AssertEquals("Orange", provider.OverallControlTypeCode);
		}

		public void TestTypeOfControls()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Collection Contains", 2, provider.TypeOfControls.Count);
				AssertType<ControlTypeProvider>("Is ControlTypeProvider", provider.TypeOfControls.First());
			});
		}

		public void TestRequestedDocuments()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Collection Contains", 2, provider.RequestedDocuments.Count);
				AssertType<IM460RequestedDocumentsProvider>("Is IM460RequestedDocumentsProvider", provider.RequestedDocuments.First());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new IM460Provider(AISInterchangeProcessorTestHelper.CreateIM460Object("LRN000213", "MRN2382385", "23IECUSREG000009R1"));
		}
		IM460Provider provider;
	}
}
