using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.DE.Business.MasterFiles
{
	public class CusClassPartPivot : EU.Business.MasterFiles.CusClassPartPivot, Integration.Customs.DE.ICusClassPartPivot
	{
		public CusClassPartPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		public new TaxForPivotCollection Taxes => (TaxForPivotCollection)base.Taxes;

		public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;

		public new AdditionalInfoCollection AdditionalInfos => (AdditionalInfoCollection)base.AdditionalInfos;

		public new PreviousDocumentCollection PreviousDocuments => (PreviousDocumentCollection)base.PreviousDocuments;

		protected override IDictionary<ZString, Type> GetCusAddInfoTypes()
		{
			var result = base.GetCusAddInfoTypes();
			result[CusAddInfoTypeAttribute.Codes.GBTax] = typeof(DETax_OnlyForPivot);
			return result;
		}

		public override ZString CI_TariffNum
		{
			get => base.CI_TariffNum;
			set
			{
				var oldValue = CI_TariffNum;
				base.CI_TariffNum = value;
				if (!IsCopying && oldValue != value && CI_ChildType == ClassificationType.EXP)
				{
					CI_SecondUnitQty = UniversalTariff?.UnitsOfMeasure.FirstOrDefault(x => x.ZZ8_Type == Constants.UnitOfMeasureTypes.AdditionalUOMType)?.ZZ8_UOM ?? ZString.Empty;
				}
			}
		}

		[ResourceStringData("14E46E7C-C059-4866-99A2-551D6FCAA694", ShortCaption = "[41] 2nd Qty", MediumCaption = "[41] Second Qty", Caption = "[41] Second Quantity")]
		[DecimalPlaces(5)]
		public override ZDecimal CI_SecondQty
		{
			get { return base.CI_SecondQty; }
			set { base.CI_SecondQty = value; }
		}

		[ResourceStringData("073E5721-26D5-4C75-A06F-5972BD602318", ShortCaption = "[41] 2nd UQ", MediumCaption = "[41] Second UQ", Caption = "[41] Second Unit Quantity")]
		public override ZString CI_SecondUnitQty
		{
			get { return base.CI_SecondUnitQty; }
			set { base.CI_SecondUnitQty = value; }
		}

		[ResourceStringData("A7555F69-439E-44A8-A45F-FB5A4C86FCE8", Caption = "Fourth Qty")]
		[DecimalPlaces(5)]
		public override ZDecimal CI_FourthQty
		{
			get => base.CI_FourthQty;
			set => base.CI_FourthQty = value;
		}

		[ResourceStringData("A517F616-F0E5-4D3A-B5B6-8DB6EF0B77BE", Caption = "Fifth Qty")]
		[DecimalPlaces(5)]
		public override ZDecimal CI_FifthQty
		{
			get => base.CI_FifthQty;
			set => base.CI_FifthQty = value;
		}

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = base.GetCusSupportingInfoTypes();
			result[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
			result[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
			result[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(PreviousDocument);
			return result;
		}

		protected override CusAddInfoCollection<EU.Business.Declaration.MultiLineAddInfos.Tax_CusAddInfoOnlyForPIVOT> CreateTaxCollection() => new TaxForPivotCollection(this);

		protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

		protected override EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection(this);

		protected override EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection(this, false);
	}
}
