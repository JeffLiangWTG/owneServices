using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.DE.Module
{
	public class MonthlyClosingModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.EU.DE.MonthlyClosing;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.MonthlyClosing;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.DE.MonthlyClosing);

		protected override IBusinessObjectCollection GetNewGridCollection() => new CusReconDeclarationCollection(Factory);

		protected override IFilterControl GetNewFilterControl() => new MonthlyClosingFilterStripControl(GridCollection, (MonthlyClosingFilterBusinessObject)FilterBusinessObject);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new MonthlyClosingFilterBusinessObject();

		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;
	}
}
