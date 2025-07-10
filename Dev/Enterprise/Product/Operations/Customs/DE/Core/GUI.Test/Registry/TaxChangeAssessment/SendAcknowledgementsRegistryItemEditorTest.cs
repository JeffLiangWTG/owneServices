using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Registry;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Registry.Testing
{
	[TestedType(typeof(SendAcknowledgementsRegistryItemEditor))]
	class SendAcknowledgementsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor() => new SendAcknowledgementsRegistryItemEditor(new SendAcknowledgementsRegistryDataType(), new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), new BusinessObjectFactory());

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			var control = (SendAcknowledgementsRegistryItemControl)editorPane;
			var acknowledgementsGrid = (ZArchitecture.ZGrid)control.Controls.Find("AcknowledgementsGrid", true).Single();
			return !acknowledgementsGrid.ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType() => typeof(SendAcknowledgementsRegistryItemControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new SendAcknowledgementsRegistryItem("", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System);

		protected override object[] GetValidRegistryValues() => new object[]
		{
			new SendAcknowledgementsRegistryCollection { new SendAcknowledgementsRegistry(GetFallbackLevel(), Factory) { EBSCode = "0001", SendGroupPK = Core.Constants.Groups.PostMastersGroupPK } }
		};

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;

		FallbackLevel GetFallbackLevel()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			company.GC_Code = "DE1";
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "DE1OH";

			var br1 = company.Branches.AddNew();
			br1.GB_OH_OrgProxy = orgHeader.PK;
			br1.OrgProxy.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, "0001", Core.Constants.CountryCodes.Germany);
			Factory.Save();

			return new FallbackLevel(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
		}
	}
}
