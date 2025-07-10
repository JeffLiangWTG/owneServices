using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Business
{
	public class ExtendedHoursRequestHeader : AutoExtendedHoursRequestHeader, ISupportMultipleResourceStringData
	{
		public ExtendedHoursRequestHeader(BusinessObjectFactory factory, string messageType, ZGuid companyPK) : base(factory)
		{
			CheckOnCreate(messageType);
			CompanyPK = companyPK;
			MessageType = messageType;
		}

		void CheckOnCreate(string messageType)
		{
			if (messageType != ElectronicDocumentTypeList.Codes._5GW && messageType != ElectronicDocumentTypeList.Codes._5AC)
			{
				throw new ArgumentException($"Param MessageType value is '5GW' or '5AC'. But, sent value is {messageType}");
			}
		}

		public bool IsExport => MessageType == ElectronicDocumentTypeList.Codes._5AC;
		public bool IsImport => MessageType == ElectronicDocumentTypeList.Codes._5GW;
		public readonly ZGuid CompanyPK;
		public bool SendWithMessageErrorsIsAllowed => Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed;

		[List(nameof(Lookups) + "." + nameof(ExtendedHoursRequestHeaderLookups.MessageTypeList))]
		[ResourceStringData("46177C74-E67D-4F1F-8228-4489346C2530", Caption = "Message Type")]
		public ZString MessageType { get; private set; }
		public ZPropertyInfo MessageTypeInfo => GetZPropertyInfo(nameof(MessageType));

		public ExtendedHoursRequestLineCollection ExtendedHoursRequestLines
		{
			get
			{
				if (extendedHoursRequestLines == null)
				{
					extendedHoursRequestLines = new ExtendedHoursRequestLineCollection(this);
					RegisterEditableChildObject(extendedHoursRequestLines);
				}
				return extendedHoursRequestLines;
			}
		}
		ExtendedHoursRequestLineCollection extendedHoursRequestLines;

		protected override ExtendedHoursRequestHeaderValidation GetNewValidation() => new ExtendedHoursRequestHeaderValidation(this);

		public ExtendedHoursRequestHeaderLookups Lookups => new ExtendedHoursRequestHeaderLookups(this);

		#region Properties

		[List(nameof(Lookups) + "." + nameof(ExtendedHoursRequestHeaderLookups.CustomsOfficeList))]
		[ResourceStringData("9127DBF7-2181-4F14-9336-9BAF946622A7", Caption = "Customs Office")]
		public override ZString CustomsOffice
		{
			get => base.CustomsOffice;
			set => base.CustomsOffice = value;
		}

		[List(nameof(Lookups) + "." + nameof(ExtendedHoursRequestHeaderLookups.CustomsDivisonList))]
		[ResourceStringData("1002FA75-03A6-46C5-8CC5-A8FF6F865838", Caption = "Department")]
		public override ZString Department
		{
			get => base.Department;
			set => base.Department = value;
		}

		[ResourceStringData("EE0C26FE-7B3C-4DCC-8463-52AABD37FB9A", Caption = "Start Period")]
		public override ZDateTime StartDate
		{
			get => base.StartDate;
			set => base.StartDate = value;
		}

		[ResourceStringData("8005C172-95B2-4F3E-A9E9-74FDC952940D", Caption = "End Period")]
		public override ZDateTime EndDate
		{
			get => base.EndDate;
			set => base.EndDate = value;
		}

		[List(nameof(Lookups) + "." + nameof(ExtendedHoursRequestHeaderLookups.Branches))]
		[RelatedBusinessObject(nameof(Branch))]
		[ResourceStringData("333BB7B3-5C45-488F-8CB8-138B88742D9F", Caption = "Branch Code")]
		public override ZGuid BranchPK
		{
			get => base.BranchPK;
			set => base.BranchPK = value;
		}
		public GlbBranch Branch => Factory.Load<GlbBranch>(BranchPK);

		[ResourceStringData("CA4023DB-FF00-4D88-9AE3-A9028E3D32C4", Caption = "Request Reason")]
		public override ZString Reason
		{
			get => base.Reason;
			set => base.Reason = value;
		}

		#endregion

		public void PopulateFromMessage(EDIMessage message)
		{
			if (IsExport)
			{
				CargoWise.Customs.KR.MessageDefinitions.GOVCBR5AC.Declaration declaration = null;
				using (var reader = message.GetEM_MessageTextReader())
				{
					declaration = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<CargoWise.Customs.KR.MessageDefinitions.GOVCBR5AC.Declaration>(reader);
				}
				Reason = declaration.Reason.Value;

				foreach (var goodShipmentLine in declaration.GoodsShipment)
				{
					var entryLineData = ExtendedHoursRequestLines.AddNew();
					entryLineData.ReferenceNumber = goodShipmentLine.AdditionalDocument.Id.Value;
					entryLineData.CustomsValue = goodShipmentLine.GovernmentAgencyGoodsItem.Commodity.DutyTaxFee.AdValoremTaxBaseAmount.Value;
					entryLineData.PackageCount = (ZInt)goodShipmentLine.GovernmentAgencyGoodsItem.Commodity.CountQuantity.Value;
					entryLineData.TotalWeight = goodShipmentLine.GovernmentAgencyGoodsItem.Commodity.SizeMeasure.Value;
					entryLineData.SupplierName = goodShipmentLine.Exporter.Name.Value;
				}
			}
			else
			{
				CargoWise.Customs.KR.MessageDefinitions.GOVCBR5GW.Declaration declaration = null;
				using (var reader = message.GetEM_MessageTextReader())
				{
					declaration = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<CargoWise.Customs.KR.MessageDefinitions.GOVCBR5GW.Declaration>(reader);
				}
				Reason = declaration.Reason?.Value ?? ZString.Empty;

				foreach (var goodShipmentLine in declaration.GoodsShipment)
				{
					var entryLineData = ExtendedHoursRequestLines.AddNew();
					entryLineData.ReferenceNumberType = ReferenceNumberTypeList.GetMappedCW1CodeFromCustomsCode(goodShipmentLine.AdditionalDocument.TypeCode.Value);
					entryLineData.ReferenceNumber = goodShipmentLine.AdditionalDocument.Id.Value;
					entryLineData.HSDescription = goodShipmentLine.GovernmentAgencyGoodsItem.Commodity.CargoDescription.Value;
					entryLineData.CustomsValue = goodShipmentLine.GovernmentAgencyGoodsItem.Commodity.DutyTaxFee.AdValoremTaxBaseAmount.Value;
					entryLineData.PackageCount = (ZInt)goodShipmentLine.GovernmentAgencyGoodsItem.Commodity.CountQuantity.Value;
					entryLineData.TotalWeight = goodShipmentLine.GovernmentAgencyGoodsItem.Commodity.SizeMeasure.Value;
					entryLineData.BondedAreaCode = goodShipmentLine.Warehouse.Id.Value;
					entryLineData.PayerCompanyName = goodShipmentLine.GovernmentAgencyGoodsItem.Payer.Name.Value;
				}
			}
		}

		public void PopulateEntryData()
		{
			var entryNumbers = ExtendedHoursRequestLines.Cast<ExtendedHoursRequestLine>().Select(x => x.ReferenceNumber);
			var krEntryHeaderDetailsViews = ExtendedHoursEntryDataRetriever.GetEntryHeaderDetailsForExtendedHoursRequest(Factory, CompanyPK, entryNumbers.ToArray());
			foreach (var entryHeader in krEntryHeaderDetailsViews)
			{
				var extendedHoursRequestLine = ExtendedHoursRequestLines.Cast<ExtendedHoursRequestLine>().FirstOrDefault(x => x.ReferenceNumber == entryHeader.KEH_EntryNum);
				var usdCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates);
				var krwCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.KoreaRepublicOf);
				var exchangeRateType = IsExport ? ExchangeRateType.CustomsSecondary : ExchangeRateType.Customs;
				var currencyConverter = CurrencyConverter.New(Factory, ZDateTime.Today, exchangeRateType, 0);
				var totalCustomsValueInUSD = currencyConverter.ConvertExact(new Money(entryHeader.KEH_TotalCustomsValueInKRW, krwCurrency), usdCurrency).Amount.Truncate();

				extendedHoursRequestLine.CustomsValue = totalCustomsValueInUSD;
				extendedHoursRequestLine.PackageCount = (int)entryHeader.KEH_ExportPackQty;
				extendedHoursRequestLine.TotalWeight = entryHeader.KEH_TotalWeightInKG;
				if (IsExport)
				{
					extendedHoursRequestLine.SupplierName = entryHeader.KEH_SupplierName.SubstringSafe(0, ExtendedHoursRequestLine.Schema.SupplierNameMaxLength);
				}
				else
				{
					extendedHoursRequestLine.UQ = entryHeader.KEH_PackType.SubstringSafe(0, ExtendedHoursRequestLine.Schema.UQMaxLength);
					extendedHoursRequestLine.ReferenceNumberType = ReferenceNumberTypeList.Codes.IMP;
					var tariffView = new TariffView.Loader(Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem, entryHeader.KEH_HSCode, ZDateTime.Today);
					extendedHoursRequestLine.HSDescription = tariffView?.ZZ1_Description ?? ZString.Empty;
					extendedHoursRequestLine.BondedAreaCode = entryHeader.KEH_BondedAreaCode.SubstringSafe(0, ExtendedHoursRequestLine.Schema.BondedAreaCodeMaxLength);
					extendedHoursRequestLine.PayerCompanyName = entryHeader.KEH_PayerName.SubstringSafe(0, ExtendedHoursRequestLine.Schema.PayerCompanyNameMaxLength);
				}
			}
		}
		public IReadOnlyList<string> MultipleKeysToUse => new string[] { MessageType };
	}
}
