using CargoWise.EntityFramework;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IT.TemporaryStorage.Module;

public partial class TempStorageRegisterFilterStripControl : EU.TemporaryStorage.Module.TempStorageRegisterFilterStripControl
{
	public TempStorageRegisterFilterStripControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
	{
		InitializeComponent();
		AddColumns();
	}

	void AddColumns()
	{
		var transportIDColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
		transportIDColumnStyleInfo.ColumnName = CusTempStorageRegHeader.Schema.SRH_TransportID;
		transportIDColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
		grid.ColumnStyles.Add(transportIDColumnStyleInfo);
	}
}
