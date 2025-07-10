using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public static class ContainerHelper
	{
		public static ZString GetFirstESContainerPackagingType(CusEntryLine entryLine)
		{
			var result = string.Empty;
			if (entryLine.ContainersPivot.Any())
			{
				var containerPivot = (Customs.Business.CusContainerInvoiceLinePivot)entryLine.ContainersPivot.FirstOrDefault();
				var codeMap = (RefContainerCodeMap)containerPivot.Container?.Container?.CodeMapCollection.FirstOrDefault(x => ((RefContainerCodeMap)x).RCM_RN_NKCountry == CountryCodes.Spain);

				result = (codeMap != null) ? codeMap.RCM_Code : (ZString)BusinessQuantityUnit.Container;
			}

			return result;
		}
	}
}
