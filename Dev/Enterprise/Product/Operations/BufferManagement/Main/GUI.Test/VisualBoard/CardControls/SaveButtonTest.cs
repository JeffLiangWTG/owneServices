using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	class SaveButtonTest : BMSTestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestClick()
		{
			var parent = new Mock<ITaskCardComponentParent>(MockBehavior.Strict);

			parent.Setup(m => m.Save(It.IsAny<object>()));
			using (var control = new SaveButton(parent.Object))
			{
				control.PerformClick();
			}
		}
	}
}
