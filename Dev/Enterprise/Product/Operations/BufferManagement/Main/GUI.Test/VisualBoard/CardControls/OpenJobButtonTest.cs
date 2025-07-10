using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	class OpenJobButtonTest : BMSTestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestClick()
		{
			var parent = new Mock<ITaskCardComponentParent>(MockBehavior.Strict);

			parent.Setup(m => m.ShowParent());
			using (var control = new OpenJobButton(parent.Object))
			{
				control.PerformClick();
			}
		}
	}
}
