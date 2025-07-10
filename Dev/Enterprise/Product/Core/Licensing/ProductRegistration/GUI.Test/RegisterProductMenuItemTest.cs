namespace Enterprise.ProductRegistration.GUI.Test
{
	using System.Linq;
	using CargoWise.Application;
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Integration.Licensing;
	using Enterprise.MasterFiles.Integration;
	using Enterprise.ZArchitecture.Environment;
	using Enterprise.ZArchitecture.GUI;
	using Moq;

	public class RegisterProductMenuItemTest : TestCaseWithFactory
	{
		public void TestClick()
		{
			var rego = new Mock<IProductRegistration>();
			rego.Setup(x => x.LocalVerify()).Returns(ProductRegistrationVerifyResult.Unregistered);
			ObjectFactory.Substitute<IProductRegistration>(rego.Object);

			using (var form = new ZForm())
			{
				var toolStripMenuItem = new ZToolStripMenuItem();
				RegisterProductMenuItem.AddRegisterProductMenuItem(toolStripMenuItem, null);

				var found = false;
				foreach (var menuItem in toolStripMenuItem.DropDownItems.OfType<ZToolStripMenuItem>())
				{
					if (menuItem.Text.Contains("Register Product"))
					{
						found = true;
						AssertNull(ZFormModaliser.LastFormShownDialogForTest);

						menuItem.PerformClick();
						AssertNotNull(ZFormModaliser.LastFormShownDialogForTest);
						break;
					}
				}

				toolStripMenuItem.Dispose();
				Assert("Should have found", found);
			}
		}

		public void TestClick_AccessDenied()
		{
			var deniedStaff = Factory.New<IGlbStaff>();
			deniedStaff.GS_Code = "bob";
			deniedStaff.GS_LoginName = "bob";
			Factory.Save();

			var rego = new Mock<IProductRegistration>();
			rego.Setup(x => x.LocalVerify()).Returns(ProductRegistrationVerifyResult.Unregistered);

			var env = EnvProxy.Instance;
			using (env.SetTemporaryUserContext("bob", env.CurrentBranch.PK, env.CurrentDepartment.PK))
			using (ZForm form = new ZForm())
			using (ObjectFactory.Substitute<IProductRegistration>(rego.Object))
			{
				var toolStripMenuItem = new ZToolStripMenuItem();
				RegisterProductMenuItem.AddRegisterProductMenuItem(toolStripMenuItem, null);

				bool found = false;
				foreach (var menuItem in toolStripMenuItem.DropDownItems.OfType<ZToolStripMenuItem>())
				{
					if (menuItem.Text.Contains("Register Product"))
					{
						found = true;
						menuItem.PerformClick();
						AssertNull(ZFormModaliser.LastFormShownForTest);
						AssertNull(ZFormModaliser.LastFormShownDialogForTest);
						AssertEquals("This requires a controller or a non-operational user, such as sysadmin, or a support user", UnitTestUserNotification.Instance.LastMessage.Text);
						break;
					}
				}

				toolStripMenuItem.Dispose();
				Assert("Should have found", found);
			}
		}

		public void TestRightUsersAreAllowedToRegister()
		{
			var repository = new Moq.MockRepository(MockBehavior.Default);
			var user = repository.Create<IUser>();
			user.Setup(u => u.IsOperational).Returns(true);
			Assert("Regular, Operational users shouldnt be able to change registration", !RegisterProductMenuItem.IsAllowedToRegisterProduct(user.Object));
			
			user = repository.Create<IUser>();
			user.Setup(u => u.LoggedInWithMasterPassword).Returns(true);
			Assert("Master password users are able to change registration", RegisterProductMenuItem.IsAllowedToRegisterProduct(user.Object));

			user = repository.Create<IUser>();
			user.Setup(u => u.IsController).Returns(true);
			Assert("Controllers are able to change registration", RegisterProductMenuItem.IsAllowedToRegisterProduct(user.Object));
		}
	}
}
