using System.Collections.Generic;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Accounting.ElectronicPayment.Common
{
	class ElectronicPaymentEDIMessagePurgeSettingsConfig : PurgeSettingsConfig
	{
		public override IEnumerable<ApplicationCodeObj> GetPurgeSettings()
		{
			yield return AddApplicationCodeMessageSubTypePurgeType(ApplicationCodeList.Codes.GlobalElectronicPayment, new InterchangeObjCollection() { NewInterchangeConfigObj(6, TimeUnit.Month) }, new MessageSubTypePurgeTypeObjCollection()
				.Add(GEPProviderAPICommandList.Codes.CreateADeal, GEPProviderAPICommandList.Descriptions.CreateADeal, 6, TimeUnit.Month)
				.Add(GEPProviderAPICommandList.Codes.GetAQuote, GEPProviderAPICommandList.Descriptions.GetAQuote, 6, TimeUnit.Month)
				.Add(GEPProviderAPICommandList.Codes.GetRates, GEPProviderAPICommandList.Descriptions.GetRates, 6, TimeUnit.Month)
				.Add(GEPProviderAPICommandList.Codes.SearchBeneficiary, GEPProviderAPICommandList.Descriptions.SearchBeneficiary, 6, TimeUnit.Month)
			);
		}
	}
}
