using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using WTG.WebSecurityRight;

namespace Enterprise.DocumentEngine.Testing
{
	sealed partial class WebRequestHelperTest : TestCaseWithFactory
	{
		public void TestGetAvailableWebReports()
		{
			AssertEquals("pre-condition", true, GlowRegistry.Instance.EnableSecurityGroupsForContactsInGLOW.Value);
			var businessContext = "RepOrdersReport";
			var businessContexts = new List<ZString> { businessContext };
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuType = Core.Constants.StmMenuItemTypes.WebReports;
			menuItem.SU_IsVisibleOnWeb = false;
			menuItem.SU_IsPublished = false;
			menuItem.SU_BusinessContext = businessContext;

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "~code~";
			var contact = Factory.New<OrgContact>();
			contact.OC_OH = org.PK;
			contact.OC_Gender = "M";
			Factory.Save();
			var code = menuItem.PK.ToString();
			var description = "desc";
			var rightCodeAndDescription = new CodeDescriptionPair(code, description);
			var webSecurityRight = new WebSecurityRight(rightCodeAndDescription.Code, (NoResString)rightCodeAndDescription.Description, WebSecurityApplication.EdiWebTracker, false);

			var orgSecurityRight = contact.Header.SecurityRights.AddNew();
			orgSecurityRight.OX_SU = webSecurityRight.SecurityGuid;
			orgSecurityRight.OX_Granted = true;
			Factory.Save();
			var glbSecurity = Factory.New<GlbSecurity>();
			var glbGroup = Factory.New<GlbGroup>();
			glbSecurity.GU_ItemGUID = webSecurityRight.SecurityGuid;
			glbSecurity.GU_GG = glbGroup.PK;
			Factory.Save();
			var glbGroupOrgContactLink = Factory.New<GlbGroupOrgContactLink>();
			glbGroupOrgContactLink.GCK_GG_Group = glbGroup.PK;
			glbGroupOrgContactLink.GCK_OC_Contact = contact.PK;
			Factory.Save();
			AssertEquals("Pre-Condition", true, contact.IsRightGrantedWithCheckingSecurityGroups(webSecurityRight));
			var webReportCommandCollection = WebReportHelper.GetAvailableWebReports(Factory, businessContexts, true, contact.IsRightGrantedWithCheckingSecurityGroups);
			AssertEquals("no report, when SU_IsVisibleOnWeb=false, SU_IsPublished=false", 0, webReportCommandCollection.Count);

			menuItem.SU_IsPublished = true;
			Factory.Save();
			webReportCommandCollection = WebReportHelper.GetAvailableWebReports(Factory, businessContexts, true, contact.IsRightGrantedWithCheckingSecurityGroups);
			AssertEquals("no report, when SU_IsVisibleOnWeb=false, SU_IsPublished=true", 0, webReportCommandCollection.Count);

			menuItem.SU_IsVisibleOnWeb = true;
			Factory.Save();
			webReportCommandCollection = WebReportHelper.GetAvailableWebReports(Factory, businessContexts, true, contact.IsRightGrantedWithCheckingSecurityGroups);
			AssertEquals("1 report, when SU_IsVisibleOnWeb=true, SU_IsPublished=true", 1, webReportCommandCollection.Count);

			menuItem.SU_IsPublished = false;
			Factory.Save();
			webReportCommandCollection = WebReportHelper.GetAvailableWebReports(Factory, businessContexts, true, contact.IsRightGrantedWithCheckingSecurityGroups);
			AssertEquals("1 report, when SU_IsVisibleOnWeb=true, SU_IsPublished=false", 1, webReportCommandCollection.Count);
		}
	}
}
