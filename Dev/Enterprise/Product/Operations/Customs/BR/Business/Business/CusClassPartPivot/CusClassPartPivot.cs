using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class CusClassPartPivot : BaseCusClassPartPivot, ICusCodeDataTypeSupporter, ITariffDetachParent, IAdditionalTariffParent, IAttributeCusCodeDataParent
	{
		public CusClassPartPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : BaseCusClassPartPivot.Schema
		{
			public const string ComplementaryDescription = "ComplementaryDescription";
			public const string TariffDetachConcatenated = "TariffDetachConcatenated";
		}

		public override ZString CI_ChildType
		{
			get => base.CI_ChildType;
			set
			{
				var oldValue = CI_ChildType;
				base.CI_ChildType = value;
				if (!IsCopying && oldValue != CI_ChildType)
				{
					Attributes.Rebuild();
					NveCusCodeDataCollection.RebuildFromCharacteristics();
				}
			}
		}

		public override ZString CI_TariffNum
		{
			get => base.CI_TariffNum;
			set
			{
				var oldValue = CI_TariffNum;
				base.CI_TariffNum = value;
				if (!IsCopying && oldValue != CI_TariffNum)
				{
					RebuildCollections();
				}
			}
		}

		void RebuildCollections()
		{
			Attributes.Rebuild();
			NveCusCodeDataCollection.RebuildFromCharacteristics();
			AdditionalTariffs.RemoveAndDeleteAll();
		}

		protected override ZString FormatTariffForSaving(ZString unformattedTariff) => unformattedTariff.KeepNumericCharacters();

		#region ComplementaryDescriptionExport

		[MaxLength(nameof(ComplementaryDescriptionMaxLength))]
		[ResourceStringData("Enterprise.Customs.BR.Business.CusClassPartPivot|ComplementaryDescription", Caption = "Comp. Description", ShortCaption = "Comp. Desc.", FullDescription = "Complementary Description")]
		public ZString ComplementaryDescription
		{
			get => ComplementaryDescriptionNote.Text;
			set => ComplementaryDescriptionNote.SetNoteText(this, ComplementaryDescriptionInfo, value);
		}

		HiddenTextNote ComplementaryDescriptionNote => complementaryDescriptionNote ?? (complementaryDescriptionNote = new HiddenTextNote(this, PredefinedNoteTypes.Instance.BRComplementaryDescription.Description));
		HiddenTextNote complementaryDescriptionNote;

		public ZPropertyInfo ComplementaryDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.ComplementaryDescription); }
		}

		int ComplementaryDescriptionMaxLength => IsImportClassification ? 4000 : 2000;

		#endregion

		[ResourceStringData("Enterprise.Customs.BR.Business.CusClassPartPivot|CI_CGC_Catalog", Caption = "Goods Catalog", FullDescription = "The Goods Catalog.")]
		public override ZGuid CI_CGC_Catalog
		{
			get => base.CI_CGC_Catalog;
			set
			{
				var oldValue = CI_CGC_Catalog;
				base.CI_CGC_Catalog = value;

				if (!IsCopying && oldValue != CI_CGC_Catalog)
				{
					CI_TariffNum = ZString.Empty;
					CI_CC = ZGuid.Empty;
					RebuildCollections();
				}
			}
		}

		public override ZGuid CI_CC
		{
			get => base.CI_CC;
			set
			{
				var oldValue = CI_CC;
				base.CI_CC = value;

				if (!IsCopying && oldValue != CI_CC)
				{
					RebuildCollections();
				}
			}
		}

		public new CusGoodsCatalog GoodsCatalog => base.GoodsCatalog as CusGoodsCatalog;

		public override void OnPartNumberChanged(ZString oldPartNum, ZString newPartNum)
		{
			base.OnPartNumberChanged(oldPartNum, newPartNum);
			GoodsCatalog?.LocalPartNumbers.UpdateLocalPartNumber(oldPartNum, newPartNum);
		}

		#region CloneFunction

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var result = (CusClassPartPivot)base.CloneInternal(args);
			using (result.GetValidationSuspender())
			{
				result.ComplementaryDescription = ComplementaryDescription;

				result.Attributes.Rebuild();
				result.NveCusCodeDataCollection.RebuildFromCharacteristics();

				result.Attributes.CopyDataFrom(Attributes);
				result.NveCusCodeDataCollection.CopyDataFrom(NveCusCodeDataCollection);
				result.TariffDetachs.CloneFrom(TariffDetachs);
				result.CusLineTariffDetails.CloneFrom(CusLineTariffDetails);
				result.LegalActInfos.CloneFrom(LegalActInfos);
			}
			return result;
		}

		#endregion

		#region Attribute

		[UniversalCopyCollectionEntity(CusCodeDataSchema.Constants.TableName, CusCodeDataSchema.Constants.CY_ParentTableCode)]
		[ChildEditable(true)]
		public AttributeCusCodeDataCollection Attributes
		{
			get
			{
				if (fAttributeCusCodeDataCollection == null)
				{
					fAttributeCusCodeDataCollection = new AttributeCusCodeDataCollection(this);
					fAttributeCusCodeDataCollection.Load();
					fAttributeCusCodeDataCollection.Rebuild();
					RegisterEditableChildObject(fAttributeCusCodeDataCollection);
				}
				return fAttributeCusCodeDataCollection;
			}
		}

		AttributeCusCodeDataCollection fAttributeCusCodeDataCollection;

		public AttributeCusCodeDataCollection GetAttributes(string type) => type == CusCodeDataTypeList.Codes.Attribute ? Attributes : null;

		#endregion

		#region NVE

		[UniversalCopyCollectionEntity(CusCodeDataSchema.Constants.TableName, CusCodeDataSchema.Constants.CY_ParentTableCode)]
		[ChildEditable(true)]
		public NveCusCodeDataCollection NveCusCodeDataCollection
		{
			get
			{
				if (fNveCusCodeDataCollection == null)
				{
					fNveCusCodeDataCollection = new NveCusCodeDataCollection(this);
					fNveCusCodeDataCollection.Load();
					fNveCusCodeDataCollection.RebuildFromCharacteristics();
					RegisterEditableChildObject(fNveCusCodeDataCollection);
				}
				return fNveCusCodeDataCollection;
			}
		}

		NveCusCodeDataCollection fNveCusCodeDataCollection;

		#endregion

		#region TariffDetachCollection

		[UniversalCopyCollectionEntity(CusCodeDataSchema.Constants.TableName, CusCodeDataSchema.Constants.CY_ParentTableCode)]
		[ChildEditable(true)]
		public TariffDetachCollection TariffDetachs
		{
			get
			{
				if (fTariffDetachCollection == null)
				{
					fTariffDetachCollection = new TariffDetachCollection(this);
					fTariffDetachCollection.Load();
					RegisterEditableChildObject(fTariffDetachCollection);
				}
				return fTariffDetachCollection;
			}
		}

		TariffDetachCollection fTariffDetachCollection;

		public ZString TariffDetachConcatenated => TariffDetachs.ConcatenatedCodes;

		public ZPropertyInfo TariffDetachConcatenatedInfo => GetZPropertyInfo(Schema.TariffDetachConcatenated);

		#endregion

		#region AdditionalTariffs

		[ChildEditable(true)]
		public AdditionalTariffCollection AdditionalTariffs
		{
			get
			{
				if (fAdditionalTariffs == null)
				{
					fAdditionalTariffs = new AdditionalTariffCollection(this);
					fAdditionalTariffs.Load();
					RegisterEditableChildObject(fAdditionalTariffs);
				}
				return fAdditionalTariffs;
			}
		}

		AdditionalTariffCollection fAdditionalTariffs;

		#endregion

		#region Legal Act Collection

		[ChildEditable(true)]
		public LegalActInfoCollection LegalActInfos
		{
			get
			{
				if (fLegalActCollection == null)
				{
					fLegalActCollection = new LegalActInfoCollection(this);
					fLegalActCollection.Load();
					RegisterEditableChildObject(fLegalActCollection);
				}
				return fLegalActCollection;
			}
		}

		LegalActInfoCollection fLegalActCollection;

		#endregion

		protected override bool IsTariffNumReadOnlyCore => !CI_CC.IsEmpty || !CI_CGC_Catalog.IsEmpty;

		protected override bool CI_CC_ReadOnlyCore => !CI_TariffNum.IsEmpty || !CI_CGC_Catalog.IsEmpty;

		protected override Customs.Business.CusClassPartPivotValidation GetNewValidation()
		{
			return new CusClassPartPivotValidation(this);
		}

		protected override Customs.Business.CusClassPartPivotLookups GetNewLookups()
		{
			return new CusClassPartPivotLookups(this);
		}

		public new CusClassPartPivotLookups Lookups => (CusClassPartPivotLookups)base.Lookups;

		public override void OnSaving()
		{
			base.OnSaving();
			if (!IsDeleted)
			{
				if (!IsImportClassification)
				{
					NveCusCodeDataCollection.RemoveAndDeleteAll();
					TariffDetachs.RemoveAndDeleteAll();
					AdditionalTariffs.RemoveAndDeleteAll();
				}

				UpdateLocalPartNumberOnSaving();
				ComplementaryDescription = ComplementaryDescription.Left(ComplementaryDescriptionMaxLength);
			}
		}

		public override void Delete()
		{
			base.Delete();
			UpdateLocalPartNumberOnSaving();
		}

		void UpdateLocalPartNumberOnSaving()
		{
			var oldCatalogPK = IsInDatabase ? (ZGuid)CI_CGC_CatalogInfo.OriginalValue : ZGuid.Empty;
			var newCatalogPK = IsDeleted ? ZGuid.Empty : CI_CGC_Catalog;

			var oldPartPK = IsInDatabase ? (ZGuid)CI_OPInfo.OriginalValue : ZGuid.Empty;
			var newPartPK = IsDeleted ? ZGuid.Empty : CI_OP;

			var oldPartNum = Factory.Load<OrgSupplierPart>(oldPartPK)?.OP_PartNum ?? ZString.Empty;
			var newPartNum = Factory.Load<OrgSupplierPart>(newPartPK)?.OP_PartNum ?? ZString.Empty;

			if (oldCatalogPK != newCatalogPK)
			{
				LoadGoodsCatalog(oldCatalogPK)?.LocalPartNumbers.UpdateLocalPartNumber(oldPartNum, ZString.Empty);
				LoadGoodsCatalog(newCatalogPK)?.LocalPartNumbers.UpdateLocalPartNumber(ZString.Empty, newPartNum);
			}
			else if (oldPartNum != newPartNum)
			{
				LoadGoodsCatalog(newCatalogPK)?.LocalPartNumbers.UpdateLocalPartNumber(oldPartNum, newPartNum);
			}
		}

		CusGoodsCatalog LoadGoodsCatalog(ZGuid catalogPK) => catalogPK.IsValid ? Factory.Load<CusGoodsCatalog>(catalogPK) : null;

		[ChildEditable(true)]
		[BusinessObjectTestExclude]
		[UniversalCopyCollectionEntity(CusLineTariffDetailSchema.Constants.TableName, CusLineTariffDetailSchema.Constants.BZ_ParentID)]
		public new ICusLineTariffDetailCollection<CusLineTariffDetail> CusLineTariffDetails => (CusLineTariffDetailCollection<CusLineTariffDetail>)base.CusLineTariffDetails;

		protected override ICusLineTariffDetailCollection<Customs.Business.CusLineTariffDetail> GetCusLineTariffDetails() => new CusLineTariffDetailCollection<CusLineTariffDetail>(this);

		protected override bool SupportsAdditionalTariffs => true;

		public ZDateTime EffectiveAssessmentDate => ZDateTime.Today;

		#region ICusCodeDataTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			return new Dictionary<ZString, Type>
			{
				{ CusCodeDataTypeList.Codes.Attribute, typeof(AttributeCusCodeData) },
				{ CusCodeDataTypeList.Codes.NVE, typeof(NveCusCodeData) },
				{ CusCodeDataTypeList.Codes.TariffDetach, typeof(TariffDetach) }
			};
		}

		#endregion
	}
}
