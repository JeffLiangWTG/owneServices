using System.Collections;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public abstract class ZFormBindingContextTester : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestBindingContextIntegrity()
		{
			using (var testForm = GetBoundForm())
			{
				AssertEquals("Form contains KBindingContext", typeof(ZBindingContext), testForm.BindingContext.GetType());
				AssertBindingContext((ZBindingContext)testForm.BindingContext, testForm.Controls);
			}
		}

		protected abstract ZForm GetBoundForm();

		protected void AssertBindingContext(ZBindingContext bindingContext, ICollection controls)
		{
			foreach (Control control in controls)
			{
				if ((control is IDataBoundControl) && !IsExcludedControl(control))
				{
					AssertEquals(control.Name + " should have the same binding context as parent form.", bindingContext, control.BindingContext);
				}

				AssertBindingContext(bindingContext, control.Controls);
			}
		}

		protected virtual bool IsExcludedControl(Control control)
		{
#if !WINZOR
			return (control.Parent.Parent != null) && (control.Parent is ZRichTextBoxToolBar);
#else
			return false;
#endif
		}
	}
}
