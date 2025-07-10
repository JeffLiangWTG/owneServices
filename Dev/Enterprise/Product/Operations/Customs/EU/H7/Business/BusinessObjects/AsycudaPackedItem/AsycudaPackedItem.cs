using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.EU.H7.Business
{
	public class AsycudaPackedItem :
		ASYCUDA.Business.AsycudaPackedItem,
		ICusSupportingInfoTypeSupporter,
		IDataGroupingProvider,
		ICanBeImportOrExport
	{
		public AsycudaPackedItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new AsycudaBill Bill => (AsycudaBill)base.Bill;

		public ZString DataGrouping
		{
			get
			{
				if (!dataGroupingCached.HasValue)
				{
					dataGroupingCached = Bill?.DataGrouping ?? ZString.Empty;
				}
				return dataGroupingCached.Value;
			}
		}
		ZString? dataGroupingCached;

		public new AsycudaPackedItemLookups Lookups => (AsycudaPackedItemLookups)base.Lookups;

		protected override ManifestBase.AsycudaPackedItemLookups GetNewLookups() => new AsycudaPackedItemLookups(this);

		public new AsycudaPackedItemValidation Validation => (AsycudaPackedItemValidation)base.Validation;

		protected override ManifestBase.AsycudaPackedItemValidation GetNewValidation() => new AsycudaPackedItemValidation(this);

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
			}
			base.Delete();
		}

		#region Properties

		[List(nameof(Lookups) + "." + nameof(AsycudaPackedItemLookups.CustomsUQListForSupplement))]
		[ResourceStringData("EUH7.AsycudaPackedItem.API_CustomsUQ2", Caption = "Supplementary UQ", MediumCaption = "Suppl. UQ.", ShortCaption = "UQ", FullDescription = "The measurement unit associated with the Supplementary Unit quantity.")]
		public override ZString API_CustomsUQ2 { get => base.API_CustomsUQ2; set => base.API_CustomsUQ2 = value; }

		[ResourceStringData("EUH7.AsycudaPackedItem.API_CustomsQty2", Caption = "Supplementary Units", MediumCaption = "Suppl. Units", ShortCaption = "Suppl.", FullDescription = "The quantity of the item in question, expressed in the unit laid down in European Union Legislation, as published in TARIC. To be provided if the declaration concerns goods referred to in Article 27 of Regulation (EC) No 1186/2009.")]
		public override ZDecimal API_CustomsQty2 { get => base.API_CustomsQty2; set => base.API_CustomsQty2 = value; }

		[ResourceStringData("EUH7.AsycudaPackedItem.API_GoodsValue", Caption = "Intrinsic Value", ShortCaption = "Int. Val.", MediumCaption = "Int. Val.", FullDescription = "Intrinsic value of the goods.")]
		public override ZDecimal API_GoodsValue { get => base.API_GoodsValue; set => base.API_GoodsValue = value; }

		[ResourceStringData("EUH7.AsycudaPackedItem.API_RX_NKGoodsValueCurrency", Caption = "Intrinsic Value Currency", ShortCaption = "Curr.", MediumCaption = "Currency", FullDescription = "Currency code associated with the Intrinsic Value.")]
		public override ZString API_RX_NKGoodsValueCurrency { get => base.API_RX_NKGoodsValueCurrency; set => base.API_RX_NKGoodsValueCurrency = value; }

		[ResourceStringData("EUH7.AsycudaPackedItem.API_FormattedTariff", Caption = "Tariff", ShortCaption = "Tariff", MediumCaption = "Tariff", FullDescription = "Integrated Tariff of the European Communities (TARIC) code associated with the item.")]
		public override ZString API_FormattedTariff { get => base.API_FormattedTariff; set => base.API_FormattedTariff = value; }

		[ResourceStringData("EUH7.AsycudaPackedItem.API_GoodsDescription", Caption = "Goods Description", ShortCaption = "Desc.", MediumCaption = "Description", FullDescription = "Free-form description of the goods.")]
		public override ZString API_GoodsDescription { get => base.API_GoodsDescription; set => base.API_GoodsDescription = value; }

		[ResourceStringData("EUH7.AsycudaPackedItem.API_MessageStatus", Caption = "Message Status", ShortCaption = "Msg. Status", MediumCaption = "Msg. Status", FullDescription = "Code identifying the status of the last customs declaration message sent to the relevant Customs authority.")]
		public override ZString API_MessageStatus { get => base.API_MessageStatus; set => base.API_MessageStatus = value; }

		[ResourceStringData("EUH7.AsycudaPackedItem.API_PackStatus", Caption = "Customs Status", ShortCaption = "Cus. Status", MediumCaption = "Cus. Status", FullDescription = "Code identifying the customs status of the declaration.")]
		public override ZString API_PackStatus { get => base.API_PackStatus; set => base.API_PackStatus = value; }

		[ResourceStringData("EUH7.AsycudaPackedItem.API_CustomsValue", Caption = "Customs Value", ShortCaption = "Cus. Val.", MediumCaption = "Customs Val.", FullDescription = "Customs Value of the goods.")]
		public override ZDecimal API_CustomsValue { get => base.API_CustomsValue; set => base.API_CustomsValue = value; }

		[ResourceStringData("EUH7.AsycudaPackedItem.API_CustomsQty", Caption = "Customs Quantity", ShortCaption = "Cus. Qty.", MediumCaption = "Customs Qty.", FullDescription = "Quantity as indicated on the commercial invoice.")]
		public override ZDecimal API_CustomsQty { get => base.API_CustomsQty; set => base.API_CustomsQty = value; }

		[List(nameof(Lookups) + "." + nameof(AsycudaPackedItemLookups.PackTypeList))]
		[ResourceStringData("EUH7.AsycudaPackedItem.API_CustomsUQ", Caption = "Customs Qty. UQ", ShortCaption = "Q. UQ", MediumCaption = "Qty. UQ", FullDescription = "Customs Quantity measurement unit.")]
		public override ZString API_CustomsUQ { get => base.API_CustomsUQ; set => base.API_CustomsUQ = value; }

		public override ZGuid API_ABL_Bill
		{
			get => base.API_ABL_Bill;
			set
			{
				var oldValue = API_ABL_Bill;
				base.API_ABL_Bill = value;
				if (!IsCopying && oldValue != API_ABL_Bill)
				{
					dataGroupingCached = null;
				}
			}
		}

		[ResourceStringData("EUH7.AsycudaPackedItem.API_GrossWeight", Caption = "Gross Weight", MediumCaption = "Gross Wgt.", ShortCaption = "Gr. Wgt.", FullDescription = "Gross Weight of the Item.")]
		public override ZDecimal API_GrossWeight { get => base.API_GrossWeight; set => base.API_GrossWeight = value; }

		[ResourceStringData("EUH7.AsycudaPackedItem.API_GrossWeightUQ", Caption = "Gross Weight Unit", MediumCaption = "Wgt. UQ", ShortCaption = "UQ", FullDescription = "Measurement unit of the Gross Weight of the Item.")]
		public override ZString API_GrossWeightUQ { get => base.API_GrossWeightUQ; set => base.API_GrossWeightUQ = value; }

		[ResourceStringData("EUH7.AsycudaPackedItem.API_NetWeight", Caption = "Net Weight", MediumCaption = "Net Wgt.", ShortCaption = "Net Wgt.", FullDescription = "Net Weight of the Item.")]
		public override ZDecimal API_NetWeight { get => base.API_NetWeight; set => base.API_NetWeight = value; }

		[ResourceStringData("EUH7.AsycudaPackedItem.API_NetWeightUQ", Caption = "Net Weight Unit", MediumCaption = "Wgt. UQ", ShortCaption = "UQ", FullDescription = "Measurement unit of the Net Weight of the Item.")]
		public override ZString API_NetWeightUQ { get => base.API_NetWeightUQ; set => base.API_NetWeightUQ = value; }

		#endregion

		#region Additional Document

		[ChildEditable(true)]
		public IAdditionalDocumentCollection<AdditionalDocument> AdditionalDocuments
		{
			get
			{
				if (additionalDocuments == null)
				{
					additionalDocuments = CreateNewAdditionalDocumentCollection();
					additionalDocuments.Load();
					RegisterEditableChildObject(additionalDocuments);
				}

				return additionalDocuments;
			}
		}

		IAdditionalDocumentCollection<AdditionalDocument> additionalDocuments;

		protected virtual IAdditionalDocumentCollection<AdditionalDocument> CreateNewAdditionalDocumentCollection() => new AdditionalDocumentCollection<AdditionalDocument>(this);

		#endregion

		#region Additional Infos

		[ChildEditable(true)]
		public IAdditionalInfoCollection<AdditionalInfo> AdditionalInfos
		{
			get
			{
				if (additionalInfos == null)
				{
					additionalInfos = CreateAdditionalInfoCollection();
					additionalInfos.Load();
					RegisterEditableChildObject(additionalInfos);
				}

				return additionalInfos;
			}
		}
		IAdditionalInfoCollection<AdditionalInfo> additionalInfos;

		protected virtual IAdditionalInfoCollection<AdditionalInfo> CreateAdditionalInfoCollection() => new AdditionalInfoCollection<AdditionalInfo>(this);

		#endregion

		#region Supporting Document

		[ChildEditable(true)]
		public ISupportingDocumentCollection<SupportingDocument> SupportingDocuments
		{
			get
			{
				if (supportingDocuments == null)
				{
					supportingDocuments = CreateNewSupportingDocumentCollection();
					supportingDocuments.Load();
					RegisterEditableChildObject(supportingDocuments);
				}

				return supportingDocuments;
			}
		}
		ISupportingDocumentCollection<SupportingDocument> supportingDocuments;

		protected virtual ISupportingDocumentCollection<SupportingDocument> CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection<SupportingDocument>(this);

		#endregion

		#region Previous Document

		[ChildEditable(true)]
		public IPreviousDocumentCollection<PreviousDocument> PreviousDocuments
		{
			get
			{
				if (previousDocuments == null)
				{
					previousDocuments = CreateNewPreviousDocumentCollection();
					previousDocuments.Load();
					RegisterEditableChildObject(previousDocuments);
				}

				return previousDocuments;
			}
		}
		IPreviousDocumentCollection<PreviousDocument> previousDocuments;

		protected virtual IPreviousDocumentCollection<PreviousDocument> CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection<PreviousDocument>(this);

		#endregion

		#region ICusSupportingInfoTypeSupporter

		IDictionary<ZString, Type> ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes() => GetCusSupportingInfoTypesCore();

		protected virtual IDictionary<ZString, Type> GetCusSupportingInfoTypesCore()
		{
			return new Dictionary<ZString, Type>
			{
				{ H7CusSupportingInfoTypeList.Codes.AdditionalInfo, typeof(AdditionalInfo) },
				{ H7CusSupportingInfoTypeList.Codes.AdditionalDocument, typeof(AdditionalDocument) },
				{ H7CusSupportingInfoTypeList.Codes.SupportingDocument, typeof(SupportingDocument) },
				{ H7CusSupportingInfoTypeList.Codes.PreviousDocument, typeof(PreviousDocument) },
			};
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
		}

		#endregion

		#region AsycudaPackPackedItemLink

		public AsycudaPackPackedItemPivot ToggleLinkageWithPackage(AsycudaPack package, bool value)
		{
			return ToggleLinkageWithPackageCore(this, package, value);
		}

		AsycudaPackPackedItemPivot ToggleLinkageWithPackageCore(AsycudaPackedItem packedItem, AsycudaPack package, bool value)
		{
			AsycudaPackPackedItemPivot result = null;
			var basePackagePivotCollection = ((packedItem != null && package != null) ? PackagesPivot : null);
			if (basePackagePivotCollection != null)
			{
				if (value)
				{
					result = basePackagePivotCollection.AddPivotFor(package);
				}
				else if (basePackagePivotCollection.Cast<AsycudaPackPackedItemPivot>().Any(p => p.APP_APA_Pack == package.PK))
				{
					basePackagePivotCollection.DeletePivotFor(package);
				}
			}

			return result;
		}

		[ChildEditable(true)]
		[ChildEditableTestExclude]
		public IAsycudaPackPackedItemLinkCollection<AsycudaPackPackedItemLink> AsycudaPackPackedItemLinks
		{
			get
			{
				if (asycudaPackPackedItemLinks == null)
				{
					asycudaPackPackedItemLinks = GetNewAsycudaPackPackedItemLinkCore();
					asycudaPackPackedItemLinks.Load();
					RegisterEditableChildObject(asycudaPackPackedItemLinks);
				}

				return asycudaPackPackedItemLinks;
			}
		}
		IAsycudaPackPackedItemLinkCollection<AsycudaPackPackedItemLink> asycudaPackPackedItemLinks;

		protected virtual IAsycudaPackPackedItemLinkCollection<AsycudaPackPackedItemLink> GetNewAsycudaPackPackedItemLinkCore()
		{
			return new AsycudaPackPackedItemLinkCollection<AsycudaPackPackedItemLink>(this);
		}

		[ChildEditable(true)]
		[ChildEditableTestExclude]
		public AsycudaPackPackedItemPivotCollection PackagesPivot
		{
			get
			{
				if (fPackagesPivot == null)
				{
					fPackagesPivot = GetNewPackagesPivotCore();
					fPackagesPivot.Load();
					fPackagesPivot.IsManagedForDataRefresh = true;
					RegisterEditableChildObject(fPackagesPivot);
				}
				return fPackagesPivot;
			}
		}
		AsycudaPackPackedItemPivotCollection fPackagesPivot;

		protected virtual AsycudaPackPackedItemPivotCollection GetNewPackagesPivotCore()
		{
			return new AsycudaPackPackedItemPivotCollection<AsycudaPackedItem, AsycudaPack>(this);
		}
		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			API_GrossWeightUQ = API_NetWeightUQ = Constants.Weight.Kilograms;
			API_RX_NKGoodsValueCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
		}

		#region Clone

		protected override bool SupportsCloneCore() => true;

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			return new List<string>
			{
				AsycudaPackedItemSchema.Constants.API_MessageStatus,
				AsycudaPackedItemSchema.Constants.API_PackStatus,
				AsycudaPackedItemSchema.Constants.API_LineNo,
				AsycudaPackedItemSchema.Constants.API_ABL_Bill
			};
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var templateCopy = (AsycudaPackedItem)base.CloneInternal(args);

			CloneChildCollection(CustomsEntryNumbers, templateCopy.CustomsEntryNumbers);
			CloneChildCollection(AdditionalDocuments, templateCopy.AdditionalDocuments);
			CloneChildCollection(SupportingDocuments, templateCopy.SupportingDocuments);
			CloneChildCollection(PreviousDocuments, templateCopy.PreviousDocuments);
			return templateCopy;
		}

		void CloneChildCollection(IBusinessObjectCollection collection, IBusinessObjectCollection clonedColection)
		{
			foreach (BusinessObject element in collection)
			{
				var clonedElement = element.Clone();
				clonedColection.Add(clonedElement);
			}
		}

		#endregion

		#region ICanBeImportOrExport

		ZBool ICanBeImportOrExport.IsImport => Bill.IsImport;

		ZBool ICanBeImportOrExport.IsExport => Bill.IsExport;

		string ICanBeImportOrExport.Level => EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Both;

		string ICanBeImportOrExport.TrueCountryCode => CountryCode;

		string ICanBeImportOrExport.DataGroupingCode => Header.DataGrouping;

		void ICanBeImportOrExport.ValidatePreviousDocuments()
		{
		}

		#endregion
	}
}
