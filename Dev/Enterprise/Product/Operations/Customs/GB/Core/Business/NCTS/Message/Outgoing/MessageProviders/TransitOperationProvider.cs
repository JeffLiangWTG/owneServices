using System;
using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class TransitOperationProvider : ITransitOperation
	{
		readonly NctsHeader header;
		protected readonly NctsDepartureMovementHeader depHeader;

		public TransitOperationProvider(NctsHeader nctsHeader)
		{
			header = Argument.NotNull(nctsHeader, nameof(nctsHeader));
			depHeader = Argument.NotNull(header.MovementHeader, nameof(depHeader));
		}

		public string LRN => (depHeader.BM_EntryDate.IsEmpty || isDepartureDeclaration) && !depHeader.BM_PaperlessInbondNum.IsEmpty ? (string)depHeader.BM_PaperlessInbondNum : null;

		public string DeclarationType => depHeader.BM_InBondEntryType;

		public virtual string AdditionalDeclarationType => depHeader.BM_AdditionalDeclarationType;

		public string TIRCarnetNumber => !depHeader.TirCarnetNumber.IsEmpty ? (string)depHeader.TirCarnetNumber : null;

		public virtual int? Security
		{
			get
			{
				switch (depHeader.BM_TypeOfSecurity)
				{
					case NctsTypeOfSecurityList.Codes.NON:
						return 0;
					case NctsTypeOfSecurityList.Codes.ENT:
						return 1;
					case NctsTypeOfSecurityList.Codes.EXI:
						return 2;
					case NctsTypeOfSecurityList.Codes.BTH:
						return 3;
					default:
						return null;
				}
			}
		}

		public virtual bool ReducedDatasetIndicator => depHeader.BM_ReducedDatasetIndicator;

		public bool BindingItinerary => header.CountriesOfRouting.Any();

		public DateTime? LimitDate => depHeader.IsSimplifiedNctsProcedure && depHeader.BM_ExportDate.IsValid ? DataProviderHelper.GetProviderDateTime(depHeader.BM_ExportDate.ToDateTime()) : null;

		public virtual string SpecificCircumstanceIndicator => !depHeader.BM_SpecificCircumstance.IsEmpty ? (string)depHeader.BM_SpecificCircumstance : null;

		public virtual string CommunicationLanguageAtDeparture => GlbStaff.CurrentUser.GS_WorkingLanguage.ToLower().Left(2);

		public string MRN => !depHeader.BM_EntryDate.IsEmpty && !header.MovementReferenceNumber.IsEmpty ? (string)header.MovementReferenceNumber : null;

		public bool AmendmentTypeFlag => amendmentType;

		public DateTime? PresentationDateAndTime => header.MovementHeader.BM_ArrivalDate.IsValid ? DataProviderHelper.GetProviderDateTime(header.MovementHeader.BM_ArrivalDate) : null;

		bool amendmentType => depHeader.BM_Phase == GB_NCTS5DeparturePhaseList.Codes.Amendment && depHeader.BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid && header.EffectiveMessageStatus == LogicalStatusList.Codes.Accepted;

		bool isDepartureDeclaration => depHeader.BM_Phase == GB_NCTS5DeparturePhaseList.Codes.DepartureDeclaration;
	}
}
