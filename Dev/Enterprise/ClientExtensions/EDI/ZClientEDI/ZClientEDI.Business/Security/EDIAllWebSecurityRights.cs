using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.Security
{
	public class EDIAllWebSecurityRights : AllWebSecurityRights
	{
		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = (factory) => { return new EDIAllWebSecurityRights(factory); };
		}

		protected EDIAllWebSecurityRights(BusinessObjectFactory factory)
			: base()
		{
			this.Add(WebSecurityRightsList.New());
			this.Add(ReportsWebSecurityRights.New(factory));
		}
	}
}

