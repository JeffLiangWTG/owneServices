using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IExportDocumentCommon : IDocumentsCommon
	{
		#region Fields For DTM

		ZDateTime DateOfIssue { get; }
		ZDateTime DateOfExpiry { get; }

		#endregion
	}

	public interface IExportLineCommon
	{
		#region Fields For CST

		ZInt GoodsItemNumber { get; }

		#endregion

		#region Fields For MEA

		ZDecimal GrossWeightInKG { get; }
		ZDecimal NetWeightInKG { get; }
		ZDecimal OtherUnitsNumber { get; }
		ZString OtherUnitsQualifier { get; }

		#endregion

		#region Fields For MOA

		ZDecimal TotalGoodValueInEuros { get; }

		#endregion
	}

	public interface IExportDeclarantPartyIdProvider : IPartyNameProvider
	{
		ZString PartyQualifier { get; }
		ZString EmailAddress { get; }
		ZString NameCode { get; }
	}

	public interface IExportMessageDataProviderCommon : IEDIFACTMessageDataProvider
	{
		#region Fields For BGM

		ZString LocalReferenceNumber { get; }

		#endregion

		#region Fields For NAD

		IExportDeclarantPartyIdProvider Declarant { get; }

		#endregion

		#region Fields For TOD

		ZString TermsOfDeliveryCode { get; }

		#endregion

		#region Fields For LOC

		ZString DeliveryLocation { get; }

		#endregion

		#region Fields For MOA

		ZDecimal TotalAmount { get; }
		ZString TotalAmountCurrencyCode { get; }

		#endregion

		#region Fields For CNT

		ZInt TotalNumberOfGoods { get; }

		#endregion
	}
}
