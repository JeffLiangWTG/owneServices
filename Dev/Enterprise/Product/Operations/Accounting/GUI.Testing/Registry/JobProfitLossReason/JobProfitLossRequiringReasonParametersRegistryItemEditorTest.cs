using System;
using System.Windows.Forms;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(JobProfitLossRequiringReasonParametersRegistryItemEditor))]
	public class JobProfitLossRequiringReasonParametersRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new JobProfitLossRequiringReasonParametersRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((JobProfitLossRequiringReasonParametersControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(JobProfitLossRequiringReasonParametersControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new JobProfitLossRequiringReasonParametersRegistryItem("", null, null, null, RegistryStorageFlags.System, new JobProfitLossRequiringReasonParameters());
		}

		protected override object[] GetValidRegistryValues()
		{
			JobProfitLossRequiringReasonParameters validRegistryValue = new JobProfitLossRequiringReasonParameters();

			validRegistryValue.ProfitThreshold = 1M;
			validRegistryValue.LossThreshold = 1M;
			validRegistryValue.JobStatusCollection.AddNew().Code = "WRK";

			return new object[] { validRegistryValue };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
