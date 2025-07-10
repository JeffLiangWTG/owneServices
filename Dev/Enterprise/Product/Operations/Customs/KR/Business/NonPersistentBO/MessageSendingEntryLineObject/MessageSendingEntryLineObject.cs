using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5FN;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.KR.Business
{
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class MessageSendingEntryLineObject : AutoMessageSendingEntryLineObject
	{
		public MessageSendingEntryLineObject(BusinessObjectFactory factory) : base(factory)
		{
		}

		public MessageSendingEntryLineObject(string messageType, CusEntryHeader entry) : this(entry.Factory)
		{
			Entry = entry;
			MessageType = messageType;
		}

		public readonly CusEntryHeader Entry;
		public readonly ZString MessageType;
		public JobComInvoiceLine InvoiceLine { get; private set; }

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			if (property.Name == Schema.IsGoldOrItsProduct || property.Name == Schema.ShouldSend)
			{
				return false;
			}
			return true;
		}
		public MessageSendingEntryLineObjectLookups Lookups => new MessageSendingEntryLineObjectLookups(this);
		#region Properties
		[List(nameof(Lookups) + "." + nameof(MessageSendingEntryLineObjectLookups.PostClearanceYNCodeList))]
		public override ZString PostClearanceYN { get => base.PostClearanceYN; set => base.PostClearanceYN = value; }

		[List(nameof(Lookups) + "." + nameof(MessageSendingEntryLineObjectLookups.SpecificUseProductTypeList))]
		public override ZString ProductType { get => base.ProductType; set => base.ProductType = value; }

		[List(nameof(Lookups) + "." + nameof(MessageSendingEntryLineObjectLookups.CustomsOfficeList))]
		public override ZString CustomsOffice { get => base.CustomsOffice; set => base.CustomsOffice = value; }

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(MessageSendingEntryLineObjectLookups.ConsigneeAddress))]
		public override ZGuid GoodsLocation { get => base.GoodsLocation; set => base.GoodsLocation = value; }

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress GoodsLocation_ZAddress
		{
			get
			{
				if (goodsLocation_ZAddress == null)
				{
					goodsLocation_ZAddress = GetNewGoodsLocation_ZAddress();
				}
				return goodsLocation_ZAddress;
			}
		}
		ZAddress goodsLocation_ZAddress;

		ZAddress GetNewGoodsLocation_ZAddress() => new ZAddress(GoodsLocationInfo);

		[List(nameof(Lookups) + "." + nameof(MessageSendingEntryLineObjectLookups.DutyReductionClassificationList))]
		public override ZString DutyReduction { get => base.DutyReduction; set => base.DutyReduction = value; }

		[List(nameof(Lookups) + "." + nameof(MessageSendingEntryLineObjectLookups.ReductionRateRegulationList))]
		public override ZString GroupNumber { get => base.GroupNumber; set => base.GroupNumber = value; }

		[List(nameof(Lookups) + "." + nameof(MessageSendingEntryLineObjectLookups.CustomsOfficeList))]
		public override ZString ReExportCustomsOffice { get => base.ReExportCustomsOffice; set => base.ReExportCustomsOffice = value; }

		[List(nameof(Lookups) + "." + nameof(MessageSendingEntryLineObjectLookups.GoodsDestination))]
		public override ZString DestinationCountry { get => base.DestinationCountry; set => base.DestinationCountry = value; }

		[List(nameof(Lookups) + "." + nameof(MessageSendingEntryLineObjectLookups.SecondaryPreferences))]
		public override ZString DutyReductionCode { get => base.DutyReductionCode; set => base.DutyReductionCode = value; }

		[List(nameof(Lookups) + "." + nameof(MessageSendingEntryLineObjectLookups.InstallmentCodes))]
		public override ZString InstalmentCode { get => base.InstalmentCode; set => base.InstalmentCode = value; }

		[List(nameof(Lookups) + "." + nameof(MessageSendingEntryLineObjectLookups.NetWeightUnitList))]
		public override ZString NetWeightUnit { get => base.NetWeightUnit; set => base.NetWeightUnit = value; }

		[List(nameof(Lookups) + "." + nameof(MessageSendingEntryLineObjectLookups.CountryOfOrigins))]
		public override ZString CountryOfOrigin { get => base.CountryOfOrigin; set => base.CountryOfOrigin = value; }

		[List(nameof(Lookups) + "." + nameof(MessageSendingEntryLineObjectLookups.PreferenceCodeList))]
		public override ZString Preference { get => base.Preference; set => base.Preference = value; }

		public ZString PreferenceDescription { get; set; }

		[List(nameof(Lookups) + "." + nameof(MessageSendingEntryLineObjectLookups.COOProductTypeList))]
		public override ZString COOProductType { get => base.COOProductType; set => base.COOProductType = value; }

		[List(nameof(Lookups) + "." + nameof(MessageSendingEntryLineObjectLookups.ThirdCountryAdditionalInvoiceIssuedYNCodeList))]
		public override ZString ThirdCountryAdditionalInvoiceIssued { get => base.ThirdCountryAdditionalInvoiceIssued; set => base.ThirdCountryAdditionalInvoiceIssued = value; }

		[List(nameof(Lookups) + "." + nameof(MessageSendingEntryLineObjectLookups.CountryOfOrigins))]
		public override ZString ThirdCountry { get => base.ThirdCountry; set => base.ThirdCountry = value; }

		[List(nameof(Lookups) + "." + nameof(MessageSendingEntryLineObjectLookups.COOSupportingDocTypeList))]
		public override ZString COOSupportingDocType { get => base.COOSupportingDocType; set => base.COOSupportingDocType = value; }

		[List(nameof(Lookups) + "." + nameof(MessageSendingEntryLineObjectLookups.COOIssuerTypeList))]
		public override ZString COOIssuerType { get => base.COOIssuerType; set => base.COOIssuerType = value; }

		[List(nameof(Lookups) + "." + nameof(MessageSendingEntryLineObjectLookups.CountryOfOrigins))]
		public override ZString AssociatedCOOIssuingCountryCode { get => base.AssociatedCOOIssuingCountryCode; set => base.AssociatedCOOIssuingCountryCode = value; }

		[List(nameof(Lookups) + "." + nameof(MessageSendingEntryLineObjectLookups.COOIssuerTypeList))]
		public override ZString COOIssuingAgencyType { get => base.COOIssuingAgencyType; set => base.COOIssuingAgencyType = value; }

		public override ZDecimal NetWeightInKG { get => base.NetWeightInKG; set => base.NetWeightInKG = value; }

		public override ZDecimal TotalNetWeight { get => base.TotalNetWeight; set => base.TotalNetWeight = value; }

		public ZDecimal NetWeightInGrams { get; private set; }
		#endregion

		public void DecorateFromCusEntryLine(CusEntryLine entryLine, string messageType)
		{
			entryLinePK = entryLine.PK;
			InvoiceLine = entryLine.RandomLine;
			EntryLineNo = entryLine.CL_LineNumber;
			HSCode = InvoiceLine.JI_Tariff;
			FormattedHSCode = ElectronicDocumentTypeList.GetOriginalFTAMessageTypes().Contains(messageType)
								? MessageFunctions.GetFormattedNumber(InvoiceLine.JI_Tariff.SubstringSafe(0, 6), new int[] { 0, 4 }, ".")
								: InvoiceLine.JI_FormattedTariff;
			HSDescription = entryLine.TariffDescription;
			NetWeightInKG = entryLine.NetWeightInKG;
			NetWeightUnit = Messaging.Constants.DefaultWeightUnit;
			ValueForVAT = entryLine.CL_ValueForVAT;
			VAT = entryLine.Fees.GetAmount(Core.Constants.Customs.CusEntryFeeTypes.VAT);

			ModelName = InvoiceLine.JI_Description;
			DutyReduction = InvoiceLine.DutyReductionClassificationCode;
			SerialNumber = InvoiceLine.JI_SerialNumber;
			DutyReductionCode = InvoiceLine.JI_SecondaryPreference;
			InstalmentCode = InvoiceLine.JI_InstallmentCode;
			SpecificUse = InvoiceLine.JI_IsSpecificUseCode;
			PostClearanceYN = InvoiceLine.JI_PCProcedure;
			Preference = InvoiceLine.JI_PrimaryPreference;
			PreferenceDescription = InvoiceLine.PreferenceCodeDescription;
			Remark = InvoiceLine.AdditionalInformationContent;
			UseCodeDescription = InvoiceLine.JI_SpecificUseCodeDescription;
			ProductType = InvoiceLine.JI_SpecificUseProductType;
			CustomsOffice = InvoiceLine.JI_JurisdictionalCusOffice;
			GoodsLocation = InvoiceLine.JI_OA_ConsigneeAddress;
			GroupNumber = InvoiceLine.DutyReductionGroupNumber;
			SequenceNumber = InvoiceLine.DutyReductionSeqNumber;
			ItemNumber = InvoiceLine.DutyReductionItemNumber;
			ReExportCustomsOffice = InvoiceLine.JI_ScheduledReExportCustomsOffice;
			DestinationCountry = InvoiceLine.JI_RN_NKReExportDestinationCountry;
			EstimateDate = InvoiceLine.JI_ScheduledReExportDate;
			FTASequenceNumber = entryLine.CL_FTASequenceNumber;
			SplitOrder = InvoiceLine.COOSplitOrder;
			CountryOfOrigin = InvoiceLine.JI_CountryOfOrigin;
			TariffRate = entryLine.DutyRate;
			ThirdCountryAdditionalInvoiceIssued = InvoiceLine.JI_RN_NKSecondCommercialInvoiceCountry.IsEmpty ? Constants.YesNo.No : Constants.YesNo.Yes;
			ThirdCountry = InvoiceLine.JI_RN_NKSecondCommercialInvoiceCountry;
			COOExporterNumber = InvoiceLine.InvoiceHeader?.Supplier?.CustomsCodes?.GetCustomsRegNo(Constants.IdentificationType.CertificateOfOriginExporterNumber) ?? ZString.Empty;
			COOSupportingDocType = InvoiceLine.JI_COOSupportingDocType;
			COONo = InvoiceLine.CertificateOfOriginNo;
			COOIssuedDate = InvoiceLine.CertificateOfOriginIssueDate;
			COOProductType = InvoiceLine.CertificateOfOriginIssueStatus;
			AssociatedCOOIssuingCountryCode = InvoiceLine.CertificateOfOriginIssuingCountry;
			COOAgencyName = InvoiceLine.CertificateOfOriginAgencyName;
			COOIssuingAgencyType = CertifiticateOfOriginIssuedTypeList.GetCertifiticateOfOriginIssuingAgencyType(InvoiceLine.COOIssuerType);
			COOIssuerType = CertifiticateOfOriginIssuedTypeList.GetCertifiticateOfOriginIssuerType(InvoiceLine.COOIssuerType);
			TotalNetWeight = entryLine.CertificateOfOriginTotalNetWeightInKG;
			NetWeightInGrams = entryLine.NetWeightInGrams;
			ReExportDestinationDescription = Universal.ZZRefCusMapCombined.MapCW1CodeToCustomsCode(Factory, Core.Constants.CountryCodes.KoreaSouth, Messaging.Constants.ZZ.RefCusMap.CountryKRCCode, DestinationCountry, ZDateTime.Now);
		}

		public void DecorateFrom5FNCusEntryLine(CusEntryNumber entryNum, EDIMessage message)
		{
			MessageStatus = entryNum.CE_EntryStatus;
			MessageStatusDesc = entryNum.EntryStatusDescription;
			AcceptedDate = entryNum.CE_IssueDate;

			if (message != null)
			{
				EntryLineNo = ZInt.ParseSafe(message.EM_ApplicationReference, 0);

				Declaration declaration = null;

				using (var reader = message.GetEM_MessageTextReader())
				{
					declaration = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<Declaration>(reader);
				}

				HSCode = declaration.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.Classification.Id.Value;
				var tariffView = new Universal.TariffView.Loader(Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem, HSCode, entryNum.CE_IssueDate);
				HSDescription = tariffView?.ZZ1_Description.Substring(0, 150) ?? ZString.Empty;
				DutyReduction = declaration.TransactionNatureCode.Value;
				PostClearanceYN = declaration.AdditionalInformation.StatementCode.Value;
				FormattedHSCode = new TariffFormatter().DisplayFormat(HSCode);
				GoodsLocationAddressLine = string.Join(" ", declaration.GoodsShipment.Warehouse?.Address?.Description?.Value, declaration.GoodsShipment.Warehouse?.Address?.Line?.Value);
				GoodsLocationTelNo = declaration.GoodsShipment.Warehouse?.Communication?.Id.Value;
				var postCode = declaration.GoodsShipment.Warehouse?.Address?.PostcodeId?.Value;
				FormattedGoodsLocationPostcode = MessageFunctions.GetFormattedNumber(postCode, [0, 3]);
				ModelName = declaration.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.Name?.Value;
				SerialNumber = declaration.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.LotNumberId?.Value;
				UseCodeDescription = declaration.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.IntendedUse?.Value;
				CustomsOffice = declaration.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.ResponsibleGovernmentAgency?.Id.Value;
				ReExportCustomsOffice = declaration.GoodsShipment.ExitOffice?.Id.Value;
				DestinationCountry = declaration.GoodsShipment.Consignment?.GoodsConsignedPlace?.Id.Value;
				GroupNumber = declaration.GoodsShipment.GovernmentAgencyGoodsItem.AdditionalDocument?.TypeCode.Value;
				SequenceNumber = declaration.GoodsShipment.GovernmentAgencyGoodsItem.AdditionalDocument?.SequenceNumeric.ToString();
				ItemNumber = declaration.GoodsShipment.GovernmentAgencyGoodsItem.AdditionalDocument?.Id.Value;
				if (DateTime.TryParseExact(declaration.GoodsShipment.Consignment?.BorderTransportMeans?.DepartureDateTime, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var estimateDate))
				{
					EstimateDate = new ZDateTime(estimateDate);
				}
				ReExportDestinationDescription = Universal.ZZRefCusMapCombined.MapCW1CodeToCustomsCode(Factory, Core.Constants.CountryCodes.KoreaSouth, Messaging.Constants.ZZ.RefCusMap.CountryKRCCode, DestinationCountry, ZDateTime.Now);
				Remark = declaration.GoodsShipment.GovernmentAgencyGoodsItem.AdditionalInformation?.Content.Value;
			}
		}

		public MessageSendingValidation MessageSendingValidation
		{
			get
			{
				if (fMessageSendingValidation == null)
				{
					fMessageSendingValidation = MessageSendingValidation.New(Entry.Declaration, EntryLineNotificationCollector?.GetMessageErrors() ?? System.Array.Empty<INotification>(), Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed);
				}
				return fMessageSendingValidation;
			}
		}
		MessageSendingValidation fMessageSendingValidation;

		protected ZGuid entryLinePK;
		protected JobDeclarationEntryLineSelectiveNotificationCollector EntryLineNotificationCollector
		{
			get
			{
				if (fEntryLineNotificationCollector == null)
				{
					var entryLine = Factory.Load<CusEntryLine>(entryLinePK);
					if (entryLine != null)
					{
						fEntryLineNotificationCollector = new JobDeclarationEntryLineSelectiveNotificationCollector(Entry.Declaration, entryLine);
					}
				}
				return fEntryLineNotificationCollector;
			}
		}
		JobDeclarationEntryLineSelectiveNotificationCollector fEntryLineNotificationCollector;

		public void DecorateFromImportEntryHeader(ImportEntryHeader entryHeader)
		{
			var importEntryLine = entryHeader.EntryLines.FirstOrDefault(x => x.EntryLineNo == EntryLineNo);
			if (importEntryLine != null)
			{
				DutyReductionOrInstallmentCode = importEntryLine.DutyReductionOrInstallmentCode;
				ItemDescription = importEntryLine.InvoiceLines[0].ItemDescription;
				CustomsQuantity = importEntryLine.Quantity;
				CustomsUnitQty = importEntryLine.QuantityUnit;
				CustomsValue = importEntryLine.CustomsValueUSD;
				DutyReductionAmount = importEntryLine.DutyReductionAmount;
			}
		}

		public ZString BizObjValidationMessageErrors => ShouldSend ? GetBizObjValidationMessageErrors() : ZString.Empty;
		ZString GetBizObjValidationMessageErrors()
		{
			return Regex.Replace(MessageSendingValidation.CheckBusinessObjectLevelValidation().NotificationsAsString(), "(?<!\r)\n", "\r\n");
		}
	}
}
