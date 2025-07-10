using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	sealed class SailingUserControlTest : TestCaseWithFactory
	{
		public void TestEditSailingButton()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "ALH");

			using (var frm = new ZForm(header))
			using (var ctr = new SailingUserControl())
			{
				frm.Controls.Add(ctr);
				frm.Show();

				Application.DoEvents();

				var editSailingBtn = (ZButton)frm.Controls.Find("EditSailingButton", true).First();
				editSailingBtn.PerformClick();

				var message = "There is no Sailing Schedule to edit.";

				AssertEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);

				var sailing = Factory.NewWithValidTestData<JobSailing>();

				header.AMA_ParentId = sailing.PK;
				header.AMA_ParentTableCode = JobSailingSchema.Constants.Prefix;

				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				editSailingBtn.PerformClick();

				Application.DoEvents();

				AssertNotEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);

				var voyageForm = Application.OpenForms.Cast<Form>().First(c => c.GetType() == typeof(ZJobVoyageForm));
				AssertNotNull("Should open a voyage form.", voyageForm);

				voyageForm.Close();
			}
		}

		public void TestClearSailingButton()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Eritrea, "ASY");
			var sailing = Factory.NewWithValidTestData<JobSailing>();

			PopulateData();

			void PopulateData()
			{
				header.ChangeSailing(sailing.PK);

				header.AMA_RL_NKPortOfLoading = "AUSYD";
				header.AMA_RL_NKPortOfDischarge = "SGSIN";
				header.AMA_E_DEP = ZDateTime.Now;
				header.AMA_E_ARV = ZDateTime.Now;
				header.AMA_RN_NKConveyanceNationality = "AU";

				header.AMA_Voyage = "Voyage00";
				header.AMA_VesselName = "Vessel00";
				header.AMA_OA_Carrier = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			}

			var propertyInfo = new[]
			{
				header.AMA_VoyageInfo,
				header.AMA_VesselNameInfo,
				header.AMA_RL_NKPortOfLoadingInfo,
				header.AMA_RL_NKPortOfDischargeInfo,
				header.AMA_E_DEPInfo,
				header.AMA_E_ARVInfo,
				header.AMA_OA_CarrierInfo,
				header.AMA_RN_NKConveyanceNationalityInfo,
				header.AMA_OA_Carrier_ZAddress.OrgPKInfo
			};

			using (var frm = new ZForm(header))
			using (var ctr = new SailingUserControl())
			{
				frm.Controls.Add(ctr);
				frm.Show();

				Application.DoEvents();

				var clearSailingBtn = (ZButton)frm.Controls.Find("ClearSailingButton", true).First();
				clearSailingBtn.PerformClick();

				CombineAssertions(() =>
				{
					Assert("Should remove all linked sailings.", !header.Sailings.Any());
					AssertNull("Should remove the sailing.", header.Sailing);

					foreach (var info in propertyInfo)
					{
						Assert($"Should clear values on {info.Name}.", info.Value.IsEmpty);
					}
				});

				header.AMA_OverrideFreightDefaults = true;
				PopulateData();

				clearSailingBtn.PerformClick();

				CombineAssertions(() =>
				{
					Assert("Should remove all linked sailings.", !header.Sailings.Any());
					AssertNull("Should remove the sailing.", header.Sailing);

					foreach (var info in propertyInfo)
					{
						Assert($"Should not clear values on {info.Name} when the AMA_OverrideFreightDefaults is true.", !info.Value.IsEmpty);
					}
				});
			}
		}

		public void TestCanChangeSailing()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = string.Empty;

			var sailing = Factory.NewWithValidTestData<JobSailing>();
			header.ChangeSailing(sailing.PK);
			header.Messages.AddNew();

			using (var frm = new ZForm(header))
			using (var ctr = new SailingUserControl())
			{
				frm.Controls.Add(ctr);
				frm.Show();

				Application.DoEvents();

				var selectSailingBtn = (ZButton)frm.Controls.Find("SelectSailingButton", true).First();
				selectSailingBtn.PerformClick();

				var message = "Please choose an valid transport mode.";
				AssertEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				header.AMA_TransportMode = Core.Constants.TransportModes.Air;
				selectSailingBtn.PerformClick();

				message = "Once messages are sent to customs, you cannot change the sailing.";
				AssertEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				var clearSailingBtn = (ZButton)frm.Controls.Find("ClearSailingButton", true).First();
				clearSailingBtn.PerformClick();

				AssertEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestVisbleOfControls()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			using (var frm = new ZForm(header))
			using (var ctr = new SailingUserControl())
			{
				frm.Controls.Add(ctr);
				frm.Show();

				Application.DoEvents();

				AssertEquals("Precondition", 0, header.Sailings.Count);

				var sailingStatisticsPanel = (ZPanel)frm.Controls.Find("SailingStatisticsPanel", true).First();
				var refreshStatisticsButton = (ZButton)frm.Controls.Find("RefreshStatisticsButton", true).First();

				Assert("Should default to false.", !sailingStatisticsPanel.Visible);
				Assert("Should default to false.", !refreshStatisticsButton.Visible);

				header.Sailings.AddNew();
				Assert("Should be true when the manifest header has at least one sailing.", sailingStatisticsPanel.Visible);
				Assert("Should be true when the manifest header has at least one sailing.", refreshStatisticsButton.Visible);

				header.Sailings.RemoveAndDeleteAll();
				Assert("Should be false when the manifest header does not have any sailings.", !sailingStatisticsPanel.Visible);
				Assert("Should be false when the manifest header does not have any sailings.", !refreshStatisticsButton.Visible);
			}
		}
	}
}
