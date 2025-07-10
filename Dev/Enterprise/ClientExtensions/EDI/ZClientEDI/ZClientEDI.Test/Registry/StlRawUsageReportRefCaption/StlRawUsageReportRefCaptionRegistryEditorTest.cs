using System;
using System.Windows.Forms;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(StlRawUsageReportRefCaptionRegistryEditor))]
	public class StlRawUsageReportRefCaptionRegistryEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new StlRawUsageReportRefCaptionRegistryItem("StlRawUsageReportRefCaption", (NoResString)"Dummy Category", (NoResString)"Dummy Caption", null);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new StlRawUsageReportRefCaptionRegistryEditor(new StlRawUsageReportRefCaptionDataType(), null, Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(StlRawUsageReportRefCaptionControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			var items = new StlRawUsageReportRefCaptionCollection[1];
			items[0] = new StlRawUsageReportRefCaptionCollection();
			var item = items[0].AddNew();
			item.UsageCode = "USR";
			item.UsageDescription = "Staff";
			item.Ref1Caption = "Staff Code";
			item.Ref2Caption = "Staff Name";
			return items;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get
			{
				return RegistryItemEditor.EditorPaneAnchor.All;
			}
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((StlRawUsageReportRefCaptionControl)editorPane).ReadOnly;
		}
		#endregion Implementation
	}
}
