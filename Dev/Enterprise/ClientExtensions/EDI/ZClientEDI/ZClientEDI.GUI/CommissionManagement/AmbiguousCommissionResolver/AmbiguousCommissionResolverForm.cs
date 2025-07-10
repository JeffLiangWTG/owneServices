using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.GUI;
using Enterprise.Client.EDI.CommissionManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.CommissionManagement.GUI
{
	public partial class AmbiguousCommissionResolverForm : ZChildForm
	{
		#region Constructors

		public AmbiguousCommissionResolverForm()
		{
		}

		public AmbiguousCommissionResolverForm(AmbiguousCommissionResolver resolver)
			: base(resolver)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();

			if (!DesignModeFinder.IsDesigning)
			{
				this.BackColor = SystemDataRegistry.Instance.ColorTheme.TabBackgroundColor; // Set BackColor before calling InitializeComponent() so that child checkboxes inherit the BackColor
			}
			InitializeComponent();
		}

		#endregion

		#region BusinessEntity

		public new AmbiguousCommissionResolver BusinessEntity
		{
			get { return (AmbiguousCommissionResolver)base.BusinessEntity; }
		}

		AmbiguousCommissionResolverFilterBusinessObject FilterBizObj
		{
			get { return BusinessEntity.FilterBizObj; }
		}

		#endregion

		#region Load

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			AddPossibleOverallItemsGridMenuItems();
			SetupInvoiceFilters();

			BusinessEntity.LoadResolveItems();
		}

		#endregion

		#region Find

		void FindButton_Click(object sender, EventArgs e)
		{
			var filterBizObj = BusinessEntity.FilterBizObj;
			filterBizObj.RunPreSaveValidation();

			if (filterBizObj.HasErrors)
			{
				var message = new ZNotificationCollector(filterBizObj, false, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).ToMessageListString();
				Globals.Message.ShowError(
					message,
					Res.GetString("c867f90b-e6fa-4000-b99a-8872d051aeac", "Errors..."));
			}
			else
			{
				BusinessEntity.LoadResolveItems();
				if (BusinessEntity.ResolveItemCollection.Count == 0)
				{
					Globals.Message.ShowWarning(
						Res.GetString("18a8408d-5536-4977-b977-bd6fd7d25d9b", "No ambiguous commissions found."),
						Res.GetString("a5ebd819-4f7f-4ef2-bedd-24d248dde5be", "Find Result"));
				}
			}
		}

		#endregion

		#region Resolve

		void ResolveButton_Click(object sender, EventArgs e)
		{
			var resolveItemsToResolve = BusinessEntity.ResolveItems.Where(x => x.ResolutionProvided).ToArray();
			if (resolveItemsToResolve.Length == 0)
			{
				Globals.Message.ShowError(
					Res.GetString("cb3197ac-917d-401a-a5f3-fd991a25a249", "No commissions have been provided with a resolution."),
					Res.GetString("845fde10-faa2-42ab-a23f-881150198a32", "Cannot Resolve"));
				return;
			}

			var confirmationResult = Globals.Message.Show(
					Res.GetString("9a4466b0-a32c-4674-8afa-2160abbdf34b", "Are you sure you want to resolve {0} ambiguous commissions?", resolveItemsToResolve.Length),
					Res.GetString("0e036aa5-df85-4d00-b4ec-421b65efd783", "Resolve Commissions"),
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Warning,
					DialogResult.No);
			if (confirmationResult != DialogResult.Yes)
			{
				return;
			}

			using (var progressForm = new ProgressForm())
			{
				progressForm.Status = Res.GetString("03ef6ff2-6bda-4e46-8626-a122778f63fc", "Resolving commissions...");
				progressForm.ShowCancelButton = false;
				ZFormModaliser.Show(progressForm, this);

				Progress onResolveProgress = (string status, int percentage) =>
				{
					progressForm.Status = status;
					progressForm.PercentComplete = percentage;
				};

				try
				{
					BusinessEntity.Resolve(onResolveProgress);
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
					BusinessEntity.RefreshResolveItems();
				}
			}
		}

		#endregion

		#region Invoice Filters

		void SetupInvoiceFilters()
		{
			FilterBizObj.CompanyPkInfo.ValueChanged += CompanyPkInfo_ValueChanged;
			RefreshInvoiceFiltersVisibility();
		}

		void CompanyPkInfo_ValueChanged(object sender, EventArgs e)
		{
			RefreshInvoiceFiltersVisibility();
		}

		void RefreshInvoiceFiltersVisibility()
		{
			var companyFilterIsCurrent = FilterBizObj.CompanyPk == GlbCompany.CurrentCompany.PK;
			InvoicePkToResolveGuidFindBox.Visible = companyFilterIsCurrent;
			InvoiceNumberToResolveTextBox.Visible = !companyFilterIsCurrent;
		}

		#endregion

		#region InvoicesGrid

		#region Double Click

		void UnresolvedCommissionsGrid_MouseDown(object sender, MouseEventArgs e)
		{
			DoActionIfDoubleClick(UnresolvedCommissionsGrid, e, (target) =>
			{
				OnAmbiguousCommissionResolveItemDoubleClick((AmbiguousCommissionResolveItem)target);
			});
		}

		void OnAmbiguousCommissionResolveItemDoubleClick(AmbiguousCommissionResolveItem resolveItem)
		{
			var invoice = resolveItem.Invoice;
			if (invoice != null)
			{
				var controller = AccountingControllerCreator.GetNewController(invoice);
				controller.ShowViewForm(invoice);
			}
		}

		#endregion

		#endregion

		#region PossibleOverallItemsGrid

		#region Menu Items

		void AddPossibleOverallItemsGridMenuItems()
		{
			var viewAgreementMenuItem = new ZMenuItem(ResString.GetMultilingualString("6525cbbf-9672-4467-87bc-48810dcd6a01", "&View Commission Agreement"), OnViewAgreementMenuItemClicked);
			PossibleOverallItemsGrid.ContextMenu.MenuItems.Add(0, viewAgreementMenuItem);
		}

		void OnViewAgreementMenuItemClicked(object sender, EventArgs e)
		{
			var current = PossibleOverallItemsGrid.ListManager.GetCurrent() as ViewCommissionAgreementOverallItem;
			if (current != null)
			{
				var agreement = current.CommissionAgreement;
				if (agreement != null)
				{
					var controller = ZControllerFactory.Create(ControllerIDs.OrgCommissionAgreement);
					controller.ShowEditForm(agreement);
				}
				return;
			}

			Globals.Message.ShowInformation(Res.GetString("085294ab-67d6-44db-86ec-6176ad95b505", "Please select a commission agreement to view."));
		}

		#endregion

		#region Double Click

		void PossibleOverallItemsGrid_MouseDown(object sender, MouseEventArgs e)
		{
			DoActionIfDoubleClick(PossibleOverallItemsGrid, e, (target) =>
			{
				OnPossibleOverallItemDoubleClick((ViewCommissionAgreementOverallItem)target);
			});
		}

		void OnPossibleOverallItemDoubleClick(ViewCommissionAgreementOverallItem overallItem)
		{
			var resolveItem = (AmbiguousCommissionResolveItem)UnresolvedCommissionsGrid.ListManager.GetCurrent();
			if (resolveItem != null)
			{
				resolveItem.SelectedAgreementPk = overallItem.VCI_CA0;
			}
		}

		#endregion

		#endregion

		#region Cancel

		void FormCancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion

		#region Form Captions

		public override string FormVerb
		{
			get { return ""; }
		}

		#endregion

		static void DoActionIfDoubleClick(ZGrid grid, MouseEventArgs e, Action<object> onDoubleClickHandler)
		{
			if (e.Clicks > 1)
			{
				DataGrid.HitTestInfo hitInfo = grid.HitTest(e.X, e.Y);
				if (hitInfo.Row >= 0 &&
					hitInfo.Row < grid.ListManager.List.Count) // valid row clicked on
				{
					var target = grid.ListManager.List[hitInfo.Row];
					onDoubleClickHandler(target);
				}
			}
		}
	}
}
