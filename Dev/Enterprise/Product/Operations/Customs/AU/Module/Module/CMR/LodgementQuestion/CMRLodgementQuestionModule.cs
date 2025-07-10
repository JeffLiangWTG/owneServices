using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module
{
	public class CMRLodgementQuestionModule : CMRSearchOnlyModule
	{
		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.AU.CMRLodgementQuestion);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CMRLodgementQuestionFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new CMRLodgementQuestionFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CMRLodgementQuestionCollection(Factory);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.CMRLodgementQuestion; }
		}

		protected override bool ShowRecentItemsCore()
		{
			return false;
		}
	}
}
