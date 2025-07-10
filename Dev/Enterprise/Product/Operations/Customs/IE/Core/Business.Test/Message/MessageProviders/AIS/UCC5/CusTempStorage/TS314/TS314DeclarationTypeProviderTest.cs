using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage.Testing
{
	sealed class TS314DeclarationTypeProviderTest : DataProviderTestCase<TS314DeclarationTypeProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("TemporaryStorageMessageSendingObject missing", () => new TS314DeclarationTypeProvider(null));
				AssertExceptionThrown<ArgumentException>("TemporaryStorageHeader missing", () => new TS314DeclarationTypeProvider(new TemporaryStorageMessageSendingObject(null)));
				AssertNoExceptionThrown(() => GetProvider());
			});
		}

		public void TestMRN()
		{
			header.MRN = "123";
			AssertEquals("MRN", "123", Provider.MRN);
		}

		public void TestInvalidationReason()
		{
			messageSendingObject.CustomsJustification = "Reason";
			AssertEquals("Reason", Provider.InvalidationReason);
		}

		[TestDate(2024, 3, 26, 10, 45, 35)]
		public void TestDateOfInvalidationRequest()
		{
			AssertEquals(new DateTime(2024, 3, 26, 10, 45, 35), Provider.DateOfInvalidationRequest);
		}

		public void TestCustomsOfficeLodgement()
		{
			header.CustomsOfficeOfLodgement = "IEDUB100";
			AssertEquals("IEDUB100", Provider.CustomsOfficeLodgement);
		}

		public void TestDeclarant()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.CustomsCodes.AddNew("EOR", "IE87654321", "IE");
			header.AMA_OA_Declarant = declarant.MainAddress.PK;
			AssertEquals("IE87654321", Provider.Declarant);
		}

		protected override TS314DeclarationTypeProvider GetProvider() => new TS314DeclarationTypeProvider(messageSendingObject);

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			messageSendingObject = new TemporaryStorageMessageSendingObject(header);
		}

		TemporaryStorageMessageSendingObject messageSendingObject;
		TemporaryStorageHeader header;
	}
}
