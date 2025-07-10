using System.Collections;
using CargoWise.Application;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.EU.EMCS.GUI
{
	public class EMCSMessageSendingFormConfiguration : IEMCSMessageSendingFormConfiguration
	{
		public static IEMCSMessageSendingFormConfiguration GetConfiguration(string countryCode)
		{
			object provider = null;

			var configs = ObjectFactory.Get<Hashtable>("EMCSMessageSendingFormConfigurations");
			if (!string.IsNullOrEmpty(countryCode))
			{
				var objectHandle = configs[countryCode] as ObjectHandle;
				provider = objectHandle?.GetObject();
			}

			if (provider == null)
			{
				var objectHandle = configs["Default"] as ObjectHandle;
				provider = objectHandle?.GetObject();
			}

			return (IEMCSMessageSendingFormConfiguration)provider;
		}

		bool IEMCSMessageSendingFormConfiguration.IsOKToSend<TSendingAction>(EMCSMessageSendingActionParent<TSendingAction> parent) => true;
	}
}
