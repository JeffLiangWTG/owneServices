using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.Common.GUI
{
	public abstract class TariffGridFindBox : ZGridFindBox
	{
		protected override IFindBoxPopup GetNewPopupForm()
		{
			return BorderWiseTariffFindBoxProvider.GetTariffFindBoxWrapper(ActiveBusinessObject, ColumnInfo.ColumnName, GetNewNonBorderWisePopupForm);
		}
		protected virtual IFindBoxPopup GetNewNonBorderWisePopupForm()
		{
			return base.GetNewPopupForm();
		}

		protected abstract IFindBoxListProvider GetNewListProvider();

		protected sealed override IFindBoxListProvider ListProvider
		{
			get { return GetNewListProvider(); }
		}

		public BusinessObject ActiveBusinessObject; // Set by the ColumnStyle
		public ZGridColumnInfo ColumnInfo; // also set by the ColumnStyle
	}
}
