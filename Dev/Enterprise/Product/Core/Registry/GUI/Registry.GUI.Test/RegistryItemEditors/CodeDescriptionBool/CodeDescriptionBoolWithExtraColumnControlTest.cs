using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(CodeDescriptionBoolWithExtraColumnControl))]
	sealed class CodeDescriptionBoolWithExtraColumnControlTest : RegistryZUserControlTestCase
	{
		public void TestSetupExtraColumn()
		{
			using (var control = GetNewControl() as CodeDescriptionBoolWithExtraColumnControl)
			{
				var collection = new CodeDescriptionBoolDisallowNewCodeReadOnlyWithExtraStringCollection();

				using var form = new ZForm();
				form.Controls.Add(control);
				form.Show();

				control.SetDataBinding(collection, null);

				var extraColumn = control.CodeDescriptionBoolGrid.Columns["String1"];
				AssertNotNull(extraColumn);
				AssertEquals("ExtraColumn.Visible", true, extraColumn.IsVisible);
			}
		}

		#region Implementation

		protected override RegistryZUserControl GetNewControl()
		{
			return new CodeDescriptionBoolWithExtraColumnControl(false);
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new CodeDescriptionBoolDisallowNewCodeReadOnlyWithExtraStringCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((CodeDescriptionBoolWithExtraColumnControl)control).CodeDescriptionBoolGrid.ReadOnly;
		}
		#endregion
	}
}
