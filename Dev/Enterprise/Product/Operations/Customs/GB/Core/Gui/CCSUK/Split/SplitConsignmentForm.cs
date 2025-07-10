using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public partial class SplitConsignmentForm : ZTemplateForm, IPostingButtonsProvider
	{
		public SplitConsignmentForm(SplitConsignment splitConsignment)
		{
			this.splitConsignment = splitConsignment;
			this.SetDataBinding(splitConsignment, string.Empty);
			AddMenu(splitConsignment);
			CcsukAirInventoryForm.AddDocumentsMenu(this);
		}

		void AddMenu(ICcsukCusAwb splitAwb)
		{
			var manager = new CusAwbDelegateProvider(delegate
			{ return splitAwb; });
			var menu = new CcsukMenu(manager, this, true);
			MainMenu.MenuItems.Add(menu);
		}

		public override string FormCaption
		{
			get { return splitConsignment.HumanReadableNameForFormCaption; }
		}

		protected override bool SupportsEDocs
		{
			get { return false; }
		}

		bool IPostingButtonsProvider.IsPostOnly
		{
			get { return true; }
			set { }
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var result = base.ShowPreSaveDialogs();
			result = CcsukAirInventoryForm.SendAutoFrcIfNeeded(result, splitConsignment);
			return result;
		}

		readonly SplitConsignment splitConsignment;
	}
}
