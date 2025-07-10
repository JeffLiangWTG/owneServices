using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;

namespace Enterprise.MailManager.Business.Testing
{
	sealed class CustomsMailItemTest : TestCaseWithFactory
	{
		public void TestMI_Header_RecipientsAreAddedAsSystemCommunication()
		{
			Env.Registry.EmailDestinationOverride = "fake@email.com";
			Env.Registry.SystemEmailDestinationOverride = false;
			var mailItem = Factory.New<CustomsMailItem>();
			mailItem.MI_Header = "To: <Mykola@w1seTek.com>, <egi@blah.com>, <alex@example.com>";
			var sortedRecipients = mailItem.MailRecipients.Cast<MailRecipient>().OrderBy(r => r.EmailAddress).ToList();
			AssertEquals(3, sortedRecipients.Count);
			AssertEquals("<alex@example.com>", sortedRecipients[0].EmailAddress);
			AssertEquals("<egi@blah.com>", sortedRecipients[1].EmailAddress);
			AssertEquals("<Mykola@w1seTek.com>", sortedRecipients[2].EmailAddress);
		}
	}
}
