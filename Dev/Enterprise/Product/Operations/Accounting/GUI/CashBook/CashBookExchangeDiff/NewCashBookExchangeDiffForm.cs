using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.CashBook.ExchangeDifference;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.CashBook
{
	public partial class NewCashBookExchangeDiffForm : AccountingZForm, IDoDisplayModeNewOverride, IDoDisplayModeBrowseOverride
	{
		public NewCashBookExchangeDiffForm(NewCashbookExchangeDiffHeader cashbookExchangeDiffNewHeader)
			: base(cashbookExchangeDiffNewHeader)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			ZFormMenuStrategy.SetMenuItemText(this, ZFormMenuStrategy.FileSaveAndCloseMenuItemName, Res.GetString("Accounting|NewCashBookExchangeDiffForm|PostMenuItem", "&Post"));
			DisplayModeChanged += CashBookExchangeDiffForm_DisplayModeChanged;
			NotificationGridContainer.AllowOverlap(FilterControl);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			InitializeFilterControl();
		}

		void InitializeFilterControl()
		{
			FilterControl = ObjectFactory.Get<INewCashbookExchangeDiffBankAccountFilterControl>("INewCashbookExchangeDiffBankAccountFilterControl", new AccBankAccountCollection(BusinessEntity.Factory, GlbCompany.CurrentCompany)) as ZFilterStripControl;
			FilterControl.Name = "NewCashbookExchangeDiffBankAccountFilterControl";
			FilterControl.Size = FilterPanel.ClientSize;
			FilterControl.BackColor = BackColor;
			FilterControl.Dock = DockStyle.Fill;
			FilterPanel.Controls.Add(FilterControl);
			FilterControl.FilteredGrid.SizeChanged += NewCashBookExchangeDiff_BoundsChanged;
			FilterControl.PerformSearch += FilterControl_PerformSearch;
			NewCashBookExchangeDiff_BoundsChanged(FilterControl.FilteredGrid, EventArgs.Empty);
		}

		void FilterControl_PerformSearch(object sender, EventArgs e)
		{
			var bankAccountCollection = FilterControl.GridCollection as AccBankAccountCollection;
			bankAccountCollection.Load(FilterControl.FilterBusinessObject.Filter);
			Header.NewCashbookExchangeDiffCollection.RemoveAndDeleteAll();
			bankAccountCollection.ForEach(x =>
			{
				var newCashbookExchangeDiff = Header.NewCashbookExchangeDiffCollection.AddNew();
				newCashbookExchangeDiff.AH_AB = x.PK;
			});
		}

		void NewCashBookExchangeDiff_BoundsChanged(object sender, EventArgs e)
		{
			NotificationGridContainer.Bounds = FilterControl.FilteredGrid.Bounds;
		}

		void CashBookExchangeDiffForm_DisplayModeChanged(object sender, DisplayModeChangedEventArgs e)
		{
			DocManagerReadOnlyOverrideHelper.TrySetReadOnlyOverride(BusinessEntity, e.ToMode);
		}

		void SelectAllButton_Click(object sender, EventArgs e)
		{
			SelectDeselectAll(true);
		}

		void DeselectAllButton_Click(object sender, EventArgs e)
		{
			SelectDeselectAll(false);
		}

		void SelectDeselectAll(bool select)
		{
			Header.NewCashbookExchangeDiffCollection.Cast<NewCashbookExchangeDiff>().ForEach(x => x.Include = select);
		}

		NewCashbookExchangeDiffHeader Header => BusinessEntity as NewCashbookExchangeDiffHeader;

		#region Implementation

		void IDoDisplayModeNewOverride.DoDisplayModeNew()
		{
			ZFormStrategy.DoDisplayModeNew(this);
			DoDisplayMode(true);
		}

		void IDoDisplayModeBrowseOverride.DoDisplayModeBrowse()
		{
			ZFormStrategy.DoDisplayModeBrowse(this);
			DoDisplayMode(false);
		}

		void DoDisplayMode(bool isNewMode)
		{
			FilterControl.Enabled = isNewMode;
			SelectAllButton.Enabled = isNewMode;
			DeselectAllButton.Enabled = isNewMode;
			Header.ReadOnly = !isNewMode;
			Header.NewCashbookExchangeDiffCollection.ForEach(x => x.ReadOnly = !isNewMode);
		}

		#endregion
	}
}

