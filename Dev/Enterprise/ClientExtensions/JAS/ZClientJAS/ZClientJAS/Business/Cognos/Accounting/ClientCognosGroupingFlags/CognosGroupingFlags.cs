using System.Data;

using CargoWise.EntityFramework;

namespace Enterprise.Client.JAS.Business.Cognos
{
	public class CognosGroupingFlags : AutoClientCognosGroupingFlags
	{
		public CognosGroupingFlags(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			T4_Company = ClientCognosGroupingFlagsLookups.IntercompanyCodes.Exclude;
		}
	}
}
