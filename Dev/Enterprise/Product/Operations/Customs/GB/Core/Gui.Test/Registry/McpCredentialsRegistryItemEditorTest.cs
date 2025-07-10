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
	[TestedType(typeof(CredentialsRegistryItemEditor))]
	public class CredentialsRegistryItemEditorTest : Enterprise.Registry.GUI.Testing.RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new CredentialsRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((CredentialsControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CredentialsControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CredentialsSettingCollectionRegistryItem("", (NoResString)"", "", "", RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			CredentialsSettingCollection collection = new CredentialsSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
			string fey = "FEY";
			string dan = "DAN";
			SetupValidMcpBadges(fey, dan);  // Need this otherwise when we run other tests for MCP the validation will fire an tell us that this is not a valid MCP badge

			CredentialsSetting credentials1 = collection.AddNew();
			credentials1.BadgeCode = fey;
			credentials1.Username = "CAW8";
			credentials1.Password = "password";
			credentials1.Printer = "PRT1";
			credentials1.Company = "CAW";

			CredentialsSetting credentials2 = collection.AddNew();
			credentials2.BadgeCode = dan;
			credentials2.Username = "DAN8";
			credentials2.Password = "secret";
			credentials2.Printer = "PRN1";
			credentials2.Company = "FEK";

			return new object[] { collection };
		}

		void SetupValidMcpBadges(params string[] codes)
		{
			BadgeCodeSettingCollection coll = new BadgeCodeSettingCollection();

			foreach (string oneBadge in codes)
			{
				BadgeCodeSetting badge = new BadgeCodeSetting(Factory);
				badge.BadgeCode = oneBadge;
				badge.CSPCode = GatewayList.Codes.MCP_CUSDECOnly;
				coll.Add(badge);
			}
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, coll);
		}

		protected override void AssertSetAndGetValuesEqual(object setValue, object getValue)
		{
			CredentialsSettingCollection collection1 = (CredentialsSettingCollection)setValue;
			CredentialsSettingCollection collection2 = (CredentialsSettingCollection)getValue;

			AssertEquals("GetValueFromEditorPane().Count", collection1.Count, collection2.Count);

			for (int i = 0; i < collection1.Count; ++i)
			{
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].Username", i.ToString()), collection1[i].Username, collection2[i].Username);
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].Password", i.ToString()), collection1[i].Password, collection2[i].Password);
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].Printer", i.ToString()), collection1[i].Printer, collection2[i].Printer);
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].Company", i.ToString()), collection1[i].Company, collection2[i].Company);
			}
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
