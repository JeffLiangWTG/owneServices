using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business
{
	public class CusExitDetail : EU.Business.CusExitDetail, Integration.Customs.DE.ICusExitDetail, IDocAddresses
	{
		public CusExitDetail(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override bool CanDelete => !StatusGreaterThanOrEqualTo310;

		public override MultilingualString ReasonForNotAbleToDelete => ResString.GetMultilingualString("Enterprise.Customs.DE.Business.CusExitDetail|ReasonForNotAbleToDelete", "Cannot delete Movements with Status >= 310");

		public override ZString CED_Status
		{
			get => base.CED_Status;
			set
			{
				var oldValue = CED_Status;
				base.CED_Status = value;
				if (oldValue != CED_Status)
				{
					statusGreaterThanOrEqualTo310?.InvalidateCache();
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "This would resolve to an abstract class which is undesirable")]
		public new class Schema : EU.Business.CusExitDetail.Schema
		{
			public const string ReferenceNumberUCR = nameof(CusExitDetail.ReferenceNumberUCR);
			public const string RegistrationNumberAWB = nameof(CusExitDetail.RegistrationNumberAWB);
			public const string StatusDescription = nameof(CusExitDetail.StatusDescription);

			public const int ReferenceNumberUCRMaxLength = 35;
			public const int RegistrationNumberAWBMaxLength = 35;
		}

		[MaxLength(18)]
		[ReadOnlyMember(nameof(StatusGreaterThanOrEqualTo310))]
		public override ZString CED_MovementReferenceNumber { get => base.CED_MovementReferenceNumber; set => base.CED_MovementReferenceNumber = value; }

		[ReadOnlyMember(nameof(StatusGreaterThanOrEqualTo310))]
		public override ZString CED_CustomsOffice { get => base.CED_CustomsOffice; set => base.CED_CustomsOffice = value; }

		[ResourceStringData("7C4DA2D8-A1D0-497A-B692-73087073A98C", Caption = "Loading Place")]
		public override ZString CED_LocationOfGoods
		{
			get => base.CED_LocationOfGoods;
			set => base.CED_LocationOfGoods = value;
		}

		public new CusExitControlHeader Header => (CusExitControlHeader)base.Header;

		public new CusExitItemCollection CusExitItems => (CusExitItemCollection)base.CusExitItems;

		public new CusExitDetailValidation Validation => (CusExitDetailValidation)base.Validation;

		public new CusExitDetailLookups Lookups => (CusExitDetailLookups)base.Lookups;

		public JobDocAddress DeclarantDocAddress
		{
			get
			{
				if (declarantDocAddress == null || declarantDocAddress.IsDeleted)
				{
					declarantDocAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.Declarant);
				}
				return declarantDocAddress;
			}
		}
		JobDocAddress declarantDocAddress;

		public JobDocAddress RepresentativeDocAddress
		{
			get
			{
				if (representativeDocAddress == null || representativeDocAddress.IsDeleted)
				{
					representativeDocAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.Representative);
				}
				return representativeDocAddress;
			}
		}
		JobDocAddress representativeDocAddress;

		[ChildEditable(true)]
		public JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (docAddresses == null)
				{
					docAddresses = new JobDocAddressDependentCollection(this);
					docAddresses.Load();
					RegisterEditableChildObject(docAddresses);
				}
				return docAddresses;
			}
		}
		JobDocAddressDependentCollection docAddresses;

		[ResourceStringData("8899713E-0786-4E26-984B-84BCBECED64B", Caption = "Reference Number UCR")]
		[MaxLength(Schema.ReferenceNumberUCRMaxLength)]
		public ZString ReferenceNumberUCR
		{
			get => UCRCusEntryNumberWrapper.EntryNumber;
			set => UCRCusEntryNumberWrapper.SetEntryNumber(value, ReferenceNumberUCRInfo);
		}

		CusEntryNumberWrapper UCRCusEntryNumberWrapper => ucrCusEntryNumberWrapper ?? (ucrCusEntryNumberWrapper = new CusEntryNumberWrapper(this, CusEntryNumberTypes.Standard.UniqueConsignementReference));
		CusEntryNumberWrapper ucrCusEntryNumberWrapper;

		public ZPropertyInfo ReferenceNumberUCRInfo => GetZPropertyInfo(Schema.ReferenceNumberUCR);

		[ResourceStringData("A6720551-B810-4EB1-8B8E-DE8D561192DC", Caption = "Registration Number (ext.)")]
		[MaxLength(Schema.RegistrationNumberAWBMaxLength)]
		public ZString RegistrationNumberAWB
		{
			get => AWBCusEntryNumberWrapper.EntryNumber;
			set => AWBCusEntryNumberWrapper.SetEntryNumber(value, RegistrationNumberAWBInfo);
		}

		CusEntryNumberWrapper AWBCusEntryNumberWrapper => awbCusEntryNumberWrapper ?? (awbCusEntryNumberWrapper = new CusEntryNumberWrapper(this, CusEntryNumberTypes.Germany.AirWaybillEntryNumber));
		CusEntryNumberWrapper awbCusEntryNumberWrapper;

		public ZPropertyInfo RegistrationNumberAWBInfo => GetZPropertyInfo(Schema.RegistrationNumberAWB);

		[ResourceStringData("C0B292B5-BF3D-4EBE-B6C8-C28292814B30", Caption = "Status Description")]
		public ZString StatusDescription
		{
			get => Lookups.StatusList.GetDescriptionFromCode(CED_Status);
		}

		public ZPropertyInfo StatusDescriptionInfo => GetZPropertyInfo(Schema.StatusDescription);

		protected override EU.Business.CusExitItemCollection GetNewCusExitItemsCollectionCore() => new CusExitItemCollection(this);

		protected override EU.Business.CusExitDetailValidation GetNewValidation() => new CusExitDetailValidation(this);

		protected override EU.Business.CusExitDetailLookups GetNewLookups() => new CusExitDetailLookups(this);

		public new AdditionalInfoCollection AdditionalInfos => (AdditionalInfoCollection)base.AdditionalInfos;

		protected override EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection GetNewAdditionalInfosCollectionCore() => new AdditionalInfoCollection(this);

		bool StatusGreaterThanOrEqualTo310 => (statusGreaterThanOrEqualTo310 ?? (statusGreaterThanOrEqualTo310 = new RecalculableCachedValue<bool>(() => int.TryParse(CED_Status, out var statusInt) && statusInt >= 310))).Value;
		RecalculableCachedValue<bool> statusGreaterThanOrEqualTo310;

		#region IDocAddresses

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType) => null;

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress) => Env.Security.None;

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate) => null;

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType) => Lookups.OrgHeaderCollection;

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes => new[] { DocAddressType.Declarant, DocAddressType.Representative };

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress) => false;

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		#endregion
	}
}
