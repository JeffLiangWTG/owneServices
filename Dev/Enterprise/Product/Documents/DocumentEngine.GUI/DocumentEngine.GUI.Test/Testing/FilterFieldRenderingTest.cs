using System;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.GUI.RuntimeOptions;
using Enterprise.DocumentEngine.RuntimeOptions;

namespace Enterprise.DocumentEngine.GUI.Testing
{
	sealed class FilterFieldRenderingTest : TestCaseWithFactory
	{
		public void TestRenderingForAllFilterFields()
		{
			Assembly assembly = Assembly.GetAssembly(typeof(FilterField));

			foreach (Type type in assembly.GetTypes())
			{
				if (type.IsSubclassOf(typeof(FilterField)) && !type.IsAbstract && type.IsPublic)
				{
					FilterField filterField = (FilterField)System.Activator.CreateInstance(type, new object[] { Factory });
					TestLabelsUseCompatibleRendering(filterField);
				}
			}
		}

		void TestLabelsUseCompatibleRendering(FilterField filterField)
		{
			if (filterField.SuggestedUserControlType != FilterFieldSuggestedUserControlType.None)
			{
				using (var userControl = (Control)RuntimeOptionUserControlFactory.New(filterField.SuggestedUserControlType))
				{
					TestLabelsUseCompatibleRendering(userControl);
				}
			}

			Assert(true);
		}

		void TestLabelsUseCompatibleRendering(Control control)
		{
			foreach (Control child in control.Controls)
			{
				Label label = child as Label;
				if (label != null && label.Name == "FieldLabel")
				{
					AssertEquals(
						"FieldLabel.UseCompatibleTextRendering should be true so that text isn't cut off. [FilterField: " + control.GetType().ToString() + "]",
						true, label.UseCompatibleTextRendering);
				}

				TestLabelsUseCompatibleRendering(child);
			}
		}
	}
}
