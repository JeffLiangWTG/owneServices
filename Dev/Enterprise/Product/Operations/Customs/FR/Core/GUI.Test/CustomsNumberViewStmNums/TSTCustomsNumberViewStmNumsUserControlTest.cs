using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.FR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.Testing
{
	public class TSTCustomsNumberViewStmNumsUserControlTest : TestCaseWithFactory
	{
		public void TestAddButton()
		{
			var wrapperCollection = provider.CustomsNumberWrappers;
			using (var form = new ZForm())
			using (var control = new TSTCustomsNumberViewStmNumsUserControl(wrapperCollection))
			{
				form.Controls.Add(control);
				form.Show();
				var addButton = form.FindSingle<ZButton>("NumberRangesAddButton");
				AssertEquals(true, addButton.Visible);
				AssertEquals("&Add", ((IExtendedControl)addButton).Extensions.Get<ILabelCaptionRenderer>().Caption);

				var addBranchButton = form.FindSingleOrDefault<ZButton>("NumberRangesAddBranchButton");
				AssertNull(addBranchButton);
			}
		}

		public void TestHideThresholdRunOutWarningGroupBox()
		{
			var wrapperCollection = provider.CustomsNumberWrappers;
			using (var form = new ZForm())
			using (var control = new TSTCustomsNumberViewStmNumsUserControl(wrapperCollection))
			{
				form.Controls.Add(control);
				form.Show();
				var thresholdRunOutWarningGroupBox = form.FindSingle<ZGroupBox>("ThresholdRunOutWarningGroupBox");
				AssertEquals(false, thresholdRunOutWarningGroupBox.Visible);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			authorisation = Factory.New<Customs.Business.CusAuthorisationHeader>();
			authorisation.CPH_OH_PermitHolder = Factory.NewWithValidTestData<OrgHeader>().PK;
			authorisation.CPH_Number = "TS000040";
			authorisation.CPH_Type = CusAuthorizationHeaderTypeList.Codes.TemporaryStorage;
			provider = new TSTCustomsNumberViewStmNumsAuthorisationProvider(Factory, authorisation.PK);
		}

		Customs.Business.CusAuthorisationHeader authorisation;
		TSTCustomsNumberViewStmNumsAuthorisationProvider provider;
	}
}
