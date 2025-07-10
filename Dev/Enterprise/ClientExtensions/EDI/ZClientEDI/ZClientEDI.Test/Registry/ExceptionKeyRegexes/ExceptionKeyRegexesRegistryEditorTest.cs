using System;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(ExceptionKeyRegexesRegistryEditor))]
	public class ExceptionKeyRegexesRegistryEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation
		protected override RegistryItemEditor GetEditor()
		{
			return new ExceptionKeyRegexesRegistryEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((ExceptionKeyRegexesControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ExceptionKeyRegexesControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ExceptionKeyRegexesRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.NotCached);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new ExceptionKeyRegexCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
			collection.Add(new ExceptionKeyRegex()
			{ Regex = "TestRegex1", Description = "TestDescription1" });
			collection.Add(new ExceptionKeyRegex()
			{ Regex = "TestRegex2", Description = "TestDescription2" });
			collection.Add(new ExceptionKeyRegex()
			{ Regex = "TestRegex3", Description = "TestDescription3" });
			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get
			{
				return RegistryItemEditor.EditorPaneAnchor.All;
			}
		}
		#endregion
	}
}
