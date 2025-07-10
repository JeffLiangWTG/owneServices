using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Billing.GUI
{
	public partial class FaxPricesForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public FaxPricesForm()
		{
			InitializeComponent();
		}

		public FaxPricesForm(BulkClientFaxPriceUpdater bulkClientFaxPriceUpdater) : base(bulkClientFaxPriceUpdater)
		{
			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			bulkClientFaxPriceUpdater.HasChangesChanged += new EventHandler<HasChangesChangedEventArgs>(BulkClientFaxPriceUpdater_HasChangesChanged);
		}

		BulkClientFaxPriceUpdater Updater
		{
			get { return (BulkClientFaxPriceUpdater)base.BusinessEntity; }
		}

		void BulkClientFaxPriceUpdater_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			var updater = Updater;
			if (updater != null)
			{
				bool hasChanges = updater.HasChangesInChildren;
				MonthAndYearPeriodDateEdit.ReadOnly = hasChanges;
				SaveChangesWarningLabel.Visible = hasChanges;
			}
		}

		protected override bool AllowNew
		{
			get { return false; }
		}
	}
}

