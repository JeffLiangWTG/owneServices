using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public partial class SplitConsignmentUserControl : ZUserControl
	{
		public SplitConsignmentUserControl()
		{
			InitializeComponent();

			TemporaryStorageEndDateEdit.AllowOutsideOfParent();
			ChiefDucrLinkLabel.AllowOutsideOfParent();
#if DEBUG
			System.ComponentModel.TypeDescriptor.AddAttributes(AgentCodeFindBox, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		void ChiefDucrLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			var split = SplitConsignmentBindingSource;
			if (split != null && split.HasOwnDeclaration)
			{
				new DependentObjectControllerHelper(new DependentFormPresenter()).EditExisting(split.OwnDeclaration, ControllerIDs.Customs.JobDeclaration);
			}
		}

		SplitConsignment SplitConsignmentBindingSource
		{
			get { return BindingSource.DataSource as SplitConsignment; }
		}
	}
}
