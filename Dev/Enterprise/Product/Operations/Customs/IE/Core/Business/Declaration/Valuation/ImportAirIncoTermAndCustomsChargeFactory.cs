using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ImportAirIncoTermAndCustomsChargeFactory : ImportIncoTermAndCustomsChargeFactory
	{
		protected override IReadOnlyList<ICustomsChargeCode> IncludedInInvoiceCharges => includedInInvoiceChargesLazy.Value;

		readonly Lazy<ICustomsChargeCode[]> includedInInvoiceChargesLazy = new Lazy<ICustomsChargeCode[]>(
			() => ImportChargesProvider.IncludedInInvoice.Where(code => code.Code != AISChargeCodeList.Codes.BA).ToArray()
		);

		public override bool CanThisIncoTermHaveThisChargeForValidation(ZString incoTerm, ICustomsChargeCode charge)
		{
			var chargeConfiguration = GetConfiguration(incoTerm, charge?.Code ?? ZString.Empty);
			return chargeConfiguration != null && (chargeConfiguration.IsIncludedInInvoice || charge?.Code == AISChargeCodeList.Codes.BA);
		}
	}
}
