using System;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;

namespace Enterprise.Client.EDI.Test
{
	internal class EDIWebSecurityRightsListTest : WebSecurityRightsListTest
	{
		public override void TestDeniedByDefaultRegistry()
		{
			using (OrganisationRegistry.Instance.WebSecurityRightsDeniedByDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var list = WebSecurityRightsList.New();
				WebSecurityRight securityRight;
				Assert(list.TryGetValue("Classroom Sessions", out securityRight));
				AssertEquals("granted by default, denied due to security right", false, securityRight.IsGrantedByDefault);
				Assert(list.TryGetValue("Download Upgrades", out securityRight));
				AssertEquals("denied by default", false, securityRight.IsGrantedByDefault);
			}

			using (OrganisationRegistry.Instance.WebSecurityRightsDeniedByDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var list = WebSecurityRightsList.New();
				WebSecurityRight securityRight;
				Assert(list.TryGetValue("Classroom Sessions", out securityRight));
				AssertEquals("granted by default", true, securityRight.IsGrantedByDefault);
				Assert(list.TryGetValue("Download Upgrades", out securityRight));
				AssertEquals("denied by default", false, securityRight.IsGrantedByDefault);
			}
		}

		public void TestEDISpecificWebSecurityRights()
		{
			WebSecurityRightsList list = WebSecurityRightsList.New();
			AssertCollectionContains(EDIWebSecurityRightsList.CustomerService, list);
			AssertCollectionContains(EDIWebSecurityRightsList.Downloads, list);
			AssertCollectionContains(EDIWebSecurityRightsList.ClassroomSessions, list);
			AssertCollectionContains(EDIWebSecurityRightsList.EDIMyAccountReports, list);
			AssertCollectionContains(EDIWebSecurityRightsList.LicenceUsageReports, list);
			AssertCollectionContains(EDIWebSecurityRightsList.EDIEnterpriseWiseLearning, list);
			AssertCollectionContains(EDIWebSecurityRightsList.SapphireWiseLearning, list);
			AssertCollectionContains(EDIWebSecurityRightsList.OdysseyWiseLearning, list);
			AssertCollectionContains(EDIWebSecurityRightsList.UpdateNotes, list);
			AssertCollectionContains(EDIWebSecurityRightsList.CargoWiseTechnicalGuides, list);
			AssertCollectionContains(EDIWebSecurityRightsList.TranslogixTechnicalGuides, list);
			AssertCollectionContains(EDIWebSecurityRightsList.CountryGuides, list);
			AssertCollectionContains(EDIWebSecurityRightsList.LearningArchive, list);
			AssertCollectionContains(EDIWebSecurityRightsList.SapphireProductVideos, list);
			AssertCollectionContains(EDIWebSecurityRightsList.WiseBusinessPartner, list);
			AssertCollectionContains(EDIWebSecurityRightsList.WiseServicePartner, list);
			AssertCollectionContains(EDIWebSecurityRightsList.MediaAnalyticsReport, list);
			AssertCollectionContains(EDIWebSecurityRightsList.WTGInternalExams, list);
			AssertCollectionContains(EDIWebSecurityRightsList.ELearningContentManagement, list);
			AssertCollectionContains(WebSecurityRightsList.eRequestPortalViewAll, list);
			AssertCollectionContains(WebSecurityRightsList.eRequestPortalViewOwn, list);
			AssertCollectionContains(WebSecurityRightsList.eRequestPortalSubmit, list);
			AssertCollectionContains(EDIWebSecurityRightsList.BorderWise, list);
			AssertCollectionContains(EDIWebSecurityRightsList.WebSecurityAdministration, list);
			AssertCollectionContains(EDIWebSecurityRightsList.WiseTechAcademy, list);
			AssertCollectionContains(EDIWebSecurityRightsList.CommunityForum, list);
			AssertCollectionContains(EDIWebSecurityRightsList.CargoWiseCertificationPrograms, list);
			AssertCollectionContains(EDIWebSecurityRightsList.Transtream, list);
			AssertCollectionContains(EDIWebSecurityRightsList.EmailSubscriptions, list);
			AssertCollectionContains(EDIWebSecurityRightsList.WiseTechLearning, list);
			AssertCollectionContains(EDIWebSecurityRightsList.CargoSphere, list);
			AssertCollectionContains(EDIWebSecurityRightsList.Cargoguide, list);
			AssertCollectionContains(EDIWebSecurityRightsList.Containerchain, list);
			WebSecurityRight ignored;
			Assert("Should not contain Tracking security rights", !list.TryGetValue(EDIWebSecurityRightsList.WebOrdersView.Code, out ignored));
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlowRegistry.Instance.EnableSecurityGroupsForContactsInGLOW.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}
	}
}
