using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.Registry;

namespace Enterprise.DocumentEngineCore
{
	public static class DeliveryMethodValidation
	{
		public static void ValidateEPrint(ZPropertyInfo useEPrintInfo)
		{
			if (string.IsNullOrEmpty(DocumentsDataRegistry.Instance.EPrintEmailAddress.Value))
			{
				useEPrintInfo.AddError(Res.GetString("45d17575-3ed5-454c-8ada-58fa0ec29632", "ePrint email address is not defined. This can be set in the registry setting Documents > ePrint Email Address."));
			}
		}
	}
}
