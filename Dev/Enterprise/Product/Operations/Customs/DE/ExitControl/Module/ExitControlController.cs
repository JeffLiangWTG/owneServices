using System;
using Enterprise.Customs.DE.ExitControl.Business;

namespace Enterprise.Customs.DE.ExitControl.Module
{
	public class ExitControlController : EU.ExitControl.Module.ExitControlController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(CusExitHeader);
	}
}
