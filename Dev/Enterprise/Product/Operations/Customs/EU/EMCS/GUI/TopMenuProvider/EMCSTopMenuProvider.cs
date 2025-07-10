using System.Collections;
using CargoWise.Application;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.EU.EMCS.GUI
{
	public class EMCSTopMenuProvider : IEMCSTopMenuProvider
	{
		public static IEMCSTopMenuProvider GetTopMenuProvider(string countryCode)
		{
			object provider = null;

			var providers = ObjectFactory.Get<Hashtable>("EMCSTopMenuProviders");
			if (!string.IsNullOrEmpty(countryCode))
			{
				var objectHandle = providers[countryCode] as ObjectHandle;
				provider = objectHandle?.GetObject();
			}
			if (provider == null)
			{
				var objectHandle = providers["Default"] as ObjectHandle;
				provider = objectHandle?.GetObject();
			}
			return (IEMCSTopMenuProvider)provider;
		}

		EMCSMenu IEMCSTopMenuProvider.TopLevelMenu(EMCSJobDeclaration declaration)
		{
			return new EMCSMenu(declaration);
		}
	}
}
