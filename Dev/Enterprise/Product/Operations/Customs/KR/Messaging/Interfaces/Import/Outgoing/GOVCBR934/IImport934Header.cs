using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImport934Header : IMessageDataProvider, IImport934_5SMHeader
	{
		ZString ImportDeclarationNumber { get; }
		IImport934FormA FormAData { get; }
		IImport934FormB FormBData { get; }
		ZString InvoiceNo { get; }
		ZDate InvoiceDate { get; }
		ZString ContractNo { get; }
		ZDate ContractDate { get; }
		ZDecimal TotalCustomsValueKRW { get; }
		ZBool ProvisionalPricingYN { get; }
		ZDecimal ProvisionalAdditionRate { get; }
		ZDate EstimatedDateOfFinalPrice { get; }
		ZDate ContractExpirationDate { get; }
		IEnumerable<IValueDeclarationCode> ProvisionalPricingReasons { get; }
		ZDecimal ProvisionalAdditionalAmount { get; }
	}
}
