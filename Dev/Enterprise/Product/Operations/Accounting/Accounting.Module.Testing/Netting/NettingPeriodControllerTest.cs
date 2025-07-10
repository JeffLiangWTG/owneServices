using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(NettingPeriodController))]
	class NettingPeriodControllerTest : ZSingletonControllerBasherTest
	{
		protected override Type GetBusinessObjectType()
		{
			return typeof(NettingSystemPeriod);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.NettingPeriod;
		}

		public override void TestNewForm()
		{
			AssertNull("NewForm should be null", Controller.ShowNewForm());
			AssertEquals("New netting period can be created from Glow desktop version.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public override void TestEditForm()
		{
			var sourceEntity = GetBusinessObjectThatIsInTheDatabase();
			AssertNull("EditForm should be null", Controller.ShowEditForm(sourceEntity));
			AssertEquals("Please edit relevant netting period in Glow desktop version.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public override void TestDeleteForm()
		{
			var sourceEntity = GetBusinessObjectThatIsInTheDatabase();
			AssertNull("DeleteForm should be null", Controller.ShowDeleteForm(sourceEntity));
			AssertEquals("Netting periods can not be deleted.", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}
}
