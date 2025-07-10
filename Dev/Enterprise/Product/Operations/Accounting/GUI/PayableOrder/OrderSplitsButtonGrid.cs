using Enterprise.Accounting.Business.PayableOrder;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.PayableOrder
{
	public partial class OrderSplitsButtonGrid : ZModuleButtonGridWithoutColumnStylesSerialisation
	{
		protected CustomLabelsGridLayoutPersister fGridLayoutPersister;

		public OrderSplitsButtonGrid()
		{
			InitializeComponent();
			InnerGrid.ReadOnly = true;
		}

		public AccPayableOrderHeader Order { get; set; }

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (this.fGridLayoutPersister != null)
				{
					this.fGridLayoutPersister.Dispose();
				}

				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Implementation

		protected bool IsBound
		{
			get { return DataSource != null; }
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (fGridLayoutPersister != null)
			{
				fGridLayoutPersister.Dispose();
				fGridLayoutPersister = null;
			}
			base.SetDataBinding(dataSource, dataMember);
			if (dataSource != null)
			{
				//fGridLayoutPersister = new CustomLabelsGridLayoutPersister(InnerGrid, new AccPayableOrderHeader.CustomLabelsProvider(Order, true));
			}
		}

		#endregion
	}
}

