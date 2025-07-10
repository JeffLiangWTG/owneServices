using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.ImportSiscomex.Outgoing;

namespace Enterprise.Customs.BR.Business.ImportSiscomex
{
	public class DeclarationCustomsValuationProvider : IDeclarationCustomsValuation
	{
		public DeclarationCustomsValuationProvider(NveCusCodeData nveCusCodeData)
		{
			this.nveCusCodeData = Argument.NotNull(nveCusCodeData, nameof(nveCusCodeData));
		}

		readonly NveCusCodeData nveCusCodeData;

		public static DeclarationCustomsValuationProvider New(NveCusCodeData nveCusCodeData) => nveCusCodeData == null ? null : new DeclarationCustomsValuationProvider(nveCusCodeData);

		public string Position => nveCusCodeData.CY_Order.ToString();
		public string Attribute => nveCusCodeData.CY_Code;
		public string Specification => nveCusCodeData.CY_Data;
	}
}
