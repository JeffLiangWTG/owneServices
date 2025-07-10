namespace Enterprise.Client.EDI.Test
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.Client.EDI.UserManagement.Business;
	using Enterprise.Client.EDI.UserManagement.Module;
	using Enterprise.Environment;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.Testing;
	using NUnit.Framework;

	[TestedType(typeof(EdiUserAgreementAcceptanceLogController))]
	public class EdiUserAgreementAcceptanceLogControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return Modules.ClientControllerRegistration.UserAgreementAcceptances;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var agreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			var userAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			var acceptanceLog = Factory.NewWithValidTestData<EdiUserAgreementAcceptanceLog>();
			acceptanceLog.EUL_ERA = agreement.PK;
			acceptanceLog.EUL_EUA = userAccount.PK;
			Factory.Save();
			return acceptanceLog;
		}

		protected override Type GetBusinessObjectType() => typeof(EdiUserAgreementAcceptanceLog);

		public void TestSecurityCheckpoints()
		{
			var controller = new EdiUserAgreementAcceptanceLogController();
			CombineAssertions(() =>
			{
				AssertEquals("For New", Env.Security.None, controller.CheckPointForNewExposedForTest);
				AssertEquals("For View", Env.Security.None, controller.CheckPointForViewExposedForTest);
				AssertEquals("For Edit", Env.Security.None, controller.CheckPointForEditExposedForTest);
				AssertEquals("For Delete", Env.Security.None, controller.CheckPointForDeleteExposedForTest);
			});
		}

		public override void TestNewForm()
		{
			AssertNull("NewForm should be null", Controller.ShowNewForm());
		}

		public override void TestViewForm()
		{
			var sourceEntity = GetBusinessObjectThatIsInTheDatabase();
			AssertNull("ViewForm should be null", Controller.ShowViewForm(sourceEntity));
		}

		public override void TestEditForm()
		{
			var sourceEntity = GetBusinessObjectThatIsInTheDatabase();
			AssertNull("EditForm should be null", Controller.ShowEditForm(sourceEntity));
		}

		public override void TestDeleteForm()
		{
			var sourceEntity = GetBusinessObjectThatIsInTheDatabase();
			AssertNull("DeleteForm should be null", Controller.ShowDeleteForm(sourceEntity));
		}
	}
}
