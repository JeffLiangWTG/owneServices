using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.VisualBoards.GUI.Test
{
	class DragDropHelperTest : TestCase
	{
		public void TestChildSubscriptionsEnded()
		{
			using (var panel = new ZPanel())
			using (var label = new TrackerLabel())
			{
				panel.Controls.Add(label);

				var handler = DragDropHelper.AddDragDropSupport(panel);
				AssertEquals(3, label.MouseEventSubscriptionCount);

				label.Dispose();

				AssertEquals(0, label.MouseEventSubscriptionCount);
			}
		}

		class TrackerLabel : ZLabel
		{
			public int MouseEventSubscriptionCount
			{
				get { return GetEventHandlers(Events).Where(e => e != null).Count(e => e.ToString().Contains("Mouse")); }
			}

			IEnumerable<object> GetEventHandlers(object events)
			{
				var mt = typeof(EventHandlerList);

				var result = mt.GetField("head", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(events);

				var innert = result.GetType();
				while (result != null)
				{
					yield return innert.GetField("handler", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(result);

					result = innert.GetField("next", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(result);
				}
			}
		}

		public void TestAddDragDropSupportAndDispose_ShouldNotLeakControl()
		{
			var controlRef = CreateAndDisposeControlWithDragAndDropSupport();

			GC.Collect(); // Test case - go away
			GC.WaitForFullGCComplete();
			AssertNull(controlRef.Target);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI005:WeakReferenceTargetRaceConditionRule")]
		WeakReference CreateAndDisposeControlWithDragAndDropSupport()
		{
			var controlRef = AddDragDropSupportAndReturnControlReference();
			GC.Collect(); // Test case - go away
			GC.WaitForFullGCComplete();
			AssertNotNull(controlRef.Target);

			((IDisposable)controlRef.Target).Dispose();

			return controlRef;
		}

		static WeakReference AddDragDropSupportAndReturnControlReference()
		{
			var panel = new ZPanel();
			AssertNotNull(DragDropHelper.AddDragDropSupport(panel));
			return new WeakReference(panel);
		}

		public void TestAddDragDropSupportForUserInteractiveControl_ShouldNotActuallyAdd()
		{
			var controlRef = AddDragDropSupportAndReturnControlReference_UserInteractive();
			GC.Collect(); // Test case - go away
			GC.WaitForFullGCComplete();
			AssertNull(controlRef.Target);
		}

		static WeakReference AddDragDropSupportAndReturnControlReference_UserInteractive()
		{
			var button = new Button();
			AssertNull(DragDropHelper.AddDragDropSupport(button));
			return new WeakReference(button);
		}

		public void TestIsUserInteractiveControl()
		{
			using (var button = new ZButton())
			using (var checkbox = new ZCheckBox())
			using (var dropEdit = new ZDropEdit())
			using (var pictureBox = new PictureBox())
			using (var panel = new ZPanel())
			{
				Assert(DragDropHelper.IsUserInteractiveControl(button));
				Assert(DragDropHelper.IsUserInteractiveControl(checkbox));
				Assert(DragDropHelper.IsUserInteractiveControl(dropEdit));
				Assert(DragDropHelper.IsUserInteractiveControl(pictureBox));
				Assert(!DragDropHelper.IsUserInteractiveControl(panel));
			}
		}

		public void TestDragDropHelper_ThreadUnsafe()
		{
			var panels = new[] { new ZPanel(), new ZPanel(), new ZPanel(), new ZPanel(), new ZPanel(), new ZPanel(), new ZPanel(), new ZPanel(), new ZPanel(), new ZPanel() };
			var phase1 = new EventWaitHandle(false, EventResetMode.ManualReset);
			var phase2 = new EventWaitHandle(false, EventResetMode.ManualReset);
			var threadCount = 0L;
			Exception threadsException = null;

			foreach (var panel in panels)
			{
				var thread = new Thread(new ThreadStart(delegate
				{
					try
					{
						Interlocked.Increment(ref threadCount);

						phase1.WaitOne();
						DragDropHelper.AddDragDropSupport(panel);
						phase2.WaitOne();
						panel.Dispose();
					}
					catch (Exception ex)
					{
						threadsException = ex;
					}
					finally
					{
						Interlocked.Decrement(ref threadCount);
					}
				}))
				{ Name = "HelperThread" };
				thread.Start();
			}

			while (Interlocked.Read(ref threadCount) < panels.Length)
			{
				Thread.Sleep(100);
			}

			phase1.Set();
			Thread.Sleep(TimeSpan.FromSeconds(1));
			AssertNull("DragDropHelper.AddDragDropSupport should not throw exception", threadsException);

			phase2.Set();
			while (Interlocked.Read(ref threadCount) > 0)
			{
				Thread.Sleep(100);
			}
			AssertNull("Disposing panel should not throw exception", threadsException);
		}

		public void TestIsAnyPartOfControlVisibleWithinParent()
		{
			using (var parentPanel = new ZPanel { Size = new Size(50, 50) })
			using (var childPanel = new ZPanel { Size = new Size(10, 10) })
			{
				Assert(!DragDropHelper.IsAnyPartOfControlVisibleWithinParent(childPanel));

				parentPanel.Controls.Add(childPanel);
				childPanel.Location = new Point(0, 0);
				Assert(DragDropHelper.IsAnyPartOfControlVisibleWithinParent(childPanel));

				childPanel.Location = new Point(-5, 0);
				Assert(DragDropHelper.IsAnyPartOfControlVisibleWithinParent(childPanel));
				Assert(!DragDropHelper.IsAnyPartOfControlVisibleWithinParent(childPanel, 6));

				childPanel.Location = new Point(0, -5);
				Assert(DragDropHelper.IsAnyPartOfControlVisibleWithinParent(childPanel));
				Assert(!DragDropHelper.IsAnyPartOfControlVisibleWithinParent(childPanel, 6));

				childPanel.Location = new Point(45, 0);
				Assert(DragDropHelper.IsAnyPartOfControlVisibleWithinParent(childPanel));
				Assert(!DragDropHelper.IsAnyPartOfControlVisibleWithinParent(childPanel, 6));

				childPanel.Location = new Point(0, 45);
				Assert(DragDropHelper.IsAnyPartOfControlVisibleWithinParent(childPanel));
				Assert(!DragDropHelper.IsAnyPartOfControlVisibleWithinParent(childPanel, 6));
			}
		}
	}
}
