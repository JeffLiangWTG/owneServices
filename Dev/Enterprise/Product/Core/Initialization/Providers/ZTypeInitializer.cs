using CargoWise.Application;
using CargoWise.Types;

namespace Enterprise.Initialisation.Providers
{
	class ZTypeInitializer : IMandatoryStateInitializer
	{
		#region IMandatoryStateInitializer Members

		public void Initialize()
		{
			ObjectCache.Initialize(new ZTypeObjectCache());
		}

		#endregion
	}
}
