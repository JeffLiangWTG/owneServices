using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.GUI.Testing
{
	[TestedType(typeof(AFRReporterIDRegistryItemEditor))]
	sealed class AFRReporterIDRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		new class RegistryFormForTest : RegistryItemEditorTestCase.RegistryFormForTest
		{
			public RegistryFormForTest(IRegistryItem registryItem)
				: base(registryItem)
			{
			}

			public override void DisplayRegistryItem()
			{
				RegistriesTreeView.SelectedNode = RegistriesTreeView.Nodes[0];
				FallbackTreeView.SelectedNode = FallbackTreeView.Nodes[0];
			}
		}

		protected override RegistryItemEditorTestCase.RegistryFormForTest GetNewRegistryFormForTest(IRegistryItem registryItem)
		{
			return new RegistryFormForTest(registryItem);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new AFRReporterIDRegistryItem("", null, null, null);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new AFRReporterIDRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(AFRReporterIDControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			var filer = new AFRReporterID
			{
				ReporterID = "12345",
				Password = "E"
			};

			return new object[] { filer };
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((AFRReporterIDControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;

		public void TestSystemCreateTimeShouldBeChangeAfterUpdate()
		{
			var setting = new AFRReporterID
			{
				ReporterID = "12345",
				Password = "111"
			};

			var regItem = new RegistryItemTagForTest(Enterprise.Customs.JP.AFR.Business.JPAFRRegistry.Instance.AFRReporterIDForDocument)
			{
				CompanyPKForTest = GlbCompany.CurrentCompany.PK.ToGuid(),
				BranchPKForTest = Guid.Empty,
				DepartmentPKForTest = Guid.Empty,
				NewValue = setting,
				IsChanged = true,
				HasValue = true
			};

			regItem.SaveAllValues();

			var zQuery = new ZQuery(EDIInterchangeSchema.EI_To, "eHub");
			zQuery.OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " DESC";
			var interchange = Factory.LoadTop1<EDIInterchange>(zQuery);

			var systemCreateTimeUtc = interchange.EI_SystemCreateTimeUtc;
			interchange.Delete();

			setting.ReporterID = "12346";

			regItem.NewValue = setting;
			regItem.IsChanged = true;
			regItem.HasValue = true;

			regItem.SaveAllValues();

			interchange = Factory.LoadTop1<EDIInterchange>(zQuery);
			AssertNotEquals("EI_SystemCreateTimeUtc", systemCreateTimeUtc, interchange.EI_SystemCreateTimeUtc);
		}
	}
}
