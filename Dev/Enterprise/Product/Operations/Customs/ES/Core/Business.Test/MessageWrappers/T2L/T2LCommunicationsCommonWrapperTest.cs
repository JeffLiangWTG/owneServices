using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing
{
	class T2LCommunicationsCommonWrapperTest : Customs.Business.Testing.DataProviderTestCase<T2LCommunicationsCommonWrapper>
	{
		public void TestT2LCommunicationsCommonWrapperConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new T2LCommunicationsCommonWrapper(null));
		}

		public void TestDeclarationEmail()
		{
			const string clearanceEmail = "mail1.mail@mail.com";
			const string mailboxEmail = "mail2.mail@mail.com";
			CombineAssertions(() =>
			{
				using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(clearanceEmail))
				using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(mailboxEmail))
				{
					AssertEquals("Expected filled DeclarationEmail with clearance email recipient when filled", clearanceEmail, wrapper.DeclarationEmail);
				}

				using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(string.Empty))
				using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(mailboxEmail))
				{
					jobDeclaration = Factory.New<JobDeclaration>();
					wrapper = new T2LCommunicationsCommonWrapper(jobDeclaration);
					AssertEquals("Expected filled DeclarationEmail with mailbox email address when filled and clearance email recipient is empty", mailboxEmail, wrapper.DeclarationEmail);
				}

				using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(string.Empty))
				using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(string.Empty))
				{
					jobDeclaration = Factory.New<JobDeclaration>();
					wrapper = new T2LCommunicationsCommonWrapper(jobDeclaration);
					AssertEquals("Expected empty DeclarationEmail when clearance email recipient and mailbox email address are empty", ZString.Empty, wrapper.DeclarationEmail);
				}
			});
		}

		public void TestOtherEmail()
		{
			const string otherEmail = "other.mail@mail.com";
			jobDeclaration.ZG_OtherEmailAddr = otherEmail;
			AssertEquals("Expected filled OtherEmail", otherEmail, wrapper.OtherEmail);
		}

		public void TestGreenCircuitIndicator()
		{
			AssertEquals("Expected true GreenCircuitIndicator", true, wrapper.GreenCircuitIndicator);
		}

		protected override void SetUp()
		{
			base.SetUp();

			jobDeclaration = Factory.New<JobDeclaration>();
			wrapper = new T2LCommunicationsCommonWrapper(jobDeclaration);
		}

		JobDeclaration jobDeclaration;
		T2LCommunicationsCommonWrapper wrapper;

		protected override T2LCommunicationsCommonWrapper GetProvider() => wrapper;
	}
}
