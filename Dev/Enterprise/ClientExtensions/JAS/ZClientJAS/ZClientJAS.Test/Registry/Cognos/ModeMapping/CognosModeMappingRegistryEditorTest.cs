using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.JAS.Registry.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Registry.GUI.Testing
{
	[TestedType(typeof(CognosModeMappingRegistryEditor))]
	class CognosModeMappingRegistryEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new CognosModeMappingRegistryEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CognosModeMappingControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CognosModeMappingControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CognosModeMappingRegistryItem("");
		}

		protected override object[] GetValidRegistryValues()
		{
			CognosModeMapping[] result = new CognosModeMapping[1];
			result[0] = new CognosModeMapping();
			result[0].SelectedMode = "AI";
			result[0].MapDepartments(DeptCollection[0], DeptCollection[1]);
			result[0].SelectedMode = "AE";
			result[0].MapDepartments(DeptCollection[2]);
			result[0].SelectedMode = "ME";
			result[0].MapDepartments(DeptCollection[3], DeptCollection[4]);
			return result;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get
			{
				return RegistryItemEditor.EditorPaneAnchor.TopLeft;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			DeptCollection = LoadDepartmentCollection();
		}

		GlbDepartmentCollection LoadDepartmentCollection()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			GlbDepartmentCollection collection = new GlbDepartmentCollection(factory);
			ZQuery query = new ZQuery();
			query.MaximumRows = 5;
			collection.AdditionalFilter = query;
			AssertEquals("Pre-condition. There should be at least 5 Departments in test database, add manually if this fails", 5, collection.Count);
			return collection;
		}

		GlbDepartmentCollection DeptCollection;
	}
}
