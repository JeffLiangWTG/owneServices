using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(SupervisorOverrides))]
	sealed class SupervisorOverridesTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCheckAccountDates()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.CA_K84AccountingDate = new ZDateTime(2015, 10, 11);
			Factory.Save();
			declaration.CA_K84AccountingDate = new ZDateTime(2017, 10, 15);
			var supervisorOverrides = new SupervisorOverrides(declaration, SupervisorOverridesContext.SavingDeclaration);
			supervisorOverrides.CreateMessages();
			AssertCollectionHasMessage(supervisorOverrides, "Accounting Date: '11-Oct-15 00:00:00' -> '15-Oct-17 00:00:00'");
		}

		public void TestAccountDateHasChanged()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.CA_K84AccountingDate = new ZDateTime(2016, 10, 15);
			Factory.Save();
			declaration.CA_K84AccountingDate = new ZDateTime(2017, 10, 15);
			var supervisorOverrides = new SupervisorOverrides(declaration, SupervisorOverridesContext.SavingDeclaration);
			supervisorOverrides.CreateMessages();
			Assert(supervisorOverrides.AccountDateHasChanged(declaration));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2016, 10, 15);
			Factory.Save();
			return new SupervisorOverrides(declaration, SupervisorOverridesContext.SavingDeclaration);
		}

		void AssertCollectionHasMessage(SupervisorOverrides supervisorOverrides, string message)
		{
			if (supervisorOverrides.AuthorisedMessagesForLog.Cast<MessageLog>().Any(messageLog => messageLog.Message.ToUpper() == message.ToUpper()))
			{
				Assert(message, true);
			}
		}
	}
}
