using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using System.Collections.Generic;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Manifest.Business
{
	public class AsycudaBill : ASYCUDA.Business.AsycudaBill, Integration.Customs.ASYCUDA.BRManifest.IAsycudaBill
	{
		public AsycudaBill(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : ASYCUDA.Business.AsycudaBill.Schema
		{
			public const string DocumentType = "DocumentType";
			public const int DocumentTypeMaxLength = 3;
			public const string To_Order = "To_Order";
			public const string BL_Service = "BL_Service";
			public const string FRTMode = "FRTMode";
			public const int FRTModeMaxLength = 2;
			public const string CustomsOwnNumber = "CustomsOwnNumber";
		}

		AsycudaBillValidationForRegularBill RegularBillValidation => Validation as AsycudaBillValidationForRegularBill;

		protected override ManifestBase.AsycudaBillValidation GetNewValidationForRegularBill() => new AsycudaBillValidationForRegularBill(this);

		public new AsycudaBillLookups Lookups => (AsycudaBillLookups)base.Lookups;

		protected override ManifestBase.AsycudaBillLookups GetNewLookups() => new AsycudaBillLookups(this);

		public new AsycudaPackCollection Packs => (AsycudaPackCollection)base.Packs;

		protected override ManifestBase.IAsycudaPackCollection<ManifestBase.AsycudaPack, ManifestBase.AsycudaBill> CreateNewAsycudaPackCollection() => new AsycudaPackCollection(this);

		protected override Type GetPackTypeCore() => typeof(AsycudaPack);

		public new AsycudaTaxCollection AsycudaTaxes => (AsycudaTaxCollection)base.AsycudaTaxes;

		protected override ASYCUDA.Business.IAsycudaTaxCollection<ASYCUDA.Business.AsycudaTax, ASYCUDA.Business.AsycudaBill> CreateNewAsycudaTaxCollection() => new AsycudaTaxCollection(this);

		protected override Type GetAsycudaTaxTypeCore() => typeof(AsycudaTax);

		#region ABL_RN_NKSellerCountry

		[ResourceStringData("f6f5be1e-c01c-47ae-8f61-64201f3518d2", Caption = "Seller Country")]
		public override ZString ABL_RN_NKSellerCountry
		{
			get => base.ABL_RN_NKSellerCountry;
			set => base.ABL_RN_NKSellerCountry = value;
		}

		#endregion

		#region DocumentType

		[ResourceStringData("BR.Manifest.Business.AsycudaBill.DocumentType", Caption = "Document Type")]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.DocumentTypeList))]
		[MaxLength(Schema.DocumentTypeMaxLength)]
		public ZString DocumentType
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.DocumentType);
			set
			{
				var oldValue = DocumentType;
				CheckMaximumLength(DocumentTypeInfo, value);
				this.SetSystemDefinedValue(Schema.DocumentType, value);
				if (!IsValidationSuspended)
				{
					RegularBillValidation.ValidateDocumentType();
				}
				DocumentTypeInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo DocumentTypeInfo => GetZPropertyInfo(Schema.DocumentType);

		#endregion

		#region HBLToOrder

		[ResourceStringData("AsycudaBill.To_Order", Caption = "HBL To Order")]
		public ZBool To_Order
		{
			get => this.GetSystemDefinedValue<ZBool>(Schema.To_Order);
			set
			{
				var oldValue = To_Order;
				this.SetSystemDefinedValue(Schema.To_Order, value);
				To_OrderInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo To_OrderInfo => GetZPropertyInfo(Schema.To_Order);

		#endregion

		#region FRTMode

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.FRTModes))]
		[ResourceStringData("AsycudaBill.FRTMode", Caption = "Freight Mode")]
		public ZString FRTMode
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.FRTMode);
			set
			{
				var oldValue = FRTMode;
				CheckMaximumLength(FRTModeInfo, value);

				this.SetSystemDefinedValue(Schema.FRTMode, value);

				if (!IsValidationSuspended)
				{
					RegularBillValidation.ValidateFRTMode();
				}

				FRTModeInfo.RefreshBinding(oldValue);
			}
		}

		#endregion

		public ZPropertyInfo FRTModeInfo => GetZPropertyInfo(Schema.FRTMode);

		#region BLService

		[ResourceStringData("AsycudaBill.BL_Service", Caption = "BL Service")]
		public ZBool BL_Service
		{
			get => this.GetSystemDefinedValue<ZBool>(Schema.BL_Service);
			set
			{
				var oldValue = To_Order;
				this.SetSystemDefinedValue(Schema.BL_Service, value);
				BL_ServiceInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo BL_ServiceInfo => GetZPropertyInfo(Schema.BL_Service);

		#endregion

		public bool IsMercante => ((AsycudaManifestHeader)Header)?.IsMercante ?? false;

		public override bool CanDelete => true;

		public override MultilingualString GetWarningBeforeBeingDeleted()
		{
			MultilingualString result = (NoResString)string.Empty;

			return Header.AMA_MessageStatus == MessageStatusCodeList.Codes.Accepted || Header.AMA_MessageStatus == MessageStatusCodeList.Codes.Sent || Header.AMA_MessageStatus == MessageStatusCodeList.Codes.Cancel
				? ResString.GetMultilingualString("E5E4AA24-BEB5-44CC-94D1-77400796D7B8", "This Bill is already sent to Customs.")
				: result;
		}

		public override ZString[] ConsigneeRegNoTypes() => IsMercante ? RegNoTypes(ABL_RN_NKConsigneeCountry) : base.ConsigneeRegNoTypes();

		public override ZString[] NotifyPartyRegNoTypes() => IsMercante ? RegNoTypes(ABL_RN_NKNotifyPartyCountry) : base.NotifyPartyRegNoTypes();

		ZString[] RegNoTypes(ZString countryCode)
		{
			var regNoTypes = new List<ZString> { BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration };
			if (countryCode != Enterprise.Core.Constants.CountryCodes.Brazil)
			{
				regNoTypes.Add(OrgCusCode.CodeTypes.PassportID);
			}
			return regNoTypes.ToArray();
		}

		#region CEMercante
		[MaxLength(Enterprise.Customs.Common.CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		[ResourceStringData("BRAsycudaBill.CustomsOwnNumber", ShortCaption = "CE Merchant", Caption = "CE Merchant")]
		public ZString CustomsOwnNumber
		{
			get => CustomsOwnEntryNumber?.CE_EntryNum ?? ZString.Empty;
			set
			{
				var cusEntryNumber = CustomsOwnEntryNumber;
				var oldValue = cusEntryNumber?.CE_EntryNum ?? ZString.Empty;
				if (!value.IsEmpty)
				{
					if (cusEntryNumber == null)
					{
						customsOwnEntryNumber = Common.CusEntryNumber.LoadOrCreate(this, CusEntryNumberTypes.Brazil.CustomsOwnNumber, Header.AMA_RN_NKCountry);
						RegisterEditableChildObject(customsOwnEntryNumber);
					}
					customsOwnEntryNumber.CE_EntryNum = value;
				}
				else
				{
					if (cusEntryNumber != null)
					{
						customsOwnEntryNumber.Delete();
						customsOwnEntryNumber = null;
					}
				}

				if (!IsValidationSuspended && !this.IsChildMasterBill)
				{
					RegularBillValidation.ValidateCustomsOwnNumber();
				}

				CustomsOwnNumberInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CustomsOwnNumberInfo
		{
			get { return GetZPropertyInfo(Schema.CustomsOwnNumber); }
		}

		CusEntryNumber CustomsOwnEntryNumber
		{
			get
			{
				if (customsOwnEntryNumber == null || customsOwnEntryNumber.IsDeleted)
				{
					customsOwnEntryNumber = Common.CusEntryNumber.Load(this, CusEntryNumberTypes.Brazil.CustomsOwnNumber, Header.AMA_RN_NKCountry);
					if (customsOwnEntryNumber != null)
					{
						RegisterEditableChildObject(customsOwnEntryNumber);
					}
				}
				return customsOwnEntryNumber;
			}
		}
		CusEntryNumber customsOwnEntryNumber;

		#endregion

		public override void Delete()
		{
			if (!IsDeleted)
			{
				this.DeleteChildren<CusEntryNumber>(CusEntryNumSchema.CE_ParentID);
			}
			base.Delete();
		}
	}
}
