using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.BusinessObjects.CusPermit;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class NctsDepartureMovementHeader : EU.NCTS.Business.NctsDepartureMovementHeader
		, Integration.Customs.IENCTS.IDepartureMovementHeader
		, IMessageAttachee
		, IAllowPermitProcessing
	{
		public NctsDepartureMovementHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new NctsHeader Header => (NctsHeader)base.Header;

		protected override IDictionary<ZString, Type> GetCusCodeDataTypesCore()
		{
			var result = base.GetCusCodeDataTypesCore();
			result[EU.Business.CusCodeDataTypeList.Codes.OfficeCode] = typeof(NctsIEOfficeCode);
			return result;
		}

		public new INctsDepartureCargoDescCollection<NctsDepartureCargoDesc> GoodsItems => (INctsDepartureCargoDescCollection<NctsDepartureCargoDesc>)base.GoodsItems;
		protected override INctsCommonCargoDescCollection<NctsCommonCargoDesc> CreateGoodsItems() => new NctsDepartureCargoDescCollection<NctsDepartureCargoDesc>(this);

		public new NctsDepartureMovementHeaderLookups Lookups => (NctsDepartureMovementHeaderLookups)base.Lookups;

		protected override CusInBondMoveHeaderLookups GetNewPhase5Lookups() => new NctsDepartureMovementHeaderLookups(this);

		protected override CusInBondMoveHeaderLookups GetNewPhase4Lookups() => new NctsDepartureMovementHeaderLookups(this);

		public new NctsDepartureMovementHeaderValidation Validation => (NctsDepartureMovementHeaderValidation)base.Validation;
		protected override NctsDepartureMovementHeaderPhase5Validation GetNewPhase5Validation() => new NctsDepartureMovementHeaderValidation(this);

		public new INctsGuaranteeCollection<NctsGuarantee> Guarantees => (INctsGuaranteeCollection<NctsGuarantee>)base.Guarantees;

		protected override INctsGuaranteeCollection<EU.NCTS.Business.NctsGuarantee> GetGuaranteesCore() => new NctsGuaranteeCollection<NctsGuarantee>(this);

		public new CusGoodsLocation GoodsLocation => (CusGoodsLocation)base.GoodsLocation;

		protected override bool ShouldGenerateLocalReferenceNumberOnSavingCore => false;

		public override ZString BM_TransportAtDeparture
		{
			get => base.BM_TransportAtDeparture;
			set => base.BM_TransportAtDeparture = value.ToUpperInvariant();
		}

		public override ZString BM_TransportAtDepartureTrailer1RegNo
		{
			get => base.BM_TransportAtDepartureTrailer1RegNo;
			set => base.BM_TransportAtDepartureTrailer1RegNo = value.ToUpperInvariant();
		}

		public override ZString BM_TransportAtDepartureTrailer2RegNo
		{
			get => base.BM_TransportAtDepartureTrailer2RegNo;
			set => base.BM_TransportAtDepartureTrailer2RegNo = value.ToUpperInvariant();
		}

		public override ZString BM_TOLCarrierID
		{
			get => base.BM_TOLCarrierID;
			set => base.BM_TOLCarrierID = value.ToUpperInvariant();
		}

		public override ZString BM_InBondEntryType
		{
			get => base.BM_InBondEntryType;
			set
			{
				var oldValue = BM_InBondEntryType;
				base.BM_InBondEntryType = value;
				if (!IsCopying && oldValue != BM_InBondEntryType)
				{
					MarkRelatedItemsAsNeedingValidationIfValidationIsSuspended();
				}
			}
		}

		public override ZString BM_TypeOfSecurity
		{
			get => base.BM_TypeOfSecurity;
			set
			{
				var oldValue = base.BM_TypeOfSecurity;
				base.BM_TypeOfSecurity = value;

				if (!IsCopying && oldValue != value)
				{
					Header?.MarkAsNeedingValidation();
					if (!IsPhase5)
					{
						Header?.CustomsOffices?.MarkAsNeedingValidation();
					}
					else
					{
						CustomsOffices?.MarkAsNeedingValidation();
					}
				}
			}
		}

		[ResourceStringData("F2750FA3-1CFD-4220-BA10-33D2EBF123E0", ShortCaption = "Transport Border", Caption = "Transport Border (Active Border)", FullDescription = "[19 03 001 000] Transport Mode at Border", MultipleKey = NctsHeader.Phase5CaptionKey)]
		public override ZString BM_ExportTransportMode
		{
			get => base.BM_ExportTransportMode;
			set
			{
				var oldValue = BM_ExportTransportMode;
				base.BM_ExportTransportMode = value;
				if (!IsCopying && oldValue != BM_ExportTransportMode)
				{
					Header?.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("FA9DB863-C4FC-4BB5-9133-BEABDB0C5F4C", Caption = "Type of Identification", MediumCaption = "Type of ID", FullDescription = "[19 08 061 000] Type of Identification", MultipleKey = NctsHeader.Phase5CaptionKey)]
		public override ZString BM_ActiveBorderIdentificationType { get => base.BM_ActiveBorderIdentificationType; set => base.BM_ActiveBorderIdentificationType = value; }

		#region IMessageAttachee Members

		GlbBranch IMessageAttachee.Branch => Header?.Branch;
		GlbStaff IMessageAttachee.CustomsAgent => null; // TODO: should return Customs Agent
		IRelatedJob IMessageAttachee.RelatedJob => Header;
		ZString IMessageAttachee.LogicalStatus { get => BM_MessageStatus; set => BM_MessageStatus = value; }
		ZString IMessageAttachee.EntryStatus { get => BM_CustomsStatus; set => BM_CustomsStatus = value; }
		IEnumerable<EDIMessage> IMessageAttachee.Messages => Messages.Cast<EDIMessage>();
		ZString IMessageAttachee.MovementReferenceNumber => Header?.MovementReferenceNumber ?? string.Empty;

		#endregion

		protected override Type CusInBondCargoDescTypeCore => typeof(NctsDepartureCargoDesc);

		void MarkRelatedItemsAsNeedingValidationIfValidationIsSuspended()
		{
			if (!IsMarkingAsNeedingValidationSuspended)
			{
				Header.MarkAsNeedingValidation();
				CustomsOffices.MarkAsNeedingValidation();
				GoodsItems.MarkAsNeedingValidationIncludingChildren();
				Representative.MarkAsNeedingValidation();
			}
		}

		protected override void DefaultDepartureLocationCodeFromCusAuthorisationIfBlank(CusAuthorisationHeader authorizationToUse)
		{
			var configuration = Header.Configuration.LocationOfGoodsFromAuthorisationDefaulterConfiguration;
			if (configuration.IsDefaultingEnabled)
			{
				if (GoodsLocation.CGL_AdditionalIdentifier.IsEmpty)
				{
					var locationCode = GetLocationCodeFromCusAuthorisation(authorizationToUse);
					if (locationCode != null)
					{
						GoodsLocation.CGL_Qualifier = configuration.QualifierCode;
						GoodsLocation.CGL_Type = configuration.TypeCode;
						GoodsLocation.CGL_AdditionalIdentifier = locationCode.CPR_ValueFrom.SubstringSafe(0, CusGoodsLocationSchema.CGL_AdditionalIdentifier.MaxLength);
						GoodsLocationDescriptionInfo.RefreshBinding();
					}
				}
			}
		}

		ZInt IAllowPermitProcessing.PermitValueDecimalPlaceCount => Header.PermitValueDecimalPlaceCount;

		ZInt IAllowPermitProcessing.PermitQuantityDecimalPlaceCount => 5;

		ZInt IAllowPermitProcessing.PackageCount => ((IAllowPermitProcessing)Header).PackageCount;

		ZString IAllowPermitProcessing.GetPermitReference() => BM_PaperlessInbondNum;

		ZInt IAllowPermitProcessing.GetPermitReferenceNumberLine() => 0;

		IList<PermitRecord> IAllowPermitProcessing.GetPermitRecords() =>
			Header.GetPermitRecords();

		ZString IAllowPermitProcessing.GetPermitComment(PermitRecord permitRecord) => Header.GetPermitComment(permitRecord);
	}
}
