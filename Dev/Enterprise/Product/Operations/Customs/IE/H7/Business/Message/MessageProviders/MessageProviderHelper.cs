using System.Collections.Generic;
using Enterprise.Customs.IE.Business;

namespace Enterprise.Customs.IE.H7.Business
{
	public static class MessageProviderHelper
	{
		public static List<string> GetAdditionalProcedures(AsycudaPackedItem packedItem)
		{
			var additionalProcedureCodes = new List<string>();

			switch (packedItem.Bill.ABL_Procedure)
			{
				case EU.H7.Business.EUH7AdditionalProcedureCodeList.Codes.C07F48:
					additionalProcedureCodes.Add(UniversalReferenceConstants.ProcedureCodes.Concession.C07);
					additionalProcedureCodes.Add(UniversalReferenceConstants.ProcedureCodes.Concession.F48);
					break;
				case EU.H7.Business.EUH7AdditionalProcedureCodeList.Codes.C07F49:
					additionalProcedureCodes.Add(UniversalReferenceConstants.ProcedureCodes.Concession.C07);
					additionalProcedureCodes.Add(UniversalReferenceConstants.ProcedureCodes.Concession.F49);
					break;
				default:
					additionalProcedureCodes.Add(packedItem.Bill.ABL_Procedure);
					break;
			}

			return additionalProcedureCodes;
		}

		public static CargoWise.Customs.IE.MessageContracts.AIS.Interfaces.IParty GetExporter(AsycudaBill bill)
		{
			return new BillPartyProvider(bill, ManifestBase.AsycudaBillAddress.AddressType.Shipper);
		}

		public static CargoWise.Customs.IE.MessageContracts.AIS.Interfaces.IParty GetExporter(AsycudaPack pack)
		{
			if (pack is null)
			{
				return null;
			}
			return GetExporter(pack.Bill);
		}
	}
}
