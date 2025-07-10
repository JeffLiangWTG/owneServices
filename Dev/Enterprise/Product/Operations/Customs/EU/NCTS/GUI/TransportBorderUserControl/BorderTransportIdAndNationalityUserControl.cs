using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class BorderTransportIdAndNationalityUserControl : ZUserControl, ISupportMultipleResourceStringDataSupporter
	{
		public BorderTransportIdAndNationalityUserControl()
		{
			InitializeComponent();
		}

		ISupportMultipleResourceStringData ISupportMultipleResourceStringDataSupporter.SupportMultipleResourceStringData => CurrentDataItem switch
		{
			NctsDepartureMovementHeader movementHeader when !movementHeader.IsDeleted => movementHeader,
			_ => null
		};
	}
}
