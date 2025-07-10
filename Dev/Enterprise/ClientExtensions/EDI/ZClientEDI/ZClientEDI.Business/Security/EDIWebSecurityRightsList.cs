using System.Diagnostics.CodeAnalysis;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using WTG.WebSecurityRight;

namespace Enterprise.Client.EDI
{
	public class EDIWebSecurityApplication : WebSecurityApplication
	{
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static readonly EDIWebSecurityApplication MyAccount = new EDIWebSecurityApplication("MyAccount", false);

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static readonly EDIWebSecurityApplication BorderWise = new EDIWebSecurityApplication("BorderWise", false);

		protected EDIWebSecurityApplication(string name, bool shouldGrantAllAccessRightsByDefault)
			: base(name, shouldGrantAllAccessRightsByDefault)
		{
		}
	}

	public class EDIWebSecurityRightsList : WebSecurityRightsList
	{
		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = delegate { return new EDIWebSecurityRightsList(); };
		}

		protected EDIWebSecurityRightsList() { }

		protected override bool ShouldAddReflectedRight(WebSecurityRight right)
		{
			return right.WebApplication == EDIWebSecurityApplication.MyAccount ||
				right.WebApplication == EDIWebSecurityApplication.BorderWise ||
				right == WebSecurityRightsList.eRequestPortalViewAll ||
				right == WebSecurityRightsList.eRequestPortalViewOwn ||
				right == WebSecurityRightsList.eRequestPortalSubmit ||
				right == WebSecurityRightsList.WebAccreditationsViewAll ||
				right == WebSecurityRightsList.WebAccreditationsViewOwn;
		}

		public static readonly WebSecurityRight CustomerService = new WebSecurityRight("Customer Service Incidents", (NoResString)"Customer Service Incidents", EDIWebSecurityApplication.MyAccount);
		public static readonly WebSecurityRight Downloads = new WebSecurityRight("Download Upgrades", (NoResString)"Download Upgrades", EDIWebSecurityApplication.MyAccount);
		public static readonly WebSecurityRight ClassroomSessions = new WebSecurityRight("Classroom Sessions", (NoResString)"Classroom Sessions", EDIWebSecurityApplication.MyAccount, true);
		public static readonly WebSecurityRight EDIMyAccountReports = new WebSecurityRight("My Account Reports", (NoResString)"My Account Reports", EDIWebSecurityApplication.MyAccount);
		public static readonly WebSecurityRight LicenceUsageReports = new WebSecurityRight("Licence Usage Reports", (NoResString)"Licence Usage Reports", EDIWebSecurityApplication.MyAccount, true);
		public static readonly WebSecurityRight EDIEnterpriseWiseLearning = new WebSecurityRight("ediEnterprise WiseLearning", (NoResString)"CargoWise WiseLearning", EDIWebSecurityApplication.MyAccount, true);
		public static readonly WebSecurityRight SapphireWiseLearning = new WebSecurityRight("SAPPHIRE WiseLearning", (NoResString)"SAPPHIRE WiseLearning", EDIWebSecurityApplication.MyAccount, true);
		public static readonly WebSecurityRight OdysseyWiseLearning = new WebSecurityRight("Odyssey WiseLearning", (NoResString)"Odyssey WiseLearning", EDIWebSecurityApplication.MyAccount, true);
		public static readonly WebSecurityRight UpdateNotes = new WebSecurityRight("Update Notes", (NoResString)"Update Notes", EDIWebSecurityApplication.MyAccount, true);
		public static readonly WebSecurityRight CargoWiseTechnicalGuides = new WebSecurityRight("CargoWise Technical Guides", (NoResString)"CargoWise Technical Guides", EDIWebSecurityApplication.MyAccount, true);
		public static readonly WebSecurityRight TranslogixTechnicalGuides = new WebSecurityRight("Translogix Technical Guides", (NoResString)"Translogix Technical Guides", EDIWebSecurityApplication.MyAccount, true);
		public static readonly WebSecurityRight CountryGuides = new WebSecurityRight("Country Guides", (NoResString)"Country Guides", EDIWebSecurityApplication.MyAccount, true);
		public static readonly WebSecurityRight LearningArchive = new WebSecurityRight("Learning Archive", (NoResString)"Learning Archive", EDIWebSecurityApplication.MyAccount, true);
		public static readonly WebSecurityRight SapphireProductVideos = new WebSecurityRight("Sapphire Product Videos", (NoResString)"Sapphire Product Videos", EDIWebSecurityApplication.MyAccount);
		public static readonly WebSecurityRight WiseBusinessPartner = new WebSecurityRight("WiseBusiness Partner", (NoResString)"WiseBusiness Partner", EDIWebSecurityApplication.MyAccount);
		public static readonly WebSecurityRight WiseServicePartner = new WebSecurityRight("WiseService Partner", (NoResString)"WiseService Partner", EDIWebSecurityApplication.MyAccount);
		public static readonly WebSecurityRight MediaAnalyticsReport = new WebSecurityRight("Media Analytics Report", (NoResString)"Media Analytics Report", EDIWebSecurityApplication.MyAccount);
		public static readonly WebSecurityRight WTGInternalExams = new WebSecurityRight("WTG Internal Exams", (NoResString)"WTG Internal Exams", EDIWebSecurityApplication.MyAccount);
		public static readonly WebSecurityRight ELearningContentManagement = new WebSecurityRight("eLearning Content Management", (NoResString)"eLearning Content Management", EDIWebSecurityApplication.MyAccount);
		public static readonly WebSecurityRight WebSecurityAdministration = new WebSecurityRight("Web Security Administration", (NoResString)"Web Security Administration", EDIWebSecurityApplication.MyAccount);
		public static readonly WebSecurityRight WiseTechAcademy = new WebSecurityRight("WiseTech Academy", (NoResString)"WiseTech Academy", EDIWebSecurityApplication.MyAccount, true);
		public static readonly WebSecurityRight CommunityForum = new WebSecurityRight("CargoWise Community", (NoResString)"CargoWise Community Forum", EDIWebSecurityApplication.MyAccount, true);
		public static readonly WebSecurityRight CargoWiseCertificationPrograms = new WebSecurityRight("CargoWise Certification Programs", (NoResString)"CargoWise Certification Programs", EDIWebSecurityApplication.MyAccount, true);
		public static readonly WebSecurityRight Transtream = new WebSecurityRight("Transtream", (NoResString)"Transtream", EDIWebSecurityApplication.MyAccount, true);
		public static readonly WebSecurityRight EmailSubscriptions = new WebSecurityRight("Email Subscription Preferences", (NoResString)"Email Subscriptions", EDIWebSecurityApplication.MyAccount, true);
		public static readonly WebSecurityRight WiseTechLearning = new WebSecurityRight("WiseTechLearning", (NoResString)"WiseTech Learning", EDIWebSecurityApplication.MyAccount, true);
		public static readonly WebSecurityRight CargoSphere = new WebSecurityRight("CargoSphere", (NoResString)"CargoSphere", EDIWebSecurityApplication.MyAccount, true);
		public static readonly WebSecurityRight Cargoguide = new WebSecurityRight("Cargoguide", (NoResString)"Cargoguide", EDIWebSecurityApplication.MyAccount, true);
		public static readonly WebSecurityRight Containerchain = new WebSecurityRight("Containerchain", (NoResString)"Containerchain", EDIWebSecurityApplication.MyAccount, true);

		public static readonly WebSecurityRight BorderWise = new WebSecurityRight("BorderWise", (NoResString)"BorderWise & WiseLearning Content", EDIWebSecurityApplication.BorderWise, true);
	}
}

