using System;
using Enterprise.Customs.DE.Intrastat.Business;

namespace Enterprise.Customs.DE.Intrastat.Module
{
	public class IntrastatTransactionsController : EU.Intrastat.Module.IntrastatTransactionsController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(CusIntrastatHeader);
	}
}
