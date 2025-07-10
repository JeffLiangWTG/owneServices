using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.GB.Business.MasterFiles
{
	public class CusClassPartPivot : EU.Business.MasterFiles.CusClassPartPivot, Integration.Customs.GB.ICusClassPartPivot,
		EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport
	{
		public CusClassPartPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override CusAddInfoCollection<EU.Business.Declaration.MultiLineAddInfos.Tax_CusAddInfoOnlyForPIVOT> CreateTaxCollection()
		{
			return new TaxOnlyForPivotCollection(this);
		}

		public new TaxOnlyForPivotCollection Taxes => (TaxOnlyForPivotCollection)base.Taxes;

		internal IEnumerable<EU.Business.Declaration.IEuTax> IEuTaxesForValidation
		{
			get
			{
				foreach (GBTaxOnlyForPivot t in Taxes)
				{
					yield return t.Data;
				}
			}
		}

		public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;

		protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection()
		{
			return new SupportingDocumentCollection(this);
		}

		public new AdditionalInfoCollection AdditionalInfos => (AdditionalInfoCollection)base.AdditionalInfos;

		protected override EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection CreateNewAdditionalInfoCollection()
		{
			return new AdditionalInfoCollection(this);
		}

		public new PreviousDocumentCollection PreviousDocuments => (PreviousDocumentCollection)base.PreviousDocuments;

		protected override EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection CreateNewPreviousDocumentCollection()
		{
			return new PreviousDocumentCollection(this);
		}

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = base.GetCusSupportingInfoTypes();
			result[Enterprise.Customs.Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
			result[Enterprise.Customs.Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(PreviousDocument);
			result[Enterprise.Customs.Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
			return result;
		}

		string EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport.DataGroupingCode => Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

		[ResourceStringData("4DDB0B18-964C-4B86-8B54-A16C32DA7AFB", Caption = "[37] CPC", MultipleKey = JobDeclaration.MultipleKeyChief)]
		[ResourceStringData("F8ECA4F5-246A-4738-8F1A-338C5376996C", Caption = "[UCC 1/10 & 1/11] Procedure")]
		[ReadOnlyMember(nameof(IsCPCReadOnly))]
		public override ZString CI_CPC { get => base.CI_CPC; set => base.CI_CPC = value; }

		bool IsCPCReadOnly => CI_CPC.IsEmpty && (IsClassificationBoth || CI_ChildType.IsEmpty);

		[ResourceStringData("04FF5D7A-FFCF-47E8-9483-2FDC7BF8B3BD", Caption = "[UCC 6/14 & 6/15] Commodity and TARIC", MultipleKey = JobDeclaration.MultipleKeyCdsImport)]
		[ResourceStringData("F0891A90-088E-4719-919C-6D6EFFECA433", Caption = "[UCC 6/14] Commodity", MultipleKey = JobDeclaration.MultipleKeyCdsExport)]
		[ResourceStringData("16358D3C-28A9-40B9-BE0C-ACF2C76A519A", Caption = "[33] Tariff", MultipleKey = JobDeclaration.MultipleKeyChief)]
		public override ZString CI_FormattedTariffNum { get => base.CI_FormattedTariffNum; set => base.CI_FormattedTariffNum = value; }

		[ResourceStringData("43B61DDA-E01B-4A27-80D5-635CF1BBA524", Caption = "[34] Country/Region of Origin", MediumCaption = "[34] Ctry/Rgn. of Origin", ShortCaption = "[34] C/R of Origin", MultipleKey = JobDeclaration.MultipleKeyChief)]
		[ResourceStringData("BB4B71B5-9988-4469-BE74-BA3DEC3F9221", Caption = "[UCC 5/15,16] Country/Region of (Preferential) Origin", MediumCaption = "[UCC 5/15,16] Ctry/Rgn. of (Preferential) Origin", ShortCaption = "[UCC 5/15,16] C/R of (Preferential) Origin", MultipleKey = JobDeclaration.MultipleKeyCdsImport)]
		[ResourceStringData("6D902C07-0FA0-4E48-AD84-33A95B80913D", Caption = "[UCC 5/16] Country/Region of Origin", MediumCaption = "[UCC 5/16] Ctry/Rgn. of Origin", ShortCaption = "[UCC 5/16] C/R of Origin", MultipleKey = JobDeclaration.MultipleKeyCdsExport)]
		public override ZString CI_RN_NKCountryOfOrigin { get => base.CI_RN_NKCountryOfOrigin; set => base.CI_RN_NKCountryOfOrigin = value; }

		[ResourceStringData("710914C4-03CA-47ED-841F-B2CB2D6A9363", Caption = "[39] Quota", MultipleKey = JobDeclaration.MultipleKeyChief)]
		[ResourceStringData("1A8F1597-397A-4D80-90FC-258DFF41A989", Caption = "[UCC 8/1] Quota")]
		public override ZString CI_ConcessionOrder { get => base.CI_ConcessionOrder; set => base.CI_ConcessionOrder = value; }

		[ResourceStringData("21B7CBFB-4D49-4D0F-952D-65C4DCC45BBB", Caption = "Second Qty")]
		public override ZDecimal CI_SecondQty { get => base.CI_SecondQty; set => base.CI_SecondQty = value; }

		[ResourceStringData("6941830D-C0D2-4D36-B46D-7A784D72352D", Caption = "[44] Third Qty", MultipleKey = JobDeclaration.MultipleKeyChief)]
		[ResourceStringData("C17282A4-3322-444D-81D4-843C832DD0C2", Caption = "Third Qty")]
		public override ZDecimal CI_ThirdQty { get => base.CI_ThirdQty; set => base.CI_ThirdQty = value; }

		[ResourceStringData("FFAB0851-81FC-47EC-B64B-C614F824E85E", Caption = "Fourth Qty")]
		public override ZDecimal CI_FourthQty { get => base.CI_FourthQty; set => base.CI_FourthQty = value; }

		[ResourceStringData("AADBF83A-BBCB-4026-9FBA-A4E5231FE2AC", Caption = "Fifth Qty")]
		public override ZDecimal CI_FifthQty { get => base.CI_FifthQty; set => base.CI_FifthQty = value; }

		[ResourceStringData("AE49A280-45E5-4090-B37E-3B0BEAB00B17", Caption = "[33] Supplement 1", MultipleKey = JobDeclaration.MultipleKeyChief)]
		[ResourceStringData("5A99CB00-C36D-4D46-B549-920854071301", Caption = "[UCC 6/16 & 6/17] Additional Code 1")]
		public override ZString CI_Supplement1 { get => base.CI_Supplement1; set => base.CI_Supplement1 = value; }

		[ResourceStringData("CBFCAD80-8BB9-4232-BCEE-F686969E6628", Caption = "[33] Supplement 2", MultipleKey = JobDeclaration.MultipleKeyChief)]
		[ResourceStringData("58B9E0F0-4ADD-42A9-8F8A-54161F3D202F", Caption = "[UCC 6/16 & 6/17] Additional Code 2")]
		public override ZString CI_Supplement2 { get => base.CI_Supplement2; set => base.CI_Supplement2 = value; }

		[ResourceStringData("E145D015-AAD7-476A-97E4-5D615E554C56", Caption = "[36] Pref. Code", MultipleKey = JobDeclaration.MultipleKeyChief)]
		[ResourceStringData("92C87EAF-CCD1-421E-A47E-BB6C5FE6665D", Caption = "[UCC 4/17] Pref. Code")]
		public override ZString PreferenceCode { get => base.PreferenceCode; set => base.PreferenceCode = value; }

		public override ZString DataGroupingCodeForAdditionalProcedures => ((EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport)this).DataGroupingCode;

		[ResourceStringData("59C6CB89-A149-4302-921E-BCCDCEEA1DE5", Caption = "SPIMM Category", ShortCaption = "SPIMM", MediumCaption = "SPIMM Category of Goods",
			FullDescription = "SPIMM (Simplified Procedure for Internal Market Movements) Category of Goods")]
		public override ZString CI_GoodsCategory { get => base.CI_GoodsCategory; set => base.CI_GoodsCategory = value; }

		protected override IDictionary<ZString, Type> GetCusAddInfoTypes()
		{
			var result = base.GetCusAddInfoTypes();
			result[CusAddInfoTypeAttribute.Codes.GBTax] = typeof(GBTaxOnlyForPivot);
			return result;
		}

		protected override bool UseUniversalTariff => false;

		protected override Customs.Business.CusClassPartPivotLookups GetNewLookups()
		{
			return new CusClassPartPivotLookups(this);
		}

		public new CusClassPartPivotLookups Lookups
		{
			get { return (CusClassPartPivotLookups)base.Lookups; }
		}

		#region ICusClassPartPivot Members

		Integration.Customs.GB.ITaxOnlyForPivotCollection Integration.Customs.GB.ICusClassPartPivot.Taxes => Taxes;

		Integration.Customs.GB.ISupportingDocumentCollection Integration.Customs.GB.ICusClassPartPivot.SupportingDocuments => SupportingDocuments;

		Integration.Customs.GB.IPreviousDocumentCollection Integration.Customs.GB.ICusClassPartPivot.PreviousDocuments => PreviousDocuments;

		Integration.Customs.GB.IAdditionalInfoCollection Integration.Customs.GB.ICusClassPartPivot.AdditionalInfos => AdditionalInfos;

		#endregion
	}
}
