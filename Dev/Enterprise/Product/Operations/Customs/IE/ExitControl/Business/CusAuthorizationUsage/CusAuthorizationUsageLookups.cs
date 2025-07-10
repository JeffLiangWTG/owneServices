using System;
using System.Collections;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class CusAuthorizationUsageLookups : EU.Business.CusAuthorizationUsageLookups
	{
		public CusAuthorizationUsageLookups(CusAuthorizationUsage parent) : base(parent)
		{
		}

		public new CusAuthorizationUsage Parent => (CusAuthorizationUsage)base.Parent;

		public override ICollection CodeList => Parent == null ? new CodeDescriptionPairList() : Provider.GetAuthorisationTypeList(Factory);

		[ThreadSafe]
		static readonly Lazy<EU.Business.CusAuthorisationHeaderProvider> providerInstance = new Lazy<EU.Business.CusAuthorisationHeaderProvider>(() => new EU.Business.CusAuthorisationHeaderProvider(Core.Constants.CountryCodes.Ireland));
		static EU.Business.CusAuthorisationHeaderProvider Provider => providerInstance.Value;
	}
}

