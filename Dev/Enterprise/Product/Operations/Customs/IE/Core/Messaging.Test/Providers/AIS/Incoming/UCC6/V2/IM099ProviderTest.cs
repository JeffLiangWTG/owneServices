using System;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM099;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	class IM099ProviderTest : TestCaseWithFactory
	{
		public void TestLocalReferenceNumber()
		{
			AssertEquals("LRN001", provider.LocalReferenceNumber);
		}

		public void TestDateLimitOfResponse()
		{
			AssertEquals(new DateTime(2023, 08, 11), provider.DateLimitOfResponse);
		}

		public void TestRemarks()
		{
			AssertEquals("Remarks001", provider.Remarks);
		}

		public void TestCustomsOfficeOfPresentation()
		{
			AssertEquals("PCO12345", provider.CustomsOfficeOfPresentation);
		}

		public void TestSupervisingCustomsOffice()
		{
			AssertEquals("SCO12345", provider.SupervisingCustomsOffice);
		}

		public void TestCustomsOfficeLodgement()
		{
			AssertEquals("LCO123456", provider.CustomsOfficeLodgement);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new IM099Provider(new Im099()
			{
				ImportOperation = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MCciOperationType099
				{
					Lrn = "LRN001",
					DateLimitOfResponse = new DateTime(2023, 08, 11),
					Remarks = "Remarks001",
				},
				CustomsOfficeOfPresentation = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MPcoType { ReferenceNumber = "PCO12345" },
				SupervisingCustomsOffice = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MScoType { ReferenceNumber = "SCO12345" },
				CustomsOfficeLodgement = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MScoType { ReferenceNumber = "LCO123456" },
			});
		}
		IM099Provider provider;
	}
}
