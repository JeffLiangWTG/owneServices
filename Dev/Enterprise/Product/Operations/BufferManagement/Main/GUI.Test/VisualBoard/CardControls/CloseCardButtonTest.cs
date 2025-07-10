using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	class CloseCardButtonTest : BMSTestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestClick()
		{
			var parent = new Mock<ITaskCardComponentParent>(MockBehavior.Strict);

			parent.Setup(m => m.Close());
			using (var control = new CloseCardButton(parent.Object))
			{
				control.PerformClick();
			}
		}
	}
}
