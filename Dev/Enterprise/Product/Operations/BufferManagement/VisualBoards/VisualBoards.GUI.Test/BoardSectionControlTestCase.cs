using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Test;
using NUnit.Framework;

namespace Enterprise.VisualBoards.GUI.Test
{
	[TestsSubclassesOf(typeof(IBoardSectionControl))]
	public abstract class BoardSectionControlTestCase<T> : TestCaseWithFactory
		where T : Control, IBoardSectionControl, IDisposable
	{
		public void TestRefreshCompleted_ShouldBeCalledAfterRefresh()
		{
			var refreshCompleted = false;

			using (VisualBoardsTestCase.DisableAsyncBehaviour())
			using (var form = new Form()) // Just making sure the control has Handles.
			using (var control = GetControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.RefreshCompleted += (s, e) => refreshCompleted = true;

				control.Refresh(BoardRefreshEventArgs.Empty);
				AssertEquals("After calling IBoardSectionControl.Refresh, the RefreshCompleted event should have been fired.", true, refreshCompleted);
			}
		}

		protected abstract T GetControl();
	}
}
