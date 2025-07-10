using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class DepartureAndAmendmentNCTS5CommonSendMessageWrapper : NCTS5DepartureAndNotificationCommonSendMessageWrapper, IDepartureNCTSCommonMessageDataProvider
	{
		public DepartureAndAmendmentNCTS5CommonSendMessageWrapper(NctsHeader header, ICertificateProvider certificateData, ZString messageType)
			: base(header, certificateData)
		{
			this.messageType = Argument.NotNullOrEmpty(messageType, nameof(messageType));
		}
		protected readonly ZString messageType;

		public IReadOnlyCollection<INCTSCommonAuthorisation> Authorisations
		{
			get
			{
				if (authorisations == null)
				{
					var authorisationsList = new List<NCTS5CommonAuthorisationWrapper>();

					ZShort seqNum = 1;
					foreach (var authorization in departureMovement.CusAuthorizationUsages)
					{
						authorisationsList.Add(new NCTS5CommonAuthorisationWrapper(authorization, seqNum));
						seqNum++;
					}
					authorisations = authorisationsList.AsReadOnly();
				}
				return authorisations;
			}
		}
		IReadOnlyCollection<NCTS5CommonAuthorisationWrapper> authorisations;

		public ZString CustomsOfficeOfDestinationDeclared => departureMovement.CustomsOffices.Where(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination).FirstOrDefault()?.CY_Data ?? ZString.Empty;

		public IReadOnlyCollection<INCTSCommonCustomsOffice> CustomsOfficeOfTransitDeclared
		{
			get
			{
				if (customsOfficeOfTransitDeclared == null)
				{
					var customsOfficeOfTransitDeclaredList = new List<NCTS5CommonCustomsOfficeWrapper>();

					var departureType = departureMovement.BM_InBondEntryType;
					if (departureType != NctsPhase5DeclarationTypeList.Codes.TIR && departureType != NctsPhase5DeclarationTypeList.Codes.T2SM)
					{
						var offices = departureMovement.CustomsOffices.Where(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit).Select(x => x.CY_Data);
						ZShort seqNum = 1;
						foreach (var code in offices)
						{
							customsOfficeOfTransitDeclaredList.Add(new NCTS5CommonCustomsOfficeWrapper(code, seqNum));
							seqNum++;
						}
					}
					customsOfficeOfTransitDeclared = customsOfficeOfTransitDeclaredList.AsReadOnly();
				}
				return customsOfficeOfTransitDeclared;
			}
		}
		IReadOnlyCollection<NCTS5CommonCustomsOfficeWrapper> customsOfficeOfTransitDeclared;

		public IReadOnlyCollection<INCTSCommonCustomsOffice> CustomsOfficeOfExitForTransitDeclared
		{
			get
			{
				if (customsOfficeOfExitForTransitDeclared == null)
				{
					var customsOfficeOfExitForTransitDeclaredList = new List<NCTS5CommonCustomsOfficeWrapper>();

					if (departureMovement.BM_TypeOfSecurity == NctsTypeOfSecurityList.Codes.EXI)
					{
						var offices = departureMovement.CustomsOffices.Where(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit).Select(x => x.CY_Data);
						ZShort seqNum = 1;
						foreach (var code in offices)
						{
							customsOfficeOfExitForTransitDeclaredList.Add(new NCTS5CommonCustomsOfficeWrapper(code, seqNum));
							seqNum++;
						}
					}
					customsOfficeOfExitForTransitDeclared = customsOfficeOfExitForTransitDeclaredList.AsReadOnly();
				}
				return customsOfficeOfExitForTransitDeclared;
			}
		}
		IReadOnlyCollection<NCTS5CommonCustomsOfficeWrapper> customsOfficeOfExitForTransitDeclared;

		public INCTSCompleteHolderOfTheTransitProcedure HolderOfTheTransitProcedure => holderOfTheTransitProcedure ?? (holderOfTheTransitProcedure = NCTS5CompleteHolderOfTheTransitProcedureWrapper.New(nctsHeader, Representative != null, nctsHeader.IsInPhase5TransitionPeriod));
		NCTS5CompleteHolderOfTheTransitProcedureWrapper holderOfTheTransitProcedure;

		public IReadOnlyCollection<INCTSCommonGuarantee> Guarantee => guarantee ?? (guarantee = NCTS5CommonGuaranteeWrapper.GetGuaranteeList(nctsHeader));
		IReadOnlyCollection<NCTS5CommonGuaranteeWrapper> guarantee;

		public INCTSCommonConsignmentDepartureAndAmendment Consignment => consignment ?? (consignment = new NCTS5CommonConsignmentDepartureAndAmendmentWrapper(nctsHeader, messageType));
		NCTS5CommonConsignmentDepartureAndAmendmentWrapper consignment;
	}
}
