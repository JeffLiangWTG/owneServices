using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public partial class SupplyChainActorReferencesUserControl : ZUserControl
	{
		public SupplyChainActorReferencesUserControl()
		{
			InitializeComponent();
			SupplyChainActorReferencesGrid.AfterBind += SupplyChainActorReferenceGrid_AfterBind;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (SupplyChainActorReferencesGrid != null)
			{
				SupplyChainActorReferencesGrid.AfterBind -= SupplyChainActorReferenceGrid_AfterBind;
			}
		}

		void SupplyChainActorReferenceGrid_AfterBind(object sender, EventArgs e)
		{
			if (SupplyChainActorReferencesGrid.DataSource is JobDeclaration jobDeclaration)
			{
				var provider = CusSupplyChainActorReferenceProvider.GetByDataGroupingCode(jobDeclaration.GetDefaultDataGroupingCode());
				var overwrittenReferenceColumnCaption = provider.OverwrittenReferenceColumnCaption;
				if (!overwrittenReferenceColumnCaption.IsEmpty)
				{
					SupplyChainActorReferencesGrid.SetColumnCaption(CusReference.Schema.CFR_Reference, overwrittenReferenceColumnCaption);
				}

				var columnNamesInSortOrder = provider.ColumnNamesInSortOrder;
				if (columnNamesInSortOrder.Count != 0)
				{
					SupplyChainActorReferencesGrid.ReOrderColumns(columnNamesInSortOrder);
				}
			}
		}
	}
}
