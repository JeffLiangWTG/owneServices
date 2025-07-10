using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.FR.GUI.NCTS.Testing
{
	class DeclarationDetailsTabUserControlTest : TestCaseWithFactory
	{
		public void TestAuthorisedLocationCodeTextBoxVisibility()
		{
			using (var form = new ZForm(nctsHeader))
			{
				using (var control = new DeclarationDetailsTabUserControl())
				{
					form.Controls.Add(control);
					form.Show();

					var agreedLocationOfGoodsCodeCodeTextBox = (ZTextBox)form.Controls.Find("AuthorisedLocationCodeTextBox", true).First();
					Assert(!agreedLocationOfGoodsCodeCodeTextBox.Visible);
				}
			}
		}

		public void TestAuthorisedLocationCodeFindBox()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			using (var form = new ZForm(header))
			using (var control = new DeclarationDetailsTabUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var codeFindBox = (ZCodeFindBox)form.Controls.Find("AuthorisedLocationCodeFindBox", true).First();
				var codeTextBox = (ZTextBox)form.Controls.Find("AuthorisedLocationCodeTextBox", true).First();

				AssertEquals("MovementHeader.BM_LocationOfGoodsCode", codeFindBox.BindTo);
				AssertSame(ModuleIDs.Customs.CusAuthorisations, codeFindBox.ModuleID);
				AssertEquals(codeTextBox.Location, codeFindBox.Location);
			}
		}

		public void TestControlDisplay()
		{
			using (var form = new ZForm(nctsHeader))
			{
				using (var control = new DeclarationDetailsTabUserControl())
				{
					form.Controls.Add(control);
					form.Show();

					Assert("AutoScroll should be true", control.AutoScroll);
					AssertEquals("control.AutoScrollMinSize.Height", 775, control.AutoScrollMinSize.Height);
				}
			}
		}

		public void TestGuaranteesUserControlType()
		{
			using (var form = new ZForm(nctsHeader))
			{
				form.Controls.Add(new DeclarationDetailsTabUserControl());
				form.Show();

				var userControl = (ZDynamicControlCreationUserControl)form.Controls.Find("GuaranteesZDynamicUserControl", true).First();
				AssertEquals("Enterprise.Customs.FR.GUI.FRGuaranteesUserControl", userControl.UserControlType.FullName);
			}
		}

		public void TestContainersAndSealsZDynamicUserControl()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			using (var form = new ZForm(header))
			using (var control = new DeclarationDetailsTabUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var customsOfficesZDynamicUserControl = control.FindSingle<ZDynamicControlCreationUserControl>("ContainersAndSealsZDynamicUserControl");
				AssertType<FRContainersAndSealsUserControl>(customsOfficesZDynamicUserControl.HostedControl);
			}
		}

		public void TestDateLimitControlAlwaysShowing()
		{
			using (var form = new ZForm(nctsHeader))
			{
				form.Controls.Add(new DeclarationDetailsTabUserControl());
				form.Show();
				var controlResultDateLimitDateEdit = (ZDateEdit)form.Controls.Find("ControlResultDateLimitDateEdit", true).First();
				var controlResultDateLimitDateEdit2 = (ZDateEdit)form.Controls.Find("ControlResultDateLimitDateEdit2", true).First();

				nctsHeader.MovementHeader.IsSimplifiedNctsProcedure = true;
				Assert(controlResultDateLimitDateEdit.Visible);
				AssertEquals(false, controlResultDateLimitDateEdit2.Visible);

				nctsHeader.MovementHeader.IsSimplifiedNctsProcedure = false;
				AssertEquals(false, controlResultDateLimitDateEdit.Visible);
				Assert(controlResultDateLimitDateEdit2.Visible);
			}
		}

		public void TestFrenchSpecificControlsVisibility()
		{
			using (var form = new ZForm(nctsHeader))
			{
				form.Controls.Add(new DeclarationDetailsTabUserControl());
				form.Show();
				var detailedStatusDropEdit = (ZDropEdit)form.Controls.Find("DetailedStatusDropEdit", true).First();
				Assert(detailedStatusDropEdit.Visible);
				var isPrelodgedMovementCheckBox = (ZCheckBox)form.Controls.Find("IsPrelodgedMovementCheckBox", true).First();
				Assert(isPrelodgedMovementCheckBox.Visible);
				var isQueriedCheckBox = (ZCheckBox)form.Controls.Find("IsQueriedCheckBox", true).First();
				Assert(isQueriedCheckBox.Visible);
				var isQueryAvailableOnPaperCheckBox = (ZCheckBox)form.Controls.Find("IsQueryAvailableOnPaperCheckBox", true).First();
				Assert(isQueryAvailableOnPaperCheckBox.Visible);
				var queryInformationTextBox = (ZTextBox)form.Controls.Find("QueryInformationTextBox", true).First();
				Assert(queryInformationTextBox.Visible);
				var isTC11DeliveredByCustomsCheckBox = (ZCheckBox)form.Controls.Find("IsTC11DeliveredByCustomsCheckBox", true).First();
				Assert(isTC11DeliveredByCustomsCheckBox.Visible);
				var tC11DateDateEdit = (ZDateEdit)form.Controls.Find("TC11DateDateEdit", true).First();
				Assert(tC11DateDateEdit.Visible);
				var declarantDocAddressControl = (ZDocAddressControl)form.Controls.Find("DeclarantDocAddressControl", true).First();
				Assert(declarantDocAddressControl.Visible);
				var chargePaymentOrDestinationIDDropEdit = (ZDropEdit)form.Controls.Find("ChargePaymentOrDestinationIDDropEdit", true).First();
				Assert(chargePaymentOrDestinationIDDropEdit.Visible);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		}
		NctsHeader nctsHeader;
	}
}
