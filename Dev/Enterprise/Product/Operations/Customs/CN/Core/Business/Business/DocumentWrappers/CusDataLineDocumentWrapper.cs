using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.DocumentWrappers;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;

namespace Enterprise.Customs.CN.Business
{
	public class CusDataLineDocumentWrapper : DocBaseWrapper
	{
		public CusDataLineDocumentWrapper(CusEntryLine entryLine) : base(entryLine, entryLine.Factory)
		{
			cusDataLine = entryLine;
		}
		readonly ICustomsEntryLine cusDataLine;
		public CusEntryLine EntryLine => ParentBusinessObject as CusEntryLine;

		#region ICustomsEntryLine

		public ZShort EntryLineNo => cusDataLine.EntryLineNo;
		public ZInt ProductManualNo => cusDataLine.ProductManualNo;

		public ZString Tariff => cusDataLine.TariffCode;
		public ZString CIQSupplementCode => EntryLine.CIQSupplementCode;

		public CodeAndDescriptionWrapper CIQTariff => fCIQTariff ?? (fCIQTariff = CodeAndDescriptionWrapper.New(EntryLine.CIQTariffCode, EntryLine.CIQTariffDescription, Factory));
		CodeAndDescriptionWrapper fCIQTariff;

		public ZString NameOfGoods => cusDataLine.NameOfGoods;
		public ZString GoodsSpecModel => cusDataLine.GoodsSpecModel;

		public ZDecimal TradeQuantity => cusDataLine.TradeQuantity;
		public CodeAndDescriptionWrapper TradeUnitQty => fTradeUnitQty ?? (fTradeUnitQty = CodeAndDescriptionWrapper.New(EntryLine.TradeUnitQty, EntryLine.TradeUnitQtyDesc, Factory));
		CodeAndDescriptionWrapper fTradeUnitQty;

		public ZDecimal CustomsQuantity => cusDataLine.CustomsQuantity;
		public CodeAndDescriptionWrapper CustomsUnitQty => fCustomsUnitQty ?? (fCustomsUnitQty = CodeAndDescriptionWrapper.New(EntryLine.CustomsUnitQty, EntryLine.CustomsUnitQtyDescription, Factory));
		CodeAndDescriptionWrapper fCustomsUnitQty;

		public ZDecimal CustomsSecondQuantity => cusDataLine.CustomsSecondQuantity;
		public CodeAndDescriptionWrapper CustomsSecondUnit => fCustomsSecondUnit ?? (fCustomsSecondUnit = CodeAndDescriptionWrapper.New(EntryLine.CustomsSecondUnit, EntryLine.CustomsSecondUnitDesc, Factory));
		CodeAndDescriptionWrapper fCustomsSecondUnit;

		public CodeAndDescriptionWrapper GoodsOrigin => fGoodsOrigin ?? (fGoodsOrigin = CodeAndDescriptionWrapper.New(EntryLine.GoodsOriginCode, EntryLine.GoodsOriginName, Factory));
		CodeAndDescriptionWrapper fGoodsOrigin;

		public CodeAndDescriptionWrapper OriginState => fOriginState ?? (fOriginState = CodeAndDescriptionWrapper.New(EntryLine.OriginStateCode, EntryLine.OriginStateName, Factory));
		CodeAndDescriptionWrapper fOriginState;

		public CodeAndDescriptionWrapper GoodsDest => fGoodsDest ?? (fGoodsDest = CodeAndDescriptionWrapper.New(EntryLine.GoodsDestCode, EntryLine.GoodsDestName, Factory));
		CodeAndDescriptionWrapper fGoodsDest;

		public CodeAndDescriptionWrapper Currency => fCurrency ?? (fCurrency = CodeAndDescriptionWrapper.New(EntryLine.CurrencyCode, EntryLine.CurrencyDesc, Factory));
		CodeAndDescriptionWrapper fCurrency;

		public CodeAndDescriptionWrapper DutyMode => fDutyMode ?? (fDutyMode = CodeAndDescriptionWrapper.New(EntryLine.DutyModeCode, EntryLine.DutyModeDesc, Factory));
		CodeAndDescriptionWrapper fDutyMode;

		public CodeAndDescriptionWrapper DomesticDistrict => fDomesticDistrict ?? (fDomesticDistrict = CodeAndDescriptionWrapper.New(EntryLine.DomesticDistrictCode, EntryLine.DomesticDistrictName, Factory));
		CodeAndDescriptionWrapper fDomesticDistrict;

		public CodeAndDescriptionWrapper DomesticRegion => fDomesticRegion ?? (fDomesticRegion = CodeAndDescriptionWrapper.New(EntryLine.DomesticRegionCode, EntryLine.DomesticRegionName, Factory));
		CodeAndDescriptionWrapper fDomesticRegion;

		public ZDecimal UnitPrice => cusDataLine.UnitPrice;
		public ZDecimal TotalPrice => cusDataLine.TotalPrice;

		public ZString ProductCode => cusDataLine.ProductCode;
		public ZString ProductVersion => cusDataLine.ProductVersion;

		public ZString CertOfOriginNumber => cusDataLine.CertOfOriginNumber;

		CodeAndDescriptionWrapper fTradeAgreementCode;
		public CodeAndDescriptionWrapper TradeAgreementCode => fTradeAgreementCode ?? (fTradeAgreementCode = CodeAndDescriptionWrapper.New(cusDataLine.TradeAgreementCode, EntryLine.TradeAgreementDesctiption, Factory));

		public CodeAndDescriptionWrapper CertOfOriginCountry
		{
			get
			{
				if (fCertOfOriginCountry == null)
				{
					var refCountry = RefCountry.LoadFromCountryCode(Factory, cusDataLine.CertOfOriginCountry);
					fCertOfOriginCountry = CodeAndDescriptionWrapper.New(refCountry.GetCNCountryCode(), refCountry.GetCNCountryName(), Factory);
				}
				return fCertOfOriginCountry;
			}
		}
		CodeAndDescriptionWrapper fCertOfOriginCountry;

		public ZInt ItemNoOnCertOfOrigin => cusDataLine.ItemNoOnCertOfOrigin;

		public CodeAndDescriptionWrapper CertOfOriginType => fCertOfOriginType ?? (fCertOfOriginType = CodeAndDescriptionWrapper.New(cusDataLine.CertOfOriginType, EntryLine.CertOfOriginTypeDescription, Factory));
		CodeAndDescriptionWrapper fCertOfOriginType;

		#endregion

		#region CIQ Data

		public CodeAndDescriptionWrapperCollection CargoAttribute => fCargoAttribute ?? (fCargoAttribute = CodeAndDescriptionWrapperCollection.New(cusDataLine.CargoAttributes, Factory.GetCachedValue<CargoAttributeList>(), Factory));
		CodeAndDescriptionWrapperCollection fCargoAttribute;
		public ZString CargoAttributeCodes => ZString.Join(",", CargoAttribute.Cast<CodeAndDescriptionWrapper>().Select(att => att.Code).ToArray());

		public CodeAndDescriptionWrapper EndUse => fEndUse ?? (fEndUse = CodeAndDescriptionWrapper.New(cusDataLine.EndUse, Factory.GetCachedValue<EndUseList>(), Factory));
		CodeAndDescriptionWrapper fEndUse;

		public ZString Brand => cusDataLine.Brand;
		public ZString Model => cusDataLine.Model;
		public ZString Specification => cusDataLine.Specification;
		public ZInt QGPByDays => cusDataLine.QGPByDays;
		public ZDateTime ExpiryDate => cusDataLine.ExpiryDate;
		public ZString Ingredient => cusDataLine.Ingredient;
		public ZString BatchNumber => cusDataLine.BatchNumber;
		public ZDateTime ManufactureDate => cusDataLine.ManufactureDate;
		public AddressWrapper Manufacturer => fManufacturer ?? (fManufacturer = new AddressWrapper(EntryLine.Manufacturer, ContactType.NoContactType, Factory));
		AddressWrapper fManufacturer;

		public ZString ManufacturerCIQ => cusDataLine.ManufacturerCIQ;
		public ZString ManufacturerName => cusDataLine.ManufacturerName;

		public CodeAndDescriptionWrapper NonDangerousChemical => fNonDangerousChemical ?? (fNonDangerousChemical = CodeAndDescriptionWrapper.New(Factory, cusDataLine.NonDangerousChemical));
		CodeAndDescriptionWrapper fNonDangerousChemical;

		public ZString UNDGNumber => cusDataLine.UNDGNumber.SubstringSafe(0, 4);

		public ZString UNDGClass => cusDataLine.UNDGClass;

		public ZString UNDGPackingGroup => cusDataLine.UNDGNumber.IsEmpty ? string.Empty : CNCustomsPackingGroupList.GetDescriptionFromCode(cusDataLine.UNDGPackingGroup);

		public CodeDescriptionPairList CNCustomsPackingGroupList => Factory.GetCachedValue("CNCustomsUNDGPackingGroupList", () =>
		{
			var list = new CodeDescriptionPairList();
			list.AddPair(PackingGroupTypes.HighDangerCode, Constants.CNCustomsUNDGPackingGroups.HighDanger);
			list.AddPair(PackingGroupTypes.MediumDangerCode, Constants.CNCustomsUNDGPackingGroups.MediumDanger);
			list.AddPair(PackingGroupTypes.LowDangerCode, Constants.CNCustomsUNDGPackingGroups.LowDanger);
			list.AddPair(string.Empty, Constants.CNCustomsUNDGPackingGroups.Unknown);

			return list;
		});

		public CodeAndDescriptionWrapper UNDGPackageType => fUNDGPackageType ?? (fUNDGPackageType = CodeAndDescriptionWrapper.New(cusDataLine.UNDGPackageType, Factory.GetCachedValue<UNDGPackageTypeList>(), Factory));
		CodeAndDescriptionWrapper fUNDGPackageType;

		public BusinessObjectCollectionWrapper<EntryLineProductQualification> ProductQualifications
		{
			get
			{
				if (fProductQualifications == null)
				{
					fProductQualifications = new BusinessObjectCollectionWrapper<EntryLineProductQualification>(EntryLine.ProductQualifications.Cast<EntryLineProductQualification>());
				}
				return fProductQualifications;
			}
		}
		BusinessObjectCollectionWrapper<EntryLineProductQualification> fProductQualifications;

		#endregion

		#region For Customs Invoice Document

		public ZDecimal NetWeightInKG => EntryLine?.InvoiceLines.Cast<JobComInvoiceLine>().Sum(invoiceline => invoiceline.NetWeightInKG) ?? ZDecimal.Zero;

		public ZDecimal GrossWeightInKG => EntryLine?.InvoiceLines.Cast<JobComInvoiceLine>().Sum(invoiceline => invoiceline.GrossWeightInKG) ?? ZDecimal.Zero;

		#endregion
	}
}
