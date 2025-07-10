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
	public class MENTAgedScoreExtractionModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.MENTAgedScoreExtraction; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.MENTAgedScoreExtraction);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new MENTAgedScoreExtractionCollection(Factory, new ZQuery());
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new MENTAgedScoreExtractionFilterControl(GridCollection, (MENTAgedScoreExtractionFilterBusinessObject)FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new MENTAgedScoreExtractionFilterBusinessObject();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.BufferManagement; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.MENTAgedScoreExtraction; }
		}

		public override bool AllowUniversalCopy
		{
			get { return false; }
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		public override bool AllowEdit
		{
			get { return true; }
		}

		public override bool AllowView
		{
			get { return true; }
		}

		public override bool AllowNew
		{
			get { return false; }
		}
	}
}
