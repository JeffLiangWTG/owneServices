using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.DataTransfer.eNett_Integration.Testing
{
	public class eNettWebServiceFailedEmailTest : AccountingEmailDefTest
	{
		public void TestConstructor_Error()
		{
			var constructorError = AssertExceptionThrown<ArgumentException>(() =>
				new eNettWebServiceFailedEmail("WebMethodName()", ZDateTime.BrettsBirthday.ToDateTime(), "101", "This is an error message", false)
			);
			AssertContains("It is invalid to set 'isAutoFactorySave' false when no external Factory has been passed to 'factory' as it will lead to losing unsaved data", constructorError.Message);

			constructorError = AssertExceptionThrown<ArgumentException>(() =>
				new eNettWebServiceFailedEmail("WebMethodName()", ZDateTime.BrettsBirthday.ToDateTime(), "101", "This is an error message", true, Factory)
			);
			AssertContains("'factory' must be null when 'isAutoFactorySave' is true as it will be ignored.", constructorError.Message);
		}

		public void TestConstructor_Ok()
		{
			AssertNoExceptionThrown(() =>
			{
				_ = new eNettWebServiceFailedEmail("WebMethodName()", ZDateTime.BrettsBirthday.ToDateTime(), "101", "This is an error message", false, Factory);
				_ = new eNettWebServiceFailedEmail("WebMethodName()", ZDateTime.BrettsBirthday.ToDateTime(), "101", "This is an error message", true);
			});
		}

		protected override Type EmailDefType
		{
			get { return typeof(eNettWebServiceFailedEmail); }
		}

		public void TestContent()
		{
			string methodName = "WebMethodName()";
			string errorCode = "101";
			string errorMessage = "This is an error message";
			eNettWebServiceFailedEmail email = new eNettWebServiceFailedEmail(methodName, ZDateTime.BrettsBirthday.ToDateTime(), errorCode, errorMessage, true);

			string expectedResult = string.Format(@"Call to web method {0} failed.
Date: {1}
Code: {2}
Message: {3}", methodName, ZDateTime.BrettsBirthday.ToDateTime(), errorCode, errorMessage);

			AssertEquals(expectedResult, email.GetContent_ForTestOnly());
		}

		public void TestNoNewLinesInSubject()
		{
			string methodName = "WebMethodName()";
			string errorCode = "101";
			string errorMessage = "This is an error message";
			eNettWebServiceFailedEmail email = new eNettWebServiceFailedEmail(methodName, ZDateTime.BrettsBirthday.ToDateTime(), errorCode, errorMessage, true);

			ZString subject = email.GetSubject_ForTestOnly();
			Assert(!subject.Contains(System.Environment.NewLine));
			AssertEquals("Call to web method WebMethodName() failed", subject);
		}

		public void TestSend_IsAutoSave()
			=> TestSend(true);

		public void TestSend_NotAutoSave()
			=> TestSend(false);

		void TestSend(bool isAutoSave)
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			new AccountingPeriodTestHelper(Factory).SetupPeriods();

			var companyProxy = testObjectCreator.CreateOrgHeader("ABCPROXY", true, true);
			var eNettCompany = testObjectCreator.CreateNewCompany("ABC");
			eNettCompany.GC_Name = "ThrowCriticalException";
			eNettCompany.GC_OH_OrgProxy = companyProxy.PK;
			eNettCompany.OrgProxy.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "201654");

			var eNettCompanyBranch = testObjectCreator.CreateNewBranch(eNettCompany, "BR1");

			var eNettRegisteredOrg = testObjectCreator.CreateOrgHeader("VALID", true, true);
			eNettRegisteredOrg.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "201654");
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, eNettCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				new AccountingPeriodTestHelper(Factory).SetupPeriods();
				eNettRegisteredOrg.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "201654");
			}

			GlbStaff currentUserInCurrentFactory = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentUserInCurrentFactory.GS_EmailAddress = "A@B.COM";
			GlbGroup group = Factory.New<GlbGroup>();
			group.Staff.Add(currentUserInCurrentFactory);
			Factory.Save();

			AccountingConfigurationRegistry.Instance.ENettNotificationsGroup.SetValue(eNettCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, group.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ENettRegistration.SetValue(eNettCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new EnettRegistrationCode() { RegistrationCode = "201649", OrganisationPK = companyProxy.PK });

			//------
			var mailIdentityAchor = Guid.NewGuid().ToString();

			BusinessObjectFactory factoryInMail;
			var currentMailCount = Env.OutgoingMailManager.EmailsCreated.Count;
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, eNettCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var email = isAutoSave
					? new eNettWebServiceFailedEmail(mailIdentityAchor, ZDateTime.BrettsBirthday.ToDateTime(), "101", "This is an error message", true)
					: new eNettWebServiceFailedEmail(mailIdentityAchor, ZDateTime.BrettsBirthday.ToDateTime(), "101", "This is an error message", false, Factory);
				factoryInMail = email.Factory_ForTestOnly;
				email.Send();
			}
			var afterMailCount = Env.OutgoingMailManager.EmailsCreated.Count;

			AssertEquals(1, afterMailCount - currentMailCount);

			if (isAutoSave)
			{
				AssertEquals(1, getMailCount(mailIdentityAchor));
			}
			else
			{
				AssertEquals(0, getMailCount(mailIdentityAchor));
				factoryInMail.Save();
				AssertEquals(1, getMailCount(mailIdentityAchor));
			}

			int getMailCount(string miSubject)
			{
				var collection = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
				ZSqlParameterCollection @params = new ZSqlParameterCollection(
					ZSqlParameter.New("@MI_Subject", miSubject, ZArchitecture.Schema.MailDBItemsSchema.MI_Subject)
				);
				collection.Load($@"SELECT TOP(1) MI_PK FROM {ZArchitecture.Schema.MailDBItemsSchema.Constants.TableName} WHERE MI_Subject LIKE '%'+@MI_Subject+'%'", @params);
				return collection.Count;
			}
		}
	}
}
