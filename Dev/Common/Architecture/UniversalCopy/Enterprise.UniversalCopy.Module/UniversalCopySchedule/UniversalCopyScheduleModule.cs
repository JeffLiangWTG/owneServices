using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.UniversalCopy.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.UniversalCopy.Module
{
	public class UniversalCopyScheduleModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get
			{
				return ModuleIDs.UniversalCopySchedule;
			}
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return new UniversalCopyScheduleController((StmUniversalCopy)selectedBusinessObject);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new UniversalCopyScheduleFilterControl((StmUniversalCopyCollection)GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new StmUniversalCopyCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new UniversalCopyScheduleFilterBusinessObject();
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		#region Checkpoints

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.UniversalCopy; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.UniversalCopySchedules; }
		}

		#endregion
	}
}
