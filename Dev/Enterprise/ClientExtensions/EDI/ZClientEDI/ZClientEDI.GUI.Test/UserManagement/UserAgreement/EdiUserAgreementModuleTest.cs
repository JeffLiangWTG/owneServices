#region Test

using Enterprise.ZArchitecture.Environment;
#if DEBUG
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Client.EDI.UserManagement.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.UserManagement.GUI.Testing
{
	public class EdiUserAgreementModuleTest : TestCaseWithFactory
	{
		public void TestAcceptForOrganisationButton()
		{
			var userAgreement1 = Factory.NewWithValidTestData<EdiUserAgreement>();
			userAgreement1.ERA_Title = "Agreement 1";
			userAgreement1.ERA_Content = "agreement content";
			userAgreement1.ERA_Type = "MYA";
			var userAgreement2 = Factory.NewWithValidTestData<EdiUserAgreement>();
			userAgreement2.ERA_Title = "Agreement 2";
			userAgreement2.ERA_Content = "agreement content 2";
			userAgreement2.ERA_Type = "CAL";
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "WALLE";
			Factory.Save();

			using (var module = new EdiUserAgreementModuleForTest())
			{
				var acceptForOrgMenuItem = module.ActionMenuItems.First(x => x.Text == "Accept on behalf of client");
				AssertNotNull(acceptForOrgMenuItem);

				var moduleLoadedUserAgreement1 = module.Factory_Exposed.Load<EdiUserAgreement>(userAgreement1.PK);
				module.SetUserAgreements(new [] { moduleLoadedUserAgreement1 });
				module.SetOrganisation(org);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				acceptForOrgMenuItem.PerformClick();

				var log1Query = new ZQuery(EdiUserAgreementAcceptanceLogSchema.EUL_ERA, userAgreement1.PK);
				log1Query.AddToFilter(EdiUserAgreementAcceptanceLogSchema.EUL_OH, org.PK);
				var log2Query = new ZQuery(EdiUserAgreementAcceptanceLogSchema.EUL_ERA, userAgreement2.PK);
				log2Query.AddToFilter(EdiUserAgreementAcceptanceLogSchema.EUL_OH, org.PK);

				var newFactory = new BusinessObjectFactory();
				var log1 = newFactory.LoadTop1<EdiUserAgreementAcceptanceLog>(log1Query);
				var log2 = newFactory.LoadTop1<EdiUserAgreementAcceptanceLog>(log2Query);
				AssertNull(log1);
				AssertNull(log2);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				acceptForOrgMenuItem.PerformClick();

				log1 = newFactory.LoadTop1<EdiUserAgreementAcceptanceLog>(log1Query);
				log2 = newFactory.LoadTop1<EdiUserAgreementAcceptanceLog>(log2Query);
				AssertNotNull(log1);
				AssertNull(log2);
				AssertGreaterThanOrEqualTo("Should have added log today", log1.EUL_AcceptanceTimeUtc, ZDateTime.UtcNow.AddDays(-1));

				var moduleLoadedUserAgreement2 = module.Factory_Exposed.Load<EdiUserAgreement>(userAgreement2.PK);
				module.SetUserAgreements(new[] { moduleLoadedUserAgreement1, moduleLoadedUserAgreement2 });

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				acceptForOrgMenuItem.PerformClick();

				log2 = newFactory.LoadTop1<EdiUserAgreementAcceptanceLog>(log1Query);
				AssertNotNull(log2);
				AssertGreaterThanOrEqualTo("Should have added log today", log2.EUL_AcceptanceTimeUtc, ZDateTime.UtcNow.AddDays(-1));

				AssertEquals("Should not create any more logs for agreement 1", 1, newFactory.GetDatabaseCount(typeof(EdiUserAgreementAcceptanceLog), log1Query));
				AssertContains("The following agreements were skipped as they have already been accepted for this organization:\r\nAgreement 1", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				acceptForOrgMenuItem.PerformClick();
				Application.DoEvents();

				AssertEquals("Should not create any more logs for agreement 1", 1, newFactory.GetDatabaseCount(typeof(EdiUserAgreementAcceptanceLog), log1Query));
				AssertEquals("Should not create any more logs for agreement 2", 1, newFactory.GetDatabaseCount(typeof(EdiUserAgreementAcceptanceLog), log2Query));
				AssertContains("The following agreements were skipped as they have already been accepted for this organization:\r\nAgreement 1\r\nAgreement 2", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		class EdiUserAgreementModuleForTest : EdiUserAgreementModule
		{
			public MenuItem[] ActionMenuItems => GetNewActionMenuItems();

			public void SetOrganisation(OrgHeader organisation)
			{
				Organisation = organisation;
			}

			public OrgHeader Organisation { get; private set; }

			protected override OrgHeader GetOrganisation()
			{
				return Organisation;
			}

			public void SetUserAgreements(EdiUserAgreement[] userAgreements)
			{
				UserAgreements = userAgreements;
			}

			public EdiUserAgreement[] UserAgreements { get; private set; }

			protected override EdiUserAgreement[] GetSelectedUserAgreements()
			{
				return UserAgreements;
			}

			public BusinessObjectFactory Factory_Exposed => Factory;
		}
	}
}

#endif
#endregion
