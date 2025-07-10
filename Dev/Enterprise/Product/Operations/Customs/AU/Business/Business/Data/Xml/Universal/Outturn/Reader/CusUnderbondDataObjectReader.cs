using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusUnderbondDataObjectReader : ShipmentDataObjectReader<CusUnderbond>
	{
		public CusUnderbondDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory) : base(dataObject, logger, factory)
		{
		}

		protected override void PopulateBusinessObject(CusUnderbond targetBO)
		{
			var headerRow = GetColumnIndexer(targetBO);
			if (targetBO.UnderbondStatus.Code != CMRUnderbondStatuses.Codes.UnderbondApprovalAdviceReceived)
			{
				SetValue(headerRow, CusUnderbondSchema.C4_MAWB, dataObject.WayBillNumber);
				SetValue(headerRow, CusUnderbondSchema.C4_FlightNo, dataObject.VoyageFlightNo);
				SetValue(headerRow, CusUnderbondSchema.C4_PiecesManifested, dataObject.TotalNoOfPieces);
				SetValue(headerRow, CusUnderbondSchema.C4_ModeOfMovement, dataObject.TransportMode.GetNullableCodeAsUpperCase() ?? ZString.Empty);
				SetValue(headerRow, CusUnderbondSchema.C4_MovementReason, dataObject.ShipmentType.GetNullableCodeAsUpperCase() ?? ZString.Empty);

				FillDates(headerRow);
				FillOrgAddresses(headerRow);
				FillAddInfos(headerRow);
			}
			else
			{
				FillOuturnedDate(headerRow);
			}

			FillSubShipments(targetBO);

			if (targetBO.MAWB is CusMAWB mawb)
			{
				mawb.LogOutturnsReadyForSendingEvent(logger);
			}
		}

		void FillDates(IColumnIndexer headerRow)
		{
			if (dataObject.DateCollection != null && dataObject.DateCollection.Count > 0)
			{
				FillDates(headerRow, dataObject.DateCollection, ZBool.False,
					new DateTypeSchemaColumnMap(CusUnderbondSchema.C4_ArrivalDate, new[] { DateType.Arrival }),
					new DateTypeSchemaColumnMap(CusUnderbondSchema.C4_Outurned, new[] { DateType.Unpack }));
			}
		}

		void FillOuturnedDate(IColumnIndexer headerRow)
		{
			if (dataObject.DateCollection != null && dataObject.DateCollection.Count > 0)
			{
				FillDates(headerRow, dataObject.DateCollection, ZBool.False,
					new DateTypeSchemaColumnMap(CusUnderbondSchema.C4_Outurned, new[] { DateType.Unpack }));
			}
		}

		void FillOrgAddresses(IColumnIndexer headerRow)
		{
			var orgAddressCollection = dataObject.OrganizationAddressCollection;
			var additionalReferences = dataObject.AdditionalReferenceCollection;

			FillOrgAddressesCore(headerRow, orgAddressCollection, CusUnderbondSchema.C4_OA_DischargeAddress, Outturn.Constants.AddressType.DischargeAddress,
				additionalReferences, CusUnderbondSchema.C4_DischargePremiseID, Outturn.Constants.AdditionalReference.EntryType.Codes.DischargePremiseID);

			FillOrgAddressesCore(headerRow, orgAddressCollection, CusUnderbondSchema.C4_OA_OriginAddress, Outturn.Constants.AddressType.OriginAddress,
				additionalReferences, CusUnderbondSchema.C4_OriginPremiseID, Outturn.Constants.AdditionalReference.EntryType.Codes.OriginPremiseID);

			FillOrgAddressesCore(headerRow, orgAddressCollection, CusUnderbondSchema.C4_OA_DestinationAddress, Outturn.Constants.AddressType.DestinationAddress,
				additionalReferences, CusUnderbondSchema.C4_DestinationPremiseID, Outturn.Constants.AdditionalReference.EntryType.Codes.DestinationPremiseID);

			FillAdditionalReferencesCore(headerRow, additionalReferences, CusUnderbondSchema.C4_ResponsiblePartyID, DataTransfer.Universal.Constants.AdditionalReference.EntryType.Codes.ResponsiblePartyID);
		}

		void FillOrgAddressesCore(IColumnIndexer headerRow, IEnumerable<OrganizationAddress> orgAddressCollection, SchemaGuidColumn orgAddressColumn, ZString orgAddressType,
			IEnumerable<AdditionalReference> additionalReferenceCollection, SchemaStringColumn additionalReferencecolumn, ZString entryType,
			string contextInformation = Core.Constants.CountryCodes.Australia)
		{
			var organizationAddress = orgAddressCollection?.FirstOrDefault(orgAddressType);
			if (organizationAddress != null)
			{
				var orgAddress = new OrganisationDataObjectReader(organizationAddress, logger, factory).GetMatched();

				SetValue(headerRow, orgAddressColumn, orgAddress?.PK ?? ZGuid.Empty);
				if (orgAddress == null)
				{
					FillAdditionalReferencesCore(headerRow, additionalReferenceCollection, additionalReferencecolumn, entryType, contextInformation);
				}
			}
			else
			{
				FillAdditionalReferencesCore(headerRow, additionalReferenceCollection, additionalReferencecolumn, entryType, contextInformation, () => SetValue(headerRow, orgAddressColumn, ZGuid.Empty));
			}
		}

		void FillAdditionalReferencesCore(IColumnIndexer headerRow, IEnumerable<AdditionalReference> additionalReferenceCollection, SchemaStringColumn column, ZString entryType,
			string contextInformation = Core.Constants.CountryCodes.Australia, Action preAction = null)
		{
			var additionalReference = additionalReferenceCollection?.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == entryType && x.ContextInformation.GetValueOrDefault() == contextInformation);
			if (additionalReference != null)
			{
				preAction?.Invoke();
				SetValue(headerRow, column, additionalReference.ReferenceNumber.GetValueOrDefault());
			}
		}

		void FillAddInfos(IColumnIndexer headerRow)
		{
			var addInfos = dataObject.AddInfoCollection;
			if (addInfos?.Any() ?? false)
			{
				FillAddInfosCore(headerRow, addInfos, CusUnderbondSchema.C4_UnderbondBySeaVoyage, Outturn.Constants.AddInfoType.UnderbondBySeaVoyage);
				FillAddInfosCore(headerRow, addInfos, CusUnderbondSchema.C4_IsMoveFromDischarge, Outturn.Constants.AddInfoType.IsMoveFromDischarge);
				FillAddInfosCore(headerRow, addInfos, CusUnderbondSchema.C4_UnderbondBySeaVessel, Outturn.Constants.AddInfoType.UnderbondBySeaVessel);
			}
		}

		void FillAddInfosCore(IColumnIndexer headerRow, IEnumerable<AddInfo> addInfos, SchemaStringColumn column, ZString key)
		{
			var addInfo = addInfos.FirstOrDefault(x => x.Key.GetValueOrDefault() == key);
			if (addInfo != null)
			{
				SetValue(headerRow, column, addInfo.Value);
			}
		}

		void FillAddInfosCore(IColumnIndexer headerRow, IEnumerable<AddInfo> addInfos, SchemaBoolColumn column, ZString key)
		{
			var addInfo = addInfos.FirstOrDefault(x => x.Key.GetValueOrDefault() == key);
			if (addInfo != null)
			{
				SetValue(headerRow, column, addInfo.Value.GetValueOrDefault() == YesNoList.Codes.Yes ? ZBool.True : ZBool.False);
			}
		}

		void FillSubShipments(CusUnderbond underbond)
		{
			var subshipments = dataObject.SubShipmentCollection;
			if (subshipments?.Any() ?? false)
			{
				Helper.MarkUnprocessedExistingBillsFor(underbond);
				foreach (var subshipment in subshipments)
				{
					var reader = new CusOutturnDataObjectReader(subshipment, logger, factory, underbond);
					var outturn = reader.ReadIntoBusinessObject();
					Helper.MarkProcessed(outturn);
				}
				Helper.DeleteUnprocessedBillsFor(underbond, logger);
				underbond.Outturns.Reload(false);
			}
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(CusUnderbond underbond)
		{
			var builder = new ZStringBuilder();
			if (dataObject.WayBillType.GetCodeAsUpperCase() != WayBillTypeList.Codes.Master)
			{
				builder.Append(Res.GetString("323A90A5-E75B-4676-B156-77BFFD1F547A", "{1} must be '{0}'.", WayBillTypeList.Codes.Master, "WayBillType.Code"));
			}
			if (dataObject.WayBillNumber.GetValueOrDefault().IsEmpty)
			{
				builder.Append(Res.GetString("451B22CB-4FD4-4D50-B59C-643F648FF13C", "{0} must not be empty.", "WayBillNumber"));
			}
			return builder.ToStringWithNewLineBetweenAppends();
		}

		protected override CusUnderbond GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			CusUnderbond result = null;
			var masterBill = dataObject.GetMasterBill();
			if (!masterBill.IsEmpty)
			{
				var billType = dataObject.WayBillType.GetCodeAsUpperCase();
				if (!billType.IsEmpty && billType == WayBillTypeList.Codes.Master)
				{
					var query = new ZDBOnlyQuery(typeof(CusUnderbond));
					query.AddToFilter(CusUnderbondSchema.C4_MAWB, masterBill);

					var mawbQuery = new ZDBOnlySubQuery(typeof(CusMAWB), CusMAWBSchema.PK);
					mawbQuery.AddToFilter(CusMAWBSchema.CM_MAWB, masterBill);

					query.AddSubQuery(CusUnderbondSchema.C4_ParentID, mawbQuery, JoinCondition.Or);
					query.OrderBy = CusUnderbondSchema.C4_SendersMessageReference.Name + " desc";

					result = factory.LoadTop1<CusUnderbond>(query);
				}
			}
			return result;
		}

		protected override IMatchingBusinessEntityFinder<CusUnderbond> GetCombinedReferenceMatcher()
		{
			return null; // No Combined Reference Matching has been implemented for Underbond. Considering we're looking at replacing this with Reference and Party ID matching, is best not to implement.
		}

		public override DataContextType DataContextType => DataContextType.UnderBond;

		AirOutturnDataObjectReaderHelper Helper => helper ?? (helper = new AirOutturnDataObjectReaderHelper(factory));
		AirOutturnDataObjectReaderHelper helper;
	}
}
