using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.PAVE.MENT.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.PAVE.MENT.Module
{
	public class MENTAgedScoreQueryModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.MENTAgedScoreQuery; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.MENTAgedScoreQuery);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new MENTAgedScoreQueryCollection(Factory, new ZQuery());
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new MENTAgedScoreQueryFilterControl(GridCollection, (MENTAgedScoreQueryFilterBusinessObject)FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new MENTAgedScoreQueryFilterBusinessObject();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.BufferManagement; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.MENTAgedScoreQuery; }
		}

		public override bool AllowUniversalCopy
		{
			get { return false; }
		}
	}
}
