using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class CusGoodsLocation : EU.NCTS.Business.CusGoodsLocation
		, Integration.Customs.DE.INctsCusGoodsLocation
	{
		public CusGoodsLocation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "This would resolve to an abstract class which is undesirable")]
		public new class Schema : EU.NCTS.Business.CusGoodsLocation.Schema
		{
			public const string AddressIdentificationHolderPK = nameof(CusGoodsLocation.AddressIdentificationHolderPK);
			public const string AddressAuthorisationNumber = nameof(CusGoodsLocation.AddressAuthorisationNumber);
		}

		public new CusGoodsLocationValidation Validation => (CusGoodsLocationValidation)base.Validation;

		public new CusGoodsLocationLookups Lookups => (CusGoodsLocationLookups)base.Lookups;

		protected override Customs.Business.CusGoodsLocationValidation GetNewValidation() => new CusGoodsLocationValidation(this);

		protected override Customs.Business.CusGoodsLocationLookups GetNewLookups() => new CusGoodsLocationLookups(this);

		public new CusGoodsLocationAddress Address => (CusGoodsLocationAddress)base.Address;

		protected override Type AddressType => typeof(CusGoodsLocationAddress);

		[ReadOnlyMember(nameof(CGL_QualifierReadOnly))]
		public override ZString CGL_Qualifier { get => base.CGL_Qualifier; set => base.CGL_Qualifier = value; }

		ZBool CGL_QualifierReadOnly => ParentIsMovementHeader;

		[List(nameof(Address) + "." + nameof(CusGoodsLocationAddress.Lookups) + "." + nameof(CusGoodsLocationAddressLookups.OrganisationList))]
		[ReadOnlyMember(nameof(ParentIsArrivalMovementHeader))]
		[ResourceStringData("C0C67556-5A6D-4406-BA31-1A45BF6E72D5", Caption = "Organization")]
		public ZGuid AddressIdentificationHolderPK
		{
			get => Address.IdentificationHolderPK;
			set
			{
				Address.IdentificationHolderPK = value;
			}
		}

		public ZWrappedPropertyInfo AddressIdentificationHolderPKInfo => GetWrappedZPropertyInfo(Schema.AddressIdentificationHolderPK, x => Address.IdentificationHolderPKInfo);

		[List(nameof(Address) + "." + nameof(CusGoodsLocationAddress.Lookups) + "." + nameof(CusGoodsLocationAddressLookups.AuthorisationNumberList))]
		[ReadOnlyMember(nameof(ParentIsArrivalMovementHeader))]
		[ResourceStringData("F1480CA5-2BE4-40D6-9758-CA1A893FEBC4", Caption = "Authorization Number", ShortCaption = "Authorization No.")]
		public ZString AddressAuthorisationNumber
		{
			get => Address.AuthorisationNumber;
			set
			{
				Address.AuthorisationNumber = value;
			}
		}

		public ZWrappedPropertyInfo AddressAuthorisationNumberInfo => GetWrappedZPropertyInfo(Schema.AddressAuthorisationNumber, x => Address.AuthorisationNumberInfo);

		[List(nameof(Lookups) + "." + nameof(CusGoodsLocationLookups.AdditionalIdentifierList))]
		[MaxLength(nameof(CGL_AdditionalIdentifier_MaxLength))]
		public override ZString CGL_AdditionalIdentifier { get => base.CGL_AdditionalIdentifier; set => base.CGL_AdditionalIdentifier = value.Left(CGL_AdditionalIdentifier_MaxLength); }

		protected override ZString AdditionalIdentifierDescriptionCore => Lookups.AdditionalIdentifierList.GetDescriptionFromCode(CGL_AdditionalIdentifier) ?? ZString.Empty;

		public ZBool ParentIsArrivalMovementHeader => Parent is NctsArrivalMovementHeader;

		protected override void SetDefaultsForNew()
		{
			base.SetDefaultsForNew();
			if (ParentIsMovementHeader)
			{
				CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			}
		}

		internal ZBool ParentIsMovementHeader => Parent is NctsCommonMovementHeader;

		int CGL_AdditionalIdentifier_MaxLength => CGL_Qualifier == CusGoodsLocationQualifierList.Codes.UnLocode ? 5 : 4;
	}
}
