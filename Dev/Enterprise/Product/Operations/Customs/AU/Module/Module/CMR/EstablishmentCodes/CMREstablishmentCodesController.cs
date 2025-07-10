using System;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module
{
	public class CMREstablishmentCodesController : CMRSearchOnlyController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.AU.CMREstablishmentCodes; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CMREstablishmentCodes); }
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}
	}
}
