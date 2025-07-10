using System;
using CargoWise.Definitions;
using Enterprise.Customs.CA.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
namespace Enterprise.Customs.CA.Module
{
	public class B2AdjustmentsOperationalActionSupporter : OperationalActionSupporter
	{
		public override Type RootType => typeof(JobDeclaration);

		public override BusinessContext BusinessContext => BusinessContext.B2Adjustments;

		public override BusinessContext DocumentBusinessContext => BusinessContext.Customs;

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.CAB2Adjustments;

		protected override void PopulateMethods(OperationalActionMethodList list)
		{
			base.PopulateMethods(list);
			list.Add(ActionMethodProviderIDs.B2Adjustments);
		}
	}
}
