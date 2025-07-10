using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(AWSPrivateCAControl))]
	public class AWSPrivateCAControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new AWSPrivateCACollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((AWSPrivateCAControl)control).ReadOnly;
		}

		public void TestShowAccessKey()
		{
			using (var form = new ZForm())
			using (var control = new AWSPrivateCAControl())
			{
				form.Controls.Add(control);
				form.Show();
				UnitTestUserNotification.Instance.ClearMessages();
				control.SystemToSystemPrivateCAGridForTest.SelectAllElements();
				control.ShowValue("AccessKey");
				AssertStartsWith("Should have error message.", "Please select one data.", UnitTestUserNotification.Instance.LastMessage.Text);

				control.SetDataBinding(InitializeCollection(), null);
				UnitTestUserNotification.Instance.ClearMessages();
				control.SystemToSystemPrivateCAGridForTest.SelectAllElements();
				control.ShowValue("AccessKey");
				AssertStartsWith("Should have no error message.", "AccessKeyTest01", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShowSecretKey()
		{
			using (var form = new ZForm())
			using (var control = new AWSPrivateCAControl())
			{
				form.Controls.Add(control);
				form.Show();
				UnitTestUserNotification.Instance.ClearMessages();
				control.SystemToSystemPrivateCAGridForTest.SelectAllElements();
				control.ShowValue("SecretKey");
				AssertStartsWith("Should have error message.", "Please select one data.", UnitTestUserNotification.Instance.LastMessage.Text);

				control.SetDataBinding(InitializeCollection(), null);
				UnitTestUserNotification.Instance.ClearMessages();
				control.SystemToSystemPrivateCAGridForTest.SelectAllElements();
				control.ShowValue("SecretKey");
				AssertStartsWith("Should have no error message.", "SecretKeyTest01", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		AWSPrivateCACollection InitializeCollection()
		{
			var collection = new AWSPrivateCACollection();
			var awsPrivateCaArn = collection.AddNew();
			awsPrivateCaArn.AccessKey = "AccessKeyTest01";
			awsPrivateCaArn.SecretKey = "SecretKeyTest01";
			awsPrivateCaArn.Arn = "Test123456";
			awsPrivateCaArn.IssuingCA = CARootCodeDescriptionList.Codes.SystemToSystemTrust;
			return collection;
		}
	}
}
