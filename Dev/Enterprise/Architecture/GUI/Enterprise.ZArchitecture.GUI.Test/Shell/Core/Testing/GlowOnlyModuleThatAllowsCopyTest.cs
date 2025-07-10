using System.Windows.Forms;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	[TestsSubclassesOf(typeof(GlowOnlyModuleThatAllowsCopy))]
	public abstract class GlowOnlyModuleThatAllowsCopyTest<T> : GlowOnlyModuleTest<T>
		where T : GlowOnlyModuleThatAllowsCopy, new()
	{
		public void TestCouldAllowUniversalCopy()
		{
			using var module = new T();
			AssertEquals(true, module.CouldAllowUniversalCopy);
		}

		public void TestNew_ShouldShowErrorMessage_AndNotShowForm()
		{
			using (var module = new T())
			using (var form = module.ShowNewForm())
			{
				Application.DoEvents();

				CombineAssertions(() =>
					{
						AssertEquals("The module is responsible for showing a message to the user if they try to create a new item, since the New button is allowed.",
							false, UnitTestUserNotification.Instance.LastMessage.WasNone);
						AssertNull("This is a glow only module, but it has the New button so that we can enable Universal Copy. But the new button must not actually create any form.", form);
					}
				);
			}
		}

		protected override bool ExpectedAllowNew => true;
	}
}
