using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Environment.Semaphores.Testing;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZChildFormWithoutTransaction : TestCaseWithFactory
	{
		public void TestEditingChildFormShouldNotShowAccessDialog()
		{
			var bizO = Factory.New<DummyBusinessObject>();
			Factory.Save();

			using (var provider = new SemaphoreProviderWithMockUserIdForTesting(Guid.NewGuid()))
			using (var semaphoreHandle = ((ISemaphoreProvider)provider).CreateSemaphoreHandle(new PendingUserActionSemaphore(bizO.PK.ToString())))
			using (var form = new ZChildForm(bizO))
			{
				Application.DoEvents();
			}

			Assert("Should not show semaphore dialog if form shown is a child form", UnitTestUserNotification.Instance.LastMessage.WasNone);
		}
	}
}
