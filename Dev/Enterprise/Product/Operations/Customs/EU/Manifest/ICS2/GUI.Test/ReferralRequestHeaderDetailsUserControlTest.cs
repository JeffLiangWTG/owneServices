using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Manifest.ICS2.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI.Test
{
	sealed class ReferralRequestHeaderDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestLayout()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			using (var form = new ZForm(header))
			using (var control = new ReferralRequestUserControl())
			{
				control.SetDataBinding(header, string.Empty);

				form.Controls.Add(control);
				form.Show();

				var requestHeaderDetailsUserControl = control.FindSingle<ReferralRequestHeaderDetailsUserControl>("EUICS2ReferralRequestHeaderDetailsUserControl");
				AssertEquals("Request header details user control is visible", true, requestHeaderDetailsUserControl.Visible);

				EUICS2GUITestHelper.AssertGridLayout
				(
					requestHeaderDetailsUserControl,
					"zGridRequestInformations",
					("CSI_Code", typeof(ZDropEditColumnStyleInfo)),
					("CodeDescription", typeof(ZTextBoxColumnStyleInfo)),
					("CSI_SubType", typeof(ZDropEditColumnStyleInfo)),
					("SubTypeDescription", typeof(ZTextBoxColumnStyleInfo)),
					("CSI_Description", typeof(ZTextBoxColumnStyleInfo))
				);

				EUICS2GUITestHelper.AssertGridLayout
				(
					requestHeaderDetailsUserControl,
					"zGridSupportingDocuments",
					("CSI_Code", typeof(ZCodeFindBoxColumnStyleInfo)),
					("CSI_ReferenceNumber", typeof(ZTextBoxColumnStyleInfo)),
					("DocumentDescription", typeof(ZTextBoxColumnStyleInfo))
				);

				EUICS2GUITestHelper.AssertGridLayout
				(
					requestHeaderDetailsUserControl,
					"zGridRequestResponses",
					("CSI_Code", typeof(ZDropEditColumnStyleInfo)),
					("CodeDescription", typeof(ZTextBoxColumnStyleInfo)),
					("CSI_SubType", typeof(ZDropEditColumnStyleInfo)),
					("SubTypeDescription", typeof(ZTextBoxColumnStyleInfo)),
					("CSI_Description", typeof(ZTextBoxColumnStyleInfo))
				);

				EUICS2GUITestHelper.AssertGridLayout
				(
					requestHeaderDetailsUserControl,
					"zGridAttachments",
					("CSD_StorageDocReference", typeof(ZGuidDropEditColumnStyleInfo)),
					("CSD_DocType", typeof(ZTextBoxColumnStyleInfo)),
					("CSD_Description", typeof(ZTextBoxColumnStyleInfo))
				);
			}
		}

		[RequiresSTA]
		public void TestReadOnlyControls()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			using (var form = new ZForm(header))
			using (var control = new ReferralRequestUserControl())
			{
				control.SetDataBinding(header, string.Empty);

				form.Controls.Add(control);
				form.Show();

				var requestHeaderDetailsUserControl = control.FindSingle<ReferralRequestHeaderDetailsUserControl>("EUICS2ReferralRequestHeaderDetailsUserControl");

				CombineAssertions("readonly controls", () =>
				{
					Assert("Supporting Documents", requestHeaderDetailsUserControl.FindSingle<ZGrid>("zGridSupportingDocuments").ReadOnly);
					Assert("Request Informations", requestHeaderDetailsUserControl.FindSingle<ZGrid>("zGridRequestInformations").ReadOnly);
					Assert("Message Element", requestHeaderDetailsUserControl.FindSingle<ZTextBox>("MessageElementTextBox").ReadOnly);
					Assert("Request Id", requestHeaderDetailsUserControl.FindSingle<ZTextBox>("IdentifierTextBox").ReadOnly);
					Assert("House Bill Number", requestHeaderDetailsUserControl.FindSingle<ZDropEdit>("HouseBillNumberDropEdit").ReadOnly);
					Assert("Request Type", requestHeaderDetailsUserControl.FindSingle<ZDropEdit>("RequestTypeDropEdit").ReadOnly);
				});
			}
		}
	}
}
