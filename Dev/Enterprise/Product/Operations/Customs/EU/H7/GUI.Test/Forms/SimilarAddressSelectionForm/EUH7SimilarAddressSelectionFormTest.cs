using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	[TestedType(typeof(EUH7SimilarAddressesSelectionForm))]
	sealed class EUH7SimilarAddressSelectionFormTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestBillDisplayGridHasAllNeccessaryColumns()
		{
			using (var form = GetFormToBashCore())
			{
				var billAddressGrid = form.Controls.Find("BillAddressDisplayGrid", true)[0] as ZDisplayGrid;
				AssertNotNull(billAddressGrid);
				AssertEquals(9, billAddressGrid.ColumnStyles.Count);
				var columnStyles = billAddressGrid.ColumnStyles.OfType<ZGridColumnInfo>();
				CombineAssertions(() =>
				{
					Assert(columnStyles.Any(c => c.ColumnName == "BillNumber"));
					Assert(columnStyles.Any(c => c.ColumnName == "AddressType"));
					Assert(columnStyles.Any(c => c.ColumnName == "OrgName"));
					Assert(columnStyles.Any(c => c.ColumnName == "Address1"));
					Assert(columnStyles.Any(c => c.ColumnName == "Address2"));
					Assert(columnStyles.Any(c => c.ColumnName == "City"));
					Assert(columnStyles.Any(c => c.ColumnName == "State"));
					Assert(columnStyles.Any(c => c.ColumnName == "Postcode"));
					Assert(columnStyles.Any(c => c.ColumnName == "Country"));
				});
			}
		}

		public void TestSelectAddressOnTheBottomGrid()
		{
			var orgHeader = BuildSimilarOrgParty();
			using (var form = new EUH7SimilarAddressesSelectionForm(viewModelsWithSingleBill))
			{
				form.Show();

				var billGrid = form.Controls.Find("BillAddressDisplayGrid", true)[0] as ZDisplayGrid;
				AssertEquals(1, ((AsycudaBillSimilarAddressViewModelCollection)billGrid.DataSource).Count);

				billGrid.Select(0);
				var similarAddressGrid = form.Controls.Find("SimilarOrgsDisplayGrid", true)[0] as ZDisplayGrid;
				similarAddressGrid.Select(0);

				var selectAddressButton = form.Controls.Find("SelectOrgButton", true).Single() as ZButton;
				selectAddressButton.PerformClick();

				AssertEquals(orgHeader.MainAddress.PK, bill1.ABL_OA_Consignee);
			}
		}

		public void TestOpenNewOrganizationForm()
		{
			using (var form = new EUH7SimilarAddressesSelectionForm(viewModelsWithSingleBill))
			{
				form.Show();

				var billGrid = form.Controls.Find("BillAddressDisplayGrid", true)[0] as ZDisplayGrid;
				AssertEquals(1, ((AsycudaBillSimilarAddressViewModelCollection)billGrid.DataSource).Count);

				billGrid.Select(0);

				var newOrgButton = form.Controls.Find("CreateOrgButton", true).Single() as ZButton;
				newOrgButton.PerformClick();

				var organizationForm = Application.OpenForms.OfType<ZOrganisationsForm>().Single();
				var orgHeader = organizationForm.BusinessEntity as OrgHeader;

				CombineAssertions(() =>
				{
					AssertEquals("COCONUT ENTERTAINMENT", orgHeader.OH_FullName);
					AssertEquals("Cardigan Street", orgHeader.MainAddress.OA_Address1);
				});

				orgHeader.OH_RL_NKClosestPort = "AUSYD";
				organizationForm.FireSaveButton();
				organizationForm.Close();

				CombineAssertions(() =>
				{
					AssertEquals(orgHeader.MainAddress.PK, bill1.ABL_OA_Consignee);
					Assert(viewModelsWithSingleBill.AnyBillLinkedWithOrg);
				});
			}
		}

		public void TestIgnoreSelectedBill()
		{
			using (var form = new EUH7SimilarAddressesSelectionForm(viewModelsWithSingleBill))
			{
				form.Show();

				var billGrid = form.Controls.Find("BillAddressDisplayGrid", true)[0] as ZDisplayGrid;
				AssertEquals(1, ((AsycudaBillSimilarAddressViewModelCollection)billGrid.DataSource).Count);

				billGrid.Select(0);

				var ignoreButton = form.Controls.Find("IgnoreButton", true).Single() as ZButton;
				ignoreButton.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals(Guid.Empty, bill1.ABL_OA_Consignee);
					Assert(!viewModelsWithTwoBills.AnyBillLinkedWithOrg);
				});
			}
		}

		public void TestBillAddressProcessedAndRemovedFromUpperGrid()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();

				var billGrid = form.Controls.Find("BillAddressDisplayGrid", true)[0] as ZDisplayGrid;
				AssertEquals(2, ((AsycudaBillSimilarAddressViewModelCollection)billGrid.DataSource).Count);

				billGrid.Select(0);

				var newOrgButton = form.Controls.Find("CreateOrgButton", true).Single() as ZButton;
				newOrgButton.PerformClick();

				var organizationForm = Application.OpenForms.OfType<ZOrganisationsForm>().Single();
				var orgHeader = organizationForm.BusinessEntity as OrgHeader;
				orgHeader.OH_RL_NKClosestPort = "AUSYD";

				organizationForm.FireSaveButton();
				organizationForm.Close();

				CombineAssertions(() =>
				{
					AssertEquals(1, ((AsycudaBillSimilarAddressViewModelCollection)billGrid.DataSource).Count);
					Assert(billGrid.IsSelected(0));
				});
			}
		}

		public void TestBillAddressesAppliedOnlyAfterAllAddressesProcessed()
		{
			var orgHeader = BuildSimilarOrgParty();
			using (var form = GetFormToBashCore())
			{
				form.Show();

				var billGrid = form.Controls.Find("BillAddressDisplayGrid", true)[0] as ZDisplayGrid;
				AssertEquals(2, ((AsycudaBillSimilarAddressViewModelCollection)billGrid.DataSource).Count);

				billGrid.Select(0);
				var similarAddressGrid = form.Controls.Find("SimilarOrgsDisplayGrid", true)[0] as ZDisplayGrid;
				similarAddressGrid.Select(0);

				var selectAddressButton = form.Controls.Find("SelectOrgButton", true).Single() as ZButton;
				selectAddressButton.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals(1, ((AsycudaBillSimilarAddressViewModelCollection)billGrid.DataSource).Count);
					AssertEquals(Guid.Empty, bill1.ABL_OA_Consignee);
					Assert(billGrid.IsSelected(0));
				});

				similarAddressGrid.Select(0);
				selectAddressButton.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals(orgHeader.MainAddress.PK, bill1.ABL_OA_Consignee);
					AssertEquals(orgHeader.MainAddress.PK, bill2.ABL_OA_Consignee);
					Assert(viewModelsWithTwoBills.AnyBillLinkedWithOrg);
				});
			}
		}

		[RequiresSTA]
		public void TestBillAddressesDontChangeGivenUserManuallyCloseForm()
		{
			BuildSimilarOrgParty();
			using (var form = GetFormToBashCore())
			{
				form.Show();

				var billGrid = form.Controls.Find("BillAddressDisplayGrid", true)[0] as ZDisplayGrid;
				AssertEquals(2, ((AsycudaBillSimilarAddressViewModelCollection)billGrid.DataSource).Count);

				billGrid.Select(0);
				var similarAddressGrid = form.Controls.Find("SimilarOrgsDisplayGrid", true)[0] as ZDisplayGrid;
				similarAddressGrid.Select(0);

				var selectAddressButton = form.Controls.Find("SelectOrgButton", true).Single() as ZButton;
				selectAddressButton.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals(1, ((AsycudaBillSimilarAddressViewModelCollection)billGrid.DataSource).Count);
					AssertEquals(Guid.Empty, bill1.ABL_OA_Consignee);
					Assert(billGrid.IsSelected(0));
				});

				UnitTestUserNotification.Instance.AddYesAnswer();
				form.Close();

				CombineAssertions(() =>
				{
					AssertEquals(Guid.Empty, bill1.ABL_OA_Consignee);
					AssertEquals(Guid.Empty, bill2.ABL_OA_Consignee);
					Assert(!viewModelsWithTwoBills.AnyBillLinkedWithOrg);
				});
			}
		}

		public void TestShowCancellationConfirmationWhenClickingCancelButton()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();

				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
				form.Close();
				var lastMessage = UnitTestUserNotification.Instance.LastMessage;

				CombineAssertions(() =>
				{
					Assert("Message type was question", lastMessage.WasQuestion);
					AssertEquals("Message text", "Do you want to cancel the conversion to Organizations? Parties already matched with Organizations through this form will return to their original value.", lastMessage.Text);
					AssertEquals(1, Application.OpenForms.OfType<EUH7SimilarAddressesSelectionForm>().Count());
				});

				UnitTestUserNotification.Instance.AddYesAnswer();
				form.Close();

				AssertEquals(0, Application.OpenForms.OfType<EUH7SimilarAddressesSelectionForm>().Count());
			}
		}

		public void TestShowErrorsWhenSelectingBillAddress()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();

				var billGrid = form.Controls.Find("BillAddressDisplayGrid", true)[0] as ZDisplayGrid;

				AssertErrorMessageShowsUp(form, billGrid, "IgnoreButton");
				AssertErrorMessageShowsUp(form, billGrid, "CreateOrgButton");
				AssertErrorMessageShowsUp(form, billGrid, "SelectOrgButton");
			}
		}

		void AssertErrorMessageShowsUp(Form form, ZDisplayGrid billGrid, string buttonName)
		{
			billGrid.UnSelectAll();
			var ignoreButton = form.Controls.Find(buttonName, true).Single() as ZButton;
			ignoreButton.PerformClick();
			AssertEquals("Please select a bill address.", UnitTestUserNotification.Instance.LastMessage.Text);

			billGrid.SelectAllElements();
			ignoreButton.PerformClick();
			AssertEquals("Please select only one bill address.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		[RequiresSTA]
		public void TestShowErrorsWhenSelectingSimilarAddress()
		{
			BuildSimilarOrgParty();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "Org";
			orgHeader.OH_FullName = "COCONUT ENTERTAINMENT";
			orgHeader.MainAddress.OA_Address1 = "Cardigan Street";
			orgHeader.MainAddress.OA_City = "Test City";
			Factory.Save();

			using (var form = GetFormToBashCore())
			{
				form.Show();

				var billGrid = form.Controls.Find("BillAddressDisplayGrid", true)[0] as ZDisplayGrid;
				billGrid.Select(0);

				var similarAddressGrid = form.Controls.Find("SimilarOrgsDisplayGrid", true)[0] as ZDisplayGrid;
				var selectAddressButton = form.Controls.Find("SelectOrgButton", true).Single() as ZButton;
				selectAddressButton.PerformClick();
				AssertEquals("Please select an address.", UnitTestUserNotification.Instance.LastMessage.Text);

				similarAddressGrid.SelectAllElements();
				selectAddressButton.PerformClick();
				AssertEquals("Please select only one address.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new EUH7SimilarAddressesSelectionForm(viewModelsWithTwoBills);
		}

		OrgHeader BuildSimilarOrgParty()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "Org1";
			orgHeader.OH_FullName = "COCONUT ENTERTAINMENT";
			orgHeader.MainAddress.OA_Address1 = "Cardigan Street";
			orgHeader.MainAddress.OA_City = "Test City";
			Factory.Save();

			return orgHeader;
		}

		protected override void SetUp()
		{
			var shipperParty = Factory.NewWithValidTestData<OrgHeader>();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			bill1 = header.Bills.AddNew();
			bill1.ABL_ConsigneeName = "COCONUT ENTERTAINMENT";
			bill1.ABL_ConsigneeStreet1 = "Cardigan Street";
			bill1.ABL_ConsigneeCity = "DUB";
			bill1.ABL_RN_NKConsigneeCountry = "IE";
			bill1.ABL_ConsigneeState = "NSW";
			bill1.ABL_ConsigneePostcode = "123456";
			bill1.ABL_OA_Shipper = shipperParty.MainAddress.PK;

			bill2 = header.Bills.AddNew();
			bill2.ABL_ConsigneeName = "COCONUT ENTERTAINMENT";
			bill2.ABL_ConsigneeStreet1 = "Cardigan Street";
			bill2.ABL_OA_Shipper = shipperParty.MainAddress.PK;

			viewModelsWithTwoBills = new AsycudaBillSimilarAddressViewModelCollection(new List<AsycudaBill> { bill1, bill2 });
			viewModelsWithSingleBill = new AsycudaBillSimilarAddressViewModelCollection(new List<AsycudaBill> { bill1 });
		}

		AsycudaBill bill1;
		AsycudaBill bill2;
		AsycudaBillSimilarAddressViewModelCollection viewModelsWithSingleBill;
		AsycudaBillSimilarAddressViewModelCollection viewModelsWithTwoBills;
	}
}
