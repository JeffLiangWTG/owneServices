using System;
using CargoWise.Definitions;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.BR.Module
{
	public class GoodsCatalogOperationalActionSupporter : OperationalActionSupporter
	{
		public override BusinessContext BusinessContext => BusinessContext.BRGoodsCatalog;

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.GoodsCatalog;

		public override Type RootType => typeof(BaseCusGoodsCatalog);

		protected override void PopulateMethods(OperationalActionMethodList list)
		{
			base.PopulateMethods(list);
			list.Add(ActionMethodProviderIDs.BRGoodsCatalog);
		}
	}
}
