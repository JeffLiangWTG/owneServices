using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Design;
using NctsHeader = Enterprise.Customs.ES.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	class DeclarationDetailsTabUserControlTest : TestCaseWithFactory
	{
		public void TestBrokerFindBoxReadOnlyControl()
		{
			using (var form = new ZForm(nctsHeader))
			{
				form.Controls.Add(new DeclarationDetailsTabUserControl());
				form.Show();

				CombineAssertions(() =>
				{
					var brokerFindBox = (ZCodeFindBox)form.Controls.Find("BrokerFindBox", true).First();
					nctsHeader.BH_HeaderType = ZString.Empty;
					AssertEquals("Without values in BH_Headertype and without send data, the BrokerFindBox is Visible.", true, brokerFindBox.Visible);
					nctsHeader.BH_HeaderType = NctsMovementType.Codes.DepartureAndArrival;
					AssertEquals("With BH_HeaderType DA and without send data, the BrokerFindBox is not Visible.", false, brokerFindBox.Visible);
					nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
					AssertEquals("With BH_HeaderType D and without send data, the BrokerFindBox is Visible.", true, brokerFindBox.Visible);
					nctsHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationSent;
					nctsHeader.BH_HeaderType = NctsMovementType.Codes.DepartureAndArrival;
					AssertEquals("With BH_HeaderType DA and send data MDS, the BrokerFindBox is not Visible.", false, brokerFindBox.Visible);
					nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
					AssertEquals("With BH_HeaderType A and without send data, the BrokerFindBox is Visible.", true, brokerFindBox.Visible);
					nctsHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationNotSent;
					nctsHeader.BH_HeaderType = NctsMovementType.Codes.DepartureAndArrival;
					AssertEquals("With BH_HeaderType DA and send data MDN, the BrokerFindBox is not Visible.", false, brokerFindBox.Visible);
				});
			}
		}

		public void TestCertificateDropEditReadOnlyControl()
		{
			using (var form = new ZForm(nctsHeader))
			{
				form.Controls.Add(new DeclarationDetailsTabUserControl());
				form.Show();

				CombineAssertions(() =>
				{
					var certificateDropEdit = (ZDropEdit)form.Controls.Find("CertificateDropEdit", true).First();
					nctsHeader.BH_HeaderType = ZString.Empty;
					AssertEquals("Without values in BH_Headertype and without send data, the CertificateDropEdit is Visible.", true, certificateDropEdit.Visible);
					nctsHeader.BH_HeaderType = NctsMovementType.Codes.DepartureAndArrival;
					AssertEquals("With BH_HeaderType DA and without send data, the CertificateDropEdit is not Visible.", false, certificateDropEdit.Visible);
					nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
					AssertEquals("With BH_HeaderType D and without send data, the CertificateDropEdit is Visible.", true, certificateDropEdit.Visible);
					nctsHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationSent;
					nctsHeader.BH_HeaderType = NctsMovementType.Codes.DepartureAndArrival;
					AssertEquals("With BH_HeaderType DA and send data MDS, the CertificateDropEdit is not Visible.", false, certificateDropEdit.Visible);
					nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
					AssertEquals("With BH_HeaderType A and without send data, the CertificateDropEdit is Visible.", true, certificateDropEdit.Visible);
					nctsHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationNotSent;
					nctsHeader.BH_HeaderType = NctsMovementType.Codes.DepartureAndArrival;
					AssertEquals("With BH_HeaderType DA and send data MDN, the CertificateDropEdit is not Visible.", false, certificateDropEdit.Visible);
				});
			}
		}

		public void TestAgreedLocationOfGoodsCodeNormalTextBox()
		{
			using (var form = new ZForm(nctsHeader))
			{
				form.Controls.Add(new DeclarationDetailsTabUserControl());
				form.Show();

				var agreedLocationOfGoods = (ZTextBox)form.Controls.Find("AgreedLocationOfGoodsCodeNormalTextBox", true).First();
				AssertEquals(false, agreedLocationOfGoods.Visible);
			}
		}

		public void TestAgreedLocationOfGoodsCodeNormalDropEdit()
		{
			using (var form = new ZForm(nctsHeader))
			{
				form.Controls.Add(new DeclarationDetailsTabUserControl());
				form.Show();

				var agreedLocationOfGoods = (ZDropEdit)form.Controls.Find("AgreedLocationOfGoodsCodeNormalDropEdit", true).First();
				AssertEquals(true, agreedLocationOfGoods.Visible);
			}
		}

		public void TestSendMessageForFieldSafetyAndSecurity()
		{
			nctsHeader.BH_FTZMove = false;
			nctsHeader.Factory.Save();

			using (var form = new ZForm(nctsHeader))
			{
				form.Controls.Add(new DeclarationDetailsTabUserControl());
				form.Show();
				ZFormModaliser.ShowDialogsInTest = false;
				CombineAssertions(() =>
				{
					nctsHeader.BH_FTZMove = true;
					AssertEquals("When the check box is checked and not have security data", null, UnitTestUserNotification.Instance.LastMessage.Text);

					nctsHeader.BH_FTZMove = false;
					AssertEquals("When the check box is not checked and not have security data", null, UnitTestUserNotification.Instance.LastMessage.Text);

					nctsHeader.AddSecurityData();
					nctsHeader.BH_FTZMove = true;
					AssertEquals("When the check box is checked and have security data", null, UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestControlButtonForAnswerSafetyAndSecurityCheckWithoutSafetyData()
		{
			nctsHeader.AddSecurityData();
			nctsHeader.BH_FTZMove = true;
			nctsHeader.Factory.Save();

			using (var form = new ZForm(nctsHeader))
			{
				form.Controls.Add(new DeclarationDetailsTabUserControl());
				form.Show();
				ZFormModaliser.ShowDialogsInTest = false;

				CombineAssertions(() =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					form.FireSaveButton();
					nctsHeader.BH_FTZMove = false;
					nctsHeader.MovementHeader.BM_TypeOfSecurity = "ZZZ";
					AssertEquals("Confirm to uncheck the box with dialog YES", false, nctsHeader.BH_FTZMove);

					nctsHeader.BH_FTZMove = true;
					nctsHeader.MovementHeader.BM_TypeOfSecurity = "BTH";

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					form.FireSaveButton();
					nctsHeader.BH_FTZMove = false;
					AssertEquals("Confirm to check the box with dialog NO", true, nctsHeader.BH_FTZMove);
				});
			}
		}

		public void TestNationalSimplificatorIndDropEdit()
		{
			using (var frm = new ZForm(nctsHeader))
			{
				frm.Controls.Add(new DeclarationDetailsTabUserControl());
				frm.Show();

				var natSimpIndDropEdit = (ZDropEdit)frm.Controls.Find("NationalSimplificatorIndDropEdit", true).First();
				AssertEquals(true, natSimpIndDropEdit.Visible);
			}
		}

		public void TestRepresentativeDocAddressControl()
		{
			using (var frm = new ZForm(nctsHeader))
			{
				frm.Controls.Add(new DeclarationDetailsTabUserControl());
				frm.Show();

				var representativeAddressControl = (ZDocAddressControl)frm.Controls.Find("RepresentativeDocAddressControl", true).First();
				AssertEquals(true, representativeAddressControl.Visible);
			}
		}

		public void TestDeclEmailAddrTextBox()
		{
			using (var frm = new ZForm(nctsHeader))
			{
				frm.Controls.Add(new DeclarationDetailsTabUserControl());
				frm.Show();

				var declEmailAddrTextBox = (ZTextBox)frm.Controls.Find("DeclEmailAddrTextBox", true).First();
				CombineAssertions(() =>
				{
					AssertEquals("DeclEmailAddrTextBox is visible", true, declEmailAddrTextBox.Visible);
					AssertEquals("DeclEmailAddrTextBox is readonly", true, declEmailAddrTextBox.ReadOnly);
				});
			}
		}

		public void TestCertificateDropEdit()
		{
			using (var form = new ZForm(nctsHeader))
			using (var control = new DeclarationDetailsTabUserControl())
			{
				AssertNotNull(control.FindSingle<ZDropEdit>("CertificateDropEdit"));
			}
		}

		public void TestClearanceInfoGroupBox()
		{
			using (var form = new ZForm(nctsHeader))
			{
				form.Controls.Add(new DeclarationDetailsTabUserControl());
				form.Show();

				CombineAssertions(() =>
				{
					var clearanceInfoGroupBox = (ZGroupBox)form.Controls.Find("ClearanceInfoGroupBox", true).First();
					AssertEquals("ClearanceInfoGroupBox is visible", true, clearanceInfoGroupBox.Visible);

					var clearanceNumberTextBox = (ZTextBox)form.Controls.Find("ClearanceNumberTextBox", true).First();
					AssertEquals("ClearanceNumberTextBox is visible", true, clearanceNumberTextBox.Visible);
					AssertEquals("ClearanceNumberTextBox is readonly", true, clearanceNumberTextBox.ReadOnly);

					var clearanceProcedureTextBox = (ZTextBox)form.Controls.Find("ClearanceProcedureTextBox", true).First();
					AssertEquals("ClearanceProcedureTextBox is visible", true, clearanceProcedureTextBox.Visible);
					AssertEquals("ClearanceProcedureTextBox is readonly", true, clearanceProcedureTextBox.ReadOnly);

					var tadPrintTextBox = (ZTextBox)form.Controls.Find("TADPrintTextBox", true).First();
					AssertEquals("TADPrintTextBox is visible", true, tadPrintTextBox.Visible);
					AssertEquals("TADPrintTextBox is readonly", true, tadPrintTextBox.ReadOnly);

					var clearanceDateDateEdit = (ZDateEdit)form.Controls.Find("ClearanceDateDateEdit", true).First();
					AssertEquals("ClearanceDateDateEdit is visible", true, clearanceDateDateEdit.Visible);
					AssertEquals("ClearanceDateDateEdit is readonly", true, clearanceDateDateEdit.ReadOnly);

					var arrivalLimitDateEdit = (ZDateEdit)form.Controls.Find("ArrivalLimitDateEdit", true).First();
					AssertEquals("ArrivalLimitDateEdit is visible", true, arrivalLimitDateEdit.Visible);
					AssertEquals("ArrivalLimitDateEdit is readonly", true, arrivalLimitDateEdit.ReadOnly);
				});
			}
		}

		public void TestSealsShouldBeSubclassOfBusinessObjectCollection()
		{
			var sealsInfo = nctsHeader.GetType().GetProperty("Seals", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
				?? nctsHeader.GetType().GetProperty("Seals", BindingFlags.Public | BindingFlags.Instance);
			var sealsType = sealsInfo.PropertyType;
			AssertEquals(true, TypeUtilities.IsSubclassOfBusinessObjectCollection(sealsType));
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		}
		NctsHeader nctsHeader;
	}
}
