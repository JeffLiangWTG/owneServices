using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Manifest.ICS2.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI.Test
{
	sealed class HRCMScreeningResultUserControlTest : TestCaseWithFactory
	{
		public void TestLayout_NormalControls()
		{
			AssertLayout(control =>
			{
				CombineAssertions("Check user controls", () =>
				{
					var resultDropEdit = control.FindSingle<ZDropEdit>("ResultDropEdit");
					Assert("Result Drop Edit is visible", resultDropEdit.Visible);
					Assert("Result Drop Edit is enabled", resultDropEdit.Enabled);

					var personTypeDropEdit = control.FindSingle<ZDropEdit>("PersonTypeDropEdit");
					Assert("Person Type Drop Edit is visible", personTypeDropEdit.Visible);
					Assert("Person Type Drop Edit is enabled", personTypeDropEdit.Enabled);

					var authorizedPersonGuidFindBox = control.FindSingle<ZGuidFindBox>("AuthorizedPersonGuidFindBox");
					Assert("Authorized Person Guid Find Box is visible", authorizedPersonGuidFindBox.Visible);
					Assert("Authorized Person Guid Find Box is enabled", authorizedPersonGuidFindBox.Enabled);

					var authorizedPersonNameTextBox = control.FindSingle<ZTextBox>("IdentifierTextBox");
					Assert("Authorized Person Name Text Box is visible", authorizedPersonNameTextBox.Visible);
					Assert("Authorized Person Name Text Box is enabled", authorizedPersonNameTextBox.Enabled);

					var authorizedPersonIdentifierTextBox = control.FindSingle<ZTextBox>("AuthorizedPersonNameTextBox");
					Assert("Authorized Person Identifier Text Box is visible", authorizedPersonIdentifierTextBox.Visible);
					Assert("Authorized Person Identifier Text Find Box is enabled", authorizedPersonIdentifierTextBox.Enabled);
				});
			});
		}

		public void TestLayout_AdditionalInfo_NormalControls()
		{
			AssertLayout(control =>
			{
				var tabControl = control.FindSingle<ZTabControl>("BillScreeningDetailsTabControl");
				tabControl.SelectTab("BillScreeningAdditionalInfoTabPage");

				CombineAssertions("Check Additional Info user controls", () =>
				{
					var typeDropEdit = control.FindSingle<ZDropEdit>("SubTypeDropEdit");
					Assert("Sub Type Drop Edit is visible", typeDropEdit.Visible);
					Assert("Sub Type Drop Edit is enabled", typeDropEdit.Enabled);

					var codeDropEdit = control.FindSingle<ZDropEdit>("CodeDropEdit");
					Assert("Code Drop Edit is visible", codeDropEdit.Visible);
					Assert("Code Drop Edit is enabled", codeDropEdit.Enabled);

					var detailsTextBox = control.FindSingle<ZTextBox>("DetailsTextBox");
					Assert("Details Text Box is visible", detailsTextBox.Visible);
					Assert("Details Text Box is enabled", detailsTextBox.Enabled);
				});
			});
		}

		public void TestLayout_BillScreeningsGrid()
		{
			AssertLayout(control =>
			{
				EUICS2GUITestHelper.AssertGridLayout(control, nameof(control.BillScreeningsGrid),
					nameof(AsycudaBillScreening.ASR_Result),
					nameof(AsycudaBillScreening.ASR_PER_AuthorizedPerson),
					nameof(AsycudaBillScreening.ASR_AuthorizedPersonType),
					nameof(AsycudaBillScreening.FacilityPlace) + "+" + nameof(ICS2JobDocAddress.E2_City),
					nameof(AsycudaBillScreening.FacilityPlace) + "+" + nameof(ICS2JobDocAddress.E2_RN_NKCountryCode),
					nameof(AsycudaBillScreening.FacilityPlace) + "+" + nameof(ICS2JobDocAddress.SubDivision),
					nameof(AsycudaBillScreening.FacilityPlace) + "+" + nameof(ICS2JobDocAddress.E2_Address1),
					nameof(AsycudaBillScreening.FacilityPlace) + "+" + nameof(ICS2JobDocAddress.E2_Postcode),
					nameof(AsycudaBillScreening.FacilityPlace) + "+" + nameof(ICS2JobDocAddress.E2_Address2),
					nameof(AsycudaBillScreening.FacilityPlace) + "+" + nameof(ICS2JobDocAddress.Number),
					nameof(AsycudaBillScreening.FacilityPlace) + "+" + nameof(ICS2JobDocAddress.POBox),
					nameof(AsycudaBillScreening.ASR_TransportNumberType),
					nameof(AsycudaBillScreening.ASR_TransportNumber));
			});
		}

		public void TestLayout_ScreeningMethodsGrid()
		{
			AssertLayout(control =>
			{
				CombineAssertions("Assert Screening Method grid", () =>
				{
					EUICS2GUITestHelper.AssertGridLayout(control, "ScreeningMethodsGrid", "CSI_Code", "ScreeningMethodDescription");
					EUICS2GUITestHelper.AssertReadOnlyGridColumns(control, "ScreeningMethodsGrid", shouldBeReadOnly: false, "CSI_Code");
					EUICS2GUITestHelper.AssertReadOnlyGridColumns(control, "ScreeningMethodsGrid", shouldBeReadOnly: true, "ScreeningMethodDescription");
				});
			});
		}

		public void TestLayout_AttachmentsGrid()
		{
			AssertLayout(control =>
			{
				CombineAssertions("Assert Attachments grid", () =>
				{
					EUICS2GUITestHelper.AssertGridLayout(control, "AttachmentsGrid", "CSD_StorageDocReference", "CSD_Description");
					EUICS2GUITestHelper.AssertReadOnlyGridColumns(control, "AttachmentsGrid", shouldBeReadOnly: false, "CSD_StorageDocReference");
					EUICS2GUITestHelper.AssertReadOnlyGridColumns(control, "AttachmentsGrid", shouldBeReadOnly: true, "CSD_Description");
				});
			});
		}

		public void TestLayout_AdditionalInfo_AdditionalInfoGrid()
		{
			AssertLayout(control =>
			{
				var tabControl = control.FindSingle<ZTabControl>("BillScreeningDetailsTabControl");
				tabControl.SelectTab("BillScreeningAdditionalInfoTabPage");

				CombineAssertions("Assert Additional Info grid", () =>
				{
					EUICS2GUITestHelper.AssertGridLayout(control, "AdditionalInfosGrid", "CSI_SubType", "CSI_Code", "CSI_Description");
					EUICS2GUITestHelper.AssertReadOnlyGridColumns(control, "AdditionalInfosGrid", shouldBeReadOnly: false, "CSI_SubType", "CSI_Code", "CSI_Description");
				});
			});
		}

		void AssertLayout(Action<HRCMScreeningResultUserControl> assertion)
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			using (var form = new ZForm(header))
			using (var control = new HRCMScreeningResultUserControl())
			{
				control.SetDataBinding(header, string.Empty);

				form.Controls.Add(control);
				form.Show();

				assertion.Invoke(control);
			}
		}
	}
}
