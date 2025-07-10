using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ErrorReporting.Business;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ErrorReporting.Module
{
	public class ErrorReportingModule : ZFilterGridModule
	{
		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.ErrorReportDetails);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ErrorReportFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ErrorReportFilterControl(GridCollection, (ErrorReportFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new StmErrorReportCollection(Factory);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.ErrorReporting; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.AlwaysAllow; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		public override bool AllowEdit
		{
			get { return false; }
		}
	}
}
