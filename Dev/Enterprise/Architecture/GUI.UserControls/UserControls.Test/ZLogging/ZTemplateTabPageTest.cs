using System;
using System.ComponentModel;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZTemplateTabPageTest : ZTabPageControlTest
	{
		public void TestText_NotSettableInDesigner()
		{
			PropertyDescriptor textProperty = TypeDescriptor.GetProperties(TestTabPage)["Text"];
			AssertEquals(false, textProperty.IsBrowsable);
			AssertEquals(DesignerSerializationVisibility.Hidden, textProperty.SerializationVisibility);
		}

		public void TestImageIndex_NotSettableInDesigner()
		{
			PropertyDescriptor textProperty = TypeDescriptor.GetProperties(TestTabPage)["ImageIndex"];
			AssertEquals(false, textProperty.IsBrowsable);
			AssertEquals(DesignerSerializationVisibility.Hidden, textProperty.SerializationVisibility);
		}

		public void TestIsAutoSized()
		{
			AssertEquals(true, TestTabPage.IsAutoSized);
		}

		[ExpectException(typeof(InvalidOperationException))]
		public void TestCannotAddToNonTemplateTabControl()
		{
			using (ZTabControl tabControl = new ZTabControl())
			{
				tabControl.TabPages.Add(TestTabPage);
			}
		}

		#region Test Classes

		class TestTemplateTabPage : ZTemplateTabPage
		{
			public new bool IsAutoSized
			{
				get { return base.IsAutoSized; }
			}
		}

		#endregion

		#region Implementation

		new TestTemplateTabPage TestTabPage
		{
			get { return (TestTemplateTabPage)base.TestTabPage; }
		}

		protected override ZTabPage NewTabPage()
		{
			return new TestTemplateTabPage();
		}

		protected override ZTabControl NewTabControl()
		{
			return new ZTemplateTabControl();
		}

		#endregion
	}
}
