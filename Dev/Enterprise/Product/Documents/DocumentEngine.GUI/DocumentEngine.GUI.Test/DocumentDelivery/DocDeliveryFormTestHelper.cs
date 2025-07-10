using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Scheduler.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.GUI.Testing
{
	internal static class DocDeliveryFormTestHelper
	{
		internal static DeliveryInstructions CreateInstructionsWithValidData(BusinessObjectFactory factory)
		{
			var organisation = factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = organisation.Contacts.AddNew();
			contact.OC_ContactName = "Bob";
			contact.OC_Email = "bob@bob.com";

			factory.Save();

			DeliveryInstructions result = CreateInstructions(factory);
			DocDeliveryContact recipient = result.Recipients[0];
			recipient.OrgHeaderPK = organisation.PK;
			recipient.Name = "Bob";
			recipient.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			recipient.DeliveryAddress = "bob@bob.com";

			return result;
		}

		internal static DeliveryInstructions CreateInstructions(BusinessObjectFactory factory)
		{
			var command = factory.Load<ReportCommand>(ReportScheduleTaskTest.TestReportPK);
			var pack = new DocumentPack(command);
			if (pack.Count == 0)
			{
				throw new InvalidOperationException("Pack.Count should not be 0.");
			}
			return new DeliveryInstructions(pack);
		}
	}
}
