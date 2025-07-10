using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSJobComInvoiceLine : AutoEMCSJobComInvoiceLine
		, IOutturnableLine
		, Integration.Customs.ICusCodeDataTypeSupporter
		, ISynchroniserReadOnlyMembersProvider
	{
		public EMCSJobComInvoiceLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			GetConvertedStockUnit = GetConvertedStockUnitCore;
		}

		public new class Schema : AutoEMCSJobComInvoiceLine.Schema
		{
			public const string JI_WineDetailsComments = nameof(EMCSJobComInvoiceLine.JI_WineDetailsComments);
			public const int JI_WineDetailsComments_MaxLength = 350;
		}

		#region Tariff

		[List(nameof(Lookups) + "." + nameof(EMCSJobComInvoiceLineLookups.CNCodeList))]
		[ResourceStringData("8A84FB7E-60F6-49AF-A27A-2C1F9C48DBBF", Caption = "CN-Code")]
		public override ZString JI_Tariff
		{
			get => base.JI_Tariff;
			set
			{
				var oldValue = base.JI_Tariff;
				base.JI_Tariff = value;

				if (oldValue != JI_Tariff && CNCode != null)
				{
					JI_NDescription = CNCode.ZZD_Description.SubstringSafe(0, JobComInvoiceLineSchema.JI_NDescription.MaxLength);
				}
			}
		}

		ZZRefCusCodeListCombined CNCode => ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, JI_Tariff, GetDefaultDataGroupingCode(), Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSCNCodes, ZDateTime.Today);

		#endregion

		#region Properties

		[ReadOnly(true)]
		[ResourceStringData("3F91B054-E8BC-42A3-B383-1C6D80060D38", Caption = "Declared Value")]
		[DecimalPlaces(3)]
		public override ZDecimal ZG_DeclaredValue
		{
			get => base.ZG_DeclaredValue;
			set
			{
				var oldValue = ZG_DeclaredValue;
				if (oldValue != value)
				{
					base.ZG_DeclaredValue = value;
					var cusOutturn = Outturn;
					cusOutturn.ActualQuantityInfo.RefreshBinding();
					cusOutturn.ObservedDifferenceInfo.RefreshBinding();
				}
			}
		}

		[ResourceStringData("2C813F80-2B51-4851-9F64-629060A51D28", Caption = "Excise Product Code", MediumCaption = "Excise Code", ShortCaption = "Excise")]
		public override ZString ZG_ExciseProductCode
		{
			get => base.ZG_ExciseProductCode;
			set
			{
				var oldValue = ZG_ExciseProductCode;
				if (oldValue != value)
				{
					base.ZG_ExciseProductCode = value;
					UpdateCustomsQuantity();
					ClearAndDisableWineDetailsIfNeeded();
				}
			}
		}

		void ClearAndDisableWineDetailsIfNeeded()
		{
			var wineDetails_ReadOnly = WineDetails_ReadOnly;
			if (wineDetails_ReadOnly)
			{
				ZG_WineCategory = ZString.Empty;
				ZG_GrowingZone = ZString.Empty;
				ZG_WineCountryOrigin = ZString.Empty;
				JI_WineDetailsComments = ZString.Empty;
				OperationCodeDataCollection.RemoveAndDeleteAll();
			}
			OperationCodeDataCollection.SetReadOnlyIncludingChildren(wineDetails_ReadOnly);
		}

		void UpdateCustomsQuantity()
		{
			if (!ZG_ExciseProductCode.IsEmpty)
			{
				var attribute = AttributeLoader.Load(
					Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
					ZDateTime.Now,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes,
					ZG_ExciseProductCode,
					nameof(ExciseProductCodeAttribute.UnitMeasure)).FirstOrDefault();

				JI_CustomsUnitQty = attribute?.ZZE_Value ?? ZString.Empty;
			}
		}

		ZBool WineDetails_ReadOnly => !IsWine;

		public ZBool IsWine => ZG_ExciseProductCode == ExciseProductCode_W200;

		public const string ExciseProductCode_W200 = "W200";

		[ReadOnlyMember(nameof(WineDetails_ReadOnly))]
		public override ZString ZG_WineCategory
		{
			get => base.ZG_WineCategory;
			set => base.ZG_WineCategory = value;
		}

		[ReadOnlyMember(nameof(WineDetails_ReadOnly))]
		public override ZString ZG_GrowingZone
		{
			get => base.ZG_GrowingZone;
			set => base.ZG_GrowingZone = value;
		}

		[ReadOnlyMember(nameof(WineDetails_ReadOnly))]
		public override ZString ZG_WineCountryOrigin
		{
			get => base.ZG_WineCountryOrigin;
			set => base.ZG_WineCountryOrigin = value;
		}

		[MaxLength(Schema.JI_WineDetailsComments_MaxLength)]
		[ReadOnlyMember(nameof(WineDetails_ReadOnly))]
		public ZString JI_WineDetailsComments
		{
			get => WineDetailsCommentsNote?.ST_NoteDataAsText ?? ZString.Empty;
			set
			{
				var oldValue = JI_WineDetailsComments;
				CheckMaximumLength(JI_WineDetailsCommentsInfo, value);
				if (oldValue != value)
				{
					var note = WineDetailsCommentsNote;

					if (string.IsNullOrWhiteSpace(value))
					{
						note?.Delete();
					}
					else
					{
						if (note == null || note.IsDeleted)
						{
							note = Notes.AddNew(true, PredefinedNoteTypes.Instance.WineDetailsComments.Description, string.Empty);
						}

						note.ST_NoteDataAsText = value;
					}
				}

				HasChanges = true;
				JI_WineDetailsCommentsInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo JI_WineDetailsCommentsInfo => GetZPropertyInfo(Schema.JI_WineDetailsComments);

		[ChildEditable(true)]
		StmNote WineDetailsCommentsNote
		{
			get
			{
				if (wineDetailsCommentsNote == null || wineDetailsCommentsNote.IsDeleted)
				{
					wineDetailsCommentsNote = Notes.FindByDescription(PredefinedNoteTypes.Instance.WineDetailsComments.Description).FirstOrDefault(c => !c.IsDeleted);
				}

				return wineDetailsCommentsNote;
			}
		}
		StmNote wineDetailsCommentsNote;

		[ReadOnly(true)]
		public override ZString JI_WeightUQ
		{
			get { return base.JI_WeightUQ; }
			set { base.JI_WeightUQ = value; }
		}

		[ReadOnly(true)]
		public override ZString JI_NetWeightUQ
		{
			get { return base.JI_NetWeightUQ; }
			set { base.JI_NetWeightUQ = value; }
		}

		[DecimalPlaces(3)]
		[ResourceStringData("C70743CD-7584-4A7A-8CC8-77CE083E398C", Caption = "Customs Qty")]
		public override ZDecimal JI_CustomsQuantity
		{
			get => base.JI_CustomsQuantity;
			set
			{
				var oldValue = JI_CustomsQuantity;
				base.JI_CustomsQuantity = value;
				if (JI_CustomsQuantity != oldValue)
				{
					var cusOutturn = Outturn;
					cusOutturn.ActualQuantityInfo.RefreshBinding();
					cusOutturn.ObservedDifferenceInfo.RefreshBinding();
					if (!IsValidationSuspended)
					{
						cusOutturn.MarkAsNeedingValidation();
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(EMCSJobComInvoiceLineLookups.CustomsUQList))]
		public override ZString JI_CustomsUnitQty
		{
			get => base.JI_CustomsUnitQty;
			set => base.JI_CustomsUnitQty = value;
		}

		protected override bool GetJI_CustomsUnitQtyInfoReadOnly() => true;

		protected override bool GetJI_CustomsQuantityReadOnly() => JI_CustomsUnitQty.IsEmpty;

		public ZString CustomsUnitQtyDescription => Lookups.CustomsUQList.GetDescriptionFromCode(JI_CustomsUnitQty);

		public ZPropertyInfo CustomsUnitQtyDescriptionInfo => GetZPropertyInfo(nameof(CustomsUnitQtyDescription));

		[ResourceStringData("E919AB03-0779-4255-8668-AECC4C1E43C0", Caption = "Is Main Pack?")]
		public override ZBool ZG_IsMainPack
		{
			get => base.ZG_IsMainPack;
			set => base.ZG_IsMainPack = value;
		}

		[ResourceStringData("CE05B1A9-8CD7-4D92-8D55-50A56F437950", Caption = "Maturation Period Or Age Of Products", MediumCaption = "Maturation Period/Product Age")]
		public override ZString ZG_MaturationPeriodOrAgeOfProducts
		{
			get => base.ZG_MaturationPeriodOrAgeOfProducts;
			set => base.ZG_MaturationPeriodOrAgeOfProducts = value;
		}

		[ResourceStringData("03676f46-b76f-4e77-be67-1d2c448251b1", Caption = "Independent Small Producers Declaration", MediumCaption = "Independent Small Producers")]
		public override ZString ZG_IndependentSmallProducersDeclaration
		{
			get => base.ZG_IndependentSmallProducersDeclaration;
			set => base.ZG_IndependentSmallProducersDeclaration = value;
		}
		#endregion

		#region WineCodeDataCollection

		[ChildEditable(true)]
		public WineCodeDataCollection OperationCodeDataCollection
		{
			get
			{
				if (operationCodeDataCollection == null)
				{
					operationCodeDataCollection = new WineCodeDataCollection(this);
					operationCodeDataCollection.Load();

					operationCodeDataCollection.MaxCountValidationEnable(99);
					operationCodeDataCollection.SetReadOnlyIncludingChildren(WineDetails_ReadOnly);
					RegisterEditableChildObject(operationCodeDataCollection);
				}

				return operationCodeDataCollection;
			}
		}
		WineCodeDataCollection operationCodeDataCollection;

		#endregion

		#region Outturn
		public EMCSInvoiceLineCusOutturn Outturn
		{
			get
			{
				if (outturn == null || (outturn.IsDeleted && !IsDeleted))
				{
					outturn = LoadOutturn() ?? CreateOutturn();
					outturn.SetReadOnlyIncludingChildren(ReadOnly);
					RegisterEditableChildObject(outturn);
				}
				return outturn;
			}
		}

		EMCSInvoiceLineCusOutturn LoadOutturn()
		{
			var query = new ZQuery(CusOutturnSchema.C5_ParentID, PK) { FetchOnlyFromLocalCache = !IsInDatabase };
			return Factory.LoadTop1<EMCSInvoiceLineCusOutturn>(query);
		}

		EMCSInvoiceLineCusOutturn CreateOutturn() => EMCSInvoiceLineCusOutturn.New(Factory, this);
		EMCSInvoiceLineCusOutturn outturn;

		bool IOutturnableLine.IsDeleted => IsDeleted;

		ZString IOutturnableLine.UnderbondHumanReadableName => HumanReadableName;

		ZString IOutturnableLine.CargoStatus => ZString.Empty;

		ZInt IOutturnableLine.PackagesManifested => ZInt.Zero;
		#endregion

		#region Type Safe

		protected override Type InvoiceHeaderType => typeof(EMCSJobComInvoiceHeader);

		public new EMCSJobComInvoiceLineLookups Lookups => (EMCSJobComInvoiceLineLookups)base.Lookups;

		public new EMCSJobComInvoiceLineValidation Validation => (EMCSJobComInvoiceLineValidation)base.Validation;

		#endregion

		#region Override

		public new EMCSJobDeclaration Declaration => (EMCSJobDeclaration)base.Declaration;

		protected override JobComInvoiceLineLookups GetNewLookups() => EMCSProvider.GetNewLookups(this);

		protected override JobComInvoiceLineValidation GetNewValidation() => EMCSProvider.GetNewValidation(this);

		public override void Delete()
		{
			DeleteAllPackagePivots();
			Outturn.Delete();
			Notes.RemoveAndDeleteAll();
			OperationCodeDataCollection.RemoveAndDeleteAll();

			base.Delete();
		}

		protected override void SetDefaultTaxOrFeeCode()
		{
			if (ZG_ExciseProductCode.IsEmpty)
			{
				var attributeFilterList = new[] { new RefCusCodeListAttributeFilter(nameof(ExciseProductCodeAttribute.RelTrf), JoinCondition.And, JI_Tariff) };
				ZG_ExciseProductCode = ZZRefCusCodeListCombined.Loader.Load(Factory,
					Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes,
					ZDateTime.Now,
					attributeFilterList).OrderBy(c => c.ZZD_Code).FirstOrDefault()?.ZZD_Code ?? ZString.Empty;
			}
		}

		RefCusCodeListAttribute.Loader AttributeLoader
		{
			get => attributeLoader ?? (attributeLoader = new RefCusCodeListAttribute.Loader(Factory));
		}
		RefCusCodeListAttribute.Loader attributeLoader;

		IDictionary<ZString, Type> Integration.Customs.ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			return new Dictionary<ZString, Type>
			{
				{ EU.Business.CusCodeDataTypeList.Codes.OfficeCode, typeof(EuOfficeCode) },
				{ CusCodeDataTypeList.Codes.WineCode, typeof(WineCodeData) }
			};
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusCodeDataTypeSupporterFetchStrategy(this);
		}

		public override bool ReadOnly
		{
			get => base.ReadOnly || IsMessageStatusSentOrAcknowledgedOnParent;
			set => base.ReadOnly = value;
		}

		protected override bool CanDeleteCore => base.CanDeleteCore && !IsMessageStatusSentOrAcknowledgedOnParent;

		public bool IsMessageStatusSentOrAcknowledgedOnParent => Declaration?.IsMessageStatusSentOrAcknowledged ?? false;

		protected override bool UseUniversalTariffCore => false;

		#endregion

		#region Type Decider

		public EMCSProvider EMCSProvider => emcsProvider ?? (emcsProvider = EMCSProvider.GetByCountryCode(Declaration?.GetDefaultDataGroupingCode() ?? ZString.Empty));
		EMCSProvider emcsProvider;

		public new static readonly EMCSJobComInvoiceLineTypeDecider TypeDecider = new EMCSJobComInvoiceLineTypeDecider();

		#endregion

		#region Product

		protected override BaseCusClassPartPivot GetPivotCore()
		{
			BaseCusClassPartPivot result = null;
			var part = Part;
			if (part != null && Declaration is EMCSJobDeclaration declaration)
			{
				var query = new ZQuery(CusClassPartPivotSchema.CI_OP, part.PK);
				query.AddToFilter(CusClassPartPivotSchema.CI_RN_NKCountry, CustomsCountryCode);
				query.AddToFilter(CusClassPartPivotSchema.CI_CI_Parent, null);
				query.FetchOnlyFromLocalCache = !part.IsInDatabase;

				var pivots = Factory.Load<BaseCusClassPartPivot>(query);

				if (pivots.Any())
				{
					var provider = GetClassificationTypeProvider();

					var importerPk = declaration.OwnerDocumentaryAddress.OrganisationPK.IsEmpty
						? declaration.JE_OH_Importer
						: declaration.OwnerDocumentaryAddress.OrganisationPK;

					var supplierPk = declaration.JE_OH_Supplier;

					var matcher = new ClassPartPivotMatcher(pivots, provider.HTBCode);
					result = matcher.GetMatch(provider.HTICode, importerPk, supplierPk, EffectiveDateForDutyRate, false, GetPartAttribs());
				}
			}

			return result;
		}

		public override void UpdateDetailsFromProductOnPartChangeCore()
		{
			base.UpdateDetailsFromProductOnPartChangeCore();

			var part = Part;
			if (part != null && string.IsNullOrWhiteSpace(JI_BrandName))
			{
				JI_BrandName = part.OP_Brand;
			}
		}

		ZString GetConvertedStockUnitCore(ZString unit)
		{
			var cache = Factory.GetCachedValue("EMCSPackagingTypeConversionListCache", delegate
			{
				var queryForRefPack = new ZQuery(RefPacksSchema.RP_CustomsCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				return Factory.Load<CusRefPacks>(queryForRefPack);
			});

			var packType = cache.FirstOrDefault(c => string.Equals(c.RP_CommercialPack, unit, System.StringComparison.OrdinalIgnoreCase) && Lookups.InvoiceUQList.ContainsCode(c.RP_CustomsPack));
			return packType?.RP_CustomsPack ?? string.Empty;
		}

		public override bool IsContainerLinkMandatory => false;

		#endregion

		#region SetDefaultValues

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			JI_CustomsUnitQty = string.Empty;
			_ = Outturn;
		}

		#endregion

		#region EMCS Package Pivots
		[ChildEditable]
		public NonPersistentPackagePivotCollection EMCSPackagePivots
		{
			get
			{
				if (emcsPackagePivots == null)
				{
					emcsPackagePivots = new NonPersistentPackagePivotCollection(this);
					RegisterEditableChildObject(emcsPackagePivots);
				}
				return emcsPackagePivots;
			}
		}
		NonPersistentPackagePivotCollection emcsPackagePivots;

		public void DeleteAllPackagePivots()
		{
			if (EMCSPackagePivots != null && EMCSPackagePivots.Count > 0)
			{
				EMCSPackagePivots.RemoveAll();
			}
		}
		#endregion
	}
}
