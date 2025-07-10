using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public class TSCustomsNumberViewStmNumsGuiProvider : TSCustomsNumberViewStmNumsBusinessProvider, ICustomsNumberViewStmNumsGuiProvider
	{
		public TSCustomsNumberViewStmNumsGuiProvider(BusinessObjectFactory factory, ZString countryCode, ZGuid ownerPk) : base(factory, countryCode, ownerPk)
		{
		}

		public Form GetEditorForm(CustomsNumberViewStmNumsWrapper wrapper) => new TSCustomsNumberViewStmNumsEditorForm((TSCustomsNumberViewStmNumsWrapper)wrapper);

		public Control GetUserControl() => new TSCustomsNumberViewStmNumsUserControl(CustomsNumberWrappers);
	}
}
