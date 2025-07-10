using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsPreviousDocument : EU.Business.Declaration.MultiLineAddInfos.PreviousDocument, IShortSequenceNumberLine, IUnloadedStatusSupporter
	{
		public NctsPreviousDocument(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		public new class Schema : EU.Business.Declaration.MultiLineAddInfos.PreviousDocument.Schema
		{
			public const int CSI_QuantityPrecision = 12;
			public const int CSI_QuantityScale = 3;
		}

		[ResourceStringData("755EEC9E-C064-44FA-81C7-CAC42F61D144", Caption = "Document Type", MediumCaption = "Doc. Type", ShortCaption = "Type")]
		[List(nameof(Lookups) + "." + nameof(NctsPreviousDocumentLookups.CodeList))]
		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set
			{
				var oldValue = CSI_Code;
				base.CSI_Code = value;
				if (!IsCopying && oldValue != CSI_Code && IsPhase5)
				{
					InvalidateCachedRefCusCode();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(NctsPreviousDocumentLookups.SubTypeList))]
		public override ZString CSI_SubType
		{
			get => base.CSI_SubType;
			set => base.CSI_SubType = value;
		}

		[ResourceStringData("CC11892A-4717-4944-A226-A8CA73A6C128", Caption = "Reference Number", MediumCaption = "Reference No.", ShortCaption = "Reference")]
		[MaxLength(nameof(CSI_ReferenceNumberMaxLength))]
		public override ZString CSI_ReferenceNumber
		{
			get => base.CSI_ReferenceNumber;
			set => base.CSI_ReferenceNumber = value;
		}

		protected int CSI_ReferenceNumberMaxLength => IsPhase5Departure ? CusSupportingInfoHelper.GetPhase5DepartureReferenceNumberMaxLength(IsInPhase5TransitionPeriod) : Schema.ReferenceNumberMaxLength;

		[ResourceStringData("01752126-A6B8-48A2-86BE-6F54E175C35C", Caption = "Item Number", MediumCaption = "Item No.", ShortCaption = "Item")]
		public override ZInt CSI_ItemNumber { get => base.CSI_ItemNumber; set => base.CSI_ItemNumber = value; }

		[ResourceStringData("E686B634-0953-49EE-84B7-E8A14E3EEE41", Caption = "Quantity", MediumCaption = "Quantity", ShortCaption = "Qty.")]
		[DecimalPlaces(Schema.CSI_QuantityScale)]
		public override ZDecimal CSI_Quantity
		{
			get => base.CSI_Quantity;
			set => base.CSI_Quantity = value;
		}

		[ResourceStringData("2CA29998-A415-427F-8630-5173618C365E", Caption = "Unit Of Quantity", MediumCaption = "Unit Qty.", ShortCaption = "UOM")]
		public override ZString CSI_UnitOfQuantity
		{
			get => base.CSI_UnitOfQuantity;
			set => base.CSI_UnitOfQuantity = value;
		}

		[ResourceStringData("484F1597-4CBF-421F-B619-3539C0FCF13E", Caption = "Number of Packages", MediumCaption = "No. of Packages", ShortCaption = "Pack Qty")]
		[DecimalPlaces(Schema.CSI_QuantityScale)]
		public override ZDecimal CSI_Quantity2
		{
			get => base.CSI_Quantity2;
			set => base.CSI_Quantity2 = value;
		}

		[ResourceStringData("B2D814FF-0525-4AC3-84D1-79DDF1DA7FE7", Caption = "Package Type", MediumCaption = "Pack Type", ShortCaption = "Pack Type")]
		public override ZString CSI_UnitOfQuantity2
		{
			get => base.CSI_UnitOfQuantity2;
			set => base.CSI_UnitOfQuantity2 = value;
		}

		[ResourceStringData("CDBE5C8F-2E40-4BBD-8290-C328CC6CC9E7", Caption = "Complement of Information", MediumCaption = "Complement Info.", ShortCaption = "Complement")]
		public override ZString CSI_ReferenceNumber2
		{
			get => base.CSI_ReferenceNumber2;
			set => base.CSI_ReferenceNumber2 = value;
		}

		public new NctsPreviousDocumentLookups Lookups => (NctsPreviousDocumentLookups)base.Lookups;

		public new NctsPreviousDocumentValidation Validation => (NctsPreviousDocumentValidation)base.Validation;

		public NctsCommonCargoDesc GoodsItem => (NctsCommonCargoDesc)ImportExportParent;

		public bool IsPhase5Departure => GoodsItem?.IsPhase5Departure ?? false;

		public bool IsNCTSPreviousDocument => NctsHelper.IsNCTSPreviousDocument(CSI_Code);

		protected sealed override CusSupportingInfoLookups GetNewLookups() => IsPhase5 ? GetNewPhase5Lookups() : GetNewPhase4Lookups();

		protected virtual CusSupportingInfoLookups GetNewPhase5Lookups() => new NctsPreviousDocumentPhase5Lookups(this);

		protected virtual CusSupportingInfoLookups GetNewPhase4Lookups() => new NctsPreviousDocumentPhase4Lookups(this);

		protected sealed override CusSupportingInfoValidation GetNewValidation() => IsPhase5 ? GetNewPhase5Validation() : GetNewPhase4Validation();

		protected virtual CusSupportingInfoValidation GetNewPhase5Validation() => new NctsPreviousDocumentPhase5Validation(this);

		protected virtual CusSupportingInfoValidation GetNewPhase4Validation() => new NctsPreviousDocumentPhase4Validation(this);

		public ZZRefCusCodeListCombined RefCusCode => (refCusCodeCore ?? (refCusCodeCore = new RecalculableCachedValue<ZZRefCusCodeListCombined>(GetRefCusCode)))?.Value;
		RecalculableCachedValue<ZZRefCusCodeListCombined> refCusCodeCore;

		ZZRefCusCodeListCombined GetRefCusCode() => ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndAttributes(Factory, CSI_Code, ImportExportParent.DataGroupingCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfNCTS, ZDateTime.Today,
						attributeFilters: new[] { new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.Level, SQLComparisonOperator.Equal, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item) });

		void InvalidateCachedRefCusCode()
		{
			refCusCodeCore?.InvalidateCache();
		}

		public bool IsPhase5 => GoodsItem?.Header?.IsPhase5 ?? false;

		public bool IsInPhase5TransitionPeriod => GoodsItem?.Header?.IsInPhase5TransitionPeriod ?? false;

		protected override ZString HumanReadableNameCore => Res.GetString("69C0C35C-3A54-4C19-9FAF-FD8859FC0599", "Previous Document");

		INctsPreviousDocumentCollection<NctsPreviousDocument> SequenceHeader => (GoodsItem as NctsDepartureCargoDesc)?.PreviousDocuments;

		public override ZString CSI_ParentTableCode
		{
			get => base.CSI_ParentTableCode;
			set
			{
				var oldValue = CSI_ParentTableCode;
				base.CSI_ParentTableCode = value;
				if (!IsCopying && oldValue != CSI_ParentTableCode)
				{
					if (AutomaticSequenceNumberEnabled && !CSI_ParentTableCode.IsEmpty)
					{
						SequenceHeader?.SequenceGenerator.RecalculateWhenAdded(this);
					}
				}
			}
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				if (AutomaticSequenceNumberEnabled)
				{
					SequenceHeader?.SequenceGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
				}
			}

			base.Delete();
		}

		ZGuid ISequenceNumberLine.FKToHeader => CSI_ParentID;

		ZShort ISequenceNumberLine<ZShort>.SequenceNumber
		{
			get => (ZShort)CSI_LineNo;
			set => CSI_LineNo = value;
		}

		protected virtual bool AutomaticSequenceNumberEnabled => IsPhase5;

		public ZString UnloadedStatus
		{
			get => CSI_Status;
			set => CSI_Status = value;
		}

		public IEnumerable<IUnloadedStatusSupporter> RelatedItems => Enumerable.Empty<IUnloadedStatusSupporter>();

		public INctsPreviousDocumentValidationDecider ValidationDecider => CachedValueHelper.GetValue(ref validationDeciderCached, GetValidationDecider);
		CachedValue<INctsPreviousDocumentValidationDecider> validationDeciderCached;

		protected virtual INctsPreviousDocumentValidationDecider GetValidationDecider() =>
			GoodsItem?.Header?.Configuration.GoodsItemsConfiguration.NctsPreviousDocumentConfiguration.GetValidationDecider(previousDocument: this);
	}
}
