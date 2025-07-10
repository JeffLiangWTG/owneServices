using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(NGTRegistryItemEditor))]
	sealed class NGTRegistryItemEditorTest : RegistryItemEditorTestCase
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
				FallbackTreeView.SelectedNode = FallbackTreeView.Nodes[0].Nodes[0];
			}
		}

		protected override RegistryItemEditorTestCase.RegistryFormForTest GetNewRegistryFormForTest(IRegistryItem registryItem)
		{
			return new RegistryFormForTest(registryItem);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new NGTRegistryItem("", null, null, null);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new NGTRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(NGTRegistryItemControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			var ngt = new NGT
			{
				Password = "1"
			};
			return new object[] { ngt };
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((NGTRegistryItemControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;

		public void TestSystemCreateTimeShouldBeChangeAfterUpdate()
		{
			var setting = new NGT();
			setting.Password = "pwd";

			var regItem = new RegistryItemTagForTest(AUCustomsDataRegistry.Instance.NEXDOCSGroupToken)
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
			var interchange = Factory.LoadTop1<Messaging.Business.EDIInterchange>(zQuery);

			var systemCreateTimeUtc = interchange.EI_SystemCreateTimeUtc;
			interchange.Delete();

			setting.Password = "pwd77";

			regItem.NewValue = setting;
			regItem.IsChanged = true;
			regItem.HasValue = true;

			regItem.SaveAllValues();

			interchange = Factory.LoadTop1<Messaging.Business.EDIInterchange>(zQuery);
			AssertNotEquals("EI_SystemCreateTimeUtc", systemCreateTimeUtc, interchange.EI_SystemCreateTimeUtc);
		}
	}
}
