using System;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.ServiceManager.Shared;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class TransitOperationWrapper : ITransitOperation
	{
		TransitOperationWrapper(TP5MessageSendingObject sendingObject, bool isAmendment)
		{
			this.sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
			this.nctsHeader = Argument.NotNull((NctsHeader)sendingObject.NctsHeader, nameof(sendingObject));
			this.isAmendment = isAmendment;
		}

		readonly TP5MessageSendingObject sendingObject;
		readonly NctsHeader nctsHeader;

		readonly bool isAmendment;

		public static TransitOperationWrapper New(TP5MessageSendingObject sendingObject, bool isAmendment = false) => sendingObject == null ? null : new TransitOperationWrapper(sendingObject, isAmendment);

		NctsDepartureMovementHeader movementHeader => cachedMovementHeader ?? (cachedMovementHeader = nctsHeader.MovementHeader);
		NctsDepartureMovementHeader cachedMovementHeader;

		public string LRN => lrn ?? (lrn = movementHeader.BM_PaperlessInbondNum);
		string lrn;

		public string MRN => mrn ?? (mrn = nctsHeader.MovementReferenceNumber);
		string mrn;

		public string EORIOperateurBeneficiaireAgrement => eORIOperateurBeneficiaireAgrement ?? (eORIOperateurBeneficiaireAgrement = nctsHeader.Principal.Organisation?.GetEORI().Left(17) ?? string.Empty);
		string eORIOperateurBeneficiaireAgrement;

		public string DeclarationType => declarationType ?? (declarationType = movementHeader.BM_InBondEntryType);
		string declarationType;

		public string AdditionalDeclarationType => additionalDeclarationType ?? (additionalDeclarationType = movementHeader.BM_AdditionalDeclarationType);
		string additionalDeclarationType;
		public string TIRCarnetNumber => movementHeader.TirCarnetNumber;

		public DateTime? PresentationOfTheGoodsDateAndTime => presentationOfTheGoodsDateAndTime ?? (presentationOfTheGoodsDateAndTime = movementHeader.BM_PresentationDateTime.ToZDateTime().ToNullableDateTime());
		DateTime? presentationOfTheGoodsDateAndTime;

		public string Security => security ?? (security = GetSecurity(movementHeader.BM_TypeOfSecurity));
		string security;

		string GetSecurity(string typeOfSecurity)
		{
			switch (typeOfSecurity)
			{
				case EU.NCTS.Business.NctsTypeOfSecurityList.Codes.NON:
					return "0";
				case EU.NCTS.Business.NctsTypeOfSecurityList.Codes.ENT:
					return "1";
				case EU.NCTS.Business.NctsTypeOfSecurityList.Codes.EXI:
					return "2";
				case EU.NCTS.Business.NctsTypeOfSecurityList.Codes.BTH:
					return "3";
				default:
					return string.Empty;
			}
		}

		public bool ReducedDatasetIndicator => movementHeader.BM_ReducedDatasetIndicator;

		public string SpecificCircumstanceIndicator => specificCircumstanceIndicator ?? (specificCircumstanceIndicator = movementHeader.BM_SpecificCircumstance);
		string specificCircumstanceIndicator;

		public string CommunicationLanguageAtDeparture => communicationLanguageAtDeparture ?? (communicationLanguageAtDeparture = Core.Constants.CountryCodes.France.ToLowerInvariant());
		string communicationLanguageAtDeparture;

		public bool BindingItinerary => movementHeader.BM_TypeOfSecurity != EU.NCTS.Business.NctsTypeOfSecurityList.Codes.NON;

		public DateTime? LimitDate => limitDate ?? (limitDate = movementHeader.BM_ExportDate.ToNullableDateTime());
		DateTime? limitDate;

		public string OtherThingsToReport => null;

		public bool AmendmentTypeFlag => isAmendment;

		public string Motif => motif ?? (motif = sendingObject.Justification);
		string motif;
	}
}
