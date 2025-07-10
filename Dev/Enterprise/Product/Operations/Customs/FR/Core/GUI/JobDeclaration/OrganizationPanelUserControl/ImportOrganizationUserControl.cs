using System;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.GUI
{
	public partial class ImportOrganizationUserControl : EU.GUI.ImportOrganizationUserControl
	{
		public ImportOrganizationUserControl()
		{
			InitializeComponent();
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (Declaration is JobDeclaration declaration)
			{
				declaration.JE_ApplicationCodeInfo.ValueChanged -= OnApplicationCodeChanged;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (Declaration is JobDeclaration declaration)
			{
				SetBuyerFieldVisibility();
				declaration.JE_ApplicationCodeInfo.ValueChanged -= OnApplicationCodeChanged;
				declaration.JE_ApplicationCodeInfo.ValueChanged += OnApplicationCodeChanged;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
				if (Declaration is JobDeclaration declaration)
				{
					declaration.JE_ApplicationCodeInfo.ValueChanged -= OnApplicationCodeChanged;
				}
			}
			base.Dispose(disposing);
		}

		void OnApplicationCodeChanged(object sender, EventArgs e) => SetBuyerFieldVisibility();

		void SetBuyerFieldVisibility()
		{
			if (Declaration != null)
			{
				BuyerOrganisationGuidFindBox.Visible = Declaration.IsUCC6;
			}
		}

		JobDeclaration Declaration => (JobDeclaration)CurrentDataItem;
	}
}
