using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.Registry.GUI.Testing
{
	[TestsSubclassesOf(typeof(RegistryZUserControl))]
	public abstract class RegistryZUserControlTestCase : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestExpectedControlType()
		{
			using (RegistryZUserControl control = GetNewControl())
			{
				AssertEquals("Control.GetType()", ExpectedControlType, control.GetType());
			}
		}

		[RequiresSTA]
		public void TestReadOnly()
		{
			using (ZForm form = new ZForm())
			using (RegistryZUserControl control = GetNewControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.ReadOnly = true;
				IBusiness businessEntity = GetNewBusinessEntity();
				control.SetDataBinding(businessEntity, null);
				AssertEquals("BusinessEntity.ReadOnly", true, IsControlOrBusinessEntityReadOnly(control, businessEntity));
				AssertAdditionalObjectsAreReadOnly(control, businessEntity, true);

				control.ReadOnly = false;
				AssertEquals("ReadOnly", false, control.ReadOnly);
				AssertEquals("BusinessEntity.ReadOnly", false, IsControlOrBusinessEntityReadOnly(control, businessEntity));
				AssertAdditionalObjectsAreReadOnly(control, businessEntity, false);

				control.ReadOnly = true;
				AssertEquals("ReadOnly", true, control.ReadOnly);
				AssertEquals("BusinessEntity.ReadOnly", true, IsControlOrBusinessEntityReadOnly(control, businessEntity));
				AssertAdditionalObjectsAreReadOnly(control, businessEntity, true);
			}
		}

		#region Implementation

		public override Type FormToBashType
		{
			get { return typeof(ZEmptyFormForBasherTest); }
		}

		protected override Form GetFormToBashCore()
		{
			ZEmptyFormForBasherTest result = new ZEmptyFormForBasherTest();

			result.MinimumSize = ControlDpiScalingHelper.NewScaledSize(1024, 600);
			result.Size = ControlDpiScalingHelper.NewScaledSize(1024, 600);
			result.CaptionRenderingEnabled = true;

			RegistryZUserControl control = GetNewControl();
			result.Controls.Add(control);

			IBusiness businessEntity = GetNewBusinessEntity();
			control.SetDataBinding(businessEntity, "");

			return result;
		}

		protected override void BashControl(Control controlToBash)
		{
			base.BashControl(controlToBash);
			BashForDescriptionColumn(controlToBash);
		}

		protected virtual void BashForDescriptionColumn(Control controlToBash)
		{
			if (controlToBash is ZGrid)
			{
				Assert("Bind to EnglishDescription, not Description", !(((ZGrid)controlToBash).Columns.Contains("Description") && ((ZGrid)controlToBash).ElementTypeFromCollection != null && ((ZGrid)controlToBash).ElementTypeFromCollection.GetProperty("EnglishDescription") != null));
			}
		}

		protected virtual void AssertAdditionalObjectsAreReadOnly(RegistryZUserControl control, IBusiness businessEntity, bool readOnly)
		{
		}

		protected virtual RegistryZUserControl GetNewControl()
		{
			return (RegistryZUserControl)Activator.CreateInstance(ExpectedControlType, null);
		}

		internal Type ExpectedControlType => TestedTypeHelper.GetTestedType(GetType());

		protected abstract IBusiness GetNewBusinessEntity();
		protected abstract bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity);

		#endregion
	}
}
