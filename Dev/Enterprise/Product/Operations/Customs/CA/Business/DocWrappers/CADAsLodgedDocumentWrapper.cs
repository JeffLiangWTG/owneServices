using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CA.MessageDefinitions.CAD.Outbound;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	class CADAsLodgedDocumentWrapper : ICADHeader
	{
		internal CADAsLodgedDocumentWrapper(CADMessage cadMessage)
		{
			CargoWise.Common.Argument.NotNull(cadMessage, "cadMessage");
			this.factory = cadMessage.Factory;
			this.documentMetaData = XmlObjectSerializer.Deserialize<DocumentMetaData>(cadMessage.EM_MessageText);
			if (this.documentMetaData == null)
			{
				throw new ArgumentException("Syntax error in CAD response message.", nameof(cadMessage));
			}
			this.declaration = documentMetaData.Response.Declaration;
			if (this.declaration == null)
			{
				throw new ArgumentException("The declaration of CAD message should not be null.", nameof(cadMessage));
			}
			this.jobDeclaration = (cadMessage.EM_LinkedObject as CusEntryHeader)?.Declaration;
			CaculateDeclarationTotals();
		}

		readonly BusinessObjectFactory factory;
		readonly DocumentMetaData documentMetaData;
		readonly Declaration declaration;
		readonly JobDeclaration jobDeclaration;

		ZString ICADHeader.TypeCode_Box1 => GetMessageSegmentValue<ZString>(() => declaration.TypeCode.Value);

		ZString ICADHeader.WsSType_Box2 => ZString.Empty;

		ZDateTime ICADHeader.AccountingDate_Box3 => AcceptanceDateTime;

		ZDateTime AcceptanceDateTime
		{
			get
			{
				if (!acceptanceDateTime.HasValue)
				{
					var result = ZDateTime.Empty;
					var dateTimeString = GetMessageSegmentValue<ZString>(() => declaration.AcceptanceDateTime.DateTimeString);
					if (!dateTimeString.IsEmpty)
					{
						_ = ZDateTime.TryParseExact(dateTimeString, out result, "yyyyMMddHHmmss");
					}
					acceptanceDateTime = result;
				}
				return acceptanceDateTime.Value;
			}
		}
		ZDateTime? acceptanceDateTime;

		ZString ICADHeader.AccountSecurityCode_Box4 => GetMessageSegmentValue<ZString>(() => declaration.Id.Value).SubstringSafe(0, 5);

		ZString ICADHeader.CADTransactionNo_Box4 => GetMessageSegmentValue<ZString>(() => declaration.Id.Value).SubstringSafe(5);

		ZString ICADHeader.OfficeNo_Box5 => GetMessageSegmentValue<ZString>(() => declaration.ReleaseLocation.Id.Value);

		ZString ICADHeader.ModeOfTransport_Box6 => GetMessageSegmentValue<ZString>(() => declaration.BorderTransportMeans.ModeCode.Value);

		ZDateTime ICADHeader.ReleaseDate_Box7
		{
			get
			{
				var result = ZDateTime.Empty;
				var dateTimeString = GetMessageSegmentValue<ZString>(() => declaration.Status.ReleaseDateTime.DateTimeString);
				if (!dateTimeString.IsEmpty)
				{
					_ = ZDateTime.TryParseExact(dateTimeString, out result, "yyyyMMdd");
				}
				return result;
			}
		}

		ZDecimal ICADHeader.GrossWeightKg_Box8 => GetMessageSegmentValue<ZDecimal>(() => declaration.TotalGrossMassMeasure.Value);

		ZString ICADHeader.CarrierCodeAtImportation_Box9 => GetMessageSegmentValue<ZString>(() => declaration.Carrier.Id.Value);

		ZString ICADHeader.Pre_CARM_Box10 => GetMessageSegmentValue<ZInt>(() => declaration.AdditionalInformation.Count) > 0 ? "1" : "0";

		ZString ICADHeader.Z1_RPP => ZString.Empty;

		ZString ICADHeader.ImporterBN_Box11 => ImporterBusinessNumber;

		ZString ImporterBusinessNumber => importerBusinessNumber ??= GetMessageSegmentValue<ZString>(() => declaration.Importer.Id.Value);
		ZString? importerBusinessNumber;

		ZString ICADHeader.ImporterDetails_Box12
		{
			get
			{
				if (!importerDetails_Box12.HasValue)
				{
					importerDetails_Box12 = ZString.Empty;
					if (!ImporterBusinessNumber.IsEmpty)
					{
						var loader = new OrgHeader.Loader(factory);
						var orgs = loader.LoadDBOrganisations(Core.Constants.CountryCodes.Canada, OrgCusCode.CACodeTypes.BusinessNumberForImportExport, ImporterBusinessNumber)
									   ?? loader.LoadDBOrganisations(Core.Constants.CountryCodes.Canada, OrgCusCode.CACodeTypes.BusinessNumberImporterCommercial, ImporterBusinessNumber)
									   ?? loader.LoadDBOrganisations(Core.Constants.CountryCodes.Canada, OrgCusCode.CACodeTypes.BusinessNumberImporterNonCommercial, ImporterBusinessNumber)
									   ?? loader.LoadDBOrganisations(Core.Constants.CountryCodes.Canada, OrgCusCode.CACodeTypes.BusinessNumberForLowValueShipments, ImporterBusinessNumber);

						if (orgs.Length == 1)
						{
							importerDetails_Box12 = AdjustmentDocHelper.AddressForImporterFormatted(orgs[0].MainAddress);
						}
						else if (orgs.Length > 1 && jobDeclaration != null)
						{
							var importer = jobDeclaration.ImporterOfRecordAddress.HasRealOrganisation ? jobDeclaration.ImporterOfRecordAddress.Organisation : jobDeclaration.Importer;
							if (importer != null)
							{
								var address = orgs.FirstOrDefault(x => x.PK == importer.PK)?.MainAddress;
								importerDetails_Box12 = AdjustmentDocHelper.AddressForImporterFormatted(address);
							}
						}
					}
				}
				return importerDetails_Box12.Value;
			}
		}
		ZString? importerDetails_Box12;

		ZString ICADHeader.BrokerOrAgentBN_Box13 => GetMessageSegmentValue<ZString>(() => declaration.Declarant.Id.Value);

		ZString ICADHeader.BrokerOrAgentDetails_Box14
		{
			get
			{
				if (!brokerOrAgentDetails_Box14.HasValue)
				{
					brokerOrAgentDetails_Box14 = ZString.Empty;
					var brokerAddress = jobDeclaration.GetOrgProxyWithCABusinessNumber().MainAddress;
					brokerOrAgentDetails_Box14 = AdjustmentDocHelper.AddressForImporterFormatted(brokerAddress);
				}
				return brokerOrAgentDetails_Box14.Value;
			}
		}
		ZString? brokerOrAgentDetails_Box14;

		ZString ICADHeader.CargoControlNo_Box15 => GetMessageSegmentValue<ZString>(() => declaration.AdditionalDocument.FirstOrDefault().Id.Value);

		ZString ICADHeader.RecordOfIntentNo_Box16 => ZString.Empty;

		ZString ICADHeader.PreviousTransactionNo_Box17 => ZString.Empty;

		ZDateTime ICADHeader.AcceptedDate_Box18 => AcceptanceDateTime;

		ZString ICADHeader.OriginalTransactionNo_Box19 => ZString.Empty;

		ZString ICADHeader.PrevTransNoWarehouse_Box20 => ZString.Empty;

		ZString ICADHeader.PortOfUnlading_Box21 => GetMessageSegmentValue<ZString>(() => declaration.UnloadingLocation.Id.Value);

		ZString ICADHeader.Notes_Box35 => ZString.Empty;

		ZDecimal ICADHeader.TotalValueForDuty_Box113 => totalValueForDuty;

		ZDecimal ICADHeader.TotalPSTAndHST_Box114 => totalPSTAndHST;

		ZDecimal ICADHeader.TotalPSTCannabisAmount_Box115 => totalPSTCannabisAmount;

		ZDecimal ICADHeader.TotalProvAlcoholTaxAmount_Box116 => totalProvAlcoholTaxAmount;

		ZDecimal ICADHeader.TotalProvTobaccoAmount_Box117 => totalProvTobaccoAmount;

		ZDecimal ICADHeader.TotalDeclarationRelieved_Box118 => totalDeclarationRelieved;

		ZDecimal ICADHeader.TotalAmount_Box119 => totalAmount;

		ZDecimal ICADHeader.TotalCustomsDuties_Box120 => totalCustomsDuties;

		ZDecimal ICADHeader.TotalExciseDuties_Box121 => totalExciseDuties;

		ZDecimal ICADHeader.TotalExciseTaxes_Box122 => totalExciseTaxes;

		ZDecimal ICADHeader.TotalGST_Box123 => totalGST;

		ZDecimal ICADHeader.TotalAnti_Dumping_Box124 => totalAnti_Dumping;

		ZDecimal ICADHeader.TotalCountervailing_Box125 => totalCountervailing;

		ZDecimal ICADHeader.TotalSurtaxes_Box126 => totalSurtaxes;

		ZDecimal ICADHeader.TotalSafeguards_Box127 => totalSafeguards;

		ZDecimal ICADHeader.TotalInterest_Box128 => totalInterest;

		ZDecimal ICADHeader.TotalDutiesAndTaxesWithInterest_Box129 => totalDutiesAndTaxesWithInterest;

		ZDecimal ICADHeader.TotalDutiesAndTaxes_Box130 => totalDutiesAndTaxes;

		IEnumerable<ICADSubHeader> ICADHeader.CADSubHeaders
		{
			get
			{
				if (cADSubHeaders == null)
				{
					cADSubHeaders = declaration.GoodsShipment.OrderBy(x => x.SequenceNumeric).Select(x => new CADSubHeader(factory, x, this));
				}
				return cADSubHeaders;
			}
		}
		IEnumerable<ICADSubHeader> cADSubHeaders;

		ZDecimal FreightCharges => GetMessageSegmentValue<ZDecimal>(() => declaration.Consignment.Freight.RateAmount.Value);

		CADAmmendmentWrapper QueryAmmendmentInfoByIndex(decimal invoiceSequence, decimal lineSequence, int index)
		{
			if (amendmentMap == null)
			{
				InitialAmendmentMap();
			}
			var queryKey = GenerateAmendmentMapKey(invoiceSequence.ToString(), lineSequence.ToString());
			return amendmentMap.TryGetValue(queryKey, out var list) ? list.Skip(index).FirstOrDefault() : null;
		}

		void InitialAmendmentMap()
		{
			amendmentMap = new Dictionary<string, List<CADAmmendmentWrapper>>();
			foreach (var amendment in declaration.Amendment)
			{
				if (amendment.Pointer != null)
				{
					var reasonCode = GetMessageSegmentValue<ZString>(() => amendment.ChangeReasonCode.Value);
					var appealCode = GetMessageSegmentValue<ZString>(() => amendment.AdditionalInformation.FirstOrDefault(w => "APC" == w.StatementTypeCode.Value)?.StatementCode.Value ?? ZString.Empty);
					var description = GetMessageSegmentValue<ZString>(() => amendment.AdditionalInformation.FirstOrDefault(w => "CHG" == w.StatementTypeCode.Value)?.StatementDescription.Value ?? ZString.Empty);
					foreach (var pointer in amendment.Pointer)
					{
						var location = GetMessageSegmentValue<ZString>(() => pointer.Location.Value);
						if (!location.IsEmpty)
						{
							var locationPair = CADDeclarationAmendment.DeserializePointer(location);
							var key = GenerateAmendmentMapKey(locationPair.invoiceSequence, locationPair.lineSequence);
							if (!amendmentMap.TryGetValue(key, out var amendmentList))
							{
								amendmentList = new List<CADAmmendmentWrapper>();
								amendmentMap.Add(key, amendmentList);
							}
							var wrapper = new CADAmmendmentWrapper(this)
							{
								ReasonCode = reasonCode,
								AppealCode = appealCode,
								Description = description
							};
							amendmentList.Add(wrapper);
						}
					}
				}
			}
		}

		string GenerateAmendmentMapKey(string invoiceSequence, string lineSequence) => string.Join(",", invoiceSequence, lineSequence);

		Dictionary<string, List<CADAmmendmentWrapper>> amendmentMap;

		ZString QueryCachedCodeDescription(string codeListTypes, ZString code)
		{
			return code.IsEmpty ? ZString.Empty : factory.GetCachedValue("Amendment" + codeListTypes + code, () =>
				{
					var result = ZString.Empty;
					var cusCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, code, Core.Constants.CountryCodes.Canada, codeListTypes, ZDateTime.Today);
					if (cusCode != null)
					{
						result = cusCode.ZZD_Description.ToUpper();
					}
					return result;
				});
		}

		void CaculateDeclarationTotals()
		{
			totalValueForDuty = ZDecimal.Zero;
			totalPSTAndHST = ZDecimal.Zero;
			totalPSTCannabisAmount = ZDecimal.Zero;
			totalProvAlcoholTaxAmount = ZDecimal.Zero;
			totalProvTobaccoAmount = ZDecimal.Zero;
			totalDeclarationRelieved = ZDecimal.Zero;
			totalAmount = ZDecimal.Zero;
			totalCustomsDuties = ZDecimal.Zero;
			totalExciseDuties = ZDecimal.Zero;
			totalExciseTaxes = ZDecimal.Zero;
			totalGST = ZDecimal.Zero;
			totalAnti_Dumping = ZDecimal.Zero;
			totalCountervailing = ZDecimal.Zero;
			totalSurtaxes = ZDecimal.Zero;
			totalSafeguards = ZDecimal.Zero;
			totalInterest = ZDecimal.Zero;
			totalDutiesAndTaxesWithInterest = ZDecimal.Zero;
			totalDutiesAndTaxes = ZDecimal.Zero;

			foreach (var dutyTaxFee in declaration.DutyTaxFee)
			{
				var typeCode = GetMessageSegmentValue<ZString>(() => dutyTaxFee.TypeCode.Value);
				var paymentAmount = GetMessageSegmentValue<ZDecimal>(() => dutyTaxFee.Payment?.PaymentAmount.Value ?? ZDecimal.Zero);
				switch (typeCode)
				{
					case CADDutyTaxFeeTypeCodes.Codes.AAI:
						totalPSTAndHST += paymentAmount;
						break;
					case CADDutyTaxFeeTypeCodes.Codes.PAT:
						totalPSTCannabisAmount += paymentAmount;
						break;
					case CADDutyTaxFeeTypeCodes.Codes.TAC:
						totalProvAlcoholTaxAmount += paymentAmount;
						break;
					case CADDutyTaxFeeTypeCodes.Codes.AAD:
						totalProvTobaccoAmount += paymentAmount;
						break;
					case CADDutyTaxFeeTypeCodes.Codes.TOT:
						var adValoremTaxBaseAmount = GetMessageSegmentValue<ZDecimal>(() => dutyTaxFee.AdValoremTaxBaseAmount.Value);
						totalValueForDuty += adValoremTaxBaseAmount;
						totalDutiesAndTaxes += paymentAmount;
						totalDutiesAndTaxesWithInterest += paymentAmount;
						break;
					case CADDutyTaxFeeTypeCodes.Codes.CUD:
						totalCustomsDuties += paymentAmount;
						break;
					case CADDutyTaxFeeTypeCodes.Codes.EXC:
						totalExciseDuties += paymentAmount;
						break;
					case CADDutyTaxFeeTypeCodes.Codes.EXD:
						var deductAmount = GetMessageSegmentValue<ZDecimal>(() => dutyTaxFee.DeductAmount.Value);
						totalDeclarationRelieved += deductAmount;
						break;
					case CADDutyTaxFeeTypeCodes.Codes.FET:
						totalExciseTaxes += paymentAmount;
						break;
					case CADDutyTaxFeeTypeCodes.Codes.GST:
						totalGST += paymentAmount;
						break;
					case CADDutyTaxFeeTypeCodes.Codes.ADD:
						totalAnti_Dumping += paymentAmount;
						break;
					case CADDutyTaxFeeTypeCodes.Codes.CVD:
						totalCountervailing += paymentAmount;
						break;
					case CADDutyTaxFeeTypeCodes.Codes.SUR:
						totalSurtaxes += paymentAmount;
						break;
					case CADDutyTaxFeeTypeCodes.Codes.OTH:
						totalSafeguards += paymentAmount;
						break;
					case CADDutyTaxFeeTypeCodes.Codes.INT:
						totalInterest += paymentAmount;
						totalDutiesAndTaxesWithInterest += paymentAmount;
						break;
					default:
						break;
				}
			}

			totalAmount = totalCustomsDuties + totalExciseDuties + totalExciseTaxes + totalGST + totalAnti_Dumping + totalCountervailing + totalSurtaxes + totalSafeguards;
		}

		ZDecimal totalValueForDuty;
		ZDecimal totalPSTAndHST;
		ZDecimal totalPSTCannabisAmount;
		ZDecimal totalProvAlcoholTaxAmount;
		ZDecimal totalProvTobaccoAmount;
		ZDecimal totalDeclarationRelieved;
		ZDecimal totalAmount;
		ZDecimal totalCustomsDuties;
		ZDecimal totalExciseDuties;
		ZDecimal totalExciseTaxes;
		ZDecimal totalGST;
		ZDecimal totalAnti_Dumping;
		ZDecimal totalCountervailing;
		ZDecimal totalSurtaxes;
		ZDecimal totalSafeguards;
		ZDecimal totalInterest;
		ZDecimal totalDutiesAndTaxesWithInterest;
		ZDecimal totalDutiesAndTaxes;

		static T GetMessageSegmentValue<T>(Func<T> getValue)
		{
			var result = default(T);
			try
			{
				result = getValue();
			}
			catch (NullReferenceException)
			{
			}
			return result;
		}

		class CADAmmendmentWrapper
		{
			readonly CADAsLodgedDocumentWrapper parent;

			public CADAmmendmentWrapper() { }

			public CADAmmendmentWrapper(CADAsLodgedDocumentWrapper parent)
			{
				this.parent = parent;
			}

			public ZString ReasonCode;

			public ZString ReasonCodeDescription => parent?.QueryCachedCodeDescription(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CARMChangeReasonCode, ReasonCode) ?? ZString.Empty;

			public ZString ReasonFormatted => parent == null ? ZString.Empty : $"{ReasonCode} {ReasonCodeDescription}";

			public ZString AppealCode;

			public ZString AppealCodeDescription => parent?.QueryCachedCodeDescription(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CARMAppealsProgramCode, AppealCode) ?? ZString.Empty;

			public ZString AppealFormatted => parent == null ? ZString.Empty : $"{AppealCode} {AppealCodeDescription}";

			public ZString Description;
		}

		class CADSubHeader : ICADSubHeader
		{
			public CADSubHeader(BusinessObjectFactory factory, DeclarationGoodsShipment goodsShipment, CADAsLodgedDocumentWrapper parent)
			{
				this.factory = factory;
				this.parent = parent;
				this.goodsShipment = goodsShipment;
				this.invoice = goodsShipment.Invoice.FirstOrDefault();
			}

			readonly BusinessObjectFactory factory;
			public readonly CADAsLodgedDocumentWrapper parent;
			readonly DeclarationGoodsShipment goodsShipment;
			readonly DeclarationGoodsShipmentInvoice invoice;

			public decimal SequenceNumeric => goodsShipment.SequenceNumeric;

			ZString ICADSubHeader.VendorDetails_Box36
			{
				get
				{
					var result = ZString.Empty;
					var vendor = goodsShipment.Seller;
					if (vendor != null)
					{
						result = Helper.GetPostalAddressAsASingleLine(factory,
							GetMessageSegmentValue<ZString>(() => vendor.Name.Value),
							GetMessageSegmentValue<ZString>(() => vendor.Address.Line.Value),
							GetMessageSegmentValue<ZString>(() => vendor.Address.CityName.Value),
							GetMessageSegmentValue<ZString>(() => vendor.Address.CountrySubDivisionCode.Value),
							GetMessageSegmentValue<ZString>(() => vendor.Address.PostcodeId.Value),
							GetMessageSegmentValue<ZString>(() => vendor.Address.CountryCode.Value));
					}
					return result;
				}
			}

			ZString ICADSubHeader.PurchaserDetails_Box37
			{
				get
				{
					var result = ZString.Empty;
					var purchaser = goodsShipment.Buyer;
					if (purchaser != null)
					{
						result = Helper.GetPostalAddressAsASingleLine(factory,
							GetMessageSegmentValue<ZString>(() => purchaser.Name.Value),
							GetMessageSegmentValue<ZString>(() => purchaser.Address.Line.Value),
							GetMessageSegmentValue<ZString>(() => purchaser.Address.CityName.Value),
							GetMessageSegmentValue<ZString>(() => purchaser.Address.CountrySubDivisionCode.Value),
							GetMessageSegmentValue<ZString>(() => purchaser.Address.PostcodeId.Value),
							GetMessageSegmentValue<ZString>(() => purchaser.Address.CountryCode.Value));
					}
					return result;
				}
			}

			ZString ICADSubHeader.InvoiceNo_Box38 => GetMessageSegmentValue<ZString>(() => invoice.Id.Value);

			ZDecimal ICADSubHeader.InvoiceValue_Box39 => GetMessageSegmentValue<ZDecimal>(() => invoice.AmountAmount.Value);

			ZString ICADSubHeader.InvoiceCurrencyCode_Box40 => GetMessageSegmentValue<ZString>(() => invoice.AmountAmount.CurrencyId);

			ZString ICADSubHeader.PurchaseOrderNo_Box41 => ZString.Empty;

			ZDecimal ICADSubHeader.FreightCharges_Box42 => parent.FreightCharges;

			ZString ICADSubHeader.USPortOfExit_Box43 => GetMessageSegmentValue<ZString>(() => goodsShipment.ExitOffice.Id.Value);

			IEnumerable<ICADLine> ICADSubHeader.CADLines
			{
				get
				{
					if (cADLines == null)
					{
						cADLines = goodsShipment.GovernmentAgencyGoodsItem.OrderBy(x => x.SequenceNumeric).Select(x => new CADLine(x, this));
					}
					return cADLines;
				}
			}
			IEnumerable<ICADLine> cADLines;

			B3AndCADDocumentHelper Helper => helper ?? (helper = new B3AndCADDocumentHelper());
			B3AndCADDocumentHelper helper;
		}

		class CADLine : ICADLine
		{
			public CADLine(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity commodity, CADSubHeader parent)
			{
				this.commodity = commodity;
				this.parent = parent;
				CaculateDeclarationTotals();
				this.classification = commodity.Classification.FirstOrDefault(x => x.BindingTariffReferenceId.Value == "10");
				this.tariff = commodity.Classification.FirstOrDefault(x => x.BindingTariffReferenceId.Value == "06");
			}

			readonly CADSubHeader parent;
			readonly DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity commodity;
			readonly DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassification classification;
			readonly DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassification tariff;

			ZShort ICADLine.CADLineNo_Box56 => ZShort.ParseSafe(GetMessageSegmentValue<ZDecimal>(() => commodity.SequenceNumeric).ToString(), ZShort.Zero);

			ZString ICADLine.PreviousLineNoWarehouse_Box57 => ZString.Empty;

			ZString ICADLine.ClassificationNo_Box58 => GetMessageSegmentValue<ZString>(() => classification.Id.Value);

			ZString ICADLine.ClassificationDescription_Box59 => CommodityDescription;

			ZString ICADLine.NarrativeDescription_Box60 => CommodityDescription;

			ZString CommodityDescription => commodityDescription ??= GetMessageSegmentValue<ZString>(() => commodity.Description.Value);
			ZString? commodityDescription;

			ZDecimal ICADLine.Quantity_Box61 => GetMessageSegmentValue<ZDecimal>(() => commodity.CountQuantity.Value);

			ZString ICADLine.UnitOfMeasure_Box62 => GetMessageSegmentValue<ZString>(() => commodity.CountQuantity.UnitCode);

			ZString ICADLine.TimeLimitType_Box63 => ZString.Empty;

			ZDateTime ICADLine.ExtensionDate_Box64 => ZDateTime.Empty;

			ZString ICADLine.CountryOfOrigin_Box65 => GetMessageSegmentValue<ZString>(() => commodity.Origin.CountryCode.Value);

			ZString ICADLine.USState_Box66 => ZString.Empty;

			ZString ICADLine.PlaceOfExport_Box67 => GetMessageSegmentValue<ZString>(() => commodity.ExportCountry.CountryCode.Value);

			ZString ICADLine.PlaceOfExportCodeState_Box68 => GetMessageSegmentValue<ZString>(() => commodity.ExportCountry.RegionId.Value);

			ZDateTime ICADLine.DirectShipmentDate_Box69
			{
				get
				{
					var result = ZDateTime.Empty;
					var dateTimeString = GetMessageSegmentValue<ZString>(() => commodity.ExitDateTime.DateTimeString);
					if (!dateTimeString.IsEmpty)
					{
						_ = ZDateTime.TryParseExact(dateTimeString, out result, "yyyyMMdd");
					}
					return result;
				}
			}

			ZString ICADLine.TariffTreatment_Box70 => tariffTreatment;

			ZString ICADLine.TariffCode_Box71 => GetMessageSegmentValue<ZString>(() => tariff.Id.Value);

			ZString ICADLine.TimeLimitFrom_Box72 => GetMessageSegmentValue<ZString>(() => commodity.AdditionalInformation.FirstOrDefault(f => f.StatementTypeCode.Value == AdditionalInformationTypeCodes.Codes.TLS)?.LimitDateTime.DateTimeString ?? ZString.Empty);

			ZString ICADLine.TimeLimitTo_Box73 => GetMessageSegmentValue<ZString>(() => commodity.AdditionalInformation.FirstOrDefault(f => f.StatementTypeCode.Value == AdditionalInformationTypeCodes.Codes.TLE)?.LimitDateTime.DateTimeString ?? ZString.Empty);

			ZString ICADLine.DestinationProvince_Box74 => GetMessageSegmentValue<ZString>(() => commodity.Destination.RegionId.Value);

			ZDecimal ICADLine.ValueForCurrencyConversion_Box75 => valueForCurrencyConversion;

			ZString ICADLine.Currency_Box76 => valueForCurrencyConversionId;

			ZDecimal ICADLine.ExchangeRate_Box77 => exchangeRate;

			ZDecimal ICADLine.ValueForDuty_Box78 => valueForDuty;

			ZString ICADLine.DRPLicense_Box79 => GetMessageSegmentValue<ZString>(() => commodity.AdditionalDocument.FirstOrDefault(f => f.TypeCode.Value == RemissionTypeList.Codes.DutiesReliefProgramLicense)?.Id.Value ?? ZString.Empty);

			ZString ICADLine.SpecialAuthOIC_Box80 => GetMessageSegmentValue<ZString>(() => commodity.AdditionalDocument.FirstOrDefault(f => f.TypeCode.Value == RemissionTypeList.Codes.OrderInCouncil)?.Id.Value ?? ZString.Empty);

			ZString ICADLine.SpecialAuthorityPermit_Box81 => GetMessageSegmentValue<ZString>(() => commodity.AdditionalDocument.FirstOrDefault(x => x.TypeCode.Value == RemissionTypeList.Codes.Permit)?.Id.Value ?? ZString.Empty);

			ZDecimal ICADLine.CustomsDuty_Box82 => customsDuties;

			ZDecimal ICADLine.ExciseTax_Box83 => exciseTaxes;

			ZDecimal ICADLine.ExciseDuty_Box84 => exciseDuties;

			ZDecimal ICADLine.Surtax_Box85 => surtaxes;

			ZDecimal ICADLine.Anti_Dumping_Box86 => anti_Dumping;

			ZDecimal ICADLine.Safeguard_Box87 => safeguards;

			ZDecimal ICADLine.Countervailing_Box88 => countervailing;

			ZDecimal ICADLine.ValueForTax_Box89 => valueForTax;

			ZDecimal ICADLine.GST_Box90 => gST;

			ZDecimal ICADLine.PSTAndHSTAmount_Box91 => pSTAndHST;

			ZDecimal ICADLine.ProvincialAlcoholTax_Box92 => provAlcoholTaxAmount;

			ZDecimal ICADLine.ProvincialTobaccoAmount_Box93 => provTobaccoAmount;

			ZDecimal ICADLine.AlcohosPercent_Box94 => GetMessageSegmentValue<ZDecimal>(() => commodity.Constituent.ElementPercentNumeric.ValueOrNullIfZero() ?? ZDecimal.Zero);

			ZDecimal ICADLine.ProvincialCannabisExciseDuty_Box95 => pSTCannabisAmount;

			ZString ICADLine.CBSACaseNo_Box96 => ZString.Empty;

			ZString ICADLine.RulingNo_Box97 => GetMessageSegmentValue<ZString>(() => commodity.AdditionalInformation.FirstOrDefault(x => x.StatementTypeCode.Value == AdditionalInformationTypeCodes.Codes.MIF).StatementCode.Value);

			ZString ICADLine.AppealsCaseNo_Box98 => ZString.Empty;

			ZString ICADLine.ComplianceCaseNo_Box99 => ZString.Empty;

			ZDecimal ICADLine.LineTotalDutiesAndTaxes_Box100 => totalDutiesAndTaxes;

			ZString ICADLine.CommodityReason1_Box101 => AmendmentInfo1.ReasonFormatted;

			ZString ICADLine.Authority1_Box102 => AmendmentInfo1.AppealFormatted;

			ZString ICADLine.CommodityRemark1_Box103 => AmendmentInfo1.Description;

			CADAmmendmentWrapper AmendmentInfo1 => amendmentInfo1 ??= QueryAmmendmentInfoByIndex(0);
			CADAmmendmentWrapper amendmentInfo1;

			ZString ICADLine.CommodityReason2_Box105 => AmendmentInfo2.ReasonFormatted;

			ZString ICADLine.Authority2_Box106 => AmendmentInfo2.AppealFormatted;

			ZString ICADLine.CommodityRemark2_Box107 => AmendmentInfo2.Description;

			CADAmmendmentWrapper AmendmentInfo2 => amendmentInfo2 ??= QueryAmmendmentInfoByIndex(1);
			CADAmmendmentWrapper amendmentInfo2;

			ZString ICADLine.CommodityReason3_Box109 => AmendmentInfo3.ReasonFormatted;

			ZString ICADLine.Authority3_Box110 => AmendmentInfo3.AppealFormatted;

			ZString ICADLine.CommodityRemark3_Box111 => AmendmentInfo3.Description;

			CADAmmendmentWrapper AmendmentInfo3 => amendmentInfo3 ??= QueryAmmendmentInfoByIndex(2);
			CADAmmendmentWrapper amendmentInfo3;

			CADAmmendmentWrapper QueryAmmendmentInfoByIndex(int index) => parent.parent.QueryAmmendmentInfoByIndex(parent.SequenceNumeric, commodity.SequenceNumeric, index) ?? new CADAmmendmentWrapper();

			void CaculateDeclarationTotals()
			{
				valueForDuty = ZDecimal.Zero;
				pSTAndHST = ZDecimal.Zero;
				pSTCannabisAmount = ZDecimal.Zero;
				provAlcoholTaxAmount = ZDecimal.Zero;
				provTobaccoAmount = ZDecimal.Zero;
				customsDuties = ZDecimal.Zero;
				exciseDuties = ZDecimal.Zero;
				exciseTaxes = ZDecimal.Zero;
				gST = ZDecimal.Zero;
				anti_Dumping = ZDecimal.Zero;
				countervailing = ZDecimal.Zero;
				surtaxes = ZDecimal.Zero;
				safeguards = ZDecimal.Zero;
				totalDutiesAndTaxes = ZDecimal.Zero;
				tariffTreatment = ZString.Empty;
				exchangeRate = ZDecimal.Zero;
				valueForCurrencyConversion = ZDecimal.Zero;
				valueForCurrencyConversionId = ZString.Empty;
				valueForTax = ZDecimal.Zero;

				foreach (var dutyTaxFee in commodity.DutyTaxFee)
				{
					var typeCode = GetMessageSegmentValue<ZString>(() => dutyTaxFee.TypeCode.Value);
					var paymentAmount = GetMessageSegmentValue<ZDecimal>(() => dutyTaxFee.Payment?.PaymentAmount?.Value ?? ZDecimal.Zero);
					switch (typeCode)
					{
						case CADDutyTaxFeeTypeCodes.Codes.AAI:
							pSTAndHST += paymentAmount;
							break;
						case CADDutyTaxFeeTypeCodes.Codes.PAT:
							pSTCannabisAmount += paymentAmount;
							break;
						case CADDutyTaxFeeTypeCodes.Codes.TAC:
							provAlcoholTaxAmount += paymentAmount;
							break;
						case CADDutyTaxFeeTypeCodes.Codes.AAD:
							provTobaccoAmount += paymentAmount;
							break;
						case CADDutyTaxFeeTypeCodes.Codes.TOT:
							var vFCC = dutyTaxFee.DutyTaxFeeAssessmentBasis.FirstOrDefault(y => y.AdValoremTaxBaseAmount.CurrencyId != Core.Constants.CurrencyCodes.Canada);
							valueForCurrencyConversion = GetMessageSegmentValue<ZDecimal>(() => vFCC?.AdValoremTaxBaseAmount.Value ?? ZDecimal.Zero);
							valueForCurrencyConversionId = GetMessageSegmentValue<ZString>(() => vFCC?.AdValoremTaxBaseAmount.CurrencyId ?? ZString.Empty);
							var adValoremTaxBaseAmount = GetMessageSegmentValue<ZDecimal>(() => dutyTaxFee.DutyTaxFeeAssessmentBasis.FirstOrDefault(y => y.AdValoremTaxBaseAmount.CurrencyId == Core.Constants.CurrencyCodes.Canada)?.AdValoremTaxBaseAmount.Value ?? ZDecimal.Zero);
							valueForDuty += adValoremTaxBaseAmount;
							exchangeRate = GetMessageSegmentValue<ZDecimal>(() => dutyTaxFee.DutyTaxFeeAssessmentBasis.FirstOrDefault(x => x.RateNumeric != null && x.RateNumeric.Value > 0).RateNumeric.Value);
							totalDutiesAndTaxes += paymentAmount;
							break;
						case CADDutyTaxFeeTypeCodes.Codes.CUD:
							customsDuties += paymentAmount;
							tariffTreatment = GetMessageSegmentValue<ZString>(() => dutyTaxFee.DutyRegimeCode.Value);
							break;
						case CADDutyTaxFeeTypeCodes.Codes.EXC:
							exciseDuties += paymentAmount;
							break;
						case CADDutyTaxFeeTypeCodes.Codes.FET:
							exciseTaxes += paymentAmount;
							break;
						case CADDutyTaxFeeTypeCodes.Codes.GST:
							gST += paymentAmount;
							break;
						case CADDutyTaxFeeTypeCodes.Codes.ADD:
							anti_Dumping += paymentAmount;
							break;
						case CADDutyTaxFeeTypeCodes.Codes.CVD:
							countervailing += paymentAmount;
							break;
						case CADDutyTaxFeeTypeCodes.Codes.SUR:
							surtaxes += paymentAmount;
							break;
						case CADDutyTaxFeeTypeCodes.Codes.OTH:
							safeguards += paymentAmount;
							break;
						case CADDutyTaxFeeTypeCodes.Codes.VFT:
							valueForTax += GetMessageSegmentValue<ZDecimal>(() => dutyTaxFee.Payment.TaxAssessedAmount.Value);
							break;
						default:
							break;
					}
				}
			}
			ZDecimal valueForDuty;
			ZDecimal pSTAndHST;
			ZDecimal pSTCannabisAmount;
			ZDecimal provAlcoholTaxAmount;
			ZDecimal provTobaccoAmount;
			ZDecimal customsDuties;
			ZDecimal exciseDuties;
			ZDecimal exciseTaxes;
			ZDecimal gST;
			ZDecimal anti_Dumping;
			ZDecimal countervailing;
			ZDecimal surtaxes;
			ZDecimal safeguards;
			ZDecimal totalDutiesAndTaxes;
			ZString tariffTreatment;
			ZDecimal exchangeRate;
			ZDecimal valueForCurrencyConversion;
			ZDecimal valueForTax;
			ZString valueForCurrencyConversionId;
		}
	}
}
