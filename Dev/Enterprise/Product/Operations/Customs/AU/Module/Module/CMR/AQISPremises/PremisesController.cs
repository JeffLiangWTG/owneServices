using System;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module
{
	public class PremisesController : CMRSearchOnlyController
	{
		public PremisesController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.AU.Premises; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CMRAqisPremises); }
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}
	}
}
