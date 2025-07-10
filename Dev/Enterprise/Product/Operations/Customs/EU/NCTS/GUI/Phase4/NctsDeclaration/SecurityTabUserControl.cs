using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class SecurityTabUserControl : ZUserControl
	{
		public SecurityTabUserControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (dataSource is NctsHeader header)
			{
				ConfigurePlaceOfUnloadingControlType(header);
			}
		}

		#region Implementation

		void ConfigurePlaceOfUnloadingControlType(NctsHeader header)
		{
			var placeOfUnloadingControlType = GetPlaceOfUnloadingControlType(header);
			PlaceOfUnloadingTextBox.Visible = placeOfUnloadingControlType == PlaceOfUnloadingControlType.TextBox;
			PlaceOfUnloadingFindBox.Visible = placeOfUnloadingControlType == PlaceOfUnloadingControlType.CodeFindBox;
		}

		protected virtual PlaceOfUnloadingControlType GetPlaceOfUnloadingControlType(NctsHeader header) => header.IsDepartureMovement ? PlaceOfUnloadingControlType.TextBox : PlaceOfUnloadingControlType.CodeFindBox;

		protected enum PlaceOfUnloadingControlType
		{
			TextBox,
			CodeFindBox,
		}

		#endregion
	}
}
