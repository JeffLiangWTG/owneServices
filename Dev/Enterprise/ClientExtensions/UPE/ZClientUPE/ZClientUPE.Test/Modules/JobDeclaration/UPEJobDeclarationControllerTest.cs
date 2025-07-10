using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.Module.Testing
{
	internal class UPEJobDeclarationControllerTest : TestCaseWithFactory
	{
		public void TestGetForm()
		{
			using (IZForm form = GetNewController().GetForm(Factory.New(typeof(UPEJobDeclaration))))
			{
				AssertEquals(typeof(UPEAUCustomsDeclarationForm), form.GetType());
			}
		}

		public void TestTwoUsersCannotEditTheSameFormAtTheSameTime()
		{
			BusinessObject jobDec = Factory.New(typeof(UPEJobDeclaration));
			Factory.Save();
			IZForm form1 = GetNewController().ShowEditForm(jobDec);
			IZForm form2 = GetNewController().ShowEditForm(jobDec);
			try
			{
				AssertEquals(typeof(UPEAUCustomsDeclarationForm), form1.GetType());
				AssertNull("Should Not Create second form", form2);
				ZString lastMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				ZString startsWith = "Access to Customs Declaration : 'B00001000' is denied.\nThe record has been locked since '";
				Assert(lastMessage.StartsWith(startsWith));
				Assert(lastMessage.EndsWith("' by '" + EnvProxy.Instance.CurrentUser.FullName + "'.\nPlease wait until the lock has been released before trying to edit the record.\n"));
				int byWord = lastMessage.IndexOf("' by '" + EnvProxy.Instance.CurrentUser.FullName + "'");
				ZString dateTimePart = lastMessage.Substring(startsWith.Length, byWord - startsWith.Length);
			}
			finally
			{
				CloseAndDisposeForm(form1);
			}

			try
			{
				form2 = GetNewController().ShowEditForm(jobDec);
				AssertEquals(typeof(UPEAUCustomsDeclarationForm), form2.GetType());
			}
			finally
			{
				CloseAndDisposeForm(form2);
			}
		}

		public void TestOneUserCanViewFormWhileOtherUserEditsTheSameForm()
		{
			BusinessObject jobDec = Factory.New(typeof(UPEJobDeclaration));
			Factory.Save();
			IZForm form1 = GetNewController().ShowEditForm(jobDec);
			IZForm form2 = GetNewController().ShowViewForm(jobDec);
			try
			{
				AssertEquals(typeof(UPEAUCustomsDeclarationForm), form1.GetType());
				AssertEquals(typeof(UPEAUCustomsDeclarationForm), form2.GetType());
			}
			finally
			{
				CloseAndDisposeForm(form1);
				CloseAndDisposeForm(form2);
			}
		}

		public void TestOneUserCanEditFormWhileOtherUserViewsTheSameForm()
		{
			BusinessObject jobDec = Factory.New(typeof(UPEJobDeclaration));
			Factory.Save();
			IZForm form1 = GetNewController().ShowViewForm(jobDec);
			IZForm form2 = GetNewController().ShowEditForm(jobDec);
			try
			{
				AssertEquals(typeof(UPEAUCustomsDeclarationForm), form1.GetType());
				AssertEquals(typeof(UPEAUCustomsDeclarationForm), form2.GetType());
			}
			finally
			{
				CloseAndDisposeForm(form1);
				CloseAndDisposeForm(form2);
			}
		}

		public void TestEditForm_IfRecordCannotBeDisplayed()
		{
			BusinessObject jobDec = Factory.New(typeof(UPEJobDeclaration));
			AssertNoExceptionThrown("No NullReferenceException should be thrown", () => GetNewController().ShowEditForm(jobDec));
		}

		void CloseAndDisposeForm(IZForm form)
		{
			if (form != null)
			{
				((ZForm)form).Close();
				form.Dispose();
			}
		}

		#region Implementation
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		UPEJobDeclarationControllerForTest GetNewController()
		{
			return new UPEJobDeclarationControllerForTest();
		}

		class UPEJobDeclarationControllerForTest : UPEJobDeclarationController
		{
			public new IZForm GetForm(IBusiness businessEntity)
			{
				return base.GetForm(businessEntity);
			}
		}
		#endregion
	}
}
