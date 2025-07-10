using System;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module
{
	public class AQISProducerCodeController : CMRSearchOnlyController
	{
		public AQISProducerCodeController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.AU.AQISProducerCode; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CMRAqisProducer); }
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}
	}
}
