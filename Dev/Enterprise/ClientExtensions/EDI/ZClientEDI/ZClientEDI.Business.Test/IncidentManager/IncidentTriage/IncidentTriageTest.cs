using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.CustomerService.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(IncidentTriage))]
	public class IncidentTriageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIMT_IsActiveDefault()
		{
			var triage = Factory.New<IncidentTriage>();
			AssertEquals("Default should be true", true, triage.IMT_IsActive);
		}

		public void TestIMT_IsPublishedDefault()
		{
			var triage = Factory.New<IncidentTriage>();
			AssertEquals("Default should be false", false, triage.IMT_IsPublished);
		}

		public void TestModuleType()
		{
			var triage = Factory.NewWithValidTestData<IncidentTriage>();
			triage.IMT_Type = IncidentTriageTypes.Codes.Support;
			AssertEquals("Support type should be unspecified module", ModuleListType.MenuSection, triage.ModuleType);

			triage.IMT_Type = IncidentTriageTypes.Codes.Compliance;
			AssertEquals("Compliance type should be CR8", ModuleListType.Cr8, triage.ModuleType);

			triage.IMT_Type = IncidentTriageTypes.Codes.Service;
			AssertEquals("Service type should be CR9", ModuleListType.Cr9, triage.ModuleType);
		}

		public void TestLevel()
		{
			var triage = Factory.New<IncidentTriage>();
			AssertEquals("", triage.IMT_Level);
			AssertEquals("", triage.LevelDescription);

			triage.IMT_Level = IncidentTriageLevels.Codes.PreliminaryAssignment;
			AssertEquals(IncidentTriageLevels.Descriptions.PreliminaryAssignment, triage.LevelDescription);

			triage.IMT_Level = IncidentTriageLevels.Codes.Diagnostics;
			AssertEquals(IncidentTriageLevels.Descriptions.Diagnostics, triage.LevelDescription);
		}

		public void TestHumanReadableName()
		{
			var triage = Factory.NewWithValidTestData<IncidentTriage>();
			triage.IMT_SupportDescription = "DESC:123";
			AssertEquals("Incident Triage - DESC:123", triage.HumanReadableShortcutName);
			AssertEquals("Incident Triage", triage.HumanReadableName);
		}

		public void TestIMT_ProductArea_ReadOnly()
		{
			var triage = Factory.New<IncidentTriage>();
			triage.IMT_SetProductAreaByMenuItem = false;
			AssertEquals(false, triage.IMT_ProductArea_ReadOnly);
			triage.IMT_SetProductAreaByMenuItem = true;
			AssertEquals(true, triage.IMT_ProductArea_ReadOnly);
		}

		public void TestIMT_SetProductAreaByMenuItem()
		{
			var triage = Factory.New<IncidentTriage>();
			triage.IMT_ProductArea = "ENT";
			AssertEquals(false, triage.IMT_SetProductAreaByMenuItem);
			triage.IMT_SetProductAreaByMenuItem = true;
			AssertEquals("", triage.IMT_ProductArea);
		}

		public void TestNoAutoLog()
		{
			var triage = Factory.NewWithValidTestData<IncidentTriage>();
			Factory.Save();

			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var triage1 = factory1.Load<IncidentTriage>(triage.PK);

			AssertEquals(0, triage1.Logs.Find(log => log.SL_SE_NKEvent == "ADD").Count());

			triage.IMT_ProductArea = "ENT";
			Factory.Save();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var triage2 = factory1.Load<IncidentTriage>(triage.PK);

			AssertEquals(0, triage2.Logs.Find(log => log.SL_SE_NKEvent == "EDT").Count());
		}

		public void TestIsPublishedPropertiesReadOnly()
		{
			var triage = Factory.NewWithValidTestData<IncidentTriage>();
			Factory.Save();

			EDISecurityCheckpoints.CustomerServiceIncidentTriagePublishAccess.IsAllowed = false;
			Assert(triage.IMT_IsPublishedInfo.ReadOnly);
			Assert(triage.IMT_IsPublishedToAssistInfo.ReadOnly);

			EDISecurityCheckpoints.CustomerServiceIncidentTriagePublishAccess.IsAllowed = true;
			Assert(!triage.IMT_IsPublishedInfo.ReadOnly);
			Assert(!triage.IMT_IsPublishedToAssistInfo.ReadOnly);
		}

		#region Test HTML Properties

		public void TestHtmlProperty()
		{
			var incidentTriage = (IncidentTriage)GetNewBusinessObject();
			AssertEquals(ZBlob.Empty, incidentTriage.SupportNotesAsBlob);
			AssertEquals(ZBlob.Empty, incidentTriage.SupportNotesAsBlob_HTML);

			incidentTriage.SupportNotesAsBlob_HTML = ZBlob.FromUTF8("<p>123</p>");

			AssertEquals(@"{\rtf1\ansi\ansicpg1252\deflang3081\nouicompat\uc0{\fonttbl}{\colortbl}{{123}\par}}", ORtfTextUtil.GeneratorInfoRegex.Replace(incidentTriage.SupportNotesAsBlob.ToUTF8(), string.Empty));
			AssertEquals("<p>123</p>", incidentTriage.SupportNotesAsBlob_HTML.ToUTF8());
		}

		public void TestHtmlFromTextProperty()
		{
			var incidentTriage = (IncidentTriage)GetNewBusinessObject();
			AssertEquals(ZBlob.Empty, incidentTriage.SupportNotesAsBlob);
			AssertEquals(ZBlob.Empty, incidentTriage.SupportNotesAsBlob_HTML);

			incidentTriage.SupportNotesAsBlob = ZBlob.FromUTF8("1234\r\n5678");

			AssertEquals("<p>1234</p><p>5678</p>", incidentTriage.SupportNotesAsBlob_HTML.ToUTF8());

			incidentTriage.SupportNotesAsBlob = ZBlob.FromUTF8("{\\rtf1\\test\\ansi\\ansicpg1252\\nouicompat\\deflang3081\r\n{\\*\\generator Riched20 10.0.19041}\\viewkind4\\uc1 \\pard rtf\\par\r\n}\r\n");

			AssertEquals("<p>rtf</p>", incidentTriage.SupportNotesAsBlob_HTML.ToUTF8());
		}

		#endregion
	}
}
