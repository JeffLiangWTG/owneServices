using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module
{
	public class DrawbackEntryLineModule : EntryLineModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.AU.DrawbackEntryLine; }
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new DrawbackEntryLineFilterBusinessObject(GridCollection);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new GlobalDrawbackCusEntryLineCollection(Factory);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new EntryLineFilterControl((GlobalDrawbackCusEntryLineCollection)GridCollection, (DrawbackEntryLineFilterBusinessObject)FilterBusinessObject);
		}
	}
}
