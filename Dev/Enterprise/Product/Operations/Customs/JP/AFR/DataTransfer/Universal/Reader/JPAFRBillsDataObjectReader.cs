using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal
{
	public class JPAFRBillsDataObjectReader : ShipmentDataObjectReader<JPAFRBills>
	{
		public JPAFRBillsDataObjectReader(Shipment shipmentDataObject, IXmlImportLogger logger, JPManifestDataObjectReaderHelper helper, JPAFRHeader header)
			: base(shipmentDataObject, logger, helper.Factory)
		{
			this.header = Argument.NotNull(header, "JPAFRHeader header");
			this.headerRow = GetColumnIndexer(header);
		}

		readonly JPAFRHeader header;
		readonly IColumnIndexer headerRow;

		public override DataContextType DataContextType
		{
			get { return DataContextType.AFRBill; }
		}

		protected override JPAFRBills GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			JPAFRBills result = null;
			if (dataObject.WayBillNumber.HasValue)
			{
				var headerRow = GetColumnIndexer(header);
				var headerPK = headerRow.GetValue(JPAFRHeaderSchema.PK);
				var query = new ZQuery(JPAFRBillsSchema.JPB_JPH_Header, headerPK);
				query.AddToFilter(JPAFRBillsSchema.JPB_BillNumber, dataObject.WayBillNumber.Value);
				query.FetchOnlyFromLocalCache = !header.IsInDatabase;
				result = factory.LoadTop1<JPAFRBills>(query);
			}
			return result;
		}

		protected sealed override void PopulateBusinessObject(JPAFRBills bill)
		{
			var billRow = GetColumnIndexer(bill);
			var billPK = billRow.GetValue(JPAFRBillsSchema.PK);
			var headerPK = headerRow.GetValue(JPAFRHeaderSchema.PK);
			SetValue(billRow, JPAFRBillsSchema.JPB_JPH_Header, headerPK);
			SetValue(billRow, JPAFRBillsSchema.JPB_BillNumber, dataObject.WayBillNumber);
			SetValue(billRow, JPAFRBillsSchema.JPB_RL_NKOrigin, dataObject.PortOfOrigin);
			SetValue(billRow, JPAFRBillsSchema.JPB_RL_NKFinalDestination, dataObject.PortOfDestination);

			var packingLineData = dataObject.PackingLineCollection.FirstOrDefault();
			if (packingLineData != null)
			{
				SetValue(billRow, JPAFRBillsSchema.JPB_Tariff, packingLineData.HarmonisedCode);
				SetValue(billRow, JPAFRBillsSchema.JPB_ManifestQty, packingLineData.PackQty);
				SetValue(billRow, JPAFRBillsSchema.JPB_ManifestUQ, packingLineData.PackType);
				SetValue(billRow, JPAFRBillsSchema.JPB_GrossWeight, packingLineData.Weight);
				SetValue(billRow, JPAFRBillsSchema.JPB_GrossWeightUQ, packingLineData.WeightUnit);
				SetValue(billRow, JPAFRBillsSchema.JPB_Volume, packingLineData.Volume);
				SetValue(billRow, JPAFRBillsSchema.JPB_VolumeUQ, packingLineData.VolumeUnit);
				SetValue(billRow, JPAFRBillsSchema.JPB_RN_NKGoodsOrigin, packingLineData.CountryOfOrigin);
				SetValue(billRow, JPAFRBillsSchema.JPB_GoodsDescription, packingLineData.GetCleanSingleLineGoodsDescription());
				SetValue(billRow, JPAFRBillsSchema.JPB_MarksAndNumbers, packingLineData.MarksAndNos);

				FillUNDGItems(packingLineData, bill);
			}

			if (dataObject.CommercialInfo != null)
			{
				var freightChargeData = dataObject.CommercialInfo.CommercialChargeCollection.FirstOrDefault(x => x.ChargeType.GetCodeAsUpperCase() == CustomsChargeTypeList.Codes.OverseasFreight);
				if (freightChargeData != null)
				{
					SetValue(billRow, JPAFRBillsSchema.JPB_FreightValue, freightChargeData.Amount);
					SetValue(billRow, JPAFRBillsSchema.JPB_RX_NKFreightValueCurrency, freightChargeData.Currency);
				}
			}
			FillNotificationForwardingParties(bill);
			FillOtherRelevantLaws(bill);
			SetValue(billRow, JPAFRBillsSchema.JPB_RL_NKDelivery, dataObject.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.PlaceOfDeliveryCode, logger));

			FillInBondDetails(bill);
			var remarkNoteData = dataObject.NoteCollection.FirstOrDefault(x => x.Description.GetValueOrDefault() == AddInfoConstants.Bill.Remarks);
			if (remarkNoteData != null)
			{
				SetValue(billRow, JPAFRBillsSchema.JPB_Remarks, remarkNoteData.NoteText);
			}
			if (dataObject.OrganizationAddressCollection != null)
			{
				FillOrganizationAddress(bill, DocAddressTypes.Codes.ConsignorDocumentaryAddress, dataObject.OrganizationAddressCollection.FindBestConsignorMatch());
				FillOrganizationAddress(bill, DocAddressTypes.Codes.ConsigneeAddress, dataObject.OrganizationAddressCollection.FindBestConsigneeMatch());
				FillOrganizationAddress(bill, DocAddressTypes.Codes.NotifyParty, dataObject.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == nameof(DocAddressType.NotifyParty)));
				FillOrganizationAddress(bill, DocAddressTypes.Codes.NotifyParty2, dataObject.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == nameof(DocAddressType.NotifyParty2)));
			}
			FillContainers(bill);
		}

		void FillUNDGItems(PackingLine packingLine, JPAFRBills bill)
		{
			var undgs = packingLine.UNDGCollection ?? Enumerable.Empty<UNDG>();
			if (undgs.Any())
			{
				bill.UNDGs.DeleteAll();
			}

			var index = 0;

			foreach (var undg in undgs)
			{
				var code = undg?.UNDGCode ?? ZString.Empty;
				var standard = undg?.Standard ?? ZString.Empty;

				if (!code.IsEmpty && !standard.IsEmpty)
				{
					var substance = UNDGSubstanceLoader.LoadSubstances(bill.Factory, code.SubstringSafe(0, 4), code.SubstringSafe(4, 2), standard).FirstOrDefault();
					if (substance != null)
					{
						if (index == 0)
						{
							bill.JPB_DG = substance.PK;
						}
						else
						{
							bill.UNDGs.AddNew().DI_DG = substance.PK;
						}

						index++;
					}
				}
			}
		}

		void FillInBondDetails(JPAFRBills bill)
		{
			var goodsValue = dataObject.GoodsValue;
			var goodsValueCurrency = dataObject.GoodsValueCurrency;
			var transhipmentReasonCode = dataObject.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.TranshipmentReasonCode, logger);
			var transhipmentDuration = dataObject.AddInfoCollection.GetZIntValue(AddInfoConstants.Bill.TranshipmentDuration, logger);
			var transhipmentEstimatedStartDate = dataObject.AddInfoCollection.GetZDateTimeValue(AddInfoConstants.Bill.TranshipmentEstimatedStartDate, logger);
			var transhipmentEstimatedFinishDate = dataObject.AddInfoCollection.GetZDateTimeValue(AddInfoConstants.Bill.TranshipmentEstimatedFinishDate, logger);
			var transhipmentTransportMode = dataObject.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.TranshipmentTransportMode, logger);
			var transhipmentArrivalPlaceCode = dataObject.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.TranshipmentArrivalPlaceCode, logger);

			if (goodsValue.HasValue || goodsValueCurrency != null || transhipmentArrivalPlaceCode.HasValue ||
				transhipmentDuration.HasValue || transhipmentEstimatedStartDate.HasValue || transhipmentEstimatedFinishDate.HasValue ||
				transhipmentTransportMode.HasValue || transhipmentArrivalPlaceCode.HasValue)
			{
				var billRow = GetColumnIndexer(bill);
				var billPK = billRow.GetValue(JPAFRBillsSchema.PK);
				var query = new ZQuery(JPAFRInBondDetailsSchema.JPI_JPB_Bill, billPK);
				query.FetchOnlyFromLocalCache = !bill.IsInDatabase;
				var inBondDetailsBO = factory.LoadTop1<JPAFRInBondDetails>(query);
				if (inBondDetailsBO == null)
				{
					if (bill.IsInDatabase)
					{
						var mutex = new ZGlobalMutex(MutexIDs.AFRJobBeingCreated, billPK.ToString());

						if (!mutex.Lock())
						{
							logger.Log(LogType.Warning, Res.GetString("74224B82-BDFA-4A6F-9368-A8FC0458E121", "Could not update InBond data; someone else is already in the process of updating it for AFR ({0}).", billRow.GetValue(JPAFRHeaderSchema.JPH_JobReference)));
						}
						else
						{
							inBondDetailsBO = factory.New<JPAFRInBondDetails>();
							factory.CleanupAfterSaving += (a, b) => { if (mutex.HasLock) { mutex.Unlock(); } };
						}
					}
					else
					{
						inBondDetailsBO = factory.New<JPAFRInBondDetails>();
					}
				}
				if (inBondDetailsBO != null)
				{
					var inBondDetailsRow = GetColumnIndexer(inBondDetailsBO);
					if (!inBondDetailsBO.IsInDatabase)
					{
						SetValue(inBondDetailsRow, JPAFRInBondDetailsSchema.JPI_JPB_Bill, billPK);
					}
					SetValue(inBondDetailsRow, JPAFRInBondDetailsSchema.JPI_GoodsValue, goodsValue);
					SetValue(inBondDetailsRow, JPAFRInBondDetailsSchema.JPI_RX_NKGoodsValueCurrency, goodsValueCurrency);
					SetValue(inBondDetailsRow, JPAFRInBondDetailsSchema.JPI_TemporaryLandingReason, transhipmentReasonCode);
					SetValue(inBondDetailsRow, JPAFRInBondDetailsSchema.JPI_TemporaryLandingDuration, transhipmentDuration);
					SetValue(inBondDetailsRow, JPAFRInBondDetailsSchema.JPI_ESDT, transhipmentEstimatedStartDate);
					SetValue(inBondDetailsRow, JPAFRInBondDetailsSchema.JPI_EFDT, transhipmentEstimatedFinishDate);
					SetValue(inBondDetailsRow, JPAFRInBondDetailsSchema.JPI_TransportMode, transhipmentTransportMode);
					SetValue(inBondDetailsRow, JPAFRInBondDetailsSchema.JPI_ArrivalBondedAreaCode, transhipmentArrivalPlaceCode);
				}
			}
		}

		void FillNotificationForwardingParties(JPAFRBills bill)
		{
			var nfp1Data = dataObject.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.NotificationForwardingPartyCode1, logger);
			var nfp2Data = dataObject.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.NotificationForwardingPartyCode2, logger);
			var nfp3Data = dataObject.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.NotificationForwardingPartyCode3, logger);
			if (nfp1Data.HasValue || nfp2Data.HasValue || nfp3Data.HasValue)
			{
				var headerRow = GetColumnIndexer(bill);
				var headerPK = headerRow.GetValue(JPAFRBillsSchema.PK);
				var nfpQuery = new ZQuery(CusCodeDataSchema.CY_ParentID, headerPK);
				nfpQuery.AddToFilter(CusCodeDataSchema.CY_Type, NotificationForwardingParty.NFPType);
				nfpQuery.FetchOnlyFromLocalCache = !bill.IsInDatabase;
				var nfpBOs = new List<NotificationForwardingParty>(factory.Load<NotificationForwardingParty>(nfpQuery));
				IColumnIndexer nfp1Row = null;
				IColumnIndexer nfp2Row = null;
				IColumnIndexer nfp3Row = null;
				if (nfp1Data.HasValue)
				{
					nfp1Row = GetNotificationForwardingPartyRowOrCreateNew(nfp1Data.Value, headerPK, nfpBOs);
				}
				if (nfp2Data.HasValue)
				{
					nfp2Row = GetNotificationForwardingPartyRowOrCreateNew(nfp2Data.Value, headerPK, nfpBOs);
				}
				if (nfp3Data.HasValue)
				{
					nfp3Row = GetNotificationForwardingPartyRowOrCreateNew(nfp3Data.Value, headerPK, nfpBOs);
				}
				nfpBOs.DeleteAll();
				if (nfp1Row != null)
				{
					SetValue(nfp1Row, CusCodeDataSchema.CY_Order, (ZShort)1);
				}
				if (nfp2Row != null)
				{
					SetValue(nfp2Row, CusCodeDataSchema.CY_Order, (ZShort)2);
				}
				if (nfp3Row != null)
				{
					SetValue(nfp3Row, CusCodeDataSchema.CY_Order, (ZShort)3);
				}
			}
		}

		IColumnIndexer GetNotificationForwardingPartyRowOrCreateNew(ZString value, ZGuid billPK, List<NotificationForwardingParty> nfpBOs)
		{
			var nfpBO = nfpBOs.FirstOrDefault(x => x.CY_Data == value);
			if (nfpBO == null)
			{
				nfpBO = factory.New<NotificationForwardingParty>();
			}
			else
			{
				nfpBOs.Remove(nfpBO);
			}
			var nfpRow = GetColumnIndexer(nfpBO);
			if (!nfpBO.IsInDatabase)
			{
				SetValue(nfpRow, CusCodeDataSchema.CY_ParentID, billPK);
				SetValue(nfpRow, CusCodeDataSchema.CY_ParentTableCode, JPAFRBillsSchema.Constants.Prefix);
				SetValue(nfpRow, CusCodeDataSchema.CY_Type, NotificationForwardingParty.NFPType);
				SetValue(nfpRow, CusCodeDataSchema.CY_Data, value);
			}
			return nfpRow;
		}

		void FillOtherRelevantLaws(JPAFRBills bill)
		{
			var orl1Data = dataObject.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.OtherRelevantLawCode1, logger);
			var orl2Data = dataObject.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.OtherRelevantLawCode2, logger);
			var orl3Data = dataObject.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.OtherRelevantLawCode3, logger);
			var orl4Data = dataObject.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.OtherRelevantLawCode4, logger);
			var orl5Data = dataObject.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.OtherRelevantLawCode5, logger);
			if (orl1Data.HasValue || orl2Data.HasValue || orl3Data.HasValue || orl5Data.HasValue || orl5Data.HasValue)
			{
				var billRow = GetColumnIndexer(bill);
				var billPK = billRow.GetValue(JPAFRBillsSchema.PK);
				var orlQuery = new ZQuery(CusCodeDataSchema.CY_ParentID, billPK);
				orlQuery.AddToFilter(CusCodeDataSchema.CY_Type, OtherRelevantLaw.ORLType);
				orlQuery.FetchOnlyFromLocalCache = !bill.IsInDatabase;
				var orlBOs = new List<OtherRelevantLaw>(factory.Load<OtherRelevantLaw>(orlQuery));
				IColumnIndexer orl1Row = null;
				IColumnIndexer orl2Row = null;
				IColumnIndexer orl3Row = null;
				IColumnIndexer orl4Row = null;
				IColumnIndexer orl5Row = null;
				if (orl1Data.HasValue)
				{
					orl1Row = GetOtherRelevantLawRowOrCreateNew(orl1Data.Value, billPK, orlBOs);
				}
				if (orl2Data.HasValue)
				{
					orl2Row = GetOtherRelevantLawRowOrCreateNew(orl2Data.Value, billPK, orlBOs);
				}
				if (orl3Data.HasValue)
				{
					orl3Row = GetOtherRelevantLawRowOrCreateNew(orl3Data.Value, billPK, orlBOs);
				}
				if (orl4Data.HasValue)
				{
					orl4Row = GetOtherRelevantLawRowOrCreateNew(orl4Data.Value, billPK, orlBOs);
				}
				if (orl5Data.HasValue)
				{
					orl5Row = GetOtherRelevantLawRowOrCreateNew(orl5Data.Value, billPK, orlBOs);
				}
				orlBOs.DeleteAll();
				if (orl1Row != null)
				{
					SetValue(orl1Row, CusCodeDataSchema.CY_Order, (ZShort)1);
				}
				if (orl2Row != null)
				{
					SetValue(orl2Row, CusCodeDataSchema.CY_Order, (ZShort)2);
				}
				if (orl3Row != null)
				{
					SetValue(orl3Row, CusCodeDataSchema.CY_Order, (ZShort)3);
				}
				if (orl4Row != null)
				{
					SetValue(orl4Row, CusCodeDataSchema.CY_Order, (ZShort)4);
				}
				if (orl5Row != null)
				{
					SetValue(orl5Row, CusCodeDataSchema.CY_Order, (ZShort)5);
				}
			}
		}

		IColumnIndexer GetOtherRelevantLawRowOrCreateNew(ZString value, ZGuid billPK, List<OtherRelevantLaw> orlBOs)
		{
			var orlBO = orlBOs.FirstOrDefault(x => x.CY_Data == value);
			if (orlBO == null)
			{
				orlBO = factory.New<OtherRelevantLaw>();
			}
			else
			{
				orlBOs.Remove(orlBO);
			}
			var orlRow = GetColumnIndexer(orlBO);
			if (!orlBO.IsInDatabase)
			{
				SetValue(orlRow, CusCodeDataSchema.CY_ParentID, billPK);
				SetValue(orlRow, CusCodeDataSchema.CY_ParentTableCode, JPAFRBillsSchema.Constants.Prefix);
				SetValue(orlRow, CusCodeDataSchema.CY_Type, OtherRelevantLaw.ORLType);
				SetValue(orlRow, CusCodeDataSchema.CY_Data, value);
			}
			return orlRow;
		}

		void FillContainers(JPAFRBills bill)
		{
			if (dataObject.ContainerCollection != null)
			{
				var billRow = GetColumnIndexer(bill);
				var billPK = billRow.GetValue(JPAFRBillsSchema.PK);
				var containerQuery = new ZQuery(JPAFRContainerSchema.JPC_JPB_Bill, billPK);
				containerQuery.FetchOnlyFromLocalCache = !bill.IsInDatabase;
				var existingContainerBOs = new List<JPAFRContainer>(factory.Load<JPAFRContainer>(containerQuery));
				foreach (var containerData in dataObject.ContainerCollection)
				{
					var containerBO = new JPAFRContainerDataObjectReader(containerData, logger, factory, bill).ReadIntoBusinessObject();
					existingContainerBOs.Remove(containerBO);
				}
				existingContainerBOs.DeleteAll();
			}
		}

		void FillOrganizationAddress(JPAFRBills bill, ZString addressType, OrganizationAddress orgAddressDataObject)
		{
			if (orgAddressDataObject != null)
			{
				var billRow = GetColumnIndexer(bill);
				var billPK = billRow.GetValue(JPAFRBillsSchema.PK);
				var query = new ZQuery(JobDocAddressSchema.E2_AddressType, addressType);
				query.AddToFilter(JobDocAddressSchema.E2_AddressSequence, 0);
				query.AddToFilter(JobDocAddressSchema.E2_ParentID, billPK);
				var docAddress = factory.LoadTop1<JobDocAddress>(query)
					?? factory.New<JobDocAddress>();
				var docAddresRow = GetColumnIndexer(docAddress);
				if (!docAddress.IsInDatabase)
				{
					SetValue(docAddresRow, JobDocAddressSchema.E2_AddressType, addressType);
					SetValue(docAddresRow, JobDocAddressSchema.E2_AddressSequence, 0);
					SetValue(docAddresRow, JobDocAddressSchema.E2_ParentID, billPK);
					SetValue(docAddresRow, JobDocAddressSchema.E2_ParentTableCode, JPAFRBillsSchema.Constants.Prefix);
				}
				var reader = new OrganisationDataObjectReader(orgAddressDataObject, logger, factory);
				var orgAddress = reader.GetMatched();
				if (orgAddress != null && orgAddress.OA_OH == OrgHeader.UnmatchedOrganisationPK)
				{
					orgAddress = null;
					SetValue(docAddresRow, JobDocAddressSchema.E2_OA_Address, ZGuid.Empty);
					SetValue(docAddresRow, JobDocAddressSchema.E2_AddressOverride, ZBool.True);
				}
				reader.PopulateJobDocAddress(orgAddress, docAddress);
			}
		}

		protected override IMatchingBusinessEntityFinder<JPAFRBills> GetCombinedReferenceMatcher()
		{
			return null; // No Combined Reference MAtching has been implemented for Customs. Considering we're looking at replacing this with Reference and Party ID matching, is best not to implement.
		}
	}
}
