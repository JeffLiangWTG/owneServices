using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NctsAdditionalInfo : EU.NCTS.Business.NctsAdditionalInfo
	{
		public NctsAdditionalInfo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public bool IsPhase5Departure
		{
			get
			{
				var header = ParentAsNctsHeader ?? ParentAsGoodsItem?.Header;
				return header != null && header.IsPhase5 && header.IsDepartureMovement;
			}
		}

		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set
			{
				var oldValue = CSI_Code;
				base.CSI_Code = value;
				if (!IsCopying && oldValue != CSI_Code)
				{
					ResetReadOnlyProvider();
					InvalidateCachedRefCusCode();
				}
			}
		}

		/// <value>Constants: <see cref="AdditionalInfoSubTypeList.Codes" /></value>
		public override ZString CSI_SubType
		{
			get => base.CSI_SubType;
			set
			{
				var oldValue = CSI_SubType;
				base.CSI_SubType = value;
				if (!IsCopying && oldValue != CSI_SubType)
				{
					ResetReadOnlyProvider();
					InvalidateCachedRefCusCode();
				}
			}
		}

		[MaxLength(nameof(CSI_ReferenceNumber_MaxLength))]
		[ReadOnlyMember(nameof(ReadOnlyProvider) + "." + nameof(IAdditionalDocumentReadOnlyProvider.ReferenceNumberReadOnly))]
		public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

		public int CSI_ReferenceNumber_MaxLength =>
			CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument || CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference
				? IsInPhase5TransitionPeriod ? 35 : 70
				: 35;

		[ReadOnlyMember(nameof(ReadOnlyProvider) + "." + nameof(IAdditionalDocumentReadOnlyProvider.DescriptionReadOnly))]
		public override ZString CSI_Description { get => base.CSI_Description; set => base.CSI_Description = value; }

		protected override ZZRefCusCodeListCombined RefCusCodeCore => (refCusCodeCore ?? (refCusCodeCore = new RecalculableCachedValue<ZZRefCusCodeListCombined>(GetRefCusCode)))?.Value;
		RecalculableCachedValue<ZZRefCusCodeListCombined> refCusCodeCore;

		ZZRefCusCodeListCombined GetRefCusCode() => ImportExportParent == null ? null : ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndAttributes(Factory, CSI_Code, ImportExportParent.DataGroupingCode, CodeListType, ZDateTime.Today,
							attributeFilters: new[] { new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.Level, SQLComparisonOperator.Equal, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item) });

		void InvalidateCachedRefCusCode() => refCusCodeCore?.InvalidateCache();

		protected override CusSupportingInfoValidation GetNewPhase5Validation() => IsPhase5Arrival ? new CusSupportingInfoDisabledValidation(this) : new NctsAdditionalInfoValidation(this);

		#region ReadOnlyProvider

		protected override IAdditionalDocumentReadOnlyProvider GetNewReadOnlyProvider()
		{
			var baseReadOnlyProvider = base.GetNewReadOnlyProvider();
			return new NctsAdditionalInfoReadOnlyProvider(this, baseReadOnlyProvider);
		}

		#endregion
	}
}
