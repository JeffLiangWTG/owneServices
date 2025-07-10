using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(GuaranteeAccessCodesUserSelectionForm))]
	sealed class GuaranteeAccessCodesUserSelectionFormTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestFormHeading()
		{
			using (var form = GetForm() as ZForm)
			{
				AssertEquals("FormHeading", "Guarantee Access Codes", form.FormHeading);
				form.Show();
				AssertEquals("Text", "Guarantee Access Codes", form.Text);
			}
		}

		public void TestSize()
		{
			using (var form = GetForm())
			{
				form.Show();
				AssertEquals("MinimumSize", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 293, true), form.MinimumSize);
			}
		}

		[RequiresSTA]
		public void TestSelectGuarantee()
		{
			using (var form = GetForm())
			{
				form.Show();
				var guaranteesGrid = form.GuaranteesGrid;

				guaranteesGrid.ListManager.Position = 0;
				var currentGuarantee0 = (GuaranteeAccessCodesUserSelectionObject)guaranteesGrid.ListManager?.GetCurrent();
				Assert("Guarantee at position 0 should have IsSelected == false at the beginning", !currentGuarantee0.IsSelected);

				currentGuarantee0.IsSelected = true;
				Assert("Guarantee at position 0 should have IsSelected == true after selection", currentGuarantee0.IsSelected);

				guaranteesGrid.ListManager.Position = 1;
				var currentGuarantee1 = (GuaranteeAccessCodesUserSelectionObject)guaranteesGrid.ListManager?.GetCurrent();
				Assert("Guarantee at position 1 should have IsSelected == false at the beginning", !currentGuarantee1.IsSelected);

				currentGuarantee1.IsSelected = true;
				Assert("Guarantee at position 0 should have IsSelected == false after selection of Guarantee at position 1", !currentGuarantee0.IsSelected);
				Assert("Guarantee at position 1 should have IsSelected == true ", currentGuarantee1.IsSelected);
			}
		}

		public void TestConfirmButton_Readonly()
		{
			using (var form = GetForm())
			{
				form.Show();
				var guaranteesGrid = form.GuaranteesGrid;
				var confirmButton = form.ConfirmButton;

				guaranteesGrid.ListManager.Position = 0;
				var currentGuarantee0 = (GuaranteeAccessCodesUserSelectionObject)guaranteesGrid.ListManager?.GetCurrent();
				Assert("ConfirmButton is readonly if nothing is selected", confirmButton.ReadOnly);

				currentGuarantee0.IsSelected = true;
				Assert("ConfirmButton is NOT readonly if an element is selected", !confirmButton.ReadOnly);

				currentGuarantee0.IsSelected = false;
				Assert("ConfirmButton is readonly if nothing is selected", confirmButton.ReadOnly);

				guaranteesGrid.ListManager.Position = 1;
				var currentGuarantee1 = (GuaranteeAccessCodesUserSelectionObject)guaranteesGrid.ListManager?.GetCurrent();
				currentGuarantee1.IsSelected = true;
				Assert("ConfirmButton is NOT readonly if an element is selected", !confirmButton.ReadOnly);
			}
		}

		public void TestGuaranteesGrid_AvailableColumns()
		{
			using (var form = new GuaranteeAccessCodesUserSelectionForm(guaranteesObjectCollection))
			{
				AssertSequencesEqual("GuaranteesUserControl available Columns", new[] { nameof(GuaranteeAccessCodesUserSelectionObject.GuaranteeType), nameof(GuaranteeAccessCodesUserSelectionObject.GuaranteeReferenceNumber), nameof(GuaranteeAccessCodesUserSelectionObject.AccessCode), nameof(GuaranteeAccessCodesUserSelectionObject.IsSelected), }, form.GuaranteesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Where(col => !col.IsUnavailable).Select(x => x.ColumnName));
			}
		}

		public void TestGuaranteesGroupBox_Caption()
		{
			using (var form = new GuaranteeAccessCodesUserSelectionForm(guaranteesObjectCollection))
			{
				AssertEquals("GuaranteesGroupBox.CaptionResourceString", "Guarantees", form.GuaranteesGroupBox.CaptionResourceString.Caption);
			}
		}

		public void TestHidePW_Password()
		{
			using (var form = new GuaranteeAccessCodesUserSelectionForm(guaranteesObjectCollection))
			{
				var passwordColumn = (ZTextBoxColumnStyleInfo)form.GuaranteesGrid.GetColumnStyle(nameof(GuaranteeAccessCodesUserSelectionObject.AccessCode));
				AssertEquals('*', passwordColumn.PasswordChar);
			}
		}

		public void TestButtonsCaptions()
		{
			using (var form = new GuaranteeAccessCodesUserSelectionForm(guaranteesObjectCollection))
			{
				form.Show();
				CombineAssertions(() =>
				{
					var closeButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "CloseButton");
					AssertNotNull(closeButton);
					AssertEquals("CloseButton.Caption", "Cancel", closeButton.CaptionResourceString.Caption);

					var confirmButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "ConfirmButton");
					AssertNotNull(confirmButton);
					AssertEquals("ConfirmButton.Caption", "Update Access Code", confirmButton.CaptionResourceString.Caption);
				});
			}
		}

		protected override Form GetFormToBashCore()
		{
			SetUp();
			return GetForm();
		}

		GuaranteeAccessCodesUserSelectionForm GetForm()
		{
			return new GuaranteeAccessCodesUserSelectionForm(guaranteesObjectCollection);
		}

		protected override void SetUp()
		{
			base.SetUp();

			NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var cusGuarantee = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			cusGuarantee.CPH_StartDate = ZDate.Today.AddMonths(-1);
			cusGuarantee.CPH_EndDate = ZDate.Today.AddMonths(1);
			cusGuarantee.CPH_Number = "GUA1";
			cusGuarantee.CPH_OH_PermitHolder = org1.PK;
			cusGuarantee.CPH_Type = EU.Business.CodeDescriptionPairLists.EUGuaranteeTypeList.Codes.TRA;
			cusGuarantee.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			cusGuarantee.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;

			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.Principal.E2_OA_Address = org1.MainAddress.PK;
			var guarantee1 = header.MovementHeader.Guarantees.AddNew();
			guarantee1.PW_BondType = "1";
			guarantee1.PW_BondNumber = "GUA1";
			guarantee1.PW_Password = "DEF";
			var guarantee2 = header.MovementHeader.Guarantees.AddNew();
			guarantee2.PW_BondType = "2";
			guarantee2.PW_BondNumber = "GUA1";
			guarantee2.PW_Password = "XYZ";
			guaranteesObjectCollection = new GuaranteeAccessCodesUserSelectionObjectCollection(header);
			guaranteesObjectCollection.PopulateElements();
		}
		NctsHeader header;
		GuaranteeAccessCodesUserSelectionObjectCollection guaranteesObjectCollection;
	}
}
