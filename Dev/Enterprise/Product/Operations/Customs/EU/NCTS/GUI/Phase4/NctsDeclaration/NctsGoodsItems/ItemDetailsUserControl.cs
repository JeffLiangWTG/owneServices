using System;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class ItemDetailsUserControl : ZUserControl
	{
		public ItemDetailsUserControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			CustomsValueDropEdit.GetExtension<ILabelCaptionRenderer>().Caption = !DesignModeFinder.IsDesigning ? MovementHeader.CustomsValueCaption : (NoResString)ZString.Empty;
		}

		protected void AdditionalSupplementaryCodesEditButton_Click(object sender, EventArgs e)
		{
			if (CurrentDataItem != null)
			{
				AdditionalSupplementaryCodesForm.ShowDialog(((NctsDepartureCargoDesc)CurrentDataItem));
			}
		}
		NctsDepartureMovementHeader MovementHeader => ((NctsHeader)DataSource).MovementHeader;
	}
}
