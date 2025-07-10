using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CusGoodsLocation : EU.Business.CusGoodsLocation
		, Integration.Customs.EU.NCTS.ICusGoodsLocation
	{
		public CusGoodsLocation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public static new readonly CusGoodsLocationTypeDecider TypeDecider = new CusGoodsLocationTypeDecider();

		public new CusGoodsLocationLookups Lookups => (CusGoodsLocationLookups)base.Lookups;

		protected override Customs.Business.CusGoodsLocationLookups GetNewLookups() => new CusGoodsLocationLookups(this);

		public new CusGoodsLocationValidation Validation => (CusGoodsLocationValidation)base.Validation;

		protected override Customs.Business.CusGoodsLocationValidation GetNewValidation() => new CusGoodsLocationValidation(this);

		public new CusGoodsLocationAddress Address => (CusGoodsLocationAddress)base.Address;

		protected override Type AddressType => typeof(CusGoodsLocationAddress);

		public NctsHeader Header
		{
			get
			{
				if (header == null)
				{
					switch (CGL_ParentTableCode)
					{
						case CusInBondEventSchema.Constants.Prefix:
							header = (Parent as EnRouteIncident)?.Header;
							break;
						case CusInBondMoveHeaderSchema.Constants.Prefix:
							header = (Parent as NctsCommonMovementHeader)?.Header;
							break;
					}
				}
				return header;
			}
		}
		NctsHeader header;

		ZBool IsPhase5 => Header?.IsPhase5 ?? false;

		ZBool IsParentTableCodeMoveHeader => CGL_ParentTableCode == CusInBondMoveHeaderSchema.Constants.Prefix;

		ZBool IsDepartureMovement => Header?.IsDepartureMovement ?? false;

		ZBool IsArrivalMovement => Header?.IsArrivalMovement ?? false;

		public NctsDepartureMovementHeader DepartureMovementHeader => IsParentTableCodeMoveHeader && IsDepartureMovement ? Parent as NctsDepartureMovementHeader : null;

		public NctsArrivalMovementHeader ArrivalMovementHeader => IsParentTableCodeMoveHeader && IsArrivalMovement ? Parent as NctsArrivalMovementHeader : null;

		public override ZString CGL_Qualifier
		{
			get => base.CGL_Qualifier;
			set
			{
				base.CGL_Qualifier = value;
				if (!IsCopying && IsPhase5)
				{
					DepartureMovementHeader?.MarkAsNeedingValidation();
					Address.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString CGL_Type
		{
			get => base.CGL_Type;
			set
			{
				base.CGL_Type = value;
				if (!IsCopying && IsPhase5)
				{
					DepartureMovementHeader?.MarkAsNeedingValidation();
					Header?.ArrivalMovementHeader?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString CGL_ParentTableCode
		{
			get => base.CGL_ParentTableCode;
			set
			{
				base.CGL_ParentTableCode = value;
				if (!IsCopying)
				{
					if (IsPhase5)
					{
						DepartureMovementHeader?.MarkAsNeedingValidation();
						Address.MarkAsNeedingValidation();
					}
					Header?.ArrivalMovementHeader?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZGuid CGL_ParentID
		{
			get => base.CGL_ParentID;
			set
			{
				base.CGL_ParentID = value;
				if (!IsCopying)
				{
					if (IsPhase5)
					{
						DepartureMovementHeader?.MarkAsNeedingValidation();
						Address.MarkAsNeedingValidation();
					}
					Header?.ArrivalMovementHeader?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString CGL_LocationUse
		{
			get => base.CGL_LocationUse;
			set
			{
				base.CGL_LocationUse = value;
				if (!IsCopying)
				{
					if (IsPhase5)
					{
						DepartureMovementHeader?.MarkAsNeedingValidation();
					}
					Header?.ArrivalMovementHeader?.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("F69B173E-1ED5-4FB5-902D-EF95BF4FD0EE", Caption = "Additional Identifier", MediumCaption = "Additional Ident.", ShortCaption = "Add. Ident.")]
		public override ZString CGL_AdditionalIdentifier
		{
			get => base.CGL_AdditionalIdentifier;
			set
			{
				base.CGL_AdditionalIdentifier = value;
				if (!IsCopying && IsPhase5)
				{
					DepartureMovementHeader?.MarkAsNeedingValidation();
					Header?.ArrivalMovementHeader?.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("DFD5E496-82E1-4AD8-9EEA-59C1CA6040D7", Caption = "Additional Identifier Description", MediumCaption = "Additional Ident. Desc.", ShortCaption = "Add. Ident. Desc.")]
		public ZString AdditionalIdentifierDescription => AdditionalIdentifierDescriptionCore;

		protected virtual ZString AdditionalIdentifierDescriptionCore => ZString.Empty;

		public override ZString CGL_CustomsOffice
		{
			get => base.CGL_CustomsOffice;
			set
			{
				base.CGL_CustomsOffice = value;
				if (!IsCopying && IsPhase5)
				{
					DepartureMovementHeader?.MarkAsNeedingValidation();
					Header?.ArrivalMovementHeader?.MarkAsNeedingValidation();
				}
			}
		}

		public bool IsParentIncidentPhase5Arrival => Parent is EnRouteIncident && Header.IsPhase5Arrival;

		protected override int AdditionalIdentifierMaxLength => 17;
	}
}
