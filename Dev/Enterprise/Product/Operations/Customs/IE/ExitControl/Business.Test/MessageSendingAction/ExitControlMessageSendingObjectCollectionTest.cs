using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	[TestedType(typeof(ExitControlMessageSendingObjectCollection))]
	class ExitControlMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ExitControlMessageSendingObjectCollection>
	{
		protected override Type GetExpectedCollectionType() => typeof(ExitControlMessageSendingObjectCollection);

		protected override ExitControlMessageSendingObjectCollection GetCollectionToTest()
		{
			return new ExitControlMessageSendingObjectCollection(exitHeader.CusExitReports, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var report = exitHeader.CusExitReports.AddNew();
			return new ExitControlMessageSendingObject(report);
		}

		protected override void SetUp()
		{
			base.SetUp();
			exitHeader = Factory.New<CusExitHeader>();
			var report = exitHeader.CusExitReports.AddNew();
			report.CER_TransportID = "TRANS123";
		}
		CusExitHeader exitHeader;
	}
}
