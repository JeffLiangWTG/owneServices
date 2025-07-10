using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.ManifestBase;
using Enterprise.Integration;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer
{
	public class AsycudaManifestHeaderDataObjectReader<T> : ShipmentDataObjectReader<T> where T : AsycudaManifestHeader
	{
		public AsycudaManifestHeaderDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
			isHVLV = dataObject.IsHVLV();
			shipmentDataObject = isHVLV ? dataObject.SubShipmentCollection[0] : dataObject;
		}

		readonly bool isHVLV;

		readonly Shipment shipmentDataObject;

		public override DataContextType DataContextType
		{
			get { return DataContextType.AsycudaManifest; }
		}

		protected override T GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			foundMultiple = false;
			if (isHVLV)
			{
				return dataObject.FindRelatedCustomsJobFromHVLVShipment<T>(factory.BOFactory);
			}

			return GetHeaderFromWayBillNumber();
		}
		bool foundMultiple;

		protected override T GetNewBusinessObject()
		{
			var manifestCountryCode = GetManifestCountryCode();
			var manifestStyle = GetManifestStyle();
			var manifestType = GetManifestType();

			if (manifestType.IsEmpty && !manifestCountryCode.IsEmpty)
			{
				var providers = ApplicationBusinessProvider.GetApplicationBusinessProviders(factory.BOFactory, manifestCountryCode, dataObject.MessageType?.Code ?? ZString.Empty).Take(2).ToArray();
				if (providers.Length == 1)
				{
					manifestType = providers[0].ManifestTypes.First().Code;
				}
			}

			var result = (T)factory.New(new AsycudaManifestHeaderTypeDecider().GetGlobalManifestType(factory.BOFactory, manifestCountryCode, manifestType, manifestStyle));
			using (result.GetCheckBusinessObjectTypeSuspender())
			{
				result.AMA_RN_NKCountry = manifestCountryCode;
				result.AMA_ManifestType = manifestType;
				result.AMA_ApplicationCode = manifestStyle;
			}

			return result;
		}

		#region Get Header

		T GetHeaderFromWayBillNumber()
		{
			T result = null;
			if (ActionPurposeCode != AsycudaEventMessageConstants.ActionPurpose.ADD)
			{
				var masterBill = dataObject.WayBillNumber.GetValueOrDefault();
				if (!masterBill.IsEmpty)
				{
					result = GetMainHeaderMatchingManifestCountry(factory.Load<T>(AsycudaManifestHeaderHelper.GetManifestHeadersQuery(masterBill)));
				}
			}
			return result;
		}

		ZString GetManifestCountryCode()
		{
			if (!manifestCountryCodeCached.HasValue)
			{
				manifestCountryCodeCached = ManifestCountryData?.Type.GetCodeAsUpperCase().Left(2) ?? ZString.Empty;
			}
			return manifestCountryCodeCached.Value;
		}
		ZString? manifestCountryCodeCached;

		EntryHeader ManifestCountryData => dataObject.EntryHeaderCollection == null || dataObject.EntryHeaderCollection.Count != 1 ? null : dataObject.EntryHeaderCollection.First();

		ZString GetManifestType()
		{
			if (!manifestTypeCached.HasValue)
			{
				manifestTypeCached = (ManifestCountryAdditionalData?.Style).GetValueOrDefault();
			}
			return manifestTypeCached.Value;
		}
		ZString? manifestTypeCached;

		ZString GetManifestStyle()
		{
			if (!manifestStyleCached.HasValue)
			{
				var manifestCountryCode = GetManifestCountryCode();
				var manifestType = GetManifestType();
				var keys = ApplicationBusinessProvider.GetApplicationBusinessProvidersDictionary(factory.BOFactory)
					.Keys.Where(k => k.CountryOrGrouping == manifestCountryCode && k.ManifestTypeCode == manifestType).ToArray();
				if (keys.Length == 1)
				{
					manifestStyleCached = keys.First().ApplicationCode;
				}
				else if (string.IsNullOrEmpty(dataObject.MessagingApplicationCode?.Code))
				{
					if (keys.Length == 0 || keys.Any(k => k.ApplicationCode == ApplicationCodeTypeList.Codes.Consolidator))
					{
						manifestStyleCached = ApplicationCodeTypeList.Codes.Consolidator;
					}
					else
					{
						manifestStyleCached = keys.First().ApplicationCode;
					}
				}
				else
				{
					manifestStyleCached = dataObject.MessagingApplicationCode.Code;
				}
			}

			return manifestStyleCached.Value;
		}
		ZString? manifestStyleCached;

		EntryInstruction ManifestCountryAdditionalData
		{
			get
			{
				var result = dataObject.EntryInstructionCollection != null && dataObject.EntryInstructionCollection.Count == 1 ? dataObject.EntryInstructionCollection[0] : null;
				if (result != null)
				{
					var manifestCountryData = ManifestCountryData;
					if (manifestCountryData == null || manifestCountryData.EntryInstructionLink != result.Link)
					{
						result = null;
					}
				}
				return result;
			}
		}

		T GetMainHeaderMatchingManifestCountry(T[] headers)
		{
			T result = null;
			var countryCode = GetManifestCountryCode();
			if (!countryCode.IsEmpty)
			{
				var matchedCountryCodeManifests = headers.Where(x => x.AMA_RN_NKCountry == countryCode).ToArray();
				if (matchedCountryCodeManifests.Length > 0)
				{
					var isManifestTypeMandatory = IsManifestTypeMandatory(countryCode);
					if (isManifestTypeMandatory)
					{
						var manifestType = GetManifestType();
						var matchedManifestTypeManifests = matchedCountryCodeManifests.Where(x => x.AMA_ManifestType == manifestType).ToArray();
						if (matchedManifestTypeManifests.Length == 1)
						{
							result = matchedManifestTypeManifests[0];
						}
						else if (matchedManifestTypeManifests.Length > 1)
						{
							foundMultiple = true;
						}
					}
					else if (matchedCountryCodeManifests.Length > 1)
					{
						foundMultiple = true;
					}
					else
					{
						result = matchedCountryCodeManifests[0];
					}
				}
			}
			return result;
		}

		bool IsManifestTypeMandatory(ZString manifestCountry)
		{
			return ApplicationBusinessProvider.GetApplicationBusinessProviders(factory.BOFactory, manifestCountry, dataObject.MessageType?.Code ?? null).Take(2).Count() > 1;
		}

		#endregion

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(T header)
		{
			var message = base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(header);

			if (string.IsNullOrWhiteSpace(message))
			{
				var countries = dataObject.EntryHeaderCollection?.ToArray() ?? Array.Empty<EntryHeader>();

				if (countries.Length > 1)
				{
					message = Res.GetString("37c20362-ae26-4389-a709-8427c2a41f14", "The entry header collection contains multiple countries.");
				}
				else if (header == null)
				{
					var manifestCountry = GetManifestCountryCode();
					var manifestType = GetManifestType();
					var isManifestTypeMandatory = IsManifestTypeMandatory(manifestCountry);
					if (foundMultiple)
					{
						message = Res.GetString("{53E838AD-D9F7-4553-A38F-468583448E8E}", "Cannot update data as there are multiple jobs matching (Manifest No: {0}, Country: {1}{2}).", dataObject.WayBillNumber.GetValueOrDefault(), manifestCountry, isManifestTypeMandatory ? ", Manifest Type: " + manifestType : "");
					}
					else if (manifestCountry.IsEmpty || !manifestCountry.IsSupportedCountries(factory.BOFactory))
					{
						message = Res.GetString("e4c68553-f490-4d1b-bd90-dd548e8c26d5", "Entry Header->Type->Code {0} is not a valid manifest country.", manifestCountry);
					}
					else if (isManifestTypeMandatory && manifestType.IsEmpty)
					{
						message = Res.GetString("{9F310F5B-7199-4A34-A956-24F8240BE9E3}", "Entry Instruction->Style is required for manifest country: {0}", manifestCountry);
					}
					else if (dataObject.MessageType != null && !string.IsNullOrEmpty(dataObject.MessageType.Code))
					{
						var providers = ApplicationBusinessProvider.GetApplicationBusinessProviders(factory.BOFactory, manifestCountry, dataObject.MessageType.Code);
						if (providers.Count() > 1)
						{
							message = Res.GetString("{75B27865-1E13-4D36-90E6-ABE8EE7C49B8}", "More than one manifest types are found for country: {0}, and shipment type: {1}", dataObject.MessageType.Description);
						}
					}
				}
			}

			return message;
		}

		protected override IMatchingBusinessEntityFinder<T> GetCombinedReferenceMatcher()
		{
			return null; // No Combined Reference Matching has been implemented for AsycudaManifestHeader.
		}

		bool IsUpdateEnabled
		{
			get
			{
				if (!isUpdateEnabledCached.HasValue)
				{
					isUpdateEnabledCached = ((dataObject.DataContext?.EventType?.GetCodeAsUpperCase() ?? ZString.Empty) == Events.TransferFromCustomsToManifestCode) ||
						(ActionPurposeCode == AsycudaEventMessageConstants.ActionPurpose.UPD);
				}
				return isUpdateEnabledCached.Value;
			}
		}
		bool? isUpdateEnabledCached;

		protected override void PopulateBusinessObject(T header)
		{
			helper = header.ApplicationBusinessProvider.GetAsycudaManifestDataObjectReaderHelper(header.AMA_RN_NKCountry);
			var headerRow = GetColumnIndexer(header);
			header.IsMarkApportionmentDirtySuspended = true;
			AddFetchHintsForManifestHeaderUpdate(header);
			var isUpdateEnabled = IsUpdateEnabled;
			if (!isUpdateEnabled || !header.HasManifestBeenSubmittedToCustomsIncludingChildren)
			{
				PopulateManifestHeader(header, headerRow);
			}
			else
			{
				logger.Log(LogType.Warning, Res.GetString("{81E22E3F-5F0E-476D-BC86-45AEB4435695}", "Header data on Global Manifest Job '{0}' cannot be updated as this job has been submitted to Customs.", header.AMA_JobReference));
			}
			FillBills(header, isUpdateEnabled);
		}
		AsycudaManifestDataObjectReaderHelper helper;

		void PopulateManifestHeader(AsycudaManifestHeader header, IColumnIndexer headerRow)
		{
			var isStandAlone = header.IsStandAlone;
			if (!isStandAlone && !headerRow.GetValue(AsycudaManifestHeaderSchema.AMA_OverrideFreightDefaults))
			{
				helper.FilterAndSet(AsycudaManifestHeaderSchema.AMA_OverrideFreightDefaults, dataObject, (column) => SetValue(headerRow, column, ZBool.True));
			}

			var masterBill = header.MasterBill;
			if (masterBill != null)
			{
				var masterBillRow = GetColumnIndexer(masterBill);
				helper.FilterAndSet(AsycudaBillSchema.ABL_BillNumber, dataObject, (column) => SetValue(masterBillRow, column, dataObject.WayBillNumber));
				helper.FilterAndSet(AsycudaBillSchema.ABL_RL_NKPortOfLoading, dataObject, (column) => SetValue(masterBillRow, column, dataObject.PortOfLoading));
				helper.FilterAndSet(AsycudaBillSchema.ABL_RL_NKPortOfDischarge, dataObject, (column) => SetValue(masterBillRow, column, dataObject.PortOfDischarge));
				if (header.FeatureProvider?.SupportsCustomsPorts(header) ?? false)
				{
					helper.FilterAndSet(AsycudaBillSchema.ABL_CustomsDischargePort, dataObject, (column) => SetValue(masterBillRow, column, dataObject.CustomsDischargePort));
					helper.FilterAndSet(AsycudaBillSchema.ABL_CustomsLoadPort, dataObject, (column) => SetValue(masterBillRow, column, dataObject.CustomsLoadPort));
				}
				if (isHVLV)
				{
					helper.FilterAndSet(AsycudaBillSchema.ABL_CustomsLoadPort, dataObject, (column) => SetValue(masterBillRow, column, dataObject.PortOfLoading));
					helper.FilterAndSet(AsycudaBillSchema.ABL_CustomsDischargePort, dataObject, (column) => SetValue(masterBillRow, column, dataObject.PortOfDischarge));
				}
				FillDates(masterBillRow, headerRow);
			}

			helper.FilterAndSet(AsycudaManifestHeaderSchema.AMA_TransportMode, dataObject, (column) => SetValue(headerRow, column, dataObject.TransportMode));
			FillVoyageFlightNo(dataObject.VoyageFlightNo, dataObject.TransportMode?.Code, header);
			helper.FilterAndSet(AsycudaManifestHeaderSchema.AMA_VesselName, dataObject, (column) => SetValue(headerRow, column, dataObject.VesselName));
			helper.FilterAndSet(AsycudaManifestHeaderSchema.AMA_LloydsNumber, dataObject, (column) => SetValue(headerRow, column, dataObject.LloydsIMO));
			FillVesselConveyanceNationality(headerRow);
			helper.FilterAndSet(AsycudaManifestHeaderSchema.AMA_MasterInformation, dataObject, (column) => SetValue(headerRow, column, dataObject.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.MasterInformation, logger)));
			helper.FilterAndSet(AsycudaManifestHeaderSchema.AMA_Trailer1RegNo, dataObject, (column) => SetValue(headerRow, column, dataObject.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.Trailer1, logger)));
			helper.FilterAndSet(AsycudaManifestHeaderSchema.AMA_RN_NKTrailer1RegCountry, dataObject, (column) => SetValue(headerRow, column, dataObject.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.Trailer1CountryOfRegistration, logger)));
			helper.FilterAndSet(AsycudaManifestHeaderSchema.AMA_Trailer2RegNo, dataObject, (column) => SetValue(headerRow, column, dataObject.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.Trailer2, logger)));
			helper.FilterAndSet(AsycudaManifestHeaderSchema.AMA_RN_NKTrailer2RegCountry, dataObject, (column) => SetValue(headerRow, column, dataObject.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.Trailer2CountryOfRegistration, logger)));
			helper.FilterAndSet(AsycudaManifestHeaderSchema.AMA_RL_NKPortOfFirstArrival, dataObject, (column) => SetValue(headerRow, column, dataObject.PortOfFirstArrival));
			helper.FilterAndSet(AsycudaManifestHeaderSchema.AMA_AgentType, dataObject, (column) => SetValue(headerRow, column, dataObject.DeclarantType));
			helper.FilterAndSet(AsycudaManifestHeaderSchema.AMA_ContainerMode, dataObject, (column) => SetValue(headerRow, column, dataObject.ContainerMode));
			helper.FilterAndSet(AsycudaManifestHeaderSchema.AMA_IsBuyersConsolidation, dataObject, (column) => SetValue(headerRow, column, dataObject.IsBuyersConsol));
			helper.FilterAndSet(AsycudaManifestHeaderSchema.AMA_RadioCallSign, dataObject, (column) => SetValue(headerRow, column, dataObject.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.RadioCallSign, logger)));
			helper.FilterAndSet(AsycudaManifestHeaderSchema.AMA_CustomsOffice, dataObject, (column) => SetValue(headerRow, column, dataObject.CustomsOffice));

			FillDeConsolidator(headerRow);
			FillDischargeTerminal(headerRow);
			FillCarrier(headerRow);
			FillNotes(header);
			FillContainers(header);
			FillCustomsSupportingInfo(header);
			FillManifestSpecificData(header);

			if (isHVLV)
			{
				FillManifestType(headerRow);
				FillShippingAgent(headerRow);
			}
		}

		void FillVoyageFlightNo(ZString? voyageFlightNo, ZString? transportMode, AsycudaManifestHeader header)
		{
			helper.FillVoyageFlightNo(voyageFlightNo, transportMode ?? default, logger, header);
		}

		void FillVesselConveyanceNationality(IColumnIndexer headerRow)
		{
			var addInfoConveyanceNationality = dataObject.AddInfoCollection?.GetZStringValue(AddInfoConstants.Header.ConveyanceNationality, logger);
			if (addInfoConveyanceNationality.HasValue)
			{
				helper.FilterAndSet(AsycudaManifestHeaderSchema.AMA_RN_NKConveyanceNationality, dataObject, (column) => SetValue(headerRow, column, addInfoConveyanceNationality.Value));
			}
			else if (dataObject.VesselCountryOfRegistration is Country conveyanceNationality)
			{
				helper.FilterAndSet(AsycudaManifestHeaderSchema.AMA_RN_NKConveyanceNationality, dataObject, (column) => SetValue(headerRow, column, conveyanceNationality.Code));
			}
		}

		void AddFetchHintsForManifestHeaderUpdate(AsycudaManifestHeader header)
		{
			if (header.IsInDatabase)
			{
				var hasContainers = dataObject.ContainerCollection != null;
				var hasManifestCountries = dataObject.EntryHeaderCollection != null;
				var hasBillCountries = false;
				var hasPacks = false;
				var hasPackCountries = false;
				var hasBills = dataObject.SubShipmentCollection != null;
				if (hasBills)
				{
					foreach (var subShipment in dataObject.SubShipmentCollection)
					{
						hasBillCountries |= subShipment.EntryHeaderCollection != null;
						if (subShipment.PackingLineCollection != null)
						{
							hasPacks = true;
							foreach (var packingLine in subShipment.PackingLineCollection)
							{
								if (packingLine.PackingLineCollection != null)
								{
									hasPackCountries = true;
									break;
								}
							}
							if (hasPackCountries)
							{
								break;
							}
						}
					}
				}
				AddFetchHintsForManifestHeaderUpdate(header, hasManifestCountries, hasContainers, hasBills, hasBillCountries, hasPacks, hasPackCountries);
			}
		}

		void AddFetchHintsForManifestHeaderUpdate(AsycudaManifestHeader header, bool hasManifestCountries, bool hasContainers, bool hasBills, bool hasBillCountries, bool hasPacks, bool hasPackCountries)
		{
			var fetchHintsToBeAdded = new Dictionary<Type, FetchHintData>();
			FetchHintData cusEntryNumFetchData = null;
			if (hasManifestCountries)
			{
				fetchHintsToBeAdded.Add(typeof(AsycudaManifestHeader), GetManifestCountryFetchData());
				cusEntryNumFetchData = GetCusEntryNumFetchData();
			}
			if (hasContainers)
			{
				fetchHintsToBeAdded.Add(typeof(AsycudaContainer), GetContainerFetchData());
			}
			if (hasBills)
			{
				var billFetchData = GetBillFetchData();
				fetchHintsToBeAdded.Add(typeof(AsycudaBill), billFetchData);

				if (hasBillCountries)
				{
					AddBillFetchData(billFetchData);
					fetchHintsToBeAdded.Add(typeof(ABLEntryNum), GetABLEntryNumFetchData());
					cusEntryNumFetchData = cusEntryNumFetchData ?? GetCusEntryNumFetchData();
				}
				if (hasPacks)
				{
					fetchHintsToBeAdded.Add(typeof(AsycudaPack), GetPackFetchData());
					if (hasPackCountries)
					{
						fetchHintsToBeAdded.Add(typeof(AsycudaPackedItem), GetPackedItemFetchData());
						cusEntryNumFetchData = cusEntryNumFetchData ?? GetCusEntryNumFetchData();
					}
				}
			}
			if (cusEntryNumFetchData != null)
			{
				fetchHintsToBeAdded.Add(typeof(Common.CusEntryNumber), cusEntryNumFetchData);
			}
			if (fetchHintsToBeAdded.Count > 0)
			{
				header.AddFetchHints(fetchHintsToBeAdded);
			}
		}

		FetchHintData GetCusEntryNumFetchData()
		{
			var data = new FetchHintData();
			data.AddForeignKey(StmNoteSchema.ST_ParentID, CusEntryNumSchema.PK);
			return data;
		}

		FetchHintData GetPackedItemFetchData()
		{
			var data = new FetchHintData();
			data.AddForeignKey(StmNoteSchema.ST_ParentID, AsycudaPackedItemSchema.PK);
			data.AddForeignKey(GenAddOnColumnSchema.XA_ParentID, AsycudaPackedItemSchema.PK);
			data.AddForeignKey(CusEntryNumSchema.CE_ParentID, AsycudaPackedItemSchema.PK);
			return data;
		}

		FetchHintData GetPackFetchData()
		{
			var data = new FetchHintData();
			data.AddForeignKey(StmNoteSchema.ST_ParentID, AsycudaPackSchema.PK);
			data.AddForeignKey(GenAddOnColumnSchema.XA_ParentID, AsycudaPackSchema.PK);
			data.AddForeignKey(AsycudaContainerBillOrPackageLinkSchema.APC_APA_Pack, AsycudaPackSchema.PK, typeof(AsycudaContainerBillOrPackageLink));
			return data;
		}

		FetchHintData GetABLEntryNumFetchData()
		{
			var data = new FetchHintData();
			data.AddForeignKey(StmNoteSchema.ST_ParentID, CusEntryNumSchema.PK);
			data.AddForeignKey(GenPivotSchema.XX_Relation1ID, CusEntryNumSchema.PK);
			return data;
		}

		void AddBillFetchData(FetchHintData data)
		{
			data.AddForeignKey(EDIMessageSchema.EM_LinkUniqueID, AsycudaBillSchema.PK);
			data.AddForeignKey(GenAddOnColumnSchema.XA_ParentID, AsycudaBillSchema.PK);
			data.AddForeignKey(CusEntryNumSchema.CE_ParentID, AsycudaBillSchema.PK, typeof(ABLEntryNum));
		}

		FetchHintData GetBillFetchData()
		{
			var data = new FetchHintData();
			data.AddForeignKey(StmNoteSchema.ST_ParentID, AsycudaBillSchema.PK);
			data.AddForeignKey(AsycudaPackSchema.APA_ABL_Bill, AsycudaBillSchema.PK, typeof(AsycudaPack));
			data.AddForeignKey(AsycudaPackedItemSchema.API_ABL_Bill, AsycudaBillSchema.PK, typeof(AsycudaPackedItem));
			return data;
		}

		FetchHintData GetManifestCountryFetchData()
		{
			var data = new FetchHintData();
			data.AddForeignKey(StmNoteSchema.ST_ParentID, AsycudaManifestHeaderSchema.PK);
			data.AddForeignKey(GenAddOnColumnSchema.XA_ParentID, AsycudaManifestHeaderSchema.PK);
			data.AddForeignKey(CusEntryNumSchema.CE_ParentID, AsycudaManifestHeaderSchema.PK);
			return data;
		}

		FetchHintData GetContainerFetchData()
		{
			var data = new FetchHintData();
			data.AddForeignKey(StmNoteSchema.ST_ParentID, AsycudaContainerSchema.PK);
			return data;
		}

		void FillManifestSpecificData(AsycudaManifestHeader header)
		{
			helper.FillManifestSpecificData(dataObject, logger, header, factory);
			var manifestCountryData = ManifestCountryData;
			if (manifestCountryData != null)
			{
				new AsycudaManifestHeaderEntryHeaderDataObjectReader(manifestCountryData, logger, factory, header, helper).ReadIntoBusinessObject();
			}

			var manifestCountryAdditionalData = ManifestCountryAdditionalData;
			if (manifestCountryAdditionalData != null)
			{
				new AsycudaManifestHeaderEntryInstructionDataObjectReader(manifestCountryAdditionalData, logger, factory, header, helper).ReadIntoBusinessObject();
			}
		}

		void FillManifestType(IColumnIndexer headerRow)
		{
			var manifestType = helper.GetManifestTypeByDataObject(dataObject);
			if (!manifestType.IsEmpty)
			{
				helper.FilterAndSet(AsycudaManifestHeaderSchema.AMA_ManifestType, dataObject, (column) => SetValue(headerRow, column, manifestType));
			}
		}

		void FillShippingAgent(IColumnIndexer headerRow)
		{
			var shippingAddress = dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.SendingForwarderAddress));

			if (shippingAddress != null)
			{
				var shippingAddressBO = new OrganisationDataObjectReader(shippingAddress, logger, factory).GetMatched();
				if (shippingAddressBO != null)
				{
					helper.FilterAndSet(AsycudaManifestHeaderSchema.AMA_OA_ShippingAgent, dataObject, (column) => SetValue(headerRow, column, shippingAddressBO.PK));
				}
			}
		}

		void FillDeConsolidator(IColumnIndexer headerRow)
		{
			var deconsolidatorAddress = dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.CustomsContainerYardAddress));
			if (deconsolidatorAddress != null)
			{
				var deconsolidatorAddressBO = new OrganisationDataObjectReader(deconsolidatorAddress, logger, factory).GetMatched();
				if (deconsolidatorAddressBO != null)
				{
					helper.FilterAndSet(AsycudaManifestHeaderSchema.AMA_OA_DeconsolidateAddress, dataObject, (column) => SetValue(headerRow, column, deconsolidatorAddressBO.PK));
				}
			}
		}

		void FillCarrier(IColumnIndexer headerRow)
		{
			var addressType = isHVLV ? DocAddressType.ShippingLineAddress : DocAddressType.Carrier;
			var carrierAddress = dataObject.OrganizationAddressCollection.FirstOrDefault(addressType.ToString());
			if (carrierAddress != null)
			{
				var carrierAddressBO = new OrganisationDataObjectReader(carrierAddress, logger, factory).GetMatched();
				if (carrierAddressBO != null)
				{
					helper.FilterAndSet(AsycudaManifestHeaderSchema.AMA_OA_Carrier, dataObject, (column) => SetValue(headerRow, column, carrierAddressBO.PK));
				}
				if (carrierAddressBO?.Header.MiscServ?.Airline != null)
				{
					helper.FilterAndSet(AsycudaManifestHeaderSchema.AMA_CarrierCode, dataObject, (column) => SetValue(headerRow, column, carrierAddressBO.Header.MiscServ?.Airline.RM_TwoCharacterCode ?? ZString.Empty));
				}
			}
		}

		void FillDischargeTerminal(IColumnIndexer headerRow)
		{
			var terminalAddress = dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.CustomsContainerTerminalOperatorAddress));
			if (terminalAddress != null)
			{
				var terminalAddressAddressBO = new OrganisationDataObjectReader(terminalAddress, logger, factory).GetMatched();
				if (terminalAddressAddressBO != null)
				{
					helper.FilterAndSet(AsycudaManifestHeaderSchema.AMA_OA_DischargeTerminalAddress, dataObject, (column) => SetValue(headerRow, column, terminalAddressAddressBO.PK));
				}
			}
		}

		void FillDates(IColumnIndexer masterBillRow, IColumnIndexer header)
		{
			if (shipmentDataObject.DateCollection != null && shipmentDataObject.DateCollection.Count > 0)
			{
				FillDates(masterBillRow, shipmentDataObject.DateCollection, ZBool.False,
					new DateTypeSchemaColumnMap(AsycudaBillSchema.ABL_E_DEP, new[] { DateType.Departure }),
					new DateTypeSchemaColumnMap(AsycudaBillSchema.ABL_E_ARV, new[] { DateType.Arrival }),
					new DateTypeSchemaColumnMap(AsycudaBillSchema.ABL_BillIssueDate, new[] { DateType.BillIssued }));

				helper.FillAdditionalDates(shipmentDataObject.DateCollection, header);
			}
		}

		void FillNotes(AsycudaManifestHeader header)
		{
			if (dataObject.NoteCollection != null)
			{
				new NotesCollectionReader(dataObject.NoteCollection, logger, factory, header).ReadIntoCollection();
			}
		}

		void FillBills(AsycudaManifestHeader header, bool isUpdateEnabled)
		{
			if (shipmentDataObject.SubShipmentCollection != null)
			{
				helper.BillsReaderHelper.MarkUnprocessedExistingObjectFor(factory, header);
				foreach (var billShipmentDataObject in shipmentDataObject.SubShipmentCollection)
				{
					var bill = helper.GetBillDataObjectReader(billShipmentDataObject, logger, factory, header, helper, isUpdateEnabled).ReadIntoBusinessObject();
					helper.BillsReaderHelper.MarkProcessed(bill);
				}
				if (!isUpdateEnabled)
				{
					helper.BillsReaderHelper.DeleteUnprocessedObjectsFor(header, logger, isSubShipmentCollectionPartialContent);
				}
			}
		}

		protected bool isSubShipmentCollectionPartialContent => (shipmentDataObject.SubShipmentCollection?.Content ?? CollectionContent.Complete) == CollectionContent.Partial;

		void FillContainers(AsycudaManifestHeader header)
		{
			if (dataObject.ContainerCollection != null)
			{
				helper.ContainersReaderHelper.MarkUnprocessedExistingObjectFor(factory, header);
				foreach (var containerDataObject in dataObject.ContainerCollection)
				{
					var container = new AsycudaContainerDataObjectReader(containerDataObject, logger, factory, header).ReadIntoBusinessObject();
					helper.ContainersReaderHelper.MarkProcessed(container);
				}
				helper.ContainersReaderHelper.DeleteUnprocessedObjectsFor(header, logger);
			}
		}

		void FillCustomsSupportingInfo(AsycudaManifestHeader header)
		{
			var supportedTypes = helper.GetHeaderSupportedCusSupportingInfoCSI_Types(header);
			if (supportedTypes.Length > 0)
			{
				var reader = new CustomsSupportingInformationCollectionDataObjectReader(logger, new UniversalDataObjectReaderHelper(factory, header.AMA_RN_NKCountry, header.AMA_RN_NKCountry));
				reader.ReadIntoDataRows(header.PK, header.TablePrefix, header.IsInDatabase, dataObject, supportedTypes);
			}
		}

		ZString ActionPurposeCode => dataObject.DataContext?.ActionPurposeCode.ToUpperInvariant() ?? ZString.Empty;
	}
}
