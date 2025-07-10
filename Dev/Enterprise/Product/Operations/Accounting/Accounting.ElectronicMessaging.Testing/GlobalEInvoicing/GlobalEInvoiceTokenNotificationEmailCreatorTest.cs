using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing
{
	public class GlobalEInvoiceTokenNotificationEmailCreatorTest : TestCaseWithFactory
	{
		GlbBranch Branch;

		GlbCompany Company;

		TestObjectCreator TestObjectCreator => testObjectCreator ??= new TestObjectCreator(Factory);
		TestObjectCreator testObjectCreator;

		protected virtual IElectronicMessagingNotificationEmailCreator GetCreator()
			=> new GlobalEInvoiceTokenNotificationEmailCreator();

		public void TestArguments()
		{
			var emailCreator = GetCreator();

			var credential = TestObjectCreator.CreateCompanyCertificate(Company, "DummyIssuer", "Dummy PAC", new DateTime(2022, 07, 20, 18, 30, 59));
			var credentials = new EInvoicingCertificateCredential[] { credential };

			AssertExceptionThrown<ArgumentNullException>(
				"Should throw exception",
				"Branch or Company can't be null.",
				() => { emailCreator.Create(null, null, Guid.Empty, null); });

			AssertExceptionThrown<ArgumentException>(
				"Should throw exception",
				"Branch or Company should be null.",
				() => { emailCreator.Create(Branch, Company, Guid.Empty, null); });

			AssertNoExceptionThrown(
				() => { emailCreator.Create(Branch, null, Guid.Empty, credentials); });

			AssertNoExceptionThrown(
				() => { emailCreator.Create(null, Company, Guid.Empty, credentials); });
		}

		protected override void SetUp()
		{
			base.SetUp();

			Company = TestObjectCreator.CreateNewCompany("TAU", CountryCodes.Australia, TestObjectCreator.ABIGAS);
			Branch = TestObjectCreator.CreateBranch("AU1", Company, TestObjectCreator.ABIGAS);
		}
	}
}
