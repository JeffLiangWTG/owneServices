using System;
using System.Collections.Generic;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class RF415GoodsInformationProvider : IRF415GoodsInformation
	{
		readonly CusEntryLine entryLine;
		readonly EntryLineWrapper entryLineWrapper;
		readonly JobComInvoiceLine randomInvoiceLine;

		public RF415GoodsInformationProvider(CusEntryLine entryLine, EntryHeaderWrapper entryHeaderWrapper)
		{
			this.entryLine = entryLine;
			entryLineWrapper = new EntryLineWrapper(entryLine, entryHeaderWrapper);
			randomInvoiceLine = entryLine.RandomLine;
		}

		public IGoodsInformationTypeCommodityCode CommodityCode => new GoodsInformationTypeCommodityCodeProvider(entryLineWrapper);

		public string GoodsDescription => randomInvoiceLine.JI_Description;

		public IGoodsQuantity GoodsQuantity => null;

		public IMoney CustomsValue => new MoneyProvider(entryLine.CL_CustomsValue, Core.Constants.CurrencyCodes.EuropeanUnion);

		public IReadOnlyCollection<ITypeOfDuty> TypeOfDuty => Array.Empty<ITypeOfDuty>();

		public decimal NetMass => 0m;

		public decimal SupplementaryUnitsValue => 0m;
	}
}
