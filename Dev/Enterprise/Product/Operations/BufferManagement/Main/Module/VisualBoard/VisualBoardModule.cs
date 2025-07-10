using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Module
{
	public class VisualBoardModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.VisualBoard; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.VisualBoard);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			throw new ModuleGuiNotSupportedException("");
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new BMBoardCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new BMBoardFilterBusinessObject();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.BufferManagement; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.VisualBoards; }
		}

		public override bool AllowUniversalCopy
		{
			get { return false; }
		}
	}
}
