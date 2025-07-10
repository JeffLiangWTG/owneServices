using System;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module
{
	public class InstrumentNumberController : CMRSearchOnlyController
	{
		public InstrumentNumberController()
		{
		}

		public override ControllerID ID
		{
			get
			{
				return ControllerIDs.Customs.AU.InstrumentNumber;
			}
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CMRInstrument); }
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}
	}
}
