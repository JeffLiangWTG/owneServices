using System;
using System.Windows.Forms;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(ExceptionKeyStacktraceDepthRegistryEditor))]
	class ExceptionKeyStacktraceDepthRegistryEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new ExceptionKeyStacktraceDepthRegistryEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ExceptionKeyStacktraceDepthControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ExceptionKeyStacktraceDepthControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ExceptionKeyStacktraceDepthRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.NotCached);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new ExceptionKeyStacktraceDepthCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory)
			{ new ExceptionKeyStacktraceDepth { ExceptionType = "System.NullReferenceException", StackDepth = 2 }, new ExceptionKeyStacktraceDepth { ExceptionType = "System.ArgumentException", StackDepth = 5 }, };
			return new[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get
			{
				return RegistryItemEditor.EditorPaneAnchor.All;
			}
		}
	}
}
