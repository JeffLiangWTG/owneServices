using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Riba;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Accounting.GUI.Riba
{
	[SuppressCheckControlModuleId]
	public partial class AddTransactionsToOrderForm : ZChildForm
	{
		public AddTransactionsToOrderForm(OrderTransactionsFilterHolder filterBO)
			: base(filterBO)
		{
			FilterBuisnessObject = BusinessEntity.AddTransactionsToOrderFilter;

			FilterControl = new AddTransactionsToOrderOnFormFilterControl(new InvoicingBaseCollection(BusinessEntity.Factory), FilterBuisnessObject);
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

			FormIsLoaded = false;
			MissingResourceStringChecker.ExcludeFromTest(this.NotificationLabel);
		}
		void JobsFilteredGrid_BoundsChanged(object sender, EventArgs e)
		{
			NotificationGridContainer.Bounds = FilterControl.FilteredGrid.Bounds;
		}

		bool FormIsLoaded;

		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public AddTransactionsToOrderForm()
		{
		}

		protected new OrderTransactionsFilterHolder BusinessEntity
		{
			get { return base.BusinessEntity as OrderTransactionsFilterHolder; }
		}

		#region Form Override

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (BusinessEntity != null)
			{
				NotificationGridContainer.Dock = System.Windows.Forms.DockStyle.None;
				NotificationPanel.Visible = true;
				InvoicesGrid.Visible = true;
			}
			FormIsLoaded = true;
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			NotificationLabel.Text = Res.GetString("bb2bb4ac-3fd0-435c-9bf2-cc29726209d7", "Please enter the filter criteria");
		}

		#endregion

		#region Event Handler

		protected internal void FilterControl_PerformSearch(object sender, EventArgs e)
		{
			if (FormIsLoaded)
			{
				if (BusinessEntity.AddTransactionsToOrderFilter != null)
				{
					ZQuery query = BusinessEntity.AddTransactionsToOrderFilter.Filter;
					int count = BusinessEntity.Factory.GetDatabaseCount(typeof(TransactionHeader), query);
					DoSearch(query, count);
				}
			}
		}

		void DoSearch(ZQuery query, int count)
		{
			if (count > SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.Value)
			{
				NotificationLabel.ForeColor = System.Drawing.Color.Red;
				NotificationLabel.Text = Res.GetString("AddTransactionToOrderForm|72FF572D-E8B2-439e-B778-CDDAB7150C31", "Too many records to display ({0:G}). Please fill in more of the search screen and then click 'Find'.", count);
			}
			else
			{
				LoadTransactions(query);
			}
		}

		protected virtual void FilterControl_ClearButtonClicked(object sender, EventArgs e)
		{
			if (FormIsLoaded)
			{
				BusinessEntity.Transactions.RemoveAll();
				NotificationLabel.ForeColor = System.Drawing.Color.Black;
				NotificationLabel.Text = Res.GetString("AddTransactionToOrderForm|82620f2d-47e5-497e-a941-ee6ca0f8f96f", "Please enter the filter criteria and select transactions for batching.");
			}
		}

		protected void LoadTransactions(ZQuery query)
		{
			BusinessObject[] transactionsFetched = BusinessEntity.Factory.Load(typeof(TransactionHeader), query);
			BusinessEntity.Transactions.RemoveAll();
			BusinessEntity.Transactions.AddRange(transactionsFetched);
			NotificationLabel.ForeColor = System.Drawing.Color.Black;
			NotificationLabel.Text = Res.GetString("AddTransactionToOrderForm|383AFB7C-95C0-4cbe-B418-229F52F45DFC", "Found {0} records that match your criteria.", BusinessEntity.Transactions.Count);
		}

		#endregion

		#region Private Properties

		protected TransactionHeader SelectedTransaction
		{
			get
			{
				if (InvoicesGrid != null && InvoicesGrid.ListManager != null && InvoicesGrid.ListManager.Position >= 0)
				{
					return InvoicesGrid.ListManager.GetCurrent() as TransactionHeader;
				}
				else
				{
					return null;
				}
			}
		}

		#endregion

		#region Line Factory

		BusinessObjectFactory newFactory;
		protected BusinessObjectFactory NewFactory
		{
			get
			{
				if (newFactory == null)
				{
					newFactory = new BusinessObjectFactory();
				}
				return newFactory;
			}
		}

		#endregion

		void AddButton_Click(object sender, EventArgs e)
		{
			TransactionHeaderCollection selectedTransactions = new TransactionHeaderCollection(BusinessEntity.RelatedOrder.Factory);
			if (InvoicesGrid.SelectedElements.Length > 0)
			{
				foreach (BusinessObject transaction in InvoicesGrid.SelectedElements)
				{
					selectedTransactions.Add(transaction);
				}
			}

			var listPaymentMethodCodes = OrganisationsDataRegistry.Instance.ReceivablesCreditAgreedPaymentMethodsList.Value.GetActiveCodeDescriptionPairList().GetAllCodes().ToList();

			if (listPaymentMethodCodes.Any())
			{
				foreach (AccTransactionHeader transaction in selectedTransactions)
				{
					if (!listPaymentMethodCodes.Any(x => x == transaction.AH_AgreedPaymentMethodOverride))
					{
						listPaymentMethodCodes.Sort();
						var collectionBatchPaymentMethodList = String.Join("', '", listPaymentMethodCodes);

						Globals.Message.ShowError(Res.GetString("E1E44823-ABEE-4881-8444-CEB7E619A5F1", @"Collection order only supports INV, CRD, ADJ, JNL with '{0}' agreed payment method(s).

Please revise transactions selection.", collectionBatchPaymentMethodList));
						return;
					}
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("CA983155-A93A-4140-879A-26ED0CCA4281", @"There are no valid agreed payment methods configured for Collection Batches, it is not possible to add transactions to the order.

Please check the registry setting at {0}.", OrganisationsDataRegistry.Instance.ReceivablesCreditAgreedPaymentMethodsList.GetLocation()));
				return;
			}

			ZString transactionNum = BusinessEntity.CheckAnyTransactionsUsedByActiveOrderLine(selectedTransactions);
			if (transactionNum.IsEmpty)
			{
				if (!BusinessEntity.CheckAnyTransactionsHaveInvalidDueDate(selectedTransactions))
				{
					BusinessEntity.AddTransactionsIntoOrder(selectedTransactions);
					Close();
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("AB3AA738-0B66-422E-A09D-023E7DCB3C74", @"Some transactions due date is conflicting with order’s collection date.
Only transactions due on or before this date can be included in this order.

Please revise transactions selection."));
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("df5fcdb2-543c-4d67-8bae-a2f73b4ab986", "Some transactions are already included in active batches. Please revise transactions selection."));
			}
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}

