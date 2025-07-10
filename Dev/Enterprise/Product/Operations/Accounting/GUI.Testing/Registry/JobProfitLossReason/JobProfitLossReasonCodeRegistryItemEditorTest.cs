using System;
using System.Windows.Forms;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(JobProfitLossReasonCodeRegistryItemEditor))]
	public class JobProfitLossReasonCodeRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new JobProfitLossReasonCodeRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((JobProfitLossReasonCodeControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(JobProfitLossReasonCodeControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new JobProfitLossReasonCodeRegistryItem("", null, null, null, RegistryStorageFlags.System, new JobProfitLossReasonCodeCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			JobProfitLossReasonCodeCollection collection = new JobProfitLossReasonCodeCollection();
			JobProfitLossReasonCode copy = collection.AddNew();

			copy.Code = "TST";
			copy.Description = (NoResString)"Test code";

			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
