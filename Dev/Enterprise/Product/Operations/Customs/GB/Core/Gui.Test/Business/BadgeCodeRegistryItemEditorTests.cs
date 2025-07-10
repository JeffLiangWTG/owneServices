using System;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.Registry.Testing
{
	[TestedType(typeof(BadgeCodeRegistryItemEditor))]
	public class BadgeCodeRegistryItemEditorTest : Enterprise.Registry.GUI.Testing.RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new BadgeCodeRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((BadgeCodeControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(BadgeCodeControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new BadgeCodeSettingCollectionRegistryItem("", (NoResString)"", "", "", RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			BadgeCodeSettingCollection collection = new BadgeCodeSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);

			BadgeCodeSetting badgeCode1 = collection.AddNew();
			badgeCode1.BadgeCode = "ABC";
			badgeCode1.RL_PortCode = "GBFXT";
			badgeCode1.CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW;

			BadgeCodeSetting badgeCode2 = collection.AddNew();
			badgeCode1.BadgeCode = "CEF";
			badgeCode1.RL_PortCode = "GBHEA";
			badgeCode1.CSPCode = GatewayList.Codes.MCP_CUSDECOnly;

			return new object[] { collection };
		}

		protected override void AssertSetAndGetValuesEqual(object setValue, object getValue)
		{
			BadgeCodeSettingCollection collection1 = (BadgeCodeSettingCollection)setValue;
			BadgeCodeSettingCollection collection2 = (BadgeCodeSettingCollection)getValue;

			AssertEquals("GetValueFromEditorPane().Count", collection1.Count, collection2.Count);

			for (int i = 0; i < collection1.Count; ++i)
			{
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].BadgeCode", i.ToString()), collection1[i].BadgeCode, collection2[i].BadgeCode);
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].RL_PortCode", i.ToString()), collection1[i].RL_PortCode, collection2[i].RL_PortCode);
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].CSPCode", i.ToString()), collection1[i].CSPCode, collection2[i].CSPCode);
			}
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
