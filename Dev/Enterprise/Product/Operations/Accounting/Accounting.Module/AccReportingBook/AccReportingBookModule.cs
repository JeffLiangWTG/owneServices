using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class AccReportingBookModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.AccReportingBook;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.ReportingBooks;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.AlwaysAllow;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.AccReportingBook);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new AccReportingBookFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new AccReportingBookFilterControl(GridCollection, (AccReportingBookFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new AccReportingBookCollection(Factory);
		}

		public override bool AllowDelete => false;

		public override bool AllowUniversalCopy => false;
	}
}
