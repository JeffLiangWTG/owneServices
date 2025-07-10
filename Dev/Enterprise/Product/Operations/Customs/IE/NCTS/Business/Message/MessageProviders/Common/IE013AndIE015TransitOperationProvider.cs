using System;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	abstract class IE013AndIE015TransitOperationProvider : NctsDepartureHeaderMessageProvider, IIE013AndIE015TransitOperation
	{
		protected IE013AndIE015TransitOperationProvider(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}

		public string LRN => NCTSOutboundEDIMessage.LRNPlaceHolder;

		public string DeclarationType => MovementHeader.BM_InBondEntryType;

		public string AdditionalDeclarationType => MovementHeader.BM_AdditionalDeclarationType;

		public string TIRCarnetNumber => MovementHeader.TirCarnetNumber;

		public DateTime PresentationOfTheGoodsDateAndTime => DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(MovementHeader.BM_PresentationDateTime.ToZDateTime(), removeMillisecond: true);

		public string Security
		{
			get
			{
				switch (MovementHeader.BM_TypeOfSecurity)
				{
					case NctsTypeOfSecurityList.Codes.NON:
						return "0";
					case NctsTypeOfSecurityList.Codes.ENT:
						return "1";
					case NctsTypeOfSecurityList.Codes.EXI:
						return "2";
					case NctsTypeOfSecurityList.Codes.BTH:
						return "3";
					default:
						return "0";
				}
			}
		}

		public bool ReducedDatasetIndicator => MovementHeader.BM_ReducedDatasetIndicator;

		public string SpecificCircumstanceIndicator => MovementHeader.BM_SpecificCircumstance;

		public string CommunicationLanguageAtDeparture => EnglishLanguageCode;
		const string EnglishLanguageCode = "IE";

		public bool BindingItinerary => NctsHeader.CountriesOfRouting.Any(p => !p.CY_Data.IsEmpty);

		public DateTime LimitDate => DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(MovementHeader.BM_ExportDate, removeMillisecond: true);
	}
}
