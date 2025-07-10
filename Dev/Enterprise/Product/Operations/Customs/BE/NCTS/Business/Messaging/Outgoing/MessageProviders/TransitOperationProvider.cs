using System;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class TransitOperationProvider : ITransitOperation
	{
		protected readonly NctsHeader header;
		protected readonly NctsDepartureMovementHeader depHeader;

		public TransitOperationProvider(NctsHeader nctsHeader)
		{
			header = Argument.NotNull(nctsHeader, nameof(nctsHeader));
			depHeader = Argument.NotNull(header.MovementHeader, nameof(depHeader));
		}

		public virtual string LRN => depHeader.BM_PaperlessInbondNum;

		public virtual string MRN => null;

		public string DeclarationType => depHeader.BM_InBondEntryType;

		public virtual string AdditionalDeclarationType => depHeader.BM_AdditionalDeclarationType;

		public string TIRCarnetNumber => depHeader.TirCarnetNumber;

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

		public bool BindingItinerary => false;

		public DateTime? LimitDate => depHeader.IsSimplifiedNctsProcedure && depHeader.BM_ExportDate.IsValid ? depHeader.BM_ExportDate.ToDateTime() : null;

		public virtual string SpecificCircumstanceIndicator => depHeader.BM_SpecificCircumstance;

		public virtual string CommunicationLanguageAtDeparture => header.BH_CommunicationLanguage;

		public bool AmendmentTypeFlag => depHeader.BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid;

		public DateTime? PresentationDateAndTime
		{
			get
			{
				var arrivalDate = header.MovementHeader.BM_ArrivalDate;
				if (arrivalDate.IsValid)
				{
					return DateTime.SpecifyKind(arrivalDate.ToUniversalBranchTime().ToDateTime(), DateTimeKind.Unspecified);
				}
				return null;
			}
		}
	}
}
