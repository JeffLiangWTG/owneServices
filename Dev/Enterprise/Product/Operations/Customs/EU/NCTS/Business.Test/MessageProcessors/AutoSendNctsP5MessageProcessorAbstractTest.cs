using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using NotificationType = CargoWise.ComponentModel.NotificationType;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

[TestsSubclassesOf(typeof(AutoSendNCTSP5MessageProcessor))]
public abstract class AutoSendNctsP5MessageProcessorAbstractTest : TestCaseWithFactory
{
	protected virtual bool TestForArrival => true;
	protected virtual bool TestForDeparture => true;

	IProcessor CreateProcessor(NctsHeader nctsHeader)
	{
		return new AutoSendNCTSP5MessageProcessor(nctsHeader);
	}

	public void TestEndToEndArrival()
	{
		if (TestForArrival)
		{
			RunEndToEndTest(arrival: true);
		}
		else
		{
			Assert(true);
		}
	}

	public void TestEndToEndDeparture()
	{
		if (TestForDeparture)
		{
			RunEndToEndTest(arrival: false);
		}
		else
		{
			Assert(true);
		}
	}

	public void RunEndToEndTest(bool arrival)
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		if (arrival)
		{
			PrepareNctsHeaderForArrivalTest(nctsHeader);
			Assert("Precondition", nctsHeader.IsArrivalMovement);
		}
		else
		{
			PrepareNctsHeaderForDepartureTest(nctsHeader);
			Assert("Precondition", nctsHeader.IsDepartureMovement);
		}

		var processor = CreateProcessor(nctsHeader);
		var notifications = new NotificationBuffer();
		processor.Process(notifications);

		var errorNotifications = notifications.GetEventsByType(NotificationType.Error);
		AssertEquals($"Expected no errors, got {string.Join(",", errorNotifications.Select(n => n.Message))}", 0, errorNotifications.Length);

		var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
		AssertEquals("Notifications should contain an Information message indicating that the message was sent.", 1, informationNotifications.Length);
		AssertContains("message has been sent to customs for Job", informationNotifications[0].Message);

		var newFactory = new BusinessObjectFactory();
		var loadedHeader = newFactory.Load<NctsHeader>(nctsHeader.PK);
		if (arrival)
		{
			AssertEntryAndMessageResultForArrivalEndToEndTest(loadedHeader);
		}
		else
		{
			AssertEntryAndMessageResultForDepartureEndToEndTest(loadedHeader);
		}
	}

	protected virtual void AssertEntryAndMessageResultForDepartureEndToEndTest(NctsHeader nctsHeader) { }

	protected virtual void AssertEntryAndMessageResultForArrivalEndToEndTest(NctsHeader nctsHeader) { }

	protected void PrepareNctsHeaderForDepartureTest(NctsHeader nctsHeader)
	{
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		PrepareNctsHeaderForDepartureTestCore(nctsHeader);
	}

	protected virtual void PrepareNctsHeaderForDepartureTestCore(NctsHeader nctsHeader)
	{
	}

	protected void PrepareNctsHeaderForArrivalTest(NctsHeader nctsHeader)
	{
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		PrepareNctsHeaderForArrivalTestCore(nctsHeader);
	}

	protected virtual void PrepareNctsHeaderForArrivalTestCore(NctsHeader nctsHeader)
	{
	}
}
