using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	[TestedType(typeof(RelatedDocumentsUserControlWithGrid))]
	sealed class RelatedDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestAdditionalTabPage_SupportingDocument()
		{
			using (var control = new RelatedDocumentsUserControlWithGrid(RelatedDocumentsUserControlWithGrid.ResStringSupportingDocuments, RelatedDocumentsUserControlWithGrid.SupportingDocumentsTabSequence) as IAdditionalTabPage)
			{
				CombineAssertions("Additional Tab Page", () =>
				{
					AssertEquals("Caption", "Supporting Documents", control.AdditionalTabPageCaption.Caption);
					AssertEquals("Tab page sequence", 40, control.TabPageSequence);
				});
			}
		}

		public void TestAdditionalTabPage_PreviousDocument()
		{
			using (var control = new RelatedDocumentsUserControlWithGrid(RelatedDocumentsUserControlWithGrid.ResStringPreviousDocuments, RelatedDocumentsUserControlWithGrid.PreviousDocumentsTabSequence) as IAdditionalTabPage)
			{
				CombineAssertions("Additional Tab Page", () =>
				{
					AssertEquals("Caption", "Previous Documents", control.AdditionalTabPageCaption.Caption);
					AssertEquals("Tab page sequence", 50, control.TabPageSequence);
				});
			}
		}

		public void TestLayoutForRelatedDocumentsUserControlWithGrid()
		{
			AssertLayout(RelatedDocumentsUserControlWithGrid.ResStringSupportingDocuments, RelatedDocumentsUserControlWithGrid.SupportingDocumentsTabSequence);
			AssertLayout(RelatedDocumentsUserControlWithGrid.ResStringPreviousDocuments, RelatedDocumentsUserControlWithGrid.PreviousDocumentsTabSequence);
		}

		void AssertLayout(ResourceStringData resString, int tabSequence)
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();
			var supportingDocument = bill.SupportingDocuments.AddNew();

			using (var form = new ZForm(bill))
			using (var control = new RelatedDocumentsUserControlWithGrid(resString, tabSequence))
			{
				control.SetDataBinding(bill, string.Empty);
				form.Controls.Add(control);
				form.Show();

				var grid = control.FindSingle<ZGrid>("AdditionalInfosGrid");

				CombineAssertions(() =>
				{
					EUH7GUITestHelper.AssertGridLayout(grid,
						[
							(CusSupportingInfo.Schema.CSI_Code, typeof(ZCodeFindBoxColumnStyleInfo)),
							(CusSupportingInfo.Schema.CSI_ReferenceNumber, typeof(ZTextBoxColumnStyleInfo)),
							(SupportingDocument.Schema.DocumentDescription, typeof(ZTextBoxColumnStyleInfo))
						]);
				});

				var additionalInfosGroupBox = control.FindSingle<ZGroupBox>("AdditionalInfosGroupBox");
				AssertEquals(additionalInfosGroupBox.CaptionResourceString, resString);

				var foundDescriptionTextBox = control.FindSingle<ZTextBox>("DescriptionTextBox");
				AssertEquals(foundDescriptionTextBox.GetBindingMember(), "DocumentDescription");
			}
		}
	}
}
