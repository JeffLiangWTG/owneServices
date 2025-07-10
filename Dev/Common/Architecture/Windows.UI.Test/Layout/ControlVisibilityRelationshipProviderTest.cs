using System.ComponentModel;
using System.Windows.Forms;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Layout.Testing
{
	sealed class ControlVisibilityRelationshipProviderTest : TestCase
	{
		public void TestExtenderProviderImpl()
		{
			AssertEquals("Implements IExtenderProvider", true, VisibilityRelationshipProvider is IExtenderProvider);
			AssertEquals("Extends controls", true, ((IExtenderProvider)VisibilityRelationshipProvider).CanExtend(Control));
			AssertEquals("Doesn't extend other types", false, ((IExtenderProvider)VisibilityRelationshipProvider).CanExtend(new object()));

			ProvidePropertyAttribute[] attrs = (ProvidePropertyAttribute[])typeof(ControlVisibilityRelationshipProvider).GetCustomAttributes(typeof(ProvidePropertyAttribute), false);
			AssertEquals("Provides 1 property to controls", 1, attrs.Length);
			AssertEquals("PropertyName", "VisibleDependentOn", attrs[0].PropertyName);
			AssertEquals("ReceiverType", typeof(Control).AssemblyQualifiedName, attrs[0].ReceiverTypeName);
		}

		public void TestVisibleDependentOn()
		{
			VisibilityRelationshipProvider.SetDependency(Control, VisibleDependentOn);
			VisibleDependentOn.Visible = false;
			AssertEquals("Visibility is taken from VisibleDependentOn control", false, Control.Visible);
			VisibleDependentOn.Visible = true;
			AssertEquals("Visibility is taken from VisibleDependentOn control", true, Control.Visible);
			VisibleDependentOn.Visible = false;
			AssertEquals("Visibility is taken from VisibleDependentOn control", false, Control.Visible);

			VisibilityRelationshipProvider.ClearDependency(Control);
			VisibleDependentOn.Visible = true;
			AssertEquals("Visibility taken from nowhere when removed", false, Control.Visible);
		}

		public void TestControlCanBeHiddenWhenItsIndependentAgain()
		{
			VisibilityRelationshipProvider.SetDependency(Control, VisibleDependentOn);
			VisibilityRelationshipProvider.ClearDependency(control);

			Control.Visible = false;

			CombineAssertions(() =>
			{
				Assert("VisibleDependentOn should be visible", VisibleDependentOn.Visible);
				Assert("Control should not be visible", !Control.Visible);
			});
		}

		public void TestVisibiltyOfControlGetChangedWhenSettingTheRelation()
		{
			Control.Visible = true;
			VisibleDependentOn.Visible = false;

			VisibilityRelationshipProvider.SetDependency(Control, VisibleDependentOn);
			Assert(!control.Visible);

			VisibilityRelationshipProvider.ClearDependency(Control);
			Control.Visible = false;
			VisibleDependentOn.Visible = true;

			VisibilityRelationshipProvider.SetDependency(Control, VisibleDependentOn);
			Assert(control.Visible);
		}

		#region Implementation

		ControlVisibilityRelationshipProvider VisibilityRelationshipProvider
		{
			get { return visibilityRelationshipProvider ?? (visibilityRelationshipProvider = new ControlVisibilityRelationshipProvider()); }
		}
		ControlVisibilityRelationshipProvider visibilityRelationshipProvider;

		TextBox Control
		{
			get { return control ?? (control = new TextBox()); }
		}
		TextBox control;

		TextBox VisibleDependentOn
		{
			get { return visibleDependentOn ?? (visibleDependentOn = new TextBox()); }
		}
		TextBox visibleDependentOn;

		#endregion
	}
}
