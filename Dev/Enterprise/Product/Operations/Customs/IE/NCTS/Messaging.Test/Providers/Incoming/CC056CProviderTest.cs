using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC056C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class CC056CProviderTest : TestCaseWithFactory
	{
		public void TestEmptyValue()
		{
			CombineAssertions(() =>
			{
				var emptyProvider = new CC056CProvider(new Cc056CType());
				AssertEquals("Should return empty value", ZString.Empty, emptyProvider.MRN);
				AssertEquals("Should return empty value", ZDateTime.Empty, emptyProvider.RejectionDateAndTime);
				AssertEquals("Should return empty value", ZString.Empty, emptyProvider.RejectionCode);
				AssertEquals("Should return empty value", ZString.Empty, emptyProvider.RejectionReason);
			});
		}

		public void TestMRN()
		{
			AssertEquals("MRN", "19AA12345678901230", provider.MRN);
		}

		public void TestRejectionDateAndTime()
		{
			AssertEquals("RejectionDateAndTime", new ZDateTime(2023, 2, 16, 10, 52, 5), provider.RejectionDateAndTime);
		}

		public void TestRejectionCode()
		{
			AssertEquals("RejectionCode", "IE0001", provider.RejectionCode);
		}

		public void TestRejectionReason()
		{
			AssertEquals("RejectionReason", "Test Rejection Reason", provider.RejectionReason);
		}

		public void TestBusinessRejectionType()
		{
			AssertEquals("BusinessRejectionType", "015", provider.BusinessRejectionType);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC056CProvider(new Cc056CType
			{
				TransitOperation = new TransitOperationType20
				{
					Mrn = "19AA12345678901230",
					RejectionDateAndTime = new DateTime(2023, 2, 16, 10, 52, 5),
					RejectionCode = "IE0001",
					RejectionReason = "Test Rejection Reason",
					BusinessRejectionType = "015",
				}
			});
		}
		CC056CProvider provider;
	}
}
