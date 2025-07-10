using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.H7.GUI
{
	public partial class EUH7SimilarAddressesSelectionForm : ZChildForm
	{
		public EUH7SimilarAddressesSelectionForm(AsycudaBillSimilarAddressViewModelCollection viewModels) : base(viewModels)
		{
			ViewModels.CountChanged += OnViewModelsCountChanged;
		}

		AsycudaBillSimilarAddressViewModelCollection ViewModels => (AsycudaBillSimilarAddressViewModelCollection)BusinessEntity;

		OrgPatternMatch SelectedSimilarOrg => (OrgPatternMatch)SimilarOrgsDisplayGrid.GetFirstSelectedRow();

		AsycudaBillSimilarAddressViewModel SelectedBillAddress => (AsycudaBillSimilarAddressViewModel)BillAddressDisplayGrid.GetFirstSelectedRow();

		void CreateOrgButton_Click(object sender, EventArgs e)
		{
			if (IsBillAddressSelectionValid())
			{
				var factory = new BusinessObjectFactory();
				var newOrganisation = factory.New<OrgHeader>();
				SelectedBillAddress.FillOrgHeaderWithConsignmentDetails(newOrganisation);

				var form = ZControllerFactory.Create(ControllerIDs.Organisation).ShowFormForNewEntity(newOrganisation);
				form.Closed += OrganizationForm_OnClose;
				void OrganizationForm_OnClose(object sender, EventArgs e)
				{
					if (newOrganisation.IsInDatabase)
					{
						SetLinkingAddress(newOrganisation.MainAddress);
					}
					form.Closed -= OrganizationForm_OnClose;
				}
			}
		}

		void SelectOrgButton_Click(object sender, EventArgs e)
		{
			if (IsBillAddressSelectionValid() && IsSimilarAddressSelectionValid())
			{
				SetLinkingAddress(SelectedSimilarOrg.Address);
			}
		}

		void IgnoreButton_Click(object sender, EventArgs e)
		{
			if (IsBillAddressSelectionValid())
			{
				SetLinkingAddress(null);
			}
		}

		void SetLinkingAddress(OrgAddress address)
		{
			var pk = address != null ? address.PK : Guid.Empty;
			SelectedBillAddress.SetLinkingAddress(pk);
			ViewModels.Remove(SelectedBillAddress);
		}

		bool IsBillAddressSelectionValid()
		{
			if (BillAddressDisplayGrid.SelectedRowCount < 1)
			{
				Globals.Message.ShowError(SelectABillAddress);
				return false;
			}

			if (BillAddressDisplayGrid.SelectedRowCount > 1)
			{
				Globals.Message.ShowError(SelectOnlyOneBillAddress);
				return false;
			}

			return true;
		}

		bool IsSimilarAddressSelectionValid()
		{
			if (SimilarOrgsDisplayGrid.SelectedRowCount < 1)
			{
				Globals.Message.ShowError(SelectAnAddress);
				return false;
			}

			if (SimilarOrgsDisplayGrid.SelectedRowCount > 1)
			{
				Globals.Message.ShowError(SelectOnlyOneAddress);
				return false;
			}

			return true;
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			if (ViewModels.Count > 0)
			{
				var result = Globals.Message.Show(CancelConversionMessage, CancelConversionCaption, ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Question);
				if (result == ZDialogResult.No)
				{
					e.Cancel = true;
				}
				else
				{
					ViewModels.CountChanged -= OnViewModelsCountChanged;
					ViewModels.ResetBillAddresses();
				}
			}
			else
			{
				DialogResult = DialogResult.OK;
			}

			base.OnFormClosing(e);
		}

		void OnViewModelsCountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (ViewModels.Count > 0)
			{
				BillAddressDisplayGrid.Select(BillAddressDisplayGrid.CurrentRowIndex);
			}
			else
			{
				ViewModels.CountChanged -= OnViewModelsCountChanged;
				ViewModels.ApplyLinkBillAddresses();
				Close();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnHookEvents();
			}
			base.Dispose(disposing);
		}

		void UnHookEvents()
		{
			CreateOrgButton.Click -= CreateOrgButton_Click;
			SelectOrgButton.Click -= SelectOrgButton_Click;
			IgnoreButton.Click -= IgnoreButton_Click;
		}

		static string SelectABillAddress => Res.GetString("db4fce4c-603c-4d67-98ff-eee6e421c66f", "Please select a bill address.");

		static string SelectOnlyOneBillAddress => Res.GetString("c6c27f32-d1e9-4889-9ded-a2e5df1c7428", "Please select only one bill address.");

		static string SelectAnAddress => Res.GetString("b2cf6f99-b89d-458b-a1f1-2a56c4bf0b3f", "Please select an address.");

		static string SelectOnlyOneAddress => Res.GetString("394c8789-5175-485a-acd3-bf82cbaa2218", "Please select only one address.");

		static string CancelConversionMessage => Res.GetString("56b97422-83bd-4369-8ce8-c77d0d31b505", "Do you want to cancel the conversion to Organizations? Parties already matched with Organizations through this form will return to their original value.");

		static string CancelConversionCaption => Res.GetString("e46ca151-4772-4410-ab0a-1acfaa38a693", "Cancellation Confirmation");
	}
}
