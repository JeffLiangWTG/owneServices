#if NETFRAMEWORK
using CargoWise.Common;
#endif
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using CusEntryHeader = Enterprise.Customs.FR.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public class TemporaryStorageWrapperHeader : AutoTemporaryStorageWrapperHeader
	{
		public TemporaryStorageWrapperHeader()
		{
			TemporaryStorageLines = new TemporaryStorageWrapperLineCollection(Factory);
			TemporaryStorageContainers = new TemporaryStorageWrapperContainerCollection();
			SupportingDocuments = new List<CusSupportingInfo>();
		}

		[BusinessObjectTestExclude]
		public TemporaryStorageWrapperLineCollection TemporaryStorageLines { get; set; }

		[BusinessObjectTestExclude]
		public TemporaryStorageWrapperContainerCollection TemporaryStorageContainers { get; set; }

		[BusinessObjectTestExclude]
		public List<CusSupportingInfo> SupportingDocuments { get; } = new List<CusSupportingInfo>();

		public static TemporaryStorageWrapperHeader NewFromShipment(ForwardingShipment shipment)
		{
			var storageHeader = new TemporaryStorageWrapperHeader()
			{
				CustomerPK = shipment.ConsigneeDocumentaryAddress.OrganisationPK,
				TransportID = shipment.JS_TransportMode == Core.Constants.TransportModes.Air ? (shipment.DepartureConsol?.Transports.DepartureTransport?.JW_VoyageFlightForBinding ?? ZString.Empty) :
									shipment.JS_TransportMode == Core.Constants.TransportModes.Sea ? (shipment.DepartureConsol?.Transports.DepartureTransport?.JW_VesselForBinding ?? ZString.Empty) :
									ZString.Empty,
				TransportMode = shipment.JS_TransportMode,
				PlaceOfLoading = shipment.ConsignorPickupAddress.Organisation?.OH_RL_NKClosestPort ?? ZString.Empty,
				DepartureDate = shipment.DepartureConsol?.Transports.DepartureTransport?.JW_ATDForBinding ?? ZDateTime.Empty,
				ArrivalDate = shipment.JS_E_ARV,
				JobNumber = shipment.JobNumber,
				CustomsProfile = GetCustomsProfileFromPresenter(shipment),
				PresenterPK = GetPresenter(shipment),
				Vessel = shipment.Consols?.Cast<ForwardingConsol>().FirstOrDefault()?.MostInterestingTransportForBinding[0]?.RefVessels[0].RV_Code ?? ZString.Empty,
				ContainerCount = shipment.AllMasterConsols.SelectMany(x => x.Containers).Cast<ForwardingContainer>().Select(x => x.JC_ContainerNum).Distinct().Count()
			};
			NewLinesFromShipment(shipment).ToList().ForEach(storageHeader.TemporaryStorageLines.Add);
			NewContainersFromShipment(shipment).ToList().ForEach(storageHeader.TemporaryStorageContainers.Add);
			return storageHeader;
		}

		#region New npbo from Declaration

		public static TemporaryStorageWrapperHeader NewFromDeclaration(JobDeclaration dec)
		{
			return new TemporaryStorageWrapperHeader()
			{
				CustomerPK = dec.ImporterDocumentaryAddress == null ? ZGuid.Empty : dec.ImporterDocumentaryAddress.OrganisationPK,
				TransportID = dec.JE_TransportMode == Core.Constants.TransportModes.Air ? dec.JE_VoyageFlightNo : dec.JE_TransportMode == Core.Constants.TransportModes.Sea ? dec.JE_VesselName : ZString.Empty,
				TransportMode = dec.TransportMode,
				PlaceOfLoading = dec.JE_RL_NKPortOfLoading,
				DepartureDate = dec.JE_ExportDate,
				ArrivalDate = dec.JE_DateOfArrival,
				JobNumber = dec.JobNumber,
				TemporaryStorageContainers = NewContainersFromDeclaration(dec),
				CustomsProfile = GetCustomsProfileFromPresenter(dec),
				PresenterPK = dec.DeclarantAddress == null ? ZGuid.Empty : dec.DeclarantAddress.PK,
				PreviousEntryType = dec.JE_EntryStyle,
				PreviousEntryNumber = dec.DeclarationNumber,
				CustomsOfficeOfDestination = dec.CustomsOffices.Any() ? dec.OfficeOfDeclaration : ZString.Empty,
				CustomsOfficeOfEntry = dec.CustomsOffices.Any() ? dec.CustomsOffices.Cast<EuOfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent)?.CY_Data ?? ZString.Empty : ZString.Empty,
				TransportMeansDescription = dec.JE_VoyageFlightNo,
				ContainerCount = dec.CustomsOffices.Any() ? dec.CusContainers.Count : (int)ZInt.Zero,
				GuaranteePK = dec.CustomsGuarantee?.PK ?? ZGuid.Empty,
				TemporaryStorageLines = NewLinesFromDeclaration(dec)
			}.CloneSupportingDocumentsFromDeclaration(dec);
		}

		static ZString GetCustomsProfileFromPresenter(JobDeclaration dec)
		{
			bool isImportIntoCurrentCountry = dec.JE_RL_NKFinalDestination.StartsWith(dec.CountryCode) &&
											  !dec.JE_RL_NKOrigin.StartsWith(dec.CountryCode);
			var result = ZString.Empty;
			var presenter = isImportIntoCurrentCountry ? dec.Importer?.MainAddress.PK ?? ZGuid.Empty : dec.Exporter?.MainAddress.PK ?? ZGuid.Empty;
			if (!presenter.IsEmpty)
			{
				var headers = CusAuthorisationHeader.Loader.GetAuthorisations(dec.Factory, dec.CountryCode, new ZString[] { CusAuthorizationHeaderTypeList.Codes.TemporaryStorage }, ZDateTime.Today, presenter);
				if (headers != null)
				{
					result = headers.FirstOrDefault(y => y.CusAuthorisationRules.Any(rule => rule.CPR_RuleCode == CusAuthorisationRuleTypeList.Codes.USE && rule.CPR_ValueFrom == AuthorizationRuleUseValueFromList.Codes.IST))?.CPH_Number ?? ZString.Empty;
				}
			}
			return result;
		}

		static TemporaryStorageWrapperLineCollection NewLinesFromDeclaration(JobDeclaration dec)
		{
			var ownerReferenceType = GetOwnerReferenceTypeBasedOnTransportMode(dec);
			var ownerReferenceNumber = GetOwnerReferenceNumberBasedOnTransportMode(dec);
			var factory = dec.Factory;
			var lines = new TemporaryStorageWrapperLineCollection(factory);
			var (cphCurrencyCode, guranteeSumInCPH, customsValueSum) = (ZString.Empty, ZDecimal.Zero, ZDecimal.Zero);
			if (dec.InvoiceLines.Any())
			{
				(cphCurrencyCode, guranteeSumInCPH, customsValueSum) = GetGuranteedValues(dec);
			}
			foreach (JobComInvoiceLine line in dec.InvoiceLines)
			{
				lines.Add(new TemporaryStorageWrapperLine
				{
					OwnerReferenceType = ownerReferenceType,
					OwnerReferenceNumber = ownerReferenceNumber,
					GrossWeight = line.JI_Weight,
					GrossWeightUQ = line.JI_WeightUQ,
					PackageQty = dec.JE_TotalNoOfPacks,
					PackageType = line.SelectedPackType,
					LocationOfGoods = dec.JE_LocationOfGoods.SubstringSafe(0, TemporaryStorageWrapperLine.Schema.LocationOfGoods.Length),
					GoodsDescription = line.JI_Description.SubstringSafe(0, TemporaryStorageWrapperLine.Schema.GoodsDescriptionMaxLength),
					TemporaryStorageFurtherDetails = NewFurtherDetailsFromLine(factory, line, cphCurrencyCode, guranteeSumInCPH, customsValueSum)
				});
			}
			return lines;
		}

		static ZString GetOwnerReferenceNumberBasedOnTransportMode(JobDeclaration dec)
		{
			var ownerReferenceNumber = string.Empty;

			switch (dec.JE_TransportMode)
			{
				case Core.Constants.TransportModes.Air:
					ownerReferenceNumber = dec.JE_MasterBill;
					break;
				case Core.Constants.TransportModes.Sea:
					ownerReferenceNumber = dec.JE_MasterBill;
					break;
				case Core.Constants.TransportModes.Road:
					ownerReferenceNumber = dec.JE_VesselName;
					break;
			}
			return ownerReferenceNumber;
		}

		static ZString GetOwnerReferenceTypeBasedOnTransportMode(JobDeclaration dec)
		{
			var ownerReferenceType = string.Empty;

			switch (dec.JE_TransportMode)
			{
				case Core.Constants.TransportModes.Air:
					ownerReferenceType = OwnerReferenceTypeList.Codes.ORT_AWB;
					break;
				case Core.Constants.TransportModes.Sea:
					ownerReferenceType = OwnerReferenceTypeList.Codes.ORT_MBL;
					break;
				case Core.Constants.TransportModes.Road:
					ownerReferenceType = OwnerReferenceTypeList.Codes.ORT_TRK;
					break;
			}
			return ownerReferenceType;
		}

		static TemporaryStorageWrapperFurtherDetailCollection NewFurtherDetailsFromLine(BusinessObjectFactory factory, JobComInvoiceLine line, ZString cphCurrencyCode, ZDecimal guranteeSumInCPH, ZDecimal customsValueSum)
		{
			var guaranteedValueInCPH = (guranteeSumInCPH == 0 || customsValueSum == 0 || !line.HasGuaranteeConsumingProcedure) ? 0 : line.JI_CustomsValue / customsValueSum * guranteeSumInCPH;
			var furtherDetails = new TemporaryStorageWrapperFurtherDetailCollection(factory);
			furtherDetails.Add(new TemporaryStorageWrapperFurtherDetail
			{
				CommodityCode = line.JI_FormattedTariff,
				OriginCountry = line.JI_CountryOfOrigin,
				NetMass = line.JI_NetWeight,
				NetMassUQ = line.JI_NetWeightUQ,
				GoodsValue = line.JI_LinePrice,
				GuaranteedValue = guaranteedValueInCPH,
				Currency = cphCurrencyCode
			});

			return furtherDetails;
		}

		static (ZString cphCurrencyCode, ZDecimal guranteeSumInCPH, ZDecimal customsValueSum) GetGuranteedValues(JobDeclaration dec)
		{
			if (dec.CustomsGuarantee == null)
			{
				return (ZString.Empty, ZDecimal.Zero, ZDecimal.Zero);
			}
			var decCurrencyCode = dec.LocalCurrencyCode;
			var cphCurrencyCode = dec.CustomsGuarantee.CPH_UnitOfMeasure;
			var guranteeSumInDec = dec.ActiveEntryHeaders.Cast<CusEntryHeader>().Sum(entry => entry.AmountAndTypeToBeGuaranteeds.Sum(g => g.AmountInDeclarationCurrency));
			var guranteeSumInCPH = guranteeSumInDec;
			if (!ZString.Equals(cphCurrencyCode, decCurrencyCode))
			{
				var currencyConverter = new CurrencyConverterWithDataProvider(dec.Factory, dec);
				var guranteeSumMoneyInDec = new Money(guranteeSumInDec, RefCurrency.LoadFromCurrencyCode(dec.Factory, decCurrencyCode));
				var guranteeSumMoneyInCPH = currencyConverter.ConvertExact(guranteeSumMoneyInDec, RefCurrency.LoadFromCurrencyCode(dec.Factory, cphCurrencyCode));
				guranteeSumInCPH = guranteeSumMoneyInCPH.Amount;
			}
			var customsValueSum = dec.InvoiceLines.Cast<JobComInvoiceLine>().Sum(l => l.HasGuaranteeConsumingProcedure ? l.JI_CustomsValue : 0);
			return (cphCurrencyCode, guranteeSumInCPH, customsValueSum);
		}
		#endregion

		public static TemporaryStorageWrapperHeader NewFromDeltaT(NctsHeader deltaT)
		{
			var result = new TemporaryStorageWrapperHeader()
			{
				CustomerPK = deltaT.DestinationTrader.OrganisationPK,
				TransportID = deltaT.UnloadedMeansOfTransportAtDepartureIdentity,
				ArrivalDate = deltaT.Messages.Cast<EDIMessage>().FirstOrDefault(x => x.EM_MessageType == "007")?.EM_MessageDateTime ?? ZDateTime.Empty,
				JobNumber = deltaT.JobNumber,
				PreviousEntryType = PreviousDocumentCodeList.Codes._821,
				PreviousEntryNumber = deltaT.ArrivalMrnFromUser,
				CustomsOfficeOfDestination = deltaT.DestinationCustomsOfficeCodeForArrival,
				PresenterPK = deltaT.Declarant.E2_OA_Address,
				CustomsOfficeOfEntry = GetCustomsOfficeOfEntry(deltaT),
				PresentationDate = deltaT.UnloadingRemark.G9_UnloadingDate,
				CustomsProfile = GetCustomsProfileFromPresenter(deltaT),
			}.CloneSupportingDocumentsFromFromDeltaT(deltaT);

			NewLinesFromDeltaT(deltaT)?.ToList().ForEach(result.TemporaryStorageLines.Add);
			NewContainersFromDeltaT(deltaT)?.ToList().ForEach(result.TemporaryStorageContainers.Add);

			if (deltaT.IsDepartureMovement)
			{
				result.TransportMode = deltaT.MovementHeader.BM_InlandTransportMode;
				result.BorderTransportInfo = deltaT.MovementHeader.BM_TransportAtDeparture;
				result.TransportRegNo = deltaT.MovementHeader.BM_TransportAtDeparture.Left(Schema.TransportRegNoMaxLength);
				result.PlaceOfLoading = deltaT.MovementHeader.BM_RL_NKForeignDestPort;
				result.DepartureDate = deltaT.Messages.Cast<EDIMessage>().OrderByDescending(x => x.EM_MessageDateTime).FirstOrDefault(x => x.EM_MessageType == "029")?.EM_MessageDateTime ?? ZDateTime.Empty;
				result.ContainerCount = deltaT.DepartureHeaderContainers.Cast<EU.NCTS.Business.NctsDepartureHeaderContainer>().Select(x => x.BC_ContainerNum).Distinct().Count();
				result.GuaranteePK = deltaT.Guarantees.Cast<FRNctsGuarantee>().FirstOrDefault()?.CusGuarantee?.PK ?? ZGuid.Empty;
			}
			else
			{
				result.BorderTransportInfo = deltaT.UnloadedMeansOfTransportAtDepartureIdentity;
				result.TransportRegNo = deltaT.UnloadedMeansOfTransportAtDepartureIdentity;
				result.ContainerCount = deltaT.UnloadingMovementHeader?.GoodsItems.Cast<EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc>().SelectMany(x => x.Containers).Select(x => x.BC_ContainerNum).Distinct().Count() ?? ZInt.Zero;
			}

			return result;
		}

		static ZString GetCustomsOfficeOfEntry(NctsHeader deltaT)
		{
			var result = ZString.Empty;
			var customsOffices = deltaT.IsPhase5 ? deltaT.CommonMovementHeader.CustomsOffices.OfType<NctsFrOfficeCode>() : deltaT.CustomsOffices.OfType<NctsFrOfficeCode>();
			var departure = customsOffices.FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);

			if (departure != null)
			{
				if (!new EuropeanUnionCustomsMembersProvider().IsMemberOfEU(departure.CY_Data.Left(2)))
				{
					result = customsOffices.FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit)?.CY_Data ?? ZString.Empty;
				}
			}

			return result;
		}

		static ZString GetCustomsProfileFromPresenter(ForwardingShipment shipment)
		{
			var result = ZString.Empty;
			var presenter = GetPresenter(shipment);
			if (!presenter.IsEmpty)
			{
				var countryCode = (shipment.GetDeclaration() as JobDeclaration)?.CountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				var headers = CusAuthorisationHeader.Loader.GetAuthorisations(shipment.Factory, countryCode, new ZString[] { CusAuthorizationHeaderTypeList.Codes.TemporaryStorage }, ZDateTime.Today, presenter);
				if (headers != null)
				{
					result = headers.FirstOrDefault(y => y.CusAuthorisationRules.Any(rule => rule.CPR_RuleCode == CusAuthorisationRuleTypeList.Codes.USE && rule.CPR_ValueFrom == AuthorizationRuleUseValueFromList.Codes.IST))?.CPH_Number ?? ZString.Empty;
				}
			}
			return result;
		}

		static ZGuid GetPresenter(ForwardingShipment shipment)
		{
			var countryCode = (shipment.GetDeclaration() as JobDeclaration)?.CountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var isImportIntoCompanyCountry = shipment.JS_RL_NKDestination.StartsWith(countryCode) && !shipment.JS_RL_NKOrigin.StartsWith(countryCode);
			var presenter = isImportIntoCompanyCountry ? shipment.ImportBroker?.MainAddress.PK ?? ZGuid.Empty : shipment.ExportBroker?.MainAddress.PK ?? ZGuid.Empty;
			return presenter;
		}

		static TemporaryStorageWrapperLineCollection NewLinesFromShipment(ForwardingShipment shipment)
		{
			var tempStorageLineCollection = new TemporaryStorageWrapperLineCollection(shipment.Factory);
			var ownerRefType = ZString.Empty;
			var ownerRefNumber = ZString.Empty;
			foreach (ForwardingConsol consol in shipment.Consols)
			{
				foreach (ForwardingPackLine packLine in shipment.OuterPackLines)
				{
					switch (shipment.JS_TransportMode)
					{
						case Core.Constants.TransportModes.Air:
							ownerRefType = OwnerReferenceTypeList.Codes.ORT_AWB;
							ownerRefNumber = consol.MasterBillAirlinePrefix + consol.MasterBillMAWB;
							break;
						case Core.Constants.TransportModes.Sea:
							ownerRefType = OwnerReferenceTypeList.Codes.ORT_MBL;
							ownerRefNumber = consol.JK_MasterBillNum;
							break;
						case Core.Constants.TransportModes.Road:
							ownerRefType = OwnerReferenceTypeList.Codes.ORT_TRK;
							ownerRefNumber = shipment.JS_HouseBill;
							break;
					}
					var tempStorageLine = new TemporaryStorageWrapperLine
					{
						OwnerReferenceType = ownerRefType,
						OwnerReferenceNumber = ownerRefNumber,
						GrossWeight = shipment.JS_ActualWeight,
						GrossWeightUQ = shipment.JS_UnitOfWeight,
						PackageQty = shipment.JS_OuterPacks,
						PackageType = packLine.JL_F3_NKPackType,
						GoodsDescription = packLine.JL_Description.Left(TemporaryStorageWrapperLine.Schema.GoodsDescriptionMaxLength)
					};
					var tempStorageFurtherDetailCollection = new TemporaryStorageWrapperFurtherDetailCollection(shipment.Factory);
					var tempStorageFurtherDetail = new TemporaryStorageWrapperFurtherDetail
					{
						CommodityCode = packLine.JL_HarmonisedCode,
						OriginCountry = packLine.JL_RN_NKOrigin,
						NetMass = packLine.JL_OutturnedWeight.IsEmpty ? packLine.JL_ActualWeight : packLine.JL_OutturnedWeight,
						NetMassUQ = packLine.JL_OutturnedWeight.IsEmpty ? packLine.JL_ActualWeightUQ : packLine.PackLineWeightUnit,
						GoodsValue = packLine.JL_LinePrice
					};
					tempStorageFurtherDetailCollection.Add(tempStorageFurtherDetail);
					tempStorageLine.TemporaryStorageFurtherDetails = tempStorageFurtherDetailCollection;

					tempStorageLineCollection.Add(tempStorageLine);
				}
			}
			return tempStorageLineCollection;
		}

		static ZString GetCustomsProfileFromPresenter(NctsHeader deltaT)
		{
			var result = ZString.Empty;
			if (!deltaT.Declarant.OrganisationPK.IsEmpty)
			{
				var headers = CusAuthorisationHeader.Loader.GetAuthorisations(deltaT.Factory, deltaT.CountryCode, new ZString[] { CusAuthorizationHeaderTypeList.Codes.TemporaryStorage }, ZDateTime.Today, deltaT.Declarant.OrganisationPK);
				if (headers != null)
				{
					result = headers.FirstOrDefault(y => y.CusAuthorisationRules.Any(rule => rule.CPR_RuleCode == CusAuthorisationRuleTypeList.Codes.USE && rule.CPR_ValueFrom == AuthorizationRuleUseValueFromList.Codes.IST))?.CPH_Number ?? ZString.Empty;
				}
			}
			return result;
		}

		static TemporaryStorageWrapperContainerCollection NewContainersFromShipment(ForwardingShipment shipment)
		{
			var containers = new TemporaryStorageWrapperContainerCollection();
			foreach (ForwardingContainer container in shipment.AllMasterConsols.SelectMany(x => x.Containers))
			{
				containers.Add(new TemporaryStorageWrapperContainer
				{
					ContainerType = container.RefContainer?.RC_Code ?? ZString.Empty,
					ContainerNumber = container.JC_ContainerNum
				});
			}
			return containers;
		}

		static TemporaryStorageWrapperContainerCollection NewContainersFromDeclaration(JobDeclaration dec)
		{
			var containers = new TemporaryStorageWrapperContainerCollection();
			foreach (EU.Business.Declaration.CusContainer container in dec.CusContainers)
			{
				containers.Add(new TemporaryStorageWrapperContainer
				{
					ContainerType = container.JobContainer?.RefContainer?.RC_Code ?? ZString.Empty,
					ContainerNumber = container.JobContainer?.JC_ContainerNum ?? ZString.Empty
				});
			}
			return containers;
		}
		static TemporaryStorageWrapperContainerCollection NewContainersFromDeltaT(NctsHeader deltaT)
		{
			var containers = new TemporaryStorageWrapperContainerCollection();
			if (deltaT.IsDepartureAndArrivalMovement)
			{
				foreach (EU.NCTS.Business.NctsDepartureHeaderContainer container in deltaT.DepartureHeaderContainers)
				{
					containers.Add(new TemporaryStorageWrapperContainer
					{
						ContainerType = "N/A",
						ContainerNumber = container.BC_ContainerNum
					});
				}
			}
			return containers;
		}

		TemporaryStorageWrapperHeader CloneSupportingDocumentsFromDeclaration(JobDeclaration declaration)
		{
			var invoiceHeaders = declaration.Invoices.Cast<JobComInvoiceHeader>().ToList();
			var decDocs = declaration.SupportingDocuments.Cast<SupportingDocument>()
				.Union(invoiceHeaders.SelectMany(inv => inv.EffectiveSupportingDocumentsForLine()))
				.Union(invoiceHeaders.SelectMany(inv => inv.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(invLine => invLine.EffectiveSupportingDocuments())))
				.DistinctBy(doc => doc.PK);
			SupportingDocuments.AddRange(decDocs);
			return this;
		}

		TemporaryStorageWrapperHeader CloneSupportingDocumentsFromFromDeltaT(NctsHeader nctsHeader)
		{
			var nctsDocs = nctsHeader.MovementHeaders.Cast<EU.NCTS.Business.NctsCommonMovementHeader>()
				.SelectMany(movement => movement.GoodsItems).Cast<EU.NCTS.Business.NctsCommonCargoDesc>()
				.SelectMany(cargo => cargo.SupportingDocuments.Cast<EU.NCTS.Business.NctsSupportingDocument>());
			SupportingDocuments.AddRange(nctsDocs);
			return this;
		}

		static TemporaryStorageWrapperLineCollection NewLinesFromDeltaT(NctsHeader nctsHeader)
		{
			var lines = new TemporaryStorageWrapperLineCollection(nctsHeader.Factory);

			if (nctsHeader.IsDepartureMovement)
			{
				foreach (NctsDepartureCargoDesc goodsItem in nctsHeader.MovementHeader.GoodsItems)
				{
					var line = new TemporaryStorageWrapperLine
					{
						LocationOfGoods = nctsHeader.MovementHeader.BM_LocationOfGoods,
						GrossWeight = goodsItem.BY_GrossWeight,
						GrossWeightUQ = goodsItem.BY_GrossWeightUnit,
						PackageQty = (ZInt)goodsItem.Packages.Cast<NctsPackage>().Sum(x => x.B5_UnitCount),
						GoodsDescription = goodsItem.BY_Description.Left(TemporaryStorageWrapperLine.Schema.GoodsDescriptionMaxLength),
						PackageType = goodsItem.Packages.Cast<NctsPackage>().FirstOrDefault()?.B5_UnitType ?? ZString.Empty,
						TemporaryStorageFurtherDetails = NewFurtherDetailsFromDeltaT(goodsItem)
					};

					lines.Add(line);
				}
			}
			else
			{
				foreach (EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc goodsItem in nctsHeader.ArrivalMovementHeader.GoodsItems)
				{
					var line = new TemporaryStorageWrapperLine
					{
						LocationOfGoods = nctsHeader.ArrivalMovementHeader.BM_LocationOfGoods,
					};

					lines.Add(line);
				}
			}

			return lines;
		}

		static TemporaryStorageWrapperFurtherDetailCollection NewFurtherDetailsFromDeltaT(NctsDepartureCargoDesc goodsItem)
		{
			var storageFuthers = new TemporaryStorageWrapperFurtherDetailCollection(goodsItem.Factory);
			if (goodsItem.Header.IsDepartureMovement)
			{
				var storageFuther = new TemporaryStorageWrapperFurtherDetail
				{
					CommodityCode = goodsItem.BY_HarmonisedTariff.Left(35),
					NetMass = goodsItem.BY_NetWeight,
					NetMassUQ = goodsItem.BY_NetWeightUnit,
					GoodsValue = goodsItem.BY_MonetaryValue,
					GuaranteedValue = goodsItem.Header.MovementHeader.GoodsItems.Count == 0 ? ZDecimal.Zero : (ZDecimal)(goodsItem.Header.Guarantees.Cast<FRNctsGuarantee>().Sum(x => x.PW_BondAmount) / goodsItem.Header.MovementHeader.GoodsItems.Count),
					Currency = Enterprise.Core.Constants.CurrencyCodes.EuropeanUnion
				};
				storageFuthers.Add(storageFuther);
			}

			return storageFuthers;
		}
	}
}
