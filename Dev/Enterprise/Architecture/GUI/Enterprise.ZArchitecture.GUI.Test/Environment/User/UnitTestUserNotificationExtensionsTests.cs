using System.Xml.Linq;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class UnitTestUserNotificationExtensionsTests : TestCase
	{
		public void TestWasDefaultable()
		{
			var context = new DialogDefaultContext(ZGuid.NewZGuid(), (NoResString)"Caption", ZMessageBoxButtons.OK, ZMessageBoxIcon.None);

			Globals.Message.ShowOrDefault(context, (NoResString)"Some Text");
			Assert("Should be marked as defaultable", UnitTestUserNotification.Instance.LastMessage.WasDefaultable);
			AssertEquals(context.Caption, UnitTestUserNotification.Instance.LastMessage.Caption);
			AssertEquals("Some Text", UnitTestUserNotification.Instance.LastMessage.Text);

			Globals.Message.ShowOrDefault(context, () => new KUserControl());
			Assert("Should be marked as defaultable", UnitTestUserNotification.Instance.LastMessage.WasDefaultable);
			AssertEquals(context.Caption, UnitTestUserNotification.Instance.LastMessage.Caption);

			object o = null;
			var serializer = new Mock<ISerializer<object>>();
			serializer.Setup(s => s.Serialize(o)).Returns(new XElement("Dummy"));
			serializer.Setup(s => s.Deserialize(It.IsAny<XElement>())).Returns(null);

			UnitTestUserNotification.Instance.AddDataSourceResponse(null);

			Globals.Message.ShowOrDefault(context, ref o, p => new KUserControl(), serializer.Object);
			Assert("Should be marked as defaultable", UnitTestUserNotification.Instance.LastMessage.WasDefaultable);
			AssertEquals(context.Caption, UnitTestUserNotification.Instance.LastMessage.Caption);

			Globals.Message.Show("Im not defaultable");
			Assert("Should NOT be marked as defaultable", !UnitTestUserNotification.Instance.LastMessage.WasDefaultable);
		}
	}
}
