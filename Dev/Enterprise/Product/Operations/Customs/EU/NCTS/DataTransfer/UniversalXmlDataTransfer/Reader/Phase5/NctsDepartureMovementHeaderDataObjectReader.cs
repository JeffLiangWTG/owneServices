using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;
using static Enterprise.Customs.EU.NCTS.DataTransfer.DataObjectWriterConstants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.EU.NCTS.DataTransfer.Phase5
{
	public class NctsDepartureMovementHeaderDataObjectReader : NctsMovementHeaderDataObjectReader
	{
		public NctsDepartureMovementHeaderDataObjectReader(UniversalShipment headerDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(headerDataObject, logger, factory)
		{
			moveHeaderData = dataObject.InBondMoveHeaderCollection?.FirstOrDefault() ?? new InBondMoveHeader();
		}

		readonly InBondMoveHeader moveHeaderData;

		protected override ZString HeaderType => NctsMovementType.Codes.Departure;

		protected sealed override void PopulateBusinessObjectCore(NctsHeader header, IColumnIndexer headerRow)
		{
			var departureMovementHeader = header.MovementHeader;
			var departureMovementHeaderRow = GetColumnIndexer(departureMovementHeader);

			SetValue(departureMovementHeaderRow, CusInBondMoveHeaderSchema.BM_InBondEntryType, moveHeaderData.EntryType);
			SetValue(departureMovementHeaderRow, CusInBondMoveHeaderSchema.BM_AdditionalDeclarationType, moveHeaderData.AdditionalEntryType);
			SetValue(departureMovementHeaderRow, CusInBondMoveHeaderSchema.BM_MethodOfPayment, moveHeaderData.PaymentMethod);

			FillDates(departureMovementHeaderRow, moveHeaderData.DateCollection);
			FillTransportMeansCollection(departureMovementHeaderRow, moveHeaderData.TransportMeansCollection);
			FillDataFromAddInfos(departureMovementHeaderRow, moveHeaderData.AddInfoCollection, departureMovementHeader);
			FillPorts(departureMovementHeaderRow);
			FillGrossWeight(departureMovementHeaderRow);
			FillLocationOfGoods(departureMovementHeader);
			FillOrganizations(departureMovementHeader, dataObject.OrganizationAddressCollection);
			FillOrganizations(departureMovementHeader, moveHeaderData.OrganizationAddressCollection);
			FillRelatedDocuments(header);
			FillUniqueConsignmentReference(header);
			FillLocalReferenceNumber(header);
			FillSupplyChainActors(header);
			FillCusAuthorizationUsages(departureMovementHeader);
			FillCountriesOfRouting(header);
			FillOfficeCodes(header);
			FillContainers(header);
			FillGuarantees(departureMovementHeader);
			FillCountryData(header, departureMovementHeader);
			FillServices(header);
			FillBillsFromAdditionalBillCollection(header, dataObject.AdditionalBillCollection);
			FillBillsFromInBondMoveDetailCollection(header, departureMovementHeader, moveHeaderData.InBondMoveDetailCollection);
			FillGoodsItems(header);
		}

		protected override List<CustomsReference> GetCustomsReferenceCollection() => moveHeaderData.CustomsReferenceCollection;

		void FillRelatedDocuments(NctsHeader header)
		{
			var reader = new CustomsSupportingInformationCollectionDataObjectReader(logger, helper);
			if (header is Integration.Customs.ICusSupportingInfoTypeSupporter)
			{
				reader.ReadIntoDataRows(header.PK, CusInBondHeaderSchema.Constants.Prefix, header.IsInDatabase, moveHeaderData);
			}
			if (header.MovementHeader is Integration.Customs.ICusSupportingInfoTypeSupporter)
			{
				reader.ReadIntoDataRows(header.MovementHeader.PK, CusInBondMoveHeaderSchema.Constants.Prefix, header.MovementHeader.IsInDatabase, moveHeaderData);
			}
		}

		void FillCountriesOfRouting(NctsHeader header)
		{
			if (CustomsReferenceDictionary.TryGetValue(NctsUxmlTypeList.Codes.CountryOfRoutingCode, out var countryOfRoutingDataObjects))
			{
				var existingCountriesOfRouting = GetExistingCountriesOfRouting(header.CountriesOfRouting.Cast<CountryOfRouting>().OrderBy(p => p.CY_SystemCreateTimeUtc));
				foreach (var countryOfRoutingDataObject in countryOfRoutingDataObjects)
				{
					var code = countryOfRoutingDataObject.SubType.GetCodeAsUpperCase();
					if (!code.IsEmpty)
					{
						MatchOrAddNew(code, existingCountriesOfRouting, () =>
						{
							var countryOfRoutingRow = GetColumnIndexer(header.CountriesOfRouting.AddNew());
							SetValue(countryOfRoutingRow, CusCodeDataSchema.CY_Data, code);
							SetValue(countryOfRoutingRow, CusCodeDataSchema.CY_Order, countryOfRoutingDataObject.Order.GetValueOrDefault());
						});
					}
				}
				existingCountriesOfRouting.Values.SelectMany(x => x).DeleteAll();
			}
		}

		void FillCusAuthorizationUsages(NctsDepartureMovementHeader departureMovementHeader)
		{
			if (CustomsReferenceDictionary.TryGetValue(CusReferenceTypeCodes.AUT, out var cusAuthorizationUsageDataObjects))
			{
				var existingAuthorizationUsages = GetExistingUsages(departureMovementHeader.CusAuthorizationUsages.OrderBy(p => p.AGC_SystemCreateTimeUtc));
				foreach (var cusAuthorizationUsageDataObject in cusAuthorizationUsageDataObjects)
				{
					var (code, reference, ownerAddress) = GetCodeReferenceAndOwner(cusAuthorizationUsageDataObject);
					var ownerPK = ownerAddress?.OA_OH ?? ZGuid.Empty;
					if (!code.IsEmpty || !reference.IsEmpty || ownerPK.IsValid)
					{
						MatchOrAddNew(GetKey(code, reference, ownerPK), existingAuthorizationUsages, () =>
						{
							var authorizationRow = GetColumnIndexer(departureMovementHeader.CusAuthorizationUsages.AddNew());
							SetValue(authorizationRow, CusAuthorizationUsageSchema.AGC_Code, code);
							SetValue(authorizationRow, CusAuthorizationUsageSchema.AGC_Number, reference);
							SetValue(authorizationRow, CusAuthorizationUsageSchema.AGC_OH_Owner, ownerPK);
						});
					}
				}
				existingAuthorizationUsages.Values.SelectMany(x => x).DeleteAll();
			}
		}

		void FillLocationOfGoods(NctsDepartureMovementHeader movementHeader)
		{
			if (moveHeaderData.LocationOfGoodsCollection is List<LocationOfGoods> locations && locations.FirstOrDefault() is LocationOfGoods location)
			{
				var goodsLocation = movementHeader.GoodsLocation;
				var locationRow = GetColumnIndexer(goodsLocation);
				SetValue(locationRow, CusGoodsLocationSchema.CGL_Qualifier, location.Qualifier);
				SetValue(locationRow, CusGoodsLocationSchema.CGL_AdditionalIdentifier, location.AdditionalIdentifier);
				SetValue(locationRow, CusGoodsLocationSchema.CGL_Type, location.SubType.GetCodeAsUpperCase());
				SetValue(locationRow, CusGoodsLocationSchema.CGL_LocationUse, new LocationOfGoodsTypeConverter().FromEnumValue(location.Type));
				PopulateJobDocAddress(goodsLocation.Address, location);
			}
		}

		void PopulateJobDocAddress(JobDocAddress jobDocAddress, LocationOfGoods location)
		{
			if (location.OrgAddress != null)
			{
				var contact = location.Contact;
				SetValue(jobDocAddress, JobDocAddressSchema.E2_GovRegNum, location.AuthorizationNumber);
				SetValue(jobDocAddress, JobDocAddressSchema.E2_Postcode, location.OrgAddress.Postcode);
				SetValue(jobDocAddress, JobDocAddressSchema.E2_RN_NKCountryCode, location.OrgAddress.Country);
				SetValue(jobDocAddress, JobDocAddressSchema.E2_Contact, !string.IsNullOrEmpty(contact?.Name) ? contact.Name : location.OrgAddress.Contact);
				SetValue(jobDocAddress, JobDocAddressSchema.E2_Phone, !string.IsNullOrEmpty(contact?.PhoneNumber) ? contact.PhoneNumber : location.OrgAddress.Phone);
				SetValue(jobDocAddress, JobDocAddressSchema.E2_Email, !string.IsNullOrEmpty(contact?.Email) ? contact.Email : location.OrgAddress.Email);
			}
		}

		void FillPorts(IColumnIndexer departureMovementHeaderRow)
		{
			SetValue(departureMovementHeaderRow, CusInBondMoveHeaderSchema.BM_RN_NKCountryOfDispatch, moveHeaderData.PortOfOrigin);
			SetValue(departureMovementHeaderRow, CusInBondMoveHeaderSchema.BM_RL_NKDestinationPort, moveHeaderData.PortOfDestination);
			SetValue(departureMovementHeaderRow, CusInBondMoveHeaderSchema.BM_PortOfPresentationCode, moveHeaderData.PortOfLoading.GetNullableCodeAsUpperCase());
			SetValue(departureMovementHeaderRow, CusInBondMoveHeaderSchema.BM_ForeignDestPortKCode, moveHeaderData.PortOfDischarge.GetNullableCodeAsUpperCase());
			SetValue(departureMovementHeaderRow, CusInBondMoveHeaderSchema.BM_PlaceOfLoading, moveHeaderData.PortOfLoading?.Name);
			SetValue(departureMovementHeaderRow, CusInBondMoveHeaderSchema.BM_PlaceOfUnloading, moveHeaderData.PortOfDischarge?.Name);
		}

		void FillGrossWeight(IColumnIndexer departureMovementHeaderRow)
		{
			if (moveHeaderData.GrossWeight is ZDecimal weight)
			{
				if (moveHeaderData.GrossWeightUnit.GetNullableCodeAsUpperCase() is ZString unit && !unit.Equals(Core.Constants.Weight.Kilograms))
				{
					var weightWithUnit = new ZWeight(weight, unit);
					SetValue(departureMovementHeaderRow, CusInBondMoveHeaderSchema.BM_GrossWeight, weightWithUnit.InKilogramsSafe);
				}
				else
				{
					SetValue(departureMovementHeaderRow, CusInBondMoveHeaderSchema.BM_GrossWeight, weight);
				}
			}
		}

		void FillGuarantees(NctsDepartureMovementHeader departureMovementHeader)
		{
			if (moveHeaderData.GuaranteeCollection != null)
			{
				var existingGuarantees = departureMovementHeader.Guarantees.Cast<NctsGuarantee>().ToList();
				foreach (var guaranteeDataObject in moveHeaderData.GuaranteeCollection)
				{
					IColumnIndexer guaranteeRow = null;
					var bondType = guaranteeDataObject.BondType?.Code;
					var bondNumber = guaranteeDataObject.BondNumber;
					var bondNumber2 = guaranteeDataObject.BondNumber2;
					if (existingGuarantees
						.FirstOrDefault(x => (!bondNumber.HasValue || x.PW_BondNumber.EqualsIgnoringCase(bondNumber.Value))
							&& (!bondNumber2.HasValue || x.PW_BondNumber2.EqualsIgnoringCase(bondNumber2.Value))
							&& (!bondType.HasValue || x.PW_BondType.EqualsIgnoringCase(bondType.Value))) is NctsGuarantee guarantee)
					{
						guaranteeRow = GetColumnIndexer(guarantee);
						existingGuarantees.Remove(guarantee);
					}
					else
					{
						guaranteeRow = GetColumnIndexer(departureMovementHeader.Guarantees.AddNew());
					}
					SetValue(guaranteeRow, CusBondDetailSchema.PW_BondType, bondType);
					SetValue(guaranteeRow, CusBondDetailSchema.PW_BondNumber, bondNumber);
					SetValue(guaranteeRow, CusBondDetailSchema.PW_BondNumber2, bondNumber2);
					var accessCode = guaranteeDataObject.AccessCode;
					if (accessCode.HasValue && accessCode.Value != NctsDepartureMovementHeaderDataObjectWriter.GuaranteeAccessCodeMask)
					{
						SetValue(guaranteeRow, CusBondDetailSchema.PW_Password, accessCode.Value);
					}
					SetValue(guaranteeRow, CusBondDetailSchema.PW_BondAmount, guaranteeDataObject.BondAmount);
					SetValue(guaranteeRow, CusBondDetailSchema.PW_RX_NKCurrency, guaranteeDataObject.BondCurrency?.Code);
					SetValue(guaranteeRow, CusBondDetailSchema.PW_SuretyCode, guaranteeDataObject.SuretyCode);
					SetValue(guaranteeRow, CusBondDetailSchema.PW_BondFiledPort, guaranteeDataObject.BondFiledPort?.Code);
				}
				existingGuarantees.DeleteAll();
			}
		}

		void FillContainers(NctsHeader header)
		{
			if (dataObject.ContainerCollection != null)
			{
				var isPartial = dataObject.ContainerCollection.Content.GetValueOrDefault() == CollectionContent.Partial;
				var existingContainers = header.DepartureHeaderContainers.Cast<NctsDepartureHeaderContainer>().GroupBy(x => x.BC_ContainerNum.ToUpperInvariant()).ToDictionary(x => x.Key, y => new Stack<NctsDepartureHeaderContainer>(y));
				foreach (var containerDataObject in dataObject.ContainerCollection)
				{
					var containerNumber = containerDataObject.ContainerNumber.GetValueOrDefault().ToUpperInvariant();
					NctsDepartureHeaderContainer container = null;
					IColumnIndexer containerRow = null;
					if (existingContainers.TryGetValue(containerNumber, out var containers))
					{
						container = containers.Pop();
						containerRow = GetColumnIndexer(container);
						if (containers.Count == 0)
						{
							existingContainers.Remove(containerNumber);
						}
					}
					if (containerRow == null)
					{
						container = header.DepartureHeaderContainers.AddNew();
						containerRow = GetColumnIndexer(container);
						SetValue(containerRow, CusInBondContainerSchema.BC_ContainerNum, containerNumber);
					}
					SetValue(containerRow, CusInBondContainerSchema.BC_Mode, containerDataObject.FCL_LCL_AIR);
					SetValue(containerRow, CusInBondContainerSchema.BC_Seal1, containerDataObject.Seal);
					SetValue(containerRow, CusInBondContainerSchema.BC_Seal2, containerDataObject.SecondSeal);

					if (containerDataObject.SealCollection != null)
					{
						container.AdditionalSeals.RemoveAndDeleteAll();
						foreach (var seal in containerDataObject.SealCollection)
						{
							var sealNumber = seal.SealNumber;
							if (sealNumber is { IsEmpty: false })
							{
								var sealRow = GetColumnIndexer(container.AdditionalSeals.AddNew());
								SetValue(sealRow, CusSealSchema.BK_SealNumber, seal.SealNumber);
								SetValue(sealRow, CusSealSchema.BK_SequenceNumber, seal.Sequence);
								SetValue(sealRow, CusSealSchema.BK_UnloadingState, seal.StatusInformation);
							}
						}
					}
				}
				if (!isPartial)
				{
					existingContainers.SelectMany(x => x.Value).DeleteAll();
				}
			}
		}

		void FillGoodsItems(NctsHeader header)
		{
			if (moveHeaderData.InBondMoveDetailCollection != null)
			{
				foreach (var moveDetail in moveHeaderData.InBondMoveDetailCollection)
				{
					var billSequence = moveDetail.Sequence ?? 0;
					if (billSequence > 0 && billSequence <= header.Bills.Count)
					{
						var bill = header.Bills[billSequence - 1];
						bill.GoodsItems?.DeleteAll();
						if (moveDetail.InBondMoveLineItemCollection != null)
						{
							foreach (var lineItem in moveDetail.InBondMoveLineItemCollection)
							{
								new DepartureGoodsItemDataObjectReader(dataObject, logger, factory, helper, header, bill, lineItem, lineItem.Link).ReadIntoBusinessObject();
							}
						}
					}
				}
			}
		}

		void FillOrganizations(NctsDepartureMovementHeader departureMovementHeader, List<OrganizationAddress> organizationAddressCollection)
		{
			FillJobDocAddress(departureMovementHeader.Representative, organizationAddressCollection, DocAddressType.Representative);
			FillJobDocAddress(departureMovementHeader.Carrier, organizationAddressCollection, DocAddressType.Carrier);
		}

		void FillJobDocAddress(JobDocAddress docAddress, List<OrganizationAddress> addressCollection, DocAddressType docAddressType)
		{
			if (addressCollection != null && addressCollection.FirstOrDefault(x => docAddressType.ToString().Equals(x.AddressType)) is OrganizationAddress address)
			{
				var orgReader = new OrganisationDataObjectReader(address, logger, factory);
				var orgAddress = orgReader.GetMatched();
				if (orgAddress != null)
				{
					orgReader.PopulateJobDocAddress(orgAddress, docAddress, canOverride: false);
				}
			}
		}

		void FillUniqueConsignmentReference(NctsHeader header)
		{
			if (CustomsReferenceDictionary.TryGetValue(NctsUxmlTypeList.Codes.UniqueConsignmentReference, out var uniqueConsignmentReferenceDataObjects) && header.MovementHeader is NctsDepartureMovementHeader departureMovementHeader)
			{
				var ucrData = uniqueConsignmentReferenceDataObjects[0];
				SetValue(GetColumnIndexer(departureMovementHeader), CusInBondMoveHeaderSchema.BM_UniqueConsignmentReference, ucrData?.Reference);
			}
		}

		void FillLocalReferenceNumber(NctsHeader header)
		{
			if (EUCustomsDataRegistry.Instance.NctsIsManualDepartureCustomerReferenceEnabled.Value)
			{
				if (CustomsReferenceDictionary.TryGetValue(NctsUxmlTypeList.Codes.LocalReferenceNumber, out var localReferenceNumberDataObjects) && header.MovementHeader is NctsDepartureMovementHeader departureMovementHeader)
				{
					var lrnData = localReferenceNumberDataObjects[0];
					SetValue(GetColumnIndexer(departureMovementHeader), CusInBondMoveHeaderSchema.BM_PaperlessInbondNum, lrnData?.Reference);
				}
			}
		}

		void FillDates(IColumnIndexer departureMovementHeaderRow, List<Date> dateCollection)
		{
			if (dateCollection != null)
			{
				var dateLimit = dateCollection.FirstOrDefault(x => DateType.DateLimit.Equals(x.Type));
				if (dateLimit != null)
				{
					SetValue(departureMovementHeaderRow, CusInBondMoveHeaderSchema.BM_ExportDate, dateLimit.Value);
				}
			}
		}

		void FillTransportMeansCollection(IColumnIndexer departureMovementHeaderRow, List<TransportMeans> transportMeansCollection)
		{
			if (transportMeansCollection != null)
			{
				var departureTransportMeansSorted = new List<TransportMeans>();
				var borderTransportMeansSorted = new List<TransportMeans>();
				transportMeansCollection.OrderBy(means => means.Order).ForEach(x =>
				{
					if (x.TransportType.HasValue)
					{
						switch (x.TransportType.Value)
						{
							case TransportTypeCode.Departure:
								departureTransportMeansSorted.Add(x);
								break;
							case TransportTypeCode.Border:
								borderTransportMeansSorted.Add(x);
								break;
						}
					}
				});

				if (departureTransportMeansSorted.Count > 0)
				{
					var transportMode = ZString.Empty;
					if (departureTransportMeansSorted[0] is TransportMeans transportMeans)
					{
						var typeOfIdCode = transportMeans.TypeOfIdentification.GetNullableCodeAsUpperCase();
						if (typeOfIdCode.HasValue)
						{
							var transportModeAndIdType = Utilities.TransportMeansMapping.FirstOrDefault(item => item.transportMeansCode == typeOfIdCode.Value);
							transportMode = transportModeAndIdType.transportMode;
							if (!transportMode.IsEmpty)
							{
								SetValue(departureMovementHeaderRow, CusInBondMoveHeaderSchema.BM_InlandTransportMode, transportMode);
								SetValue(departureMovementHeaderRow, CusInBondMoveHeaderSchema.BM_TransportAtDepartureType, transportModeAndIdType.idType);
							}
						}
						SetValue(departureMovementHeaderRow, CusInBondMoveHeaderSchema.BM_TransportAtDeparture, transportMeans.IdentificationNumber);
						SetValue(departureMovementHeaderRow, CusInBondMoveHeaderSchema.BM_RN_NKTransportAtDepartureCountry, transportMeans.Nationality);
					}
					if (departureTransportMeansSorted.Count > 1 && transportMode == EU.Business.ModeOfTransportList.Codes._3_RoadTransport)
					{
						var trailerTransportMeansCollection = departureTransportMeansSorted.Where(x => x.TypeOfIdentification.GetCodeAsUpperCase() == TransportMeansList.Codes.RegistrationNumberOfTheRoadTrailer).Take(2).ToArray();
						if (trailerTransportMeansCollection.Length > 0)
						{
							var transportMeans1 = trailerTransportMeansCollection[0];
							SetValue(departureMovementHeaderRow, CusInBondMoveHeaderSchema.BM_TransportAtDepartureTrailer1RegNo, transportMeans1.IdentificationNumber);
							SetValue(departureMovementHeaderRow, CusInBondMoveHeaderSchema.BM_RN_NKTransportAtDepartureTrailer1Nationality, transportMeans1.Nationality);
							if (trailerTransportMeansCollection.Length > 1)
							{
								var transportMeans2 = trailerTransportMeansCollection[1];
								SetValue(departureMovementHeaderRow, CusInBondMoveHeaderSchema.BM_TransportAtDepartureTrailer2RegNo, transportMeans2.IdentificationNumber);
								SetValue(departureMovementHeaderRow, CusInBondMoveHeaderSchema.BM_RN_NKTransportAtDepartureTrailer2Nationality, transportMeans2.Nationality);
							}
						}
					}
				}
				if (borderTransportMeansSorted.Count > 0)
				{
					var borderTransportMeans = borderTransportMeansSorted[0];
					var typeOfIdCode = borderTransportMeans.TypeOfIdentification.GetNullableCodeAsUpperCase();

					if (typeOfIdCode.HasValue)
					{
						var transportModeAndIdType = Utilities.TransportMeansMapping.FirstOrDefault(item => item.transportMeansCode == typeOfIdCode.Value);
						var transportMode = transportModeAndIdType.transportMode;
						if (!transportMode.IsEmpty)
						{
							SetValue(departureMovementHeaderRow, CusInBondMoveHeaderSchema.BM_ExportTransportMode, transportMode);
							SetValue(departureMovementHeaderRow, CusInBondMoveHeaderSchema.BM_ActiveBorderIdentificationType, transportModeAndIdType.idType);
						}
					}
					SetValue(departureMovementHeaderRow, CusInBondMoveHeaderSchema.BM_TOLCarrierID, borderTransportMeans.IdentificationNumber);
					SetValue(departureMovementHeaderRow, CusInBondMoveHeaderSchema.BM_RN_NKTOLCarrierNationality, borderTransportMeans.Nationality);
				}
			}
		}

		void FillDataFromAddInfos(IColumnIndexer departureMovementHeaderRow, List<AddInfo> addInfoCollection, NctsDepartureMovementHeader departureMovementHeader)
		{
			if (addInfoCollection != null)
			{
				var isSimplifiedProcedure = addInfoCollection.GetZBoolValue(DepartureMovementHeader.AddInfo.IsSimplifiedProcedure);
				if (isSimplifiedProcedure.HasValue)
				{
					SetValue(departureMovementHeaderRow, CusInBondMoveHeaderSchema.BM_GONumber, isSimplifiedProcedure.Value ? (ZString)NctsControlResult.Codes.AuthorizedTrader : ZString.Empty);
				}
				SetValue(departureMovementHeaderRow, CusInBondMoveHeaderSchema.BM_ReducedDatasetIndicator, addInfoCollection.GetZBoolValue(DepartureMovementHeader.AddInfo.ReducedDateSetIndicator));
				SetValue(departureMovementHeaderRow, CusInBondMoveHeaderSchema.BM_TypeOfSecurity, addInfoCollection.GetZStringValue(DepartureMovementHeader.AddInfo.SecurityIndicator));
				SetValue(departureMovementHeaderRow, CusInBondMoveHeaderSchema.BM_SpecificCircumstance, addInfoCollection.GetZStringValue(DepartureMovementHeader.AddInfo.SpecificCircumstance));
				SetValue(departureMovementHeaderRow, CusInBondMoveHeaderSchema.BM_ConveyanceNumber, addInfoCollection.GetZStringValue(DepartureMovementHeader.AddInfo.BM_ConveyanceNumber));
				SetValue(departureMovementHeaderRow, CusInBondMoveHeaderSchema.BM_CustomsOfficeAtBorder, addInfoCollection.GetZStringValue(DepartureMovementHeader.AddInfo.BM_CustomsOfficeAtBorder));

				FillTirCarnetNumberFromAddInfo(addInfoCollection, departureMovementHeader);
			}
		}

		void FillTirCarnetNumberFromAddInfo(List<AddInfo> addInfoCollection, NctsDepartureMovementHeader departureMovementHeader)
		{
			if (addInfoCollection.GetZStringValue(DepartureMovementHeader.AddInfo.TirCarnetNumber) is ZString tirCarnetNumber && !tirCarnetNumber.IsEmpty)
			{
				var tirCarnetEntryNumRow = (IColumnIndexer)CusEntryNumber.LoadOrCreate(departureMovementHeader, CusEntryNumberTypes.EU.TIRCarnetNumber, departureMovementHeader.CountryCode);
				SetValue(tirCarnetEntryNumRow, CusEntryNumSchema.CE_EntryNum, tirCarnetNumber);
			}
		}

		void FillSupplyChainActors(NctsHeader header)
		{
			if (CustomsReferenceDictionary.TryGetValue(NctsUxmlTypeList.Codes.SupplyChainActor, out var supplyChainActorDataObjects))
			{
				var existingActors = GetExistingActors(header.MovementHeader.CusSupplyChainActors.Cast<CusSupplyChainActorReference>().OrderBy(x => x.CFR_SystemCreateTimeUtc));
				foreach (var actorDataObject in supplyChainActorDataObjects)
				{
					var (code, reference, ownerAddress) = GetCodeReferenceAndOwner(actorDataObject);
					var ownerAddressPK = ownerAddress?.PK ?? ZGuid.Empty;
					if (!code.IsEmpty || !reference.IsEmpty || ownerAddressPK.IsValid)
					{
						MatchOrAddNew(GetKey(code, reference, ownerAddressPK), existingActors, () =>
						{
							var actorRow = GetColumnIndexer(header.MovementHeader.CusSupplyChainActors.AddNew());
							SetValue(actorRow, CusReferenceSchema.CFR_Code, code);
							SetValue(actorRow, CusReferenceSchema.CFR_Reference, reference);
							SetValue(actorRow, CusReferenceSchema.CFR_OA_Owner, ownerAddressPK);
						});
					}
				}
				existingActors.Values.SelectMany(x => x).DeleteAll();
			}
		}

		void FillServices(NctsHeader header)
		{
			if (dataObject.LocalProcessing?.AdditionalServiceCollection is DataObjectList<AdditionalService> additionalServiceCollection)
			{
				var additionalServiceDataObjectCollectionReader = new AdditionalServiceDataObjectCollectionReader(additionalServiceCollection, logger, factory, header);
				additionalServiceDataObjectCollectionReader.ReadIntoCollection();
			}
		}

		void FillBillsFromAdditionalBillCollection(NctsHeader header, List<AdditionalBill> additionalBills)
		{
			if (additionalBills != null)
			{
				var i = 0;
				foreach (var additionalBill in additionalBills)
				{
					var bill = header.Bills.Count > i ? header.Bills[i] : header.Bills.AddNew();
					SetValue(GetColumnIndexer(bill), CusInBondBillSchema.B0_ReferenceID, additionalBill.BillNumber ?? ZString.Empty);
					SetValue(GetColumnIndexer(bill), CusInBondBillSchema.B0_RX_NKLinePriceCurrency, additionalBill.LinePriceCurrency?.Code ?? ZString.Empty);
					FillJobDocAddress(bill.Consignor, additionalBill.OrganizationAddressCollection, DocAddressType.ConsignorDocumentaryAddress);
					FillJobDocAddress(bill.Consignee, additionalBill.OrganizationAddressCollection, DocAddressType.ConsigneeDocumentaryAddress);
					new CustomsSupportingInformationCollectionDataObjectReader(logger, helper).ReadIntoDataRows(bill.PK, CusInBondBillSchema.Constants.Prefix, bill.IsInDatabase, additionalBill, ((Integration.Customs.ICusSupportingInfoTypeSupporter)bill).GetCusSupportingInfoTypes().Keys.ToArray());
					FillBillSupplyChainActors(bill, additionalBill.CustomsReferenceCollection);
					++i;
				}
			}
		}

		void FillBillSupplyChainActors(NctsBill bill, List<CustomsReference> customsReferences)
		{
			if (customsReferences != null)
			{
				foreach (var customsReference in customsReferences.Where(x => (string)x.Type.Code == NctsUxmlTypeList.Codes.SupplyChainActor))
				{
					var (code, reference, ownerAddress) = GetCodeReferenceAndOwner(customsReference);
					var ownerAddressPK = ownerAddress?.PK ?? ZGuid.Empty;
					if (!code.IsEmpty || !reference.IsEmpty || ownerAddressPK.IsValid)
					{
						var actorRow = GetColumnIndexer(bill.CusSupplyChainActorReferences.AddNew());
						SetValue(actorRow, CusReferenceSchema.CFR_Code, code);
						SetValue(actorRow, CusReferenceSchema.CFR_Reference, reference);
						SetValue(actorRow, CusReferenceSchema.CFR_OA_Owner, ownerAddressPK);
					}
				}
			}
		}

		void FillBillsFromInBondMoveDetailCollection(NctsHeader header, NctsDepartureMovementHeader nctsDepartureMovementHeader, List<InBondMoveDetail> inBondMoveDetails)
		{
			if (inBondMoveDetails != null)
			{
				var i = 0;
				foreach (var inBondMoveDetail in inBondMoveDetails)
				{
					var bill = header.Bills.Count > i ? header.Bills[i] : header.Bills.AddNew();
					var billRow = GetColumnIndexer(bill);
					if (inBondMoveDetail.Sequence.HasValue)
					{
						bill.SequenceNumber = (short)inBondMoveDetail.Sequence.Value;
					}
					SetValue(billRow, CusInBondBillSchema.B0_TransportPaymentMethod, inBondMoveDetail.TransportPaymentMethod?.Code);
					SetValue(billRow, CusInBondBillSchema.B0_RN_NKCountryOfExport, inBondMoveDetail.DispatchCountry?.Code);
					SetValue(billRow, CusInBondBillSchema.B0_RN_NKCountryOfDestination, inBondMoveDetail.DestinationCountry?.Code);
					SetValue(billRow, CusInBondBillSchema.B0_Weight, inBondMoveDetail.Weight);
					SetValue(billRow, CusInBondBillSchema.B0_WeightUQ, inBondMoveDetail.WeightUnit?.Code);
					SetTransportMeans(bill, nctsDepartureMovementHeader, inBondMoveDetail.TransportMeansCollection);
					++i;
				}
			}
		}

		void SetTransportMeans(NctsBill bill, NctsDepartureMovementHeader nctsDepartureMovementHeader, List<TransportMeans> transportMeansCollection)
		{
			if (transportMeansCollection != null)
			{
				var departureTransportMeansSorted = transportMeansCollection.Where(means => means.TransportType.HasValue && means.TransportType.Value == TransportTypeCode.Departure).OrderBy(means => means.Order).ToList();
				if (departureTransportMeansSorted.Count > 0)
				{
					var transportMode = nctsDepartureMovementHeader.BM_InlandTransportMode;
					if (departureTransportMeansSorted[0] is TransportMeans transportMeans)
					{
						var typeOfIdCode = transportMeans.TypeOfIdentification.GetNullableCodeAsUpperCase();
						if (typeOfIdCode.HasValue)
						{
							var transportModeAndIdType = Utilities.TransportMeansMapping.FirstOrDefault(item => item.transportMeansCode == typeOfIdCode.Value);
							var transportMode2 = transportModeAndIdType.transportMode;
							if (transportMode2.IsEmpty || transportMode2 == transportMode)
							{
								bill.TransportTypeAtDeparture = transportModeAndIdType.idType;
							}
						}
						bill.TransportAtDeparture = transportMeans.IdentificationNumber ?? ZString.Empty;
						bill.TransportCountryAtDeparture = transportMeans.Nationality?.Code ?? ZString.Empty;
					}
					if (departureTransportMeansSorted.Count > 1 && transportMode == EU.Business.ModeOfTransportList.Codes._3_RoadTransport)
					{
						var trailerTransportMeansCollection = departureTransportMeansSorted.Where(x => x.TypeOfIdentification.GetCodeAsUpperCase() == TransportMeansList.Codes.RegistrationNumberOfTheRoadTrailer).Take(2).ToArray();
						if (trailerTransportMeansCollection.Length > 0)
						{
							var transportMeans1 = trailerTransportMeansCollection[0];
							bill.Trailer1IDAtDeparture = transportMeans1.IdentificationNumber ?? ZString.Empty;
							bill.Trailer1NationalityAtDeparture = transportMeans1.Nationality?.Code ?? ZString.Empty;
							if (trailerTransportMeansCollection.Length > 1)
							{
								var transportMeans2 = trailerTransportMeansCollection[1];
								bill.Trailer2IDAtDeparture = transportMeans2.IdentificationNumber ?? ZString.Empty;
								bill.Trailer2NationalityAtDeparture = transportMeans2.Nationality?.Code ?? ZString.Empty;
							}
						}
					}
				}
			}
		}

		void MatchOrAddNew<T>(string key, Dictionary<string, Queue<T>> existingItemKeyQueueDictionary, Action addNewAction)
		{
			if (existingItemKeyQueueDictionary.TryGetValue(key, out var queue) && queue.Count > 0)
			{
				queue.Dequeue();
				if (queue.Count == 0)
				{
					existingItemKeyQueueDictionary.Remove(key);
				}
			}
			else
			{
				addNewAction();
			}
		}

		(ZString code, ZString reference, OrgAddress ownerAddress) GetCodeReferenceAndOwner(CustomsReference customsReference)
		{
			var code = customsReference.SubType.GetCodeAsUpperCase();
			var reference = customsReference.Reference.GetValueOrDefault();
			var orgAddress = customsReference.Owner == null ? null : new OrganisationDataObjectReader(customsReference.Owner, logger, factory).GetMatched();
			return (code, reference, orgAddress != null && orgAddress.OA_OH != OrgHeader.UnmatchedOrganisationPK ? orgAddress : null);
		}

		Dictionary<string, Queue<CusSupplyChainActorReference>> GetExistingActors(IEnumerable<CusSupplyChainActorReference> actors) => GetExistingItems(actors, GetActorKey);
		Dictionary<string, Queue<CusAuthorizationUsage>> GetExistingUsages(IEnumerable<CusAuthorizationUsage> usages) => GetExistingItems(usages, GetUsageKey);
		Dictionary<string, Queue<CountryOfRouting>> GetExistingCountriesOfRouting(IEnumerable<CountryOfRouting> country) => GetExistingItems(country, p => p.CY_Code.ToUpper());

		Dictionary<string, Queue<T>> GetExistingItems<T>(IEnumerable<T> items, Func<T, string> getKey)
		{
			var result = new Dictionary<string, Queue<T>>();
			foreach (var item in items)
			{
				var key = getKey(item);
				var queue = result.GetOrAdd(key, () => new Queue<T>());
				queue.Enqueue(item);
			}
			return result;
		}

		string GetUsageKey(CusAuthorizationUsage usage) => GetKey(usage.AGC_Code, usage.AGC_Number, usage.AGC_OH_Owner);
		string GetActorKey(CusSupplyChainActorReference actor) => GetKey(actor.CFR_Code, actor.CFR_Reference, actor.CFR_OA_Owner);
		string GetKey(ZString code, ZString reference, ZGuid ownerPK) => string.Join("|", new string[] { code.ToUpperInvariant(), reference.ToUpperInvariant(), ownerPK.ToStringKey() });

		protected virtual void FillCountryData(NctsHeader header, NctsDepartureMovementHeader departureMovementHeader)
		{
		}
	}
}
