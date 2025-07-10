using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.MessageSending.AirCgm.Testing;

[TestedType(typeof(AirCgmCMCHI01AdditionalDataProvider))]
sealed class AirCgmCMCHI01AdditionalDataProviderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new AirCgmCMCHI01AdditionalDataProvider(null));
			AssertNoExceptionThrown(() => new AirCgmCMCHI01AdditionalDataProvider(MessageSendingObject));
		});
	}

	public void TestGetMessageTypeForMaster()
	{
		var additionalDataProvider = CreateAirCgmCMCHI01AdditionalDataProvider("A");
		CombineAssertions(() =>
		{
			Header.MasterBill.ABL_BillStatus = "A";
			AssertEquals("A", additionalDataProvider.GetMessageType(Header));

			Header.MasterBill.ABL_BillStatus = "D";
			AssertEquals(string.Empty, additionalDataProvider.GetMessageType(Header));

			Header.MasterBill.ABL_BillStatus = "F";
			AssertEquals(string.Empty, additionalDataProvider.GetMessageType(Header));

			Header.MasterBill.ABL_BillStatus = "S";
			AssertEquals(string.Empty, additionalDataProvider.GetMessageType(Header));
		});

		additionalDataProvider = CreateAirCgmCMCHI01AdditionalDataProvider("D");
		AssertEquals("D", additionalDataProvider.GetMessageType(Header));

		additionalDataProvider = CreateAirCgmCMCHI01AdditionalDataProvider("F");
		AssertEquals("F", additionalDataProvider.GetMessageType(Header));
	}

	public void TestGetMessageTypeForHouseBill()
	{
		var additionalDataProvider = CreateAirCgmCMCHI01AdditionalDataProvider("A");
		var bill = Header.Bills.AddNew();
		CombineAssertions(() =>
		{
			bill.ABL_BillStatus = "F";
			AssertEquals(string.Empty, additionalDataProvider.GetMessageType(bill));

			bill.ABL_BillStatus = "A";
			AssertEquals("A", additionalDataProvider.GetMessageType(bill));

			bill.ABL_BillStatus = "D";
			AssertEquals("D", additionalDataProvider.GetMessageType(bill));

			bill.ABL_BillStatus = "S";
			AssertEquals("S", additionalDataProvider.GetMessageType(bill));
		});

		additionalDataProvider = CreateAirCgmCMCHI01AdditionalDataProvider("D");
		AssertEquals("D", additionalDataProvider.GetMessageType(Header));

		additionalDataProvider = CreateAirCgmCMCHI01AdditionalDataProvider("F");
		AssertEquals("F", additionalDataProvider.GetMessageType(Header));
	}

	AirCgmCMCHI01AdditionalDataProvider CreateAirCgmCMCHI01AdditionalDataProvider(string messageType)
	{
		MessageSendingObject.MessageType = messageType;
		return new AirCgmCMCHI01AdditionalDataProvider(MessageSendingObject);
	}

	ManifestMessageSendingObject MessageSendingObject => messageSendingObject ??= new ManifestMessageSendingObject(Header);
	ManifestMessageSendingObject messageSendingObject;

	CGMAsycudaManifestHeader Header => header ??= Factory.New<CGMAsycudaManifestHeader>();
	CGMAsycudaManifestHeader header;
}
