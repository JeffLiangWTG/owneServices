using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.ES;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsCusGoodsLocation : EU.NCTS.Business.CusGoodsLocation, ICusGoodsLocation
	{
		public NctsCusGoodsLocation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new NctsCusGoodsLocationLookups Lookups => (NctsCusGoodsLocationLookups)base.Lookups;

		protected override CusGoodsLocationLookups GetNewLookups() => new NctsCusGoodsLocationLookups(this);

		public new EU.NCTS.Business.CusGoodsLocationValidation Validation => (NctsCusGoodsLocationValidation)base.Validation;

		protected override CusGoodsLocationValidation GetNewValidation() => new NctsCusGoodsLocationValidation(this);

		[ReadOnlyMember(nameof(isAdditionalIdentifierReadOnly))]
		[List(nameof(Lookups) + "." + nameof(NctsCusGoodsLocationLookups.AdditionalIdentifierList))]
		[MaxLength(17)]
		public override ZString CGL_AdditionalIdentifier
		{
			get => base.CGL_AdditionalIdentifier;
			set
			{
				var oldValue = base.CGL_AdditionalIdentifier;
				base.CGL_AdditionalIdentifier = value;

				if (!IsCopying && oldValue != value && !value.IsEmpty)
				{
					Header?.ArrivalMovementHeader?.PopulateGuaranteeWhenLocationIsSelectedWithTemporaryStorage();
				}

				if (Header != null && Header.IsPhase5 && Header.ESNctsHeader.CEN_TNNArrival && Header.IsArrivalMovement && oldValue != value && Header.ArrivalMovementHeader.HeaderTNN != null)
				{
					Header.ArrivalMovementHeader.HeaderTNN.MovementHeader.GoodsLocation.CGL_AdditionalIdentifier = value;
				}
			}
		}

		protected override ZString AdditionalIdentifierDescriptionCore
		{
			get
			{
				var list = (ZZRefCusCodeListCombinedCollection)Lookups.AdditionalIdentifierList;
				list.Load(new ZQuery(ZZRefCusCodeListSchema.ZZD_Code, CGL_AdditionalIdentifier));
				return list.Count > 0 ? list.Cast<ZZRefCusCodeListCombined>().First().ZZD_Description : ZString.Empty;
			}
		}

		bool isAdditionalIdentifierReadOnly => Header.IsDepartureMovement ? DepartureMovementHeader.IsPhaseStatusTNNAndPhase5 : (bool)ZBool.False;

		public new NctsHeader Header => (NctsHeader)base.Header;

		public new NctsDepartureMovementHeader DepartureMovementHeader => (NctsDepartureMovementHeader)base.DepartureMovementHeader;

		internal ZBool ParentIsMovementHeader => Parent is EU.NCTS.Business.NctsCommonMovementHeader;

		protected override void SetDefaultsForNew()
		{
			base.SetDefaultsForNew();
			if (ParentIsMovementHeader)
			{
				CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
				CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			}
		}
	}
}
