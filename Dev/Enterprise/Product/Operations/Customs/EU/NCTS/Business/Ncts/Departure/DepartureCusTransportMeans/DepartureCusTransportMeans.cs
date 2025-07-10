using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business.Interfaces;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class DepartureCusTransportMeans : CusTransportMeans, IShortSequenceNumberLine, IAdditionalWagonProvider
	{
		public DepartureCusTransportMeans(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected TypeLoaderCollection ParentLoaders => parentLoaders ?? (parentLoaders = GetParentLoaders());
		TypeLoaderCollection parentLoaders;

		public BusinessObject Parent
		{
			get
			{
				return ParentLoaders.LoadBusinessObject(Factory, TPM_ParentTableCode, TPM_ParentID);
			}
		}

		public NctsHeader Header => (Parent as NctsCommonMovementHeader)?.Header ?? (Parent as NctsBill)?.Header;

		public NctsDepartureMovementHeader MovementHeaderParent => Parent as NctsDepartureMovementHeader;

		protected virtual TypeLoaderCollection GetParentLoaders()
		{
			return new TypeLoaderCollection(typeof(BaseCusInBondMoveHeader), typeof(NctsBill));
		}

		internal bool IsInPhase5TransitionPeriod => Header?.IsInPhase5TransitionPeriod ?? false;

		public new DepartureCusTransportMeansLookups Lookups => (DepartureCusTransportMeansLookups)base.Lookups;

		public new DepartureCusTransportMeansValidation Validation => (DepartureCusTransportMeansValidation)base.Validation;

		protected override CusTransportMeansLookups GetNewLookups() => new DepartureCusTransportMeansLookups(this);

		protected override CusTransportMeansValidation GetNewValidation() => new DepartureCusTransportMeansValidation(this);

		protected override bool SupportsCloneCore() => true;

		[ResourceStringData("E2D3184A-8336-491A-867B-1C225B681FDA", Caption = "Customs Office", ShortCaption = "Office")]
		[List(nameof(Lookups) + "." + nameof(DepartureCusTransportMeansLookups.OfficeCodeList))]
		public override ZString TPM_CustomsOffice
		{
			get => base.TPM_CustomsOffice;
			set => base.TPM_CustomsOffice = value;
		}

		[ResourceStringData("E8992FF2-45C8-4F42-B1E4-4FE397FE9DD2", Caption = "Office Description")]
		public ZString CustomsOfficeDescription => Office?.ZZD_Description ?? ZString.Empty;

		public ZZRefCusCodeListCombined Office => OfficeCore;

		protected virtual ZZRefCusCodeListCombined OfficeCore => office != null && office.ZZD_Code == TPM_CustomsOffice ? office : (office = ZZRefCusCodeListCombined.Loader.LoadTop1ByParentDataGrouping(Factory, TPM_CustomsOffice, Constants.DataGrouping.EuropeanUnion, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today));
		ZZRefCusCodeListCombined office;

		[List(nameof(Lookups) + "." + nameof(DepartureCusTransportMeansLookups.TransportAtBorderTypeOfIdList))]
		[ResourceStringData("DC5B0DB5-FEAC-40EE-9555-D3E4F7EBC1A5", Caption = "Type of Identification", MediumCaption = "Type of ID", ShortCaption = "Type")]
		public override ZString TPM_TypeOfIdentification
		{
			get => base.TPM_TypeOfIdentification;
			set => base.TPM_TypeOfIdentification = value;
		}

		[ResourceStringData("9E3CFDCA-5A9D-4760-9F59-AF85C153ED6B", Caption = "Transport Identification", MediumCaption = "Transport ID", ShortCaption = "Transp. ID")]
		public override ZString TPM_IdentificationNumber
		{
			get => base.TPM_IdentificationNumber;
			set => base.TPM_IdentificationNumber = value;
		}

		public ZString WagonNumber
		{
			get => TPM_IdentificationNumber;
			set => TPM_IdentificationNumber = value;
		}

		public ZWrappedPropertyInfo WagonNumberInfo => GetWrappedZPropertyInfo(nameof(WagonNumber), x => TPM_IdentificationNumberInfo);

		[MaxLength(17)]
		[ResourceStringData("D66BD36C-7436-44F3-A7D8-D82AB64F51CD", Caption = "Conveyance Number", MediumCaption = "Conveyance No.", ShortCaption = "Conv. No.", FullDescription = "Conveyance Reference Number")]
		public override ZString TPM_ReferenceNumber
		{
			get => base.TPM_ReferenceNumber;
			set => base.TPM_ReferenceNumber = value;
		}

		[ResourceStringData("14364534-1CB8-4A65-A247-15712FD46BA2", Caption = "Nationality", ShortCaption = "Nat.")]
		[List(nameof(Lookups) + "." + nameof(DepartureCusTransportMeansLookups.TransportNationalityList))]
		public override ZString TPM_RN_NKTransportNationality
		{
			get => base.TPM_RN_NKTransportNationality;
			set => base.TPM_RN_NKTransportNationality = value;
		}

		[List(nameof(Lookups) + "." + nameof(DepartureCusTransportMeansLookups.TransportNationalityList))]
		public ZString WagonNationality
		{
			get => TPM_RN_NKTransportNationality;
			set => TPM_RN_NKTransportNationality = value;
		}

		public ZWrappedPropertyInfo WagonNationalityInfo => GetWrappedZPropertyInfo(nameof(WagonNationality), x => TPM_RN_NKTransportNationalityInfo);

		ZShort ISequenceNumberLine<ZShort>.SequenceNumber
		{
			get => TPM_SequenceNumber;
			set => TPM_SequenceNumber = value;
		}

		public ZGuid FKToHeader => TPM_ParentID;

		public bool IsTransportBorder => TPM_ParentTableCode == CusInBondMoveHeaderSchema.Constants.Prefix;

		protected override ZString HumanReadableNameCore => Res.GetString("F7AF473F-2B99-4146-A811-0B99BC1FF227", "Departure Customs Office Transport");

		public ICusTransportMeansValidationDecider ValidationDecider => CachedValueHelper.GetValue(ref validationDeciderCached, () => Header?.Configuration.CusTransportMeansConfiguration.GetValidationDecider(Header));
		CachedValue<ICusTransportMeansValidationDecider> validationDeciderCached;
	}
}
