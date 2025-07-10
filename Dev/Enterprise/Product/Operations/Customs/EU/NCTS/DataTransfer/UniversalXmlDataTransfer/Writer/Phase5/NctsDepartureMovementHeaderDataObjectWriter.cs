using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;
using AddressTypes = Enterprise.UniversalDataBuss.Integration.AddressTypes;
using CusAuthorizationUsage = Enterprise.Customs.EU.NCTS.Business.CusAuthorizationUsage;
using Seal = Enterprise.UniversalDataBuss.DataObjects.Universal.Seal;

namespace Enterprise.Customs.EU.NCTS.DataTransfer.Phase5
{
	public class NctsDepartureMovementHeaderDataObjectWriter : NctsMovementHeaderDataObjectWriter
	{
		public NctsDepartureMovementHeaderDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override void PopulateDataObjectMain(NctsHeader headerBO, Shipment headerData)
		{
			headerData.CommercialInfo = new CommercialInfo()
			{
				CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(),
			};

			headerData.SetInBondMoveHeaderCollection(() => SetMoveHeaders(headerBO));
			if (!writeManager.HasRecipientRoleDetail(RecipientRoleType.BWR))
			{
				headerData.SetContainerCollection(() => SetContainersAndSeals(headerBO));
			}
			headerData.SetPackingLineCollection(() => SetPackingLines(headerBO));

			headerData.BookingConfirmationReference = headerBO.MovementHeader.BM_PaperlessInbondNum;

			PopulateAddresses(headerBO.MovementHeader, headerData);
			PopulateLocalProcessingFromServices(headerData, headerBO);

			PopulateClientAddress(headerBO, headerData);

			foreach (var bill in headerBO.Bills)
			{
				var invoiceHeader = new CommercialInvoiceHeader(writeManager.WriterStrategy)
					.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>()));

				ProcessCollection(bill.GoodsItems, new Phase5DepartureGoodsItemDataObjectWriter(writeManager, helper, invoiceHeader));
				headerData.CommercialInfo.CommercialInvoiceCollection.Add(invoiceHeader);
			}
			headerData.SetAdditionalBillCollection(() => PopulateAdditionalBillCollection(headerBO.Bills));

			headerData.AddOrgAddress(writeManager, ((IWarehouseIntegrationSupporter)headerBO).WarehouseAddress, DocAddressType.CustomsWarehouseAddress);
		}

		List<InBondMoveHeader> SetMoveHeaders(NctsHeader headerBO)
		{
			var departureMovementHeader = headerBO.MovementHeader;
			var lookups = departureMovementHeader.Lookups;

			var refCountryList = headerBO.Factory.GetRefCountryList();

			var moveHeader = new InBondMoveHeader(writeManager.WriterStrategy)
			{
				EntryType = ListHelper.GetWithDescription<CodeDescriptionPair9Char>(departureMovementHeader.BM_InBondEntryType, lookups.DeclarationTypeList),
				AdditionalEntryType = ListHelper.GetWithDescription<CodeDescriptionPair1Char>(departureMovementHeader.BM_AdditionalDeclarationType, lookups.AdditionalDeclarationTypeList),
				PaymentMethod = ListHelper.GetWithDescription<CodeDescriptionPair>(departureMovementHeader.BM_MethodOfPayment, departureMovementHeader.Lookups.TransportChargesModeOfPaymentList),
				PortOfOrigin = ListHelper.GetWithName(departureMovementHeader.BM_RN_NKCountryOfDispatch, refCountryList),
				PortOfDestination = ListHelper.GetWithName(departureMovementHeader.BM_RL_NKDestinationPort.Left(2), refCountryList),
				PortOfLoading = new UNLOCO() { Code = departureMovementHeader.BM_PortOfPresentationCode, Name = departureMovementHeader.BM_PlaceOfLoading },
				PortOfDischarge = new UNLOCO() { Code = departureMovementHeader.BM_ForeignDestPortKCode, Name = departureMovementHeader.BM_PlaceOfUnloading },
				GrossWeight = departureMovementHeader.BM_GrossWeight,
				GrossWeightUnit = new UnitOfWeight() { Code = Weight.Kilograms, Description = Weight.GetDescription(Weight.Kilograms, PluralState.Plural) },
				MessagingStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(departureMovementHeader.BM_MessageStatus, lookups.NctsMessageStatusList),
				PhaseStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(departureMovementHeader.BM_Phase, lookups.NctsMovementHeaderTransactionStatusList),
				CustomsStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(departureMovementHeader.BM_CustomsStatus, lookups.NctsTransitStatusList),
			};
			moveHeader.SetTransportMeansCollection(() => PopulateTransportMeansCollection(departureMovementHeader));
			moveHeader.SetAddInfoCollection(() => PopulateAddInfosData(departureMovementHeader));
			moveHeader.SetDateCollection(() => SetDates(departureMovementHeader));
			moveHeader.SetCustomsReferenceCollection(() => SetCustomsReferences(headerBO, departureMovementHeader));
			moveHeader.SetGuaranteeCollection(() => SetGuarantees(departureMovementHeader));
			moveHeader.SetLocationOfGoodsCollection(() => SetLocationOfGoods(departureMovementHeader));
			moveHeader.SetInBondMoveDetailCollection(() => PopulateInBondMoveDetailCollection(headerBO.Bills));
			moveHeader.SetCustomsSupportingInformationCollection(() => PopulateCusSupportingInfoData(headerBO, departureMovementHeader));
			PopulateAddresses(departureMovementHeader, moveHeader);
			return new List<InBondMoveHeader>() { moveHeader };
		}

		void PopulateClientAddress(NctsHeader headerBO, Shipment headerData)
		{
			headerData.AddOrgAddress(writeManager, headerBO.Consignor, DocAddressType.WarehouseClient);
			headerData.AddOrgAddress(writeManager, headerBO.Consignee, DocAddressType.ImporterDocumentaryAddress);
		}

		List<CustomsSupportingInformation> PopulateCusSupportingInfoData(NctsHeader headerBO, NctsDepartureMovementHeader departureMovementHeader)
		{
			var result = new List<CustomsSupportingInformation>();

			if (headerBO is Integration.Customs.ICusSupportingInfoTypeSupporter)
			{
				var headerSuppInfoCollection = CustomsSupportingInformationCollectionCreator.CreateCollection(helper, headerBO, writeManager);
				if (headerSuppInfoCollection != null)
				{
					result.AddRange(headerSuppInfoCollection);
				}
			}
			if (departureMovementHeader is Integration.Customs.ICusSupportingInfoTypeSupporter)
			{
				var cusSuppInfoCollection = CustomsSupportingInformationCollectionCreator.CreateCollection(helper, departureMovementHeader, writeManager);
				if (cusSuppInfoCollection != null)
				{
					result.AddRange(cusSuppInfoCollection);
				}
			}
			return result;
		}

		void PopulateLocalProcessingFromServices(Shipment headerData, NctsHeader headerBO)
		{
			var services = headerBO.Services;
			if (!services.IsNullOrEmpty())
			{
				headerData.LocalProcessing = new LocalProcessing(writeManager.WriterStrategy);
				headerData.LocalProcessing.SetAdditionalServiceCollection(() => ProcessCollection(services, new AdditionalServiceDataObjectWriter(writeManager), CollectionContent.Complete));
			}
		}

		List<LocationOfGoods> SetLocationOfGoods(NctsDepartureMovementHeader movementHeader)
		{
			var goodsLocation = Customs.Business.CusGoodsLocation.Load<Business.CusGoodsLocation>(movementHeader, CusGoodsLocationUseList.Codes.Departure);
			var result = new List<LocationOfGoods>();
			if (goodsLocation != null)
			{
				var lookups = goodsLocation.Lookups;
				var address = goodsLocation.Address;
				result.Add(new LocationOfGoods
				{
					AdditionalIdentifier = goodsLocation.CGL_AdditionalIdentifier,
					AuthorizationNumber = address.AuthorisationNumber,
					Contact = address != null ? new Contact { Name = address.E2_Contact, Email = address.E2_Email, PhoneNumber = address.E2_Phone } : null,
					OrgAddress = new JobDocAddressDataObjectWriter(writeManager).GetDataObject(address),
					Qualifier = ListHelper.GetWithDescription<CodeDescriptionPair1Char>(goodsLocation.CGL_Qualifier, lookups.QualifierList),
					SubType = ListHelper.GetWithDescription<CodeDescriptionPair2Char>(goodsLocation.CGL_Type, lookups.TypeList),
					Type = new LocationOfGoodsTypeConverter().ToEnumValue(goodsLocation.CGL_LocationUse)
				});
			}
			return result;
		}

		DataObjectList<Container> SetContainersAndSeals(NctsHeader headerBO)
		{
			var collection = new DataObjectList<Container>();
			foreach (var containerBO in headerBO.DepartureHeaderContainers)
			{
				collection.Add(new Container(writeManager.WriterStrategy)
				{
					FCL_LCL_AIR = ListHelper.GetWithDescription<ContainerMode>(containerBO.BC_Mode, containerBO.Lookups.CargoIdTypeList),
					ContainerNumber = containerBO.BC_ContainerNum,
					Seal = containerBO.Seal1,
					SecondSeal = containerBO.Seal2,
					SealCollection = PopulateSealCollection(containerBO.AdditionalSeals),
				});
			}
			return collection;
		}

		List<Seal> PopulateSealCollection(CusSealCollection sealCollection)
		{
			return sealCollection.Select(seal => new Seal
				{
					SealNumber = seal.BK_SealNumber,
					Sequence = seal.BK_SequenceNumber,
					StatusInformation = seal.BK_UnloadingState
				})
				.ToList();
		}

		List<Guarantee> SetGuarantees(NctsDepartureMovementHeader departureMovementHeader)
		{
			var collection = new List<Guarantee>();
			foreach (NctsGuarantee guaranteeBO in departureMovementHeader.Guarantees)
			{
				collection.Add(new Guarantee
				{
					BondType = ListHelper.GetWithDescription<CodeDescriptionPair1Char>(guaranteeBO.PW_BondType, guaranteeBO.Lookups.BondTypeList),
					BondNumber = guaranteeBO.PW_BondNumber,
					BondAmount = guaranteeBO.PW_BondAmount,
					BondNumber2 = guaranteeBO.PW_BondNumber2,
					AccessCode = guaranteeBO.PW_Password.IsEmpty ? string.Empty : GuaranteeAccessCodeMask,
					BondCurrency = guaranteeBO.Currency is RefCurrency currency ? new Currency() { Code = currency.RX_Code, Description = currency.RX_Desc } : null,
					SuretyCode = guaranteeBO.PW_SuretyCode,
					BondFiledPort = ListHelper.GetWithDescription<CodeDescriptionPair8Char>(guaranteeBO.PW_BondFiledPort, guaranteeBO.Lookups.OfficeCodeList),
				});
			}
			return collection;
		}

		public const string GuaranteeAccessCodeMask = "****";

		List<CustomsReference> SetCustomsReferences(NctsHeader headerBO, NctsDepartureMovementHeader departureMovementHeader)
		{
			var result = new List<CustomsReference>();
			result.Add(new CustomsReference
			{
				Type = new CodeDescriptionPair { Code = NctsUxmlTypeList.Codes.UniqueConsignmentReference, Description = NctsUxmlTypeList.Descriptions.UniqueConsignmentReference },
				Reference = departureMovementHeader.BM_UniqueConsignmentReference
			});
			result.Add(new CustomsReference
			{
				Type = new CodeDescriptionPair { Code = NctsUxmlTypeList.Codes.LocalReferenceNumber, Description = NctsUxmlTypeList.Descriptions.LocalReferenceNumber },
				Reference = departureMovementHeader.BM_PaperlessInbondNum
			});

			if (departureMovementHeader.CusSupplyChainActors.Count == 0)
			{
				result.Add(new CustomsReference
				{
					Type = new CodeDescriptionPair { Code = NctsUxmlTypeList.Codes.SupplyChainActor, Description = NctsUxmlTypeList.Descriptions.SupplyChainActor }
				});
			}
			else
			{
				result.AddRange(departureMovementHeader.CusSupplyChainActors.OfType<CusSupplyChainActorReference>().OrderBy(x => x.CFR_SystemCreateTimeUtc)
					.Select(actor => CreateCustomsReferenceFromCusSupplyChainActorReference(actor)));
			}

			if (departureMovementHeader.CusAuthorizationUsages.Count == 0)
			{
				result.Add(new CustomsReference
				{
					Type = new CodeDescriptionPair() { Code = CusReferenceTypeCodes.AUT, Description = CusReferenceTypeDescriptions.AUT }
				});
			}
			else
			{
				var authorizationTypeList = Customs.Business.CusAuthorisationHeaderProvider.GetByCountryCode(headerBO.CountryCode)?.GetAuthorisationTypeList(headerBO.Factory) ?? new ZArchitecture.Core.CodeDescriptionPairList();
				result.AddRange(departureMovementHeader.CusAuthorizationUsages.OrderBy(p => p.AGC_SystemCreateTimeUtc)
					.Select(p => CreateCustomsReferenceFromCusAuthorizationUsages(p, authorizationTypeList)));
			}

			if (departureMovementHeader.CustomsOfficesForDeparture.Count == 0)
			{
				result.Add(new CustomsReference
				{
					Type = new CodeDescriptionPair { Code = NctsUxmlTypeList.Codes.OfficeCode, Description = NctsUxmlTypeList.Descriptions.OfficeCode }
				});
			}
			else
			{
				result.AddRange(departureMovementHeader.CustomsOfficesForDeparture.Cast<NctsEuOfficeCode>().OrderBy(x => x.CY_SystemCreateTimeUtc)
					.Select(officeCode => CreateCustomsReferenceFromNctsEuOfficeCode(officeCode)));
			}

			if (headerBO.CountriesOfRouting.Count == 0)
			{
				result.Add(new CustomsReference
				{
					Type = new CodeDescriptionPair { Code = NctsUxmlTypeList.Codes.CountryOfRoutingCode, Description = NctsUxmlTypeList.Descriptions.CountryOfRoutingCode, }
				});
			}
			else
			{
				result.AddRange(headerBO.CountriesOfRouting.Cast<CountryOfRouting>().OrderBy(p => p.CY_Order)
					.Select(countryOfRouting => CreateCustomsReferenceFromCountryOfRouting(countryOfRouting)));
			}

			return result;
		}

		static CustomsReference CreateCustomsReferenceFromCountryOfRouting(CountryOfRouting countryOfRouting)
		{
			return new CustomsReference
			{
				Type = new CodeDescriptionPair { Code = NctsUxmlTypeList.Codes.CountryOfRoutingCode, Description = NctsUxmlTypeList.Descriptions.CountryOfRoutingCode, },
				SubType = ListHelper.GetWithDescription<CodeDescriptionPair35Char>(countryOfRouting.CY_Data, (CargoWise.EntityFramework.IFindBoxListProvider)countryOfRouting.Lookups.CountryList),
				Order = countryOfRouting.CY_Order,
			};
		}

		static CustomsReference CreateCustomsReferenceFromNctsEuOfficeCode(NctsEuOfficeCode officeCode)
		{
			return new CustomsReference
			{
				Type = new CodeDescriptionPair { Code = NctsUxmlTypeList.Codes.OfficeCode, Description = NctsUxmlTypeList.Descriptions.OfficeCode },
				SubType = ListHelper.GetWithDescription<CodeDescriptionPair35Char>(officeCode.CY_Code, officeCode.Lookups.CY_CodeList),
				Reference = officeCode.CY_Data,
				DateCollection = new List<Date>
				{
					new Date
					{
						Type = DateType.DateAtOffice,
						Value = officeCode.CY_Date
					}
				},
				ReferencedEntityDescription = officeCode.CY_OfficeDescription,
			};
		}

		CustomsReference CreateCustomsReferenceFromCusSupplyChainActorReference(CusSupplyChainActorReference actor)
		{
			return new CustomsReference
			{
				Type = new CodeDescriptionPair { Code = NctsUxmlTypeList.Codes.SupplyChainActor, Description = NctsUxmlTypeList.Descriptions.SupplyChainActor },
				Reference = actor.CFR_Reference,
				Owner = GetOrganisationAddress(actor.Owner, AddressTypes.Owner),
				SubType = ListHelper.GetWithDescription<CodeDescriptionPair35Char>(actor.CFR_Code, actor.Lookups.CodeList)
			};
		}

		CustomsReference CreateCustomsReferenceFromCusAuthorizationUsages(CusAuthorizationUsage authorizationUsage, ZArchitecture.Core.CodeDescriptionPairList authorizationTypeList)
		{
			return new CustomsReference()
			{
				Type = new CodeDescriptionPair() { Code = CusReferenceTypeCodes.AUT, Description = CusReferenceTypeDescriptions.AUT },
				SubType = ListHelper.GetWithDescription<CodeDescriptionPair35Char>(authorizationUsage.AGC_Code, authorizationTypeList),
				Reference = authorizationUsage.AGC_Number,
				Owner = GetOrganisationAddress(authorizationUsage.Owner?.MainAddress, AddressTypes.Owner),
			};
		}

		OrganizationAddress GetOrganisationAddress(OrgAddress address, ZString addressType)
		{
			OrganizationAddress addressDataObject = null;
			if (address != null)
			{
				addressDataObject = OrganizationAddressHelper.GetAddressDataObject(address, writeManager, addressType);
			}
			return addressDataObject;
		}

		List<Date> SetDates(NctsDepartureMovementHeader movementHeader)
		{
			var dateList = new List<Date>();
			dateList.Add(new Date() { Type = DateType.DateLimit, Value = movementHeader.BM_ExportDate });
			if (!movementHeader.BM_PresentationDateTime.IsEmpty)
			{
				dateList.Add(new Date() { Type = DateType.Presentation, Value = movementHeader.BM_PresentationDateTime.ToUtcDateTime() });
			}

			var acceptanceDate = movementHeader.BM_EntryDate;
			if (!acceptanceDate.IsEmpty)
			{
				dateList.Add(new Date() { Type = DateType.Acceptance, Value = acceptanceDate });
			}

			var mrnIssueDate = movementHeader.Header.MovementReferenceIssueDate;
			if (!mrnIssueDate.IsEmpty)
			{
				dateList.Add(new Date() { Type = DateType.Release, Value = mrnIssueDate });
			}

			return dateList;
		}

		void PopulateAddresses(NctsDepartureMovementHeader departureMovementHeader, Shipment shipment)
		{
			shipment.AddOrgAddress(writeManager, departureMovementHeader.Representative);
		}

		void PopulateAddresses(NctsDepartureMovementHeader departureMovementHeader, InBondMoveHeader moveHeader)
		{
			moveHeader.AddOrgAddress(writeManager, departureMovementHeader.Carrier);
		}

		List<TransportMeans> PopulateTransportMeansCollection(NctsDepartureMovementHeader departureMovementHeader)
		{
			var result = new List<TransportMeans>();
			var lookups = departureMovementHeader.Lookups;
			var order = 0;
			var transportMeansList = departureMovementHeader.Factory.GetCachedValue<TransportMeansList>();
			var meansOfTransportList = departureMovementHeader.Factory.GetCachedValue<ModeOfTransportList>();

			var departureModeOfTransport = GetMeansOfTransportPair(departureMovementHeader.BM_InlandTransportMode, meansOfTransportList);

			AddTransportMeans(result, lookups.TransportNationalityList, ref order, transportMeansList, departureMovementHeader.BM_TransportAtDeparture, departureMovementHeader.BM_RN_NKTransportAtDepartureCountry,
				departureMovementHeader.BM_InlandTransportMode, departureMovementHeader.BM_TransportAtDepartureType, departureModeOfTransport, departureMovementHeader.IsRoadInlandTransport,
				departureMovementHeader.Trailer1IDAtDeparture, departureMovementHeader.Trailer1NationalityAtDeparture, departureMovementHeader.Trailer2IDAtDeparture, departureMovementHeader.Trailer2NationalityAtDeparture);

			result.Add(new TransportMeans()
			{
				TransportType = TransportTypeCode.Border,
				Order = order++,
				IdentificationNumber = departureMovementHeader.BM_TOLCarrierID,
				Nationality = ListHelper.GetWithDescription<CodeDescriptionPair2Char>(departureMovementHeader.BM_RN_NKTOLCarrierNationality, lookups.TransportNationalityList),
				TypeOfIdentification = GetTransportMeansCodeDescription(departureMovementHeader.BM_ExportTransportMode, departureMovementHeader.BM_ActiveBorderIdentificationType, transportMeansList),
				ModeOfTransport = GetMeansOfTransportPair(departureMovementHeader.BM_ExportTransportMode, meansOfTransportList),
			});

			return result;
		}

		void AddTransportMeans(List<TransportMeans> result, ZZRefCusCodeListCombinedCollection transportNationalityList, ref int order, TransportMeansList transportMeansList,
			ZString transportAtDeparture, ZString transportAtDepartureCountry, ZString inlandTransportMode, ZString transportAtDepartureType,
			CodeDescriptionPair1Char departureModeOfTransport, ZBool isRoadInlandTransport,
			ZString trailer1IDAtDeparture, ZString trailer1NationalityAtDeparture,
			ZString trailer2IDAtDeparture, ZString trailer2NationalityAtDeparture)
		{
			result.Add(new TransportMeans()
			{
				TransportType = TransportTypeCode.Departure,
				Order = order++,
				IdentificationNumber = transportAtDeparture,
				Nationality = ListHelper.GetWithDescription<CodeDescriptionPair2Char>(transportAtDepartureCountry, transportNationalityList),
				TypeOfIdentification = GetTransportMeansCodeDescription(inlandTransportMode, transportAtDepartureType, transportMeansList),
				ModeOfTransport = departureModeOfTransport,
			});

			if (isRoadInlandTransport)
			{
				result.Add(new TransportMeans()
				{
					TransportType = TransportTypeCode.Departure,
					Order = order++,
					IdentificationNumber = trailer1IDAtDeparture,
					Nationality = ListHelper.GetWithDescription<CodeDescriptionPair2Char>(trailer1NationalityAtDeparture, transportNationalityList),
					TypeOfIdentification = NewTransportMeansPair(TransportMeansList.Codes.RegistrationNumberOfTheRoadTrailer, transportMeansList),
					ModeOfTransport = departureModeOfTransport,
				});

				result.Add(new TransportMeans()
				{
					TransportType = TransportTypeCode.Departure,
					Order = order++,
					IdentificationNumber = trailer2IDAtDeparture,
					Nationality = ListHelper.GetWithDescription<CodeDescriptionPair2Char>(trailer2NationalityAtDeparture, transportNationalityList),
					TypeOfIdentification = NewTransportMeansPair(TransportMeansList.Codes.RegistrationNumberOfTheRoadTrailer, transportMeansList),
					ModeOfTransport = departureModeOfTransport,
				});
			}
		}

		static CodeDescriptionPair2Char GetTransportMeansCodeDescription(ZString transportMode, ZString idType, TransportMeansList transportMeansList) => NewTransportMeansPair(Utilities.TransportMeansMapping.FirstOrDefault(item => item.transportMode == transportMode && item.idType == idType).transportMeansCode, transportMeansList);

		static CodeDescriptionPair2Char NewTransportMeansPair(ZString code, TransportMeansList transportMeansList) => new CodeDescriptionPair2Char { Code = code, Description = transportMeansList.GetDescriptionFromCode(code) };

		static CodeDescriptionPair1Char GetMeansOfTransportPair(ZString code, ModeOfTransportList meansOfTransportList) => new CodeDescriptionPair1Char() { Code = code, Description = meansOfTransportList.GetDescriptionFromCode(code) };

		List<AddInfo> PopulateAddInfosData(NctsDepartureMovementHeader departureMovementHeader)
		{
			var addInfoCollection = new List<AddInfo>();
			addInfoCollection.AddAddInfo(DataObjectWriterConstants.DepartureMovementHeader.AddInfo.IsSimplifiedProcedure, departureMovementHeader.IsSimplifiedNctsProcedure, addEmpty: true);
			addInfoCollection.AddAddInfo(DataObjectWriterConstants.DepartureMovementHeader.AddInfo.ReducedDateSetIndicator, departureMovementHeader.BM_ReducedDatasetIndicator, addEmpty: true);
			addInfoCollection.AddAddInfo(DataObjectWriterConstants.DepartureMovementHeader.AddInfo.SecurityIndicator, departureMovementHeader.BM_TypeOfSecurity, addEmpty: true);
			addInfoCollection.AddAddInfo(DataObjectWriterConstants.DepartureMovementHeader.AddInfo.SpecificCircumstance, departureMovementHeader.BM_SpecificCircumstance, addEmpty: true);
			addInfoCollection.AddAddInfo(DataObjectWriterConstants.DepartureMovementHeader.AddInfo.BM_ConveyanceNumber, departureMovementHeader.BM_ConveyanceNumber, addEmpty: true);
			addInfoCollection.AddAddInfo(DataObjectWriterConstants.DepartureMovementHeader.AddInfo.BM_CustomsOfficeAtBorder, departureMovementHeader.BM_CustomsOfficeAtBorder, addEmpty: true);
			addInfoCollection.AddAddInfo(DataObjectWriterConstants.DepartureMovementHeader.AddInfo.TirCarnetNumber, departureMovementHeader.TirCarnetNumber, addEmpty: true);

			return addInfoCollection;
		}

		List<AdditionalBill> PopulateAdditionalBillCollection(INctsBillCollection<NctsBill> bills)
		{
			return bills.Select(bill => MapBillToAdditionalBill(bill)).ToList();
		}

		AdditionalBill MapBillToAdditionalBill(NctsBill bill)
		{
			var addBill = new AdditionalBill(writeManager.WriterStrategy)
			{
				BillNumber = bill.B0_ReferenceID,
			};

			var linePriceCurrency = bill.LinePriceCurrency;
			if (linePriceCurrency != null)
			{
				addBill.LinePriceCurrency = new CodeDescriptionPair
				{
					Code = linePriceCurrency.Code, Description = linePriceCurrency.RX_Desc
				};
			}

			var jobDocAddressDataObjectWriter = new JobDocAddressDataObjectWriter(writeManager);
			var organizationAddresses = new List<OrganizationAddress>();
			AddIfNotNull(bill.Consignor, DocAddressType.ConsignorDocumentaryAddress);
			AddIfNotNull(bill.Consignee, DocAddressType.ConsigneeDocumentaryAddress);
			addBill.SetOrganizationAddressCollection(() => organizationAddresses);

			addBill.SetCustomsSupportingInformationCollection(() => CustomsSupportingInformationCollectionCreator.CreateCollection(helper, bill, writeManager, ZString.Empty, ((Integration.Customs.ICusSupportingInfoTypeSupporter)bill).GetCusSupportingInfoTypes().Keys.ToArray()));
			addBill.SetCustomsReferenceCollection(() => bill.CusSupplyChainActorReferences.Select(x => CreateCustomsReferenceFromCusSupplyChainActorReference(x)).ToList());

			return addBill;

			void AddIfNotNull(JobDocAddress jobDocAddress, DocAddressType docAddressType)
			{
				var organizationAddress = jobDocAddressDataObjectWriter.GetDataObject(jobDocAddress);
				if (organizationAddress != null)
				{
					organizationAddress.AddressType = docAddressType.ToString();
					organizationAddresses.Add(organizationAddress);
				}
			}
		}

		List<InBondMoveDetail> PopulateInBondMoveDetailCollection(INctsBillCollection<NctsBill> bills)
		{
			return bills.Select(bill => MapBillToInBondMoveDetail(bill)).ToList();
		}

		InBondMoveDetail MapBillToInBondMoveDetail(NctsBill bill)
		{
			var detail = new InBondMoveDetail(writeManager.WriterStrategy)
			{
				Sequence = bill.SequenceNumber,
				TransportPaymentMethod = new CodeDescriptionPair
				{
					Code = bill.B0_TransportPaymentMethod,
					Description = bill.Lookups.TransportPaymentMethodList.GetDescriptionFromCode(bill.B0_TransportPaymentMethod),
				},
				DispatchCountry = new CodeDescriptionPair2Char
				{
					Code = bill.B0_RN_NKCountryOfExport,
					Description = bill.Lookups.CountryList.GetDescriptionFromCode(bill.B0_RN_NKCountryOfExport),
				},
				DestinationCountry = new CodeDescriptionPair2Char
				{
					Code = bill.B0_RN_NKCountryOfDestination,
					Description = bill.Lookups.CountryList.GetDescriptionFromCode(bill.B0_RN_NKCountryOfDestination),
				},
				Weight = bill.B0_Weight,
				WeightUnit = new UnitOfWeight
				{
					Code = bill.B0_WeightUQ,
					Description = Weight.GetDescription(bill.B0_WeightUQ, PluralState.Plural)
				},
			};
			detail.SetTransportMeansCollection(() => PopulateInBondMoveDetailTransportMeansCollection(bill));
			detail.SetInBondMoveLineItemCollection(() => ProcessCollection(bill.GoodsItems, new Phase5GoodsItemDataObjectWriter(writeManager, helper)));
			return detail;
		}

		List<TransportMeans> PopulateInBondMoveDetailTransportMeansCollection(NctsBill bill)
		{
			var result = new List<TransportMeans>();
			var lookups = bill.Lookups;
			var order = 0;
			var transportMeansList = bill.Factory.GetCachedValue<TransportMeansList>();
			var meansOfTransportList = bill.Factory.GetCachedValue<ModeOfTransportList>();
			var departureMovementHeader = bill.Header.MovementHeader;
			var departureModeOfTransport = GetMeansOfTransportPair(departureMovementHeader.BM_InlandTransportMode, meansOfTransportList);

			AddTransportMeans(result, lookups.TransportNationalityList, ref order, transportMeansList, bill.TransportAtDeparture, bill.TransportCountryAtDeparture,
				departureMovementHeader.BM_InlandTransportMode, bill.TransportTypeAtDeparture, departureModeOfTransport, departureMovementHeader.IsRoadInlandTransport,
				bill.Trailer1IDAtDeparture, bill.Trailer1NationalityAtDeparture, bill.Trailer2IDAtDeparture, bill.Trailer2NationalityAtDeparture);

			return result;
		}

		DataObjectList<PackingLine> SetPackingLines(NctsHeader headerBO)
		{
			var packageLines = headerBO.Bills
				.SelectMany(bill => bill.GoodsItems)
				.SelectMany(goodsItem => goodsItem.Packages)
				.SelectMany(package => package.ContainersSelected.Select(container => new
				{
					ContainerNumber = container,
					Package = package
				}))
				.GroupBy(item => new
				{
					item.ContainerNumber,
					MarksAndNos = item.Package.B5_MarksAndNumbers,
					PackType = item.Package.B5_UnitType
				})
				.Select(group => CreatePackingLine(group.Key.MarksAndNos, group.Key.ContainerNumber, group.Key.PackType, group.Select(g => g.Package).ToArray()))
				.ToArray();
			return new DataObjectList<PackingLine>(packageLines);
		}

		PackingLine CreatePackingLine(ZString marksAndNos, ZString containerNumber, ZString packType, NctsPackage[] packages)
		{
			var result = new PackingLine(writeManager.WriterStrategy)
			{
				MarksAndNos = marksAndNos,
				PackQty = packages.Sum(p => p.B5_UnitCount),
				PackType = new PackageType { Code = packType },
				ContainerNumber = containerNumber
			};
			var packedItems = packages.Select(p => new PackedItem
			{
				InBondMoveLineItemLink = helper.GetGoodsItemLink(p.Parent),
				PackedQuantity = (ZDecimal)p.B5_UnitCount
			});
			result.SetPackedItemCollection(() => packedItems.ToList());
			return result;
		}
	}
}
