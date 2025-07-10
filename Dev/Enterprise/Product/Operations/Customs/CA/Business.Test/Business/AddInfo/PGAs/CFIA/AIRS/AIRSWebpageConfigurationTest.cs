using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(AIRSWebpageConfiguration))]
	sealed class AIRSWebpageConfigurationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestWebPageConfiguration()
		{
			CombineAssertions("FrenchLanguageIndicator True", () =>
			{
				CACustomsDataRegistry.Instance.FrenchLanguageIndicator.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
				var fraConfiguration = new AIRSWebpageConfiguration(Factory);
				AssertEquals("Url", "https://airs-sari.inspection.gc.ca/airs_external/francais/decisions-fra.aspx", fraConfiguration.Url);
				AssertEquals("EndUseTextID", "ctl00_ContentMain_lblEndUseText", fraConfiguration.EndUseTextID);
				AssertEquals("ExtensionTextID", "ctl00_ContentMain_lblOGDExtensionIDText", fraConfiguration.ExtensionTextID);
				AssertEquals("MiscTextID", "ctl00_ContentMain_lblMiscText", fraConfiguration.MiscTextID);
				AssertEquals("IIDTableID", "ctl00_ContentMain_pnlIIDTable", fraConfiguration.IIDTableID);
				Assert("PostData Tested in AIRSHtmlHelperTest", true);
				AssertEquals("IIDWebPageTitle", "Système automatisé de référence à l'importation: Déclaration intégrée des importations", fraConfiguration.IIDWebPageTitle);
				AssertEquals("MaterializedGridTitle", "LPCA matérialisés (Image requise)", fraConfiguration.MaterializedGridTitle);
				AssertEquals("DeMaterializedGridTitle", "LPCA dématérialisés (Image non requise)", fraConfiguration.DeMaterializedGridTitle);
				AssertEquals("CodeText", "CODE", fraConfiguration.CodeText);
				AssertEquals("AIRSRegistrationGridTitle", "Enregistrement SARI", fraConfiguration.AIRSRegistrationGridTitle);
				AssertEquals("RegistrationText", "ENREGISTREMENT", fraConfiguration.RegistrationText);
				AssertEquals("OrText", "OU", fraConfiguration.OrText);
				AssertEquals("LPCOCodeAttributeKey", "headers", fraConfiguration.LPCOCodeAttributeKey);
				AssertEquals("LPCOCodeAttributeCode", "col_1_code", fraConfiguration.LPCOCodeAttributeCode);
				AssertEquals("LPCOCodeAttributeDescription", "col_2_reg", fraConfiguration.LPCOCodeAttributeDescription);
			});

			CombineAssertions("FrenchLanguageIndicator False", () =>
			{
				CACustomsDataRegistry.Instance.FrenchLanguageIndicator.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false);
				var engConfiguration = new AIRSWebpageConfiguration(Factory);
				AssertEquals("Url", "https://airs-sari.inspection.gc.ca/airs_external/english/decisions-eng.aspx", engConfiguration.Url);
				AssertEquals("EndUseTextID", "ctl00_ContentMain_lblEndUseText", engConfiguration.EndUseTextID);
				AssertEquals("ExtensionTextID", "ctl00_ContentMain_lblOGDExtensionIDText", engConfiguration.ExtensionTextID);
				AssertEquals("MiscTextID", "ctl00_ContentMain_lblMiscText", engConfiguration.MiscTextID);
				AssertEquals("IIDTableID", "ctl00_ContentMain_pnlIIDTable", engConfiguration.IIDTableID);
				Assert("PostData Tested in AIRSHtmlHelperTest", true);
				AssertEquals("IIDWebPageTitle", "Automated Import Reference System: Integrated Import Declaration", engConfiguration.IIDWebPageTitle);
				AssertEquals("MaterializedGridTitle", "Materialized LPCO (Image Required)", engConfiguration.MaterializedGridTitle);
				AssertEquals("DeMaterializedGridTitle", "Dematerialized LPCO (Image Not Required)", engConfiguration.DeMaterializedGridTitle);
				AssertEquals("CodeText", "CODE", engConfiguration.CodeText);
				AssertEquals("AIRSRegistrationGridTitle", "AIRS Registration", engConfiguration.AIRSRegistrationGridTitle);
				AssertEquals("RegistrationText", "REGISTRATION", engConfiguration.RegistrationText);
				AssertEquals("OrText", "OR", engConfiguration.OrText);
				AssertEquals("LPCOCodeAttributeKey", "headers", engConfiguration.LPCOCodeAttributeKey);
				AssertEquals("LPCOCodeAttributeCode", "col_1_code", engConfiguration.LPCOCodeAttributeCode);
				AssertEquals("LPCOCodeAttributeDescription", "col_2_reg", engConfiguration.LPCOCodeAttributeDescription);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AIRSWebpageConfiguration(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var urlFra = "https://airs-sari.inspection.gc.ca/airs_external/francais/decisions-fra.aspx";
			var urlEng = "https://airs-sari.inspection.gc.ca/airs_external/english/decisions-eng.aspx";
			AIRSHtmlHelperTest.SetupRefSysConfigType(Factory, urlFra, urlEng);
		}
	}
}
