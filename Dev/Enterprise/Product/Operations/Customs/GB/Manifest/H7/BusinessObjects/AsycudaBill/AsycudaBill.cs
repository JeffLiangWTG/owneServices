using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.GB.Business.Interfaces;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.GB.H7.Business
{
	[DependentBusinessObject(typeof(AsycudaManifestHeader), nameof(AsycudaManifestHeader.Bills))]
	public class AsycudaBill : EU.H7.Business.AsycudaBill,
		Integration.Customs.GB.GBH7.IAsycudaBill,
		IMessageAttachee
	{
		public AsycudaBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;

		public new ASYCUDA.Business.IAsycudaPackCollection<AsycudaPack, AsycudaBill> Packs => (ASYCUDA.Business.IAsycudaPackCollection<AsycudaPack, AsycudaBill>)base.Packs;

		public new IAsycudaBillPackedItemCollection<AsycudaPackedItem, AsycudaBill> PackedItems => (IAsycudaBillPackedItemCollection<AsycudaPackedItem, AsycudaBill>)base.PackedItems;

		public new AsycudaBillLookups Lookups => (AsycudaBillLookups)base.Lookups;

		protected override ManifestBase.AsycudaBillLookups GetNewLookups() => new AsycudaBillLookups(this);

		protected override AsycudaBillValidation GetNewValidationForRegularBill() => new AsycudaBillValidationForRegularBill(this);

		protected override AsycudaBillValidation GetNewValidationForMasterChild() => new AsycudaBillValidationForMasterChild(this);

		public override string RequiredCurrencyCode => CurrencyCodes.UnitedKingdom;

		public override void OnSaving()
		{
			base.OnSaving();
			PopulateLocalReferenceNumberIfNeeded();
			PopulateUCRNumberIfNeeded();
		}

		#region GenAddOn

		public new static class GenAddOnColumnConstants
		{
			public const string PostponedVatAccountingEntryColumnName = "PostponedVATAccounting";
		}

		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			ABL_Procedure = "40001H7";
			ABL_SellerRegNoType = OrgCusCode.EuropeanUnionSharedCodeTypes.ImportOneStopShopVatRegistration;
		}

		void PopulateLocalReferenceNumberIfNeeded()
		{
			if (LocalReferenceNumber.IsEmpty || (!IsInDatabase && !Globals.IsTest))
			{
				LocalReferenceNumber = Header.ApplicationExtender.GetNewEntryLocalReferenceNumber(Factory);
			}
		}

		void PopulateUCRNumberIfNeeded()
		{
			if (ABL_UCRNumber.IsEmpty && !IsInDatabase && ABL_BolType != ChildBolCode)
			{
				var year = ZDate.Today.Year.ToString();
				var ucrWithoutSuffix = string.Format("{0}{1}-{2}", ((ZString)year).Right(1), GetEoriForUCR(), Header.AMA_JobReference);
				var suffix = ABL_SequenceNumber > 1 ? "/" + (ABL_SequenceNumber - 1).ToString() : string.Empty;
				ABL_UCRNumber = ucrWithoutSuffix + suffix;
			}
		}

		ZString GetEoriForUCR()
		{
			var eori = ZString.Empty;
			var partyAddress = Header.Declarant ?? Consignee;
			if (partyAddress != null)
			{
				eori = partyAddress.GetEuIdentificationNumber();
			}

			if (eori.IsEmpty)
			{
				eori = partyAddress.Header.GetEuIdentificationNumber();
			}

			return eori;
		}

		protected override void CalculateShipmentTypeCore(ASYCUDA.Business.AsycudaBill bill)
		{
		}

		#region Properties

		[ResourceStringData("Enterprise.Customs.GB.H7.Business.AsycudaBill|ABL_BillStatus", Caption = "Customs Status")]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.CustomsStatusList))]
		public override ZString ABL_BillStatus
		{
			get => base.ABL_BillStatus;
			set => base.ABL_BillStatus = value;
		}

		[ResourceStringData("Enterprise.Customs.GB.H7.Business.AsycudaBill|ABL_MessageStatus", Caption = "Message Status")]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.MessageStatusList))]
		public override ZString ABL_MessageStatus
		{
			get => base.ABL_MessageStatus;
			set => base.ABL_MessageStatus = value;
		}

		[ReadOnly(true)]
		public override ZString ABL_UCRNumber
		{
			get => base.ABL_UCRNumber;
			set => base.ABL_UCRNumber = value;
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.SubStyleList))]
		[ResourceStringData("a80ed096-ba10-4e98-aa6e-de8a5486d694", FullDescription = "Code identifying both the type of declaration and whether or not the goods have arrived at the goods location.", Caption = "Additional Declaration Type", MediumCaption = "Add. Decl. Type", ShortCaption = "Add. Decl. Type")]
		public override ZString ABL_ShipmentType
		{
			get => base.ABL_ShipmentType;
			set => base.ABL_ShipmentType = value;
		}

		[ResourceStringData("Enterprise.Customs.GB.H7.Business.AsycudaBill.ABL_BillNumber", Caption = "Bill Number", ShortCaption = "Bill No.", MediumCaption = "Bill No.", FullDescription = "The Transport Document Number used to identify the relevant Consignment on the declaration.")]
		public override ZString ABL_BillNumber
		{
			get => base.ABL_BillNumber;
			set => base.ABL_BillNumber = value;
		}

		[LightValidationTestExempt]
		public override ZString ABL_RL_NKPortOfLoading
		{
			get => base.ABL_RL_NKPortOfLoading;
			set => base.ABL_RL_NKPortOfLoading = value;
		}

		[LightValidationTestExempt]
		public override ZString ABL_RL_NKPortOfDischarge
		{
			get => base.ABL_RL_NKPortOfDischarge;
			set => base.ABL_RL_NKPortOfDischarge = value;
		}

		public override ZGuid ABL_OA_Consignee
		{
			get => base.ABL_OA_Consignee;
			set
			{
				var oldValue = ABL_OA_Consignee;
				base.ABL_OA_Consignee = value;
				if (!IsCopying && oldValue != ABL_OA_Consignee)
				{
					VATNumberInfo.RefreshBinding();
					PostponedVatAccountingCheck = ZBool.False;
					Factory.InvalidateCachedProperties();
				}
			}
		}

		public new CusGoodsLocation CusGoodsLocation => (CusGoodsLocation)base.CusGoodsLocation;

		public ZString VATNumber => Factory.GetValue(ref vatNumber, GetVATNumber);
		CachedProperty<ZString> vatNumber;

		public ZPropertyInfo VATNumberInfo => GetZPropertyInfo(nameof(VATNumber));

		ZString GetVATNumber()
		{
			var vatCustomsCode = Consignee?.Header?.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.VATCode, CountryCodes.UnitedKingdom);
			return vatCustomsCode?.OK_CustomsRegNo ?? ZString.Empty;
		}

		[ReadOnlyMember(nameof(IsVATNumberEmpty))]
		public ZBool PostponedVatAccountingCheck
		{
			get => this.GetSystemDefinedValue<ZBool>(GenAddOnColumnConstants.PostponedVatAccountingEntryColumnName);
			set
			{
				var oldValue = PostponedVatAccountingCheck;
				if (oldValue != value)
				{
					this.SetSystemDefinedValue(GenAddOnColumnConstants.PostponedVatAccountingEntryColumnName, value);
				}

				PostponedVatAccountingCheckInfo.RefreshBinding();
			}
		}

		ZBool IsVATNumberEmpty => VATNumber.IsEmpty;

		public ZPropertyInfo PostponedVatAccountingCheckInfo => GetZPropertyInfo(nameof(PostponedVatAccountingCheck));

		#region Additional Documents

		public new IAdditionalDocumentCollection<AdditionalDocument> AdditionalDocuments => base.AdditionalDocuments;

		protected override IAdditionalDocumentCollection<AdditionalDocument> CreateNewAdditionalDocumentCollection() => new AdditionalDocumentCollection<AdditionalDocument>(this);

		#endregion

		#region Additiona Info

		public new IAdditionalInfoCollection<AdditionalInfo> AdditionalInfos => (IAdditionalInfoCollection<AdditionalInfo>)base.AdditionalInfos;
		protected override IAdditionalInfoCollection<EU.H7.Business.AdditionalInfo> CreateAdditionalInfoCollection() => new AdditionalInfoCollection<AdditionalInfo>(this);

		#endregion

		#region Supporting Documents

		[ChildEditable(true)]
		public new SupportingDocumentCollection<SupportingDocument> SupportingDocuments
		{
			get
			{
				if (supportingDocumentCollection == null)
				{
					supportingDocumentCollection = new SupportingDocumentCollection<SupportingDocument>(this);
					supportingDocumentCollection.Load();
					RegisterEditableChildObject(supportingDocumentCollection);
				}
				return supportingDocumentCollection;
			}
		}
		SupportingDocumentCollection<SupportingDocument> supportingDocumentCollection;

		#endregion

		#region Previous Documents

		public new IPreviousDocumentCollection<PreviousDocument> PreviousDocuments => (IPreviousDocumentCollection<PreviousDocument>)base.PreviousDocuments;

		protected override IPreviousDocumentCollection<EU.H7.Business.PreviousDocument> CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection<PreviousDocument>(this);

		#endregion

		#endregion

		protected override string GetReleasedStatusCore() => EntryStatusList.Codes.Clear;

		protected override string GetReleasedStatusDescriptionCore() => ResString.GetMultilingualString("0383366b-ce13-4f40-a0a4-832beabd527b", "Cleared");

		protected override Type GetPackedItemTypeCore() => typeof(AsycudaPackedItem);

		protected override IAsycudaBillPackedItemCollection<ManifestBase.AsycudaPackedItem, ManifestBase.AsycudaBill> CreateNewAsycudaBillPackedItemCollection() => new AsycudaBillPackedItemCollection<AsycudaPackedItem, AsycudaBill>(this);

		protected override Type GetPackTypeCore() => typeof(AsycudaPack);

		protected override IAsycudaPackCollection<ManifestBase.AsycudaPack, ManifestBase.AsycudaBill> CreateNewAsycudaPackCollection() => new ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill>(this);

		#region ICusSupportingInfoTypeSupporter

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypesCore()
		{
			var result = base.GetCusSupportingInfoTypesCore();
			result[H7CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(EU.H7.Business.AdditionalInfo);
			result[H7CusSupportingInfoTypeList.Codes.AdditionalDocument] = typeof(AdditionalDocument);
			result[H7CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
			result[H7CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(PreviousDocument);
			return result;
		}

		#endregion

		public override ZString ABL_ConsigneeRegNo
		{
			get
			{
				var consigneeRegNo = base.ABL_ConsigneeRegNo;
				if (Consignee != null)
				{
					consigneeRegNo = Consignee.Header.GetEoriNumber(CountryCodes.UnitedKingdom);
					if (!consigneeRegNo.IsEmpty
						&& !consigneeRegNo.StartsWith(CountryCodes.UnitedKingdom)
						&& !consigneeRegNo.StartsWith(CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes))
					{
						consigneeRegNo = CountryCodes.UnitedKingdom + consigneeRegNo;
					}
				}

				return consigneeRegNo;
			}
			set => base.ABL_ConsigneeRegNo = value;
		}

		#region IMessageAttachee

		GlbBranch IMessageAttachee.Branch => Header.Branch;

		string IMessageAttachee.JobNumber => ABL_BillNumber;

		ZString IMessageAttachee.JobReference => Header.AMA_JobReference;

		string IMessageAttachee.DataGroupingCode => GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

		ZString IMessageAttachee.Gateway => Header.CSP;

		void IMessageAttachee.UpdateStatusIfNotEmpty(ZString status)
		{
			if (!status.IsEmpty)
			{
				ABL_MessageStatus = status;
			}
		}

		#endregion
	}
}
