using System;
using System.Collections;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI
{
	[SuppressCheckControlModuleId]
	public partial class InvoiceBatchForm : AccountingZForm
	{
		public InvoiceBatchForm(InvoiceBatchHeader batchHeader)
			: base(batchHeader)
		{
			if (!BusinessEntity.IsInDatabase)
			{
				FilterBuisnessObject = BusinessEntity.Filter;

				FilterControl = new InvoiceBatchOnFormFilterControl(new InvoiceBatchLineCollection(BusinessEntity.Factory), FilterBuisnessObject);
				FilterControl.Size = FilterPanel.ClientSize;
				FilterControl.BackColor = BackColor;
				FilterControl.SetMaxFilterStripPanelHeight(220);
				FilterControl.Dock = System.Windows.Forms.DockStyle.Fill;
				FilterPanel.Controls.Add(FilterControl);

				FilterControl.FilteredGrid.SizeChanged += JobsFilteredGrid_BoundsChanged;
				FilterControl.FilteredGrid.LocationChanged += JobsFilteredGrid_BoundsChanged;
				JobsFilteredGrid_BoundsChanged(FilterControl.FilteredGrid, EventArgs.Empty);
				FilterControl.PerformSearch += new EventHandler<PerformSearchEventArgs>(FilterControl_PerformSearch);
				FilterControl.FiltersCleared += new EventHandler(FilterControl_ClearButtonClicked);

				NotificationGridContainer.AllowOverlap(FilterControl);
				JobTypeCheckedListBox.AllowOutsideOfParent();
			}

			FormIsLoaded = false;
			DisplayModeChanged += InvoiceBatchForm_DisplayModeChanged;
			InitializePostingStrategy();

			MissingResourceStringChecker.ExcludeFromTest(this.NotificationLabel);
		}
		void JobsFilteredGrid_BoundsChanged(object sender, EventArgs e)
		{
			NotificationGridContainer.Bounds = FilterControl.FilteredGrid.Bounds;
		}

		protected ZCodeFindBox CurrencyCodeFindBox;
		protected ZPanel NotificationGridContainer;
		protected ZGrid BatchInvoiceLinesGrid;
		protected ZPanel NotificationPanel;
		protected ZLabel NotificationLabel;
		CargoWise.Windows.UI.KFlowLayoutPanel kFlowLayoutPanel1;
		bool FormIsLoaded;

		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public InvoiceBatchForm()
		{
		}

		new InvoiceBatchHeader BusinessEntity
		{
			get { return base.BusinessEntity as InvoiceBatchHeader; }
		}

		#region Form Override

		protected override void SaveCore(ITransactionParticipant[] factories)
		{
			ArrayList factoriesToSave = new ArrayList();
			factoriesToSave.AddRange(factories);
			factoriesToSave.Add(LineFactory);
			base.SaveCore((ITransactionParticipant[])factoriesToSave.ToArray(typeof(ITransactionParticipant)));
		}

		protected override void SetAutoAddPreviousNextButtons()
		{
			AutoAddPreviousNextButtons = true;
		}

		public override string FormVerb
		{
			get { return IsReversingMode ? Res.GetString("70dbc8de-4c18-47f2-8174-fa5b58673166", "Cancel") : base.FormVerb; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (BusinessEntity != null)
			{
				NotificationGridContainer.Dock = System.Windows.Forms.DockStyle.None;
				CancelledBatchLabel.Visible = (DisplayMode != ODisplayMode.Delete) && BusinessEntity.AH_IsCancelled;

				if (BusinessEntity.IsInDatabase)
				{
					BatchFilterGroupBox.Text = "";
					NotificationGridContainer.Dock = System.Windows.Forms.DockStyle.Fill;

					JobTypeCheckedListBox.Enabled = false;
					BatchInvoiceLinesGrid.RemoveFromAvailableColumns("IncludeInTheBatch");

					if (DisplayMode != ODisplayMode.Delete)
					{
						PostButton.Text = Res.GetString("InvoiceBatchForm|13C2BFA4-F95F-4C51-AD7D-A1D691F42BCB", "New");
					}
				}

				NotificationPanel.Visible = !BusinessEntity.IsInDatabase;
				BatchInvoiceLinesGrid.Visible = true;
				HideNonApplicableControls();
			}
			FormIsLoaded = true;
		}

		public void HideNonApplicableControls()
		{
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode != Enterprise.Core.Constants.CountryCodes.India)
			{
				BatchInvoiceLinesGrid.RemoveFromAvailableColumns(TransactionHeaderWithLines.Schema.AH_OSExtraTaxAmount, TransactionHeaderWithLines.Schema.AH_LocalExtraTaxAmount);
			}
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			ContinueWithSave baseResult = ContinueWithSave.No;
			if (DialogResult.Yes == Globals.Message.Show(Res.GetString("dedff539-97cc-4a36-9c8f-46c8e48ca7fb", "All Selected Invoices / Credit Notes will be updated with the Invoice Date and Due Date of the batch. Proceed?"), FormCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question))
			{
				baseResult = PostSaveProcessing(base.ValidateAndSave());
			}
			return baseResult;
		}

		protected ContinueWithSave PostSaveProcessing(ContinueWithSave baseResult)
		{
			if (baseResult == ContinueWithSave.Yes)
			{
				BusinessEntity.ReadOnly = true;
				JobTypeCheckedListBox.Enabled = false;
				PostButton.Enabled = false;
				this.FilterControl.Enabled = false;
				this.BatchInvoiceLinesGrid.Enabled = false;

				PrintBatches();
			}
			else
			{
				baseResult = ContinueWithSave.No;
			}
			return baseResult;
		}

		protected virtual void PrintBatches()
		{
			if (Globals.Message.Show(Res.GetString("94e4d870-ba23-445a-b127-69ff09a3eb4d", "Do you want to print Invoice Batch?"), Res.GetString("a63a1361-ef79-4d3c-9598-d9ff997071d4", "Invoice Batch"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				new InvoiceBatchHeaderPrintTask(BusinessEntity.PK).Run();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			NotificationLabel.Text = Res.GetString("bb2bb4ac-3fd0-435c-9bf2-cc29726209d7", "Please enter the filter criteria");
		}

		#endregion

		#region Posting Strategy
		protected virtual void InitializePostingStrategy()
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, null, CloseButton, PostButton);
		}

		#endregion

		#region Event Handler

		protected void FilterControl_PerformSearch(object sender, EventArgs e)
		{
			if (FormIsLoaded)
			{
				if (BusinessEntity.Filter != null)
				{
					if (BusinessEntity.SelectedJobTypeCodes.Count > 0)
					{
						ZQuery query = BusinessEntity.Filter.Filter;
						int count = BusinessEntity.Factory.GetDatabaseCount(typeof(TransactionHeader), BusinessEntity.Line.GetCombinedQuery(query));
						DoSearch(query, count);
					}
					else
					{
						Globals.Message.ShowError(Res.GetString("7aa6b223-46b7-499c-beff-78ed9429ee9a", "At least one Job Type should be selected."));
					}
				}
			}
		}

		void DoSearch(ZQuery query, int count)
		{
			if (count > SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.Value)
			{
				NotificationLabel.ForeColor = System.Drawing.Color.Red;
				NotificationLabel.Text = Res.GetString("InvoiceBatchForm|72FF572D-E8B2-439e-B778-CDDAB7150C31", "Too many records to display ({0:G}). Please fill in more of the search screen and then click 'Find'.", count);
			}
			else
			{
				LoadLines(BusinessEntity.Line.GetCombinedQuery(query));
			}
		}

		protected virtual void FilterControl_ClearButtonClicked(object sender, EventArgs e)
		{
			if (FormIsLoaded)
			{
				BusinessEntity.ClearLines();
				NotificationLabel.ForeColor = System.Drawing.Color.Black;
				NotificationLabel.Text = Res.GetString("InvoiceBatchForm|82620f2d-47e5-497e-a941-ee6ca0f8f96f", "Please enter the filter criteria and select transactions for batching.");
			}
		}

		protected virtual void InvoiceBatchForm_DisplayModeChanged(object sender, DisplayModeChangedEventArgs e)
		{
			if (e.ToMode == ODisplayMode.Delete)
			{
				PostButton.Text = Res.GetString("InvoiceBatchForm|DDB8A134-EE5F-4b6e-9FD3-67FB5D6A7F29", "Cancel Batch");
				PostButton.Enabled = true;
				CloseButton.Enabled = true;
				//HideGridColumn("IncludeInTheBatch");
			}
			else if (e.ToMode == ODisplayMode.ReadOnly)
			{
				PostButton.Enabled = false;
				CloseButton.Enabled = true;
				JobTypeCheckedListBox.Enabled = false;
				//HideGridColumn("IncludeInTheBatch");
			}
		}

		void BatchInvoiceLinesGrid_DoubleClick(object sender, EventArgs e)
		{
			ShowSelectedTransaction();
		}

		protected IZForm ShowSelectedTransaction()
		{
			if (SelectedTransaction != null)
			{
				ZController controller = AccountingControllerCreator.GetNewController(SelectedTransaction);
				return controller.ShowViewForm(SelectedTransaction);
			}
			else
			{
				return null;
			}
		}

		protected virtual void LoadLines(ZQuery query)
		{
			BusinessObject[] linesFetched = LineFactory.Load(typeof(InvoicingBase), query);
			BusinessEntity.ClearLines();
			BusinessEntity.Line.AddRange(linesFetched);
			NotificationLabel.ForeColor = System.Drawing.Color.Black;
			NotificationLabel.Text = Res.GetString("InvoiceBatchForm|383AFB7C-95C0-4cbe-B418-229F52F45DFC", "Found {0} records that match your criteria.", BusinessEntity.Line.Count);
		}

		void DebtorGuidFindBox_Leave(object sender, EventArgs e)
		{
			JobTypeCheckedListBox.Update();
		}

		#endregion

		#region Private Properties

		protected TransactionHeader SelectedTransaction
		{
			get
			{
				if (BatchInvoiceLinesGrid != null && BatchInvoiceLinesGrid.ListManager != null)
				{
					return BatchInvoiceLinesGrid.ListManager.GetCurrent() as TransactionHeader;
				}
				else
				{
					return null;
				}
			}
		}

		#endregion

		#region Line Factory

		BusinessObjectFactory fLineFactory;
		protected BusinessObjectFactory LineFactory
		{
			get
			{
				if (fLineFactory == null)
				{
					fLineFactory = new BusinessObjectFactory();
				}
				return fLineFactory;
			}
		}

		#endregion
	}
}

