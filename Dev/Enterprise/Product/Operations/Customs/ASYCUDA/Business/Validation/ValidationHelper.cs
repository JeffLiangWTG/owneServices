using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public static class ValidationHelper
	{
		public static void CheckCurrency(ZPropertyInfo currencyInfo, ZDecimal amount, bool mandatoryCurrency = false)
		{
			ListValidation.ErrorIfInvalidCode(currencyInfo);
			if (!amount.IsEmpty || mandatoryCurrency)
			{
				MandatoryValidation.MessageErrorIfNotEntered(currencyInfo);
			}
		}

		public static void CheckCustomsPort(ZPropertyInfo customsPortInfo, ZPropertyInfo portInfo, RefUNLOCO port, AsycudaManifestHeader header)
		{
			var customsPort = (ZString)customsPortInfo.Value;
			var portCode = (ZString)portInfo.Value;

			if (header != null)
			{
				if (portCode.SubstringSafe(0, 2) == header.Country.Code)
				{
					ListValidation.MessageErrorIfInvalidCodeOrEmpty(customsPortInfo);
				}
				if (port != null && !customsPort.IsEmpty && !customsPortInfo.HasMessageError(ListValidation.InvalidCodeMessageError.ToString()))
				{
					var unloco = new RefUNLOCO.Loader(header.Factory).Load(customsPort);
					if (unloco == null)
					{
						var localPorts = header.GetCustomsLocalCodeList(port);
						if (!localPorts?.Any(x => x.RY_LocalPortCode.ToUpper() == customsPort.ToUpper()) ?? true)
						{
							customsPortInfo.AddMessageError(ValidationConstants.InvalidPort(portInfo.HumanReadableName));
						}
					}
				}
			}
			else if (!customsPort.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(customsPortInfo);
			}
		}
	}
}
