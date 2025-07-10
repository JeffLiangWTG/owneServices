using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsUnloadedCargoDesc : NctsCommonCargoDesc
		, ITariffDescriptionSynchronizerSupporter
	{
		public NctsUnloadedCargoDesc(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString TariffTypeCore => ArrivalCargoDescParent?.TariffType ?? Constants.TariffTypes.Export;

		public override ZDateTime ValuationDate
		{
			get
			{
				var result = ArrivalCargoDescParent?.ValuationDate ?? ZDateTime.Today;
				return result.IsEmpty ? ZDateTime.Today : result;
			}
		}

		[BusinessObjectTestExclude]
		public override ZGuid BY_BY_Commodity
		{
			get => ZGuid.Empty;
			set => throw new System.NotSupportedException($"{NctsUnloadedCargoDesc.Schema.BY_BY_Commodity} cannot be set for NctsUnloadedCargoDesc");
		}

		public new NctsUnloadedCargoDescLookups Lookups => (NctsUnloadedCargoDescLookups)base.Lookups;

		public new NctsUnloadedCargoDescValidation Validation => (NctsUnloadedCargoDescValidation)base.Validation;

		public static new readonly TypeDecider TypeDecider = new CusInBondCargoDescTypeDecider();

		public static NctsUnloadedCargoDesc LoadOrCreate(NctsArrivalCargoDesc parent)
		{
			return Load(parent) ?? New(parent);
		}

		public static NctsUnloadedCargoDesc Load(NctsArrivalCargoDesc parent)
		{
			return parent.Factory.Load<NctsUnloadedCargoDesc>(parent.BY_BY_Commodity);
		}

		public static NctsUnloadedCargoDesc New(NctsArrivalCargoDesc parent)
		{
			var result = (NctsUnloadedCargoDesc)parent.Factory.New(parent.NctsUnloadedCargoDescType);
			result.BY_ParentTableCode = CusInBondCargoDescSchema.Constants.Prefix;
			result.BY_ParentID = parent.PK;
			parent.BY_BY_Commodity = result.PK;
			result.BY_GrossWeightUnit = parent.BY_GrossWeightUnit;
			result.BY_NetWeightUnit = parent.BY_NetWeightUnit;
			return result;
		}

		public NctsArrivalCargoDesc ArrivalCargoDescParent => Factory.Load<NctsArrivalCargoDesc>(BY_ParentID);

		protected override CusInBondCargoDescLookups GetNewLookups() => new NctsUnloadedCargoDescLookups(this);

		protected override CusInBondCargoDescValidation GetNewValidation() => new NctsUnloadedCargoDescValidation(this);

		protected override NctsHeader HeaderCore => ArrivalCargoDescParent?.Header;

		[ReadOnlyMember(nameof(GoodsItemIsReadOnly))]
		[ResourceStringData("f43b761c-7aa4-4e2b-9225-f776354cd77a", Caption = "Commodity Code", MediumCaption = "Commodity", ShortCaption = "Cmdty.")]
		public override ZString BY_HarmonisedTariff
		{
			get => base.BY_HarmonisedTariff;
			set
			{
				var oldValue = BY_HarmonisedTariff;
				var shouldSynchronizeDescription = TariffDescriptionSynchronizer.ShouldSynchronizeDescription;

				base.BY_HarmonisedTariff = value;
				if (!IsCopying && oldValue != BY_HarmonisedTariff)
				{
					if (shouldSynchronizeDescription)
					{
						TariffDescriptionSynchronizer.SynchronizeDescription();
					}
					if (ArrivalCargoDescParent?.IsLiabilityCalculationForArrivalSupported ?? false)
					{
						ArrivalCargoDescParent.LiabilityTariff = BY_HarmonisedTariff;
					}
				}
			}
		}

		TariffDescriptionSynchronizer TariffDescriptionSynchronizer => tariffDescriptionSynchronizer ?? (tariffDescriptionSynchronizer = new TariffDescriptionSynchronizer(this));
		TariffDescriptionSynchronizer tariffDescriptionSynchronizer;

		#region ITariffDescriptionSyncronizerSupporter

		ZString ITariffDescriptionSynchronizerSupporter.CurrentTariffDescription
		{
			get => BY_Description;
			set => BY_Description = value;
		}

		ZString ITariffDescriptionSynchronizerSupporter.OfficialCustomsTariffDescription => UniversalTariff?.FullTariffDescription(ValuationDate, includeSectionHeadings: false, includeChapterHeading: false, useTariffPreferredLanguage: true).Left(BY_DescriptionInfo.MaxLength) ?? ZString.Empty;

		#endregion

		[ReadOnlyMember(nameof(GoodsItemIsReadOnly))]
		[List(nameof(Lookups) + "." + nameof(NctsUnloadedCargoDescLookups.CusCodeList))]
		[ResourceStringData("37b8b3b7-2497-4c6a-9f0f-f9570c9cbdb9", ShortCaption = "CUS Cd", Caption = "CUS Code")]
		public override ZString BY_CusC4Number { get => base.BY_CusC4Number; set => base.BY_CusC4Number = value; }

		[ReadOnlyMember(nameof(GoodsItemIsReadOnly))]
		[ResourceStringData("83b6a759-865b-4a18-a545-8f9f585aed8e", Caption = "Goods Description", MediumCaption = "Description", ShortCaption = "Desc.")]
		public override ZString BY_Description { get => base.BY_Description; set => base.BY_Description = value; }

		[ReadOnlyMember(nameof(GoodsItemIsReadOnly))]
		[ResourceStringData("9a6984f6-e8fa-4bf8-b4f8-fa44067e4f9c", Caption = "Gross Weight (kg)", MediumCaption = "Gross Wgt. (kg)", ShortCaption = "Gross (kg)")]
		public override ZDecimal BY_GrossWeight { get => base.BY_GrossWeight; set => base.BY_GrossWeight = value; }

		[ReadOnlyMember(nameof(UnitIsReadOnly))]
		public override ZString BY_GrossWeightUnit { get => base.BY_GrossWeightUnit; set => base.BY_GrossWeightUnit = value; }

		[ReadOnlyMember(nameof(GoodsItemIsReadOnly))]
		[ResourceStringData("43317f0c-e0a0-4385-892b-5870a810fe37", ShortCaption = "Net Wgt.", Caption = "Net Weight", FullDescription = "Net Weight of the Goods")]
		public override ZDecimal BY_NetWeight { get => base.BY_NetWeight; set => base.BY_NetWeight = value; }

		[ReadOnlyMember(nameof(UnitIsReadOnly))]
		public override ZString BY_NetWeightUnit { get => base.BY_NetWeightUnit; set => base.BY_NetWeightUnit = value; }

		protected virtual bool UnitIsReadOnly => true;

		protected virtual bool GoodsItemIsReadOnly => IsUnloadingRemarksReadOnly || (ArrivalCargoDescParent?.AreUnloadingRemarksFullyAccepted ?? false);

		bool IsUnloadingRemarksReadOnly => ArrivalCargoDescParent?.IsUnloadingRemarksReadOnly ?? false;
	}
}
