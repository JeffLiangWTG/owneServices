using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsHeaderGenerator
	{
		public NctsHeader GenerateArrivalFromDeparture(NctsHeader departureHeader) => GenerateArrivalFromDepartureCore(departureHeader);

		public ZBool IsGenerationAllowed(NctsHeader departureHeader)
		{
			var (duplicate, _) = NctsHelper.RetrieveLRNOfDeclarationWithMatchingMRN(departureHeader.PK, departureHeader.MovementReferenceNumber, departureHeader.Factory, NctsMovementType.Codes.Arrival);
			return !duplicate;
		}

		protected virtual NctsHeader GenerateArrivalFromDepartureCore(NctsHeader departureHeader)
		{
			var factory = departureHeader.Factory;

			var arrivalHeader = factory.New<NctsHeader>();
			arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			arrivalHeader.EffectiveMessageStatus = NewMessageStatus;
			arrivalHeader.BH_ExportFlag = EventFlagList.Codes.No;

			CreateArrivalAddressFromDeparture(arrivalHeader.DestinationTrader, departureHeader.Consignee);
			CreateArrivalMovementHeaderFromDeparture(arrivalHeader.ArrivalMovementHeader, departureHeader);
			CreateArrivalEntryNumFromDeparture(arrivalHeader, departureHeader);

			factory.Save();

			return arrivalHeader;
		}

		protected virtual void CreateArrivalAddressFromDeparture(JobDocAddress arrivalAddress, JobDocAddress copyFromAddress)
		{
			var args = new BusinessObjectCloneArgs(new[] { JobDocAddressSchema.Constants.E2_ParentID, JobDocAddressSchema.Constants.E2_ParentTableCode, JobDocAddressSchema.Constants.E2_AddressType });
			arrivalAddress.CopyPersistentValuesFrom(copyFromAddress, args);
		}

		protected virtual void CreateArrivalMovementHeaderFromDeparture(NctsArrivalMovementHeader arrivalMovementHeader, NctsHeader departureHeader)
		{
			arrivalMovementHeader.BM_SubApplicationCode = NctsMoveHeaderType.Codes.Arrival;

			if (FillUnloadingDate)
			{
				arrivalMovementHeader.BM_UnloadingDate = ZDateTimeOffset.Now;
			}

			CreateArrivalOfficeFromDeparture(arrivalMovementHeader, departureHeader);

			if (departureHeader.MovementHeader.IsTIRDeclaration)
			{
				if (FillDischargeType)
				{
					arrivalMovementHeader.BM_DischargeType = NctsConstants.DischargeTypes.FullDischarge;
				}

				CreateArrivalAuthorizationUseageFromDeparture(arrivalMovementHeader, NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForTirProcedure, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir);
			}
			else
			{
				CreateArrivalAuthorizationUseageFromDeparture(arrivalMovementHeader, NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForUnionTransit, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit);
			}
		}

		protected virtual void CreateArrivalOfficeFromDeparture(NctsArrivalMovementHeader arrivalMovementHeader, NctsHeader departureHeader)
		{
			var header = arrivalMovementHeader.Header;
			var customsOfficeDestination = NctsEuOfficeCode.LoadOrCreate<NctsEuOfficeCode>(header.IsPhase5 ? arrivalMovementHeader : header, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival);
			customsOfficeDestination.CY_Data = departureHeader.IsPhase5 ? departureHeader.MovementHeader.DestinationCustomsOfficeCodeForDeparture : departureHeader.DestinationCustomsOfficeCodeForDeparture;

			var customsOffices = header.IsPhase5 ? arrivalMovementHeader.CustomsOffices : header.CustomsOffices;
			customsOffices.Reload(false);
		}

		protected virtual void CreateArrivalAuthorizationUseageFromDeparture(NctsArrivalMovementHeader arrivalMovementHeader, ZString permitType, ZString authorizationCode)
		{
			var trader = arrivalMovementHeader.Header.DestinationTrader;
			var query = new ZQuery(CusPermitHeaderSchema.CPH_ApplicationCode, CusPermitHeaderApplicationCodeList.Codes.Authorisation);
			query.AddToFilter(CusPermitHeaderSchema.CPH_Type, permitType);
			query.AddToFilter(CusPermitHeaderSchema.CPH_OH_PermitHolder, trader.OrganisationPK);
			var queryResult = arrivalMovementHeader.Factory.Load<CusAuthorisationHeader>(query);

			if (queryResult.Any())
			{
				var permit = queryResult.FirstOrDefault();

				var authorizationUsage = arrivalMovementHeader.Header.CusAuthorizationUsages.AddNew();
				authorizationUsage.AGC_Code = authorizationCode;
				authorizationUsage.AGC_Number = permit.CPH_Number;
				authorizationUsage.AGC_OH_Owner = permit.CPH_OH_PermitHolder;
			}
		}

		protected virtual void CreateArrivalEntryNumFromDeparture(NctsHeader arrivalHeader, NctsHeader departureHeader)
		{
			var entryNum = CusEntryNumber.LoadOrCreate(arrivalHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, GlbCompany.CurrentCompany.Country.Code);
			entryNum.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			entryNum.CE_EntryNum = departureHeader.MovementReferenceEntryNumber.CE_EntryNum;
		}

		protected virtual string NewMessageStatus => ZString.Empty;

		protected virtual bool FillUnloadingDate => true;
		protected virtual bool FillDischargeType => true;
	}
}
