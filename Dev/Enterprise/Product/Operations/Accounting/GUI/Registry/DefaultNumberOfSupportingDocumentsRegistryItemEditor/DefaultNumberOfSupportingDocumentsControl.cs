using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class DefaultNumberOfSupportingDocumentsControl : RegistryZUserControl
	{
		#region Controls

#if DEBUG
		public
#else
		protected 
#endif
 ZArchitecture.ZGrid DefaultNumberOfSupportingDocumentsGrid;

		#endregion

		public DefaultNumberOfSupportingDocumentsControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			DefaultNumberOfSupportingDocumentsGrid.ReadOnly = readOnly;
		}

		void DefaultNumberOfSupportingDocumentsGrid_RowDeleting(object sender, ZArchitecture.RowsDeletingEventArgs e)
		{
			e.Cancel = true;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			if (BindingSource.Current != null)
			{
				if (((DefaultNumberOfSupportingDocuments)BindingSource.Current).NumberOfDefaultVisible)
				{
					DefaultNumberOfSupportingDocumentsGrid.AddToAvailableColumns(DefaultNumberOfSupportingDocuments.Schema.NumberOfDefault);
				}
				else
				{
					DefaultNumberOfSupportingDocumentsGrid.RemoveFromAvailableColumns(DefaultNumberOfSupportingDocuments.Schema.NumberOfDefault);
				}
			}
		}
	}
}

