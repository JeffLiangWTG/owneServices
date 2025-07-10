using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZToolBarTest : TestCaseWithFactory
	{
		public void TestAppearance()
		{
			AssertEquals("Value", ToolBarAppearance.Flat, ToolBar.Appearance);
			var defaultValue = (DefaultValueAttribute)TypeDescriptor.GetProperties(ToolBar)["Appearance"].Attributes[typeof(DefaultValueAttribute)];
			AssertEquals("DefaultValueAttribute", ToolBarAppearance.Flat, defaultValue.Value);
		}

		public void TestWrappable()
		{
			AssertEquals("Value", false, ToolBar.Wrappable);
			var defaultValue = (DefaultValueAttribute)TypeDescriptor.GetProperties(ToolBar)["Wrappable"].Attributes[typeof(DefaultValueAttribute)];
			AssertEquals("DefaultValueAttribute", false, defaultValue.Value);
		}

		public void TestImageListCollected()
		{
			using (var form = new ZForm())
			{
				var imageListRef = SetupImageList();

				form.Show();
				Application.DoEvents();
				form.Controls.Add(ToolBar);

				ToolBar.Dispose();

				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
				AssertEquals("ImageList collected", false, imageListRef.IsAlive);
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		WeakReference SetupImageList()
		{
			var imageList = new ImageList();
			ToolBar.ImageList = imageList;
			return new WeakReference(imageList);
		}

		ZToolBar ToolBar
		{
			get { return toolBar ?? (toolBar = new ZToolBar()); }
		}
		ZToolBar toolBar;
	}
}
