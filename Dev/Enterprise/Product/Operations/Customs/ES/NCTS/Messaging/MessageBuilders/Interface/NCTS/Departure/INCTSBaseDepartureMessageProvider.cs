using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders
{
	public interface INctsBaseDepartureMessageProvider : IEDIFACTMessageDataProvider
	{
		#region Fields For BGM

		ZString LocalReferenceNumber { get; }

		#endregion

		#region Fields For CST

		ZString CustomsProcedureCategory5 { get; }

		#endregion

		#region Fields For LOC

		ZString CountryOfDestination { get; }
		ZString LocationOfGoodsExamCustomsOffice { get; }
		ZString LocationOfGoodsExam { get; }
		ZString CodeOfLoadingLocation { get; }
		ZString CodeOfUnloadingLocation { get; }

		#endregion

		#region Fields For GIS

		ZBool GoodsInContainerIndicator { get; }
		ZBool SecurityDeclaration { get; }

		#endregion

		#region Fields For EQD

		IReadOnlyCollection<ZString> CountryCodes { get; }

		#endregion

		#region Fields For SEL

		IReadOnlyCollection<ZString> SealCodes { get; }

		#endregion

		#region Fields For FTX

		ZString TransportMethodOfPayment { get; }
		ZString ConveyanceReferenceNumber { get; }

		#endregion

		#region Fields For RFF

		ZString ReferenceNumber { get; }
		ZString SpecificCircumstancesIndicator { get; }

		#endregion

		#region Fields For TDT

		ITransportMediumInfoCommon BorderTransportMode { get; }

		#endregion

		#region Fields For NAD

		IPartyProvider Consignor { get; }
		IPartyProvider Consignee { get; }
		IPartyEmailProvider Declarant { get; }
		IPartyProvider SecurityCarrier { get; }
		IPartyProvider SecurityConsignor { get; }
		IPartyProvider SecurityConsignee { get; }

		#endregion

		#region Fields For CNT

		ZInt TotalNumberOfGoods { get; }
		ZLong TotalNumberOfPackageElements { get; }

		#endregion
	}
}
