using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.AU.Declaration.Business.Outturn.Constants;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusUnderbondDataObjectReaderForTransitWarehouse : ShipmentDataObjectReader<CusUnderbond>
	{
		public CusUnderbondDataObjectReaderForTransitWarehouse(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory) : base(dataObject, logger, factory)
		{
			var validReferences = dataObject?.AdditionalReferenceCollection?.Where(a => a.ReferenceNumber.HasValue && a.Type != null && a.Type.Code.HasValue);

			if (validReferences != null)
			{
				masterBill = validReferences.FirstOrDefault(a => a.Type.Code.Value == (ZString)AdditionalReferenceTypes.Codes.MasterBill)?.ReferenceNumber;

				if (!string.IsNullOrEmpty(masterBill))
				{
					masterBill = masterBill.Value.Replace("-", string.Empty);
					var cusMAWBQuery = new ZQuery(CusMAWBSchema.CM_MAWB, masterBill);
					cusMAWBQuery.AddToFilter(CusMAWBSchema.CM_ApplicationCode, Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages);
					var mAWBRecyclePeriod = Enterprise.Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.Value;

					if (mAWBRecyclePeriod > 0)
					{
						cusMAWBQuery.AddToFilter(CusMAWBSchema.CM_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.Now.AddMonths(-mAWBRecyclePeriod));
					}
					cusMAWBQuery.AddToFilter(CusMAWBSchema.CM_IsCTOMAWB, ZBool.False);
					cusMAWBQuery.OrderBy = CusMAWBSchema.Constants.CM_SystemCreateTimeUtc + " desc";
					cusMAWB = factory.LoadTop1<Customs.Business.CusMAWB>(cusMAWBQuery);

					if (cusMAWB != null)
					{
						houseBill = dataObject.WayBillNumber;
						if (!string.IsNullOrEmpty(houseBill))
						{
							var cusHAWBQuery = new ZQuery(CusHAWBSchema.CS_HAWB, houseBill);
							cusMAWBQuery.AddToFilter(CusHAWBSchema.CS_CM, cusMAWB.PK);
							cusHAWB = factory.LoadTop1<Customs.Business.CusHAWB>(cusHAWBQuery);
						}
					}
				}
			}
		}

		protected override void PopulateBusinessObject(CusUnderbond underbond)
		{
			if (!underbond.IsInDatabase && cusMAWB == null)
			{
				throw new DataObjectReadFailureException(Res.GetString("B06AFBD4-A64A-4BE9-9AB5-6472571C99C6", "No MAWB found with Master Bill {0}.", masterBill));
			}
			if (!underbond.IsInDatabase && masterBill.HasValue)
			{
				PopulateUnderbond(underbond);
			}
			else if (underbond.IsInDatabase)
			{
				PopulateOutturn(underbond);
			}

			PopulateOuturnedDate(underbond);
		}

		void PopulateUnderbond(CusUnderbond underbond)
		{
			var dataContextDataSourceCollection = dataObject.DataContext?.DataSourceCollection;
			if (dataContextDataSourceCollection != null)
			{
				var consignmentId = dataContextDataSourceCollection.FirstOrDefault(o => o.Type.ToString() == nameof(DataContextType.TransitReceive))?.Key;
				var headerRow = GetColumnIndexer(underbond);

				if (consignmentId.HasValue)
				{
					SetValue(headerRow, CusUnderbondSchema.C4_MAWB, masterBill);
					SetValue(headerRow, CusUnderbondSchema.C4_FlightNo, dataObject.VoyageFlightNo);
					SetValue(headerRow, CusUnderbondSchema.C4_PiecesManifested, dataObject.TotalNoOfPiecesLanded);
					SetValue(headerRow, CusUnderbondSchema.C4_ModeOfMovement, dataObject.TransportMode.GetNullableCodeAsUpperCase() ?? ZString.Empty);
					SetValue(headerRow, CusUnderbondSchema.C4_MovementReason, CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination);
					SetValue(headerRow, CusUnderbondSchema.C4_IsMoveFromDischarge, true);
					SetValue(headerRow, CusUnderbondSchema.C4_ParentID, cusMAWB.PK);
					SetValue(headerRow, CusUnderbondSchema.C4_ParentTableCode, "CM");

					if (dataObject.DateCollection != null && dataObject.DateCollection.Count > 0)
					{
						FillDates(headerRow, dataObject.DateCollection, ZBool.False,
							new DateTypeSchemaColumnMap(CusUnderbondSchema.C4_ArrivalDate, new[] { DateType.Arrival }));
					}

					FillOrgAddresses(headerRow);
				}
				else
				{
					throw new DataObjectReadFailureException("This UXML does not come from Transit Receive.");
				}

				PopulateOutturn(underbond);
			}
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(CusUnderbond underbond)
		{
			var builder = new ZStringBuilder();
			if (!masterBill.HasValue && underbond == null)
			{
				builder.Append(Res.GetString("B06AFBD4-A64A-4BE9-9AB5-6472571C99C5", "No Air Cargo Report found with Master Bill."));
			}

			return builder.ToStringWithNewLineBetweenAppends();
		}

		protected override CusUnderbond GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			CusUnderbond result = null;
			if (cusHAWB != null)
			{
				var underbondQueryWithHouseBill = new ZQuery(CusUnderbondSchema.C4_ParentID, cusHAWB.PK);
				underbondQueryWithHouseBill.OrderBy = CusUnderbondSchema.Constants.C4_SystemCreateTimeUtc + " desc";
				result = factory.LoadTop1<CusUnderbond>(underbondQueryWithHouseBill);
			}

			if (result == null && cusMAWB != null)
			{
				var underbondQueryWithMasterBill = new ZQuery(CusUnderbondSchema.C4_ParentID, cusMAWB.PK);
				underbondQueryWithMasterBill.OrderBy = CusUnderbondSchema.Constants.C4_SystemCreateTimeUtc + " desc";
				result = factory.LoadTop1<CusUnderbond>(underbondQueryWithMasterBill);
			}

			if (result == null && masterBill.HasValue)
			{
				var underbondQueryWithMasterBill = new ZQuery(CusUnderbondSchema.C4_MAWB, masterBill);
				underbondQueryWithMasterBill.OrderBy = CusUnderbondSchema.Constants.C4_SystemCreateTimeUtc + " desc";
				result = factory.LoadTop1<CusUnderbond>(underbondQueryWithMasterBill);
			}

			return result;
		}

		void PopulateOutturn(CusUnderbond underbond)
		{
			var reader = new CusOutturnDataObjectReaderForTransitWarehouse(dataObject, logger, factory, underbond, cusHAWB);
			reader.ReadIntoBusinessObject();
		}

		void PopulateOuturnedDate(CusUnderbond underbond)
		{
			var unpackTime = underbond.C4_Outurned.IsEmpty ? ZDateTime.MinSmallDateTimeValue : underbond.C4_Outurned;
			var dateList = dataObject?.RelatedShipmentCollection?.Select(r => r.DateCollection).ToList();
			if (dateList != null)
			{
				foreach (var dates in dateList)
				{
					var unpack = dates?.FirstOrDefault(d => d != null && d.Type == DateType.Unpack)?.Value;
					if (unpack != null && unpack?.ToZDateTime() > unpackTime)
					{
						unpackTime = (ZDateTime)(unpack?.ToZDateTime());
					}
				}

				if (unpackTime != ZDateTime.MinSmallDateTimeValue)
				{
					underbond.C4_Outurned = unpackTime;
				}
			}
		}

		void FillOrgAddresses(IColumnIndexer headerRow)
		{
			var orgAddressCollection = dataObject.OrganizationAddressCollection;

			FillOrgAddressesCore(headerRow, orgAddressCollection, CusUnderbondSchema.C4_OA_OriginAddress, DocAddressType.LocalCartageCTO);

			FillOrgAddressesCore(headerRow, orgAddressCollection, CusUnderbondSchema.C4_OA_DestinationAddress, DocAddressType.LocalCartageCFS);
		}

		void FillOrgAddressesCore(IColumnIndexer headerRow, IEnumerable<OrganizationAddress> orgAddressCollection, SchemaGuidColumn orgAddressColumn, ZString orgAddressType)
		{
			var organizationAddress = orgAddressCollection?.FirstOrDefault(orgAddressType);
			if (organizationAddress != null)
			{
				var orgAddress = new OrganisationDataObjectReader(organizationAddress, logger, factory).GetMatched();

				SetValue(headerRow, orgAddressColumn, orgAddress?.PK ?? ZGuid.Empty);
			}
		}

		protected override IMatchingBusinessEntityFinder<CusUnderbond> GetCombinedReferenceMatcher()
		{
			return null;
		}

		public override DataContextType DataContextType => DataContextType.UnderBond;

		readonly ZString? houseBill;
		readonly ZString? masterBill;
		readonly Customs.Business.CusMAWB cusMAWB;
		readonly Customs.Business.CusHAWB cusHAWB;
	}
}
