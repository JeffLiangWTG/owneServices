using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class NctsArrivalMovementHeader : EU.NCTS.Business.NctsArrivalMovementHeader,
		Integration.Customs.FR.IArrivalMovementHeader
	{
		public NctsArrivalMovementHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override CusInBondMoveHeaderLookups GetNewLookups() => new NctsArrivalMovementHeaderLookups(this);
		protected override IValueSetStrategy GetValueSetStrategy() => valueSetStrategy ?? (valueSetStrategy = new NctsArrivalHeaderMovementHeaderValueSetStrategy(this));
		IValueSetStrategy valueSetStrategy;

		protected override CusInBondMoveHeaderValidation GetNewValidation() => IsPhase5 ? new NctsArrivalMovementHeaderPhase5Validation(this) : new NctsArrivalMovementHeaderPhase4Validation(this);

		public new NctsArrivalMovementHeaderLookups Lookups => (NctsArrivalMovementHeaderLookups)base.Lookups;

		public new NctsArrivalMovementHeaderValidation Validation => IsPhase5 ? (NctsArrivalMovementHeaderPhase5Validation)base.Validation : (NctsArrivalMovementHeaderPhase4Validation)base.Validation;

		protected override ZBool ShouldConsiderDIFUnloadedStateForPackageCount => true;

		public override ZString AuthorizationCode
		{
			get => base.AuthorizationCode;
			set
			{
				var oldValue = AuthorizationCode;
				base.AuthorizationCode = value;
				if (!IsCopying && oldValue != AuthorizationCode)
				{
					UpdateAuthorizationNumber();
				}
			}
		}

		public override ZString AuthorizationNumber
		{
			get => base.AuthorizationNumber;
			set
			{
				var oldValue = AuthorizationNumber;
				base.AuthorizationNumber = value;
				if (!IsCopying && oldValue != AuthorizationNumber)
				{
					UpdateGoodsLocationAfterAuthorizationNumberChange();
				}
			}
		}

		public override ZGuid AuthorizationOwner
		{
			get => base.AuthorizationOwner;
			set
			{
				var oldValue = AuthorizationOwner;
				base.AuthorizationOwner = value;
				if (!IsCopying && oldValue != AuthorizationOwner)
				{
					UpdateAuthorizationNumber();
				}
			}
		}

		protected void UpdateAuthorizationNumber()
		{
			if (!IsPhase4)
			{
				AuthorizationNumber = ZString.Empty;
				var authorisationNumberList = (CusAuthorisationHeaderCollectionFiltered)Lookups.AuthorizationNumberList;
				authorisationNumberList.RefreshFromDb();
				var authorisationCollection = authorisationNumberList.Cast<CusAuthorisationHeader>().Where(x => x.CPH_IsActive);
				if (authorisationCollection.Count() == 1)
				{
					AuthorizationNumber = authorisationCollection.Single().CPH_Number;
				}
			}
		}

		protected void UpdateGoodsLocationAfterAuthorizationNumberChange()
		{
			if (!IsPhase4)
			{
				var goodsLocation = GoodsLocation;
				if (!AuthorizationNumber.IsEmpty)
				{
					goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
					goodsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(NctsArrivalMovementHeaderLookups.NctsTransitStatusList))]
		public override ZString BM_CustomsStatus
		{
			get => base.BM_CustomsStatus;
			set
			{
				base.BM_CustomsStatus = value;
			}
		}

		[List(nameof(Lookups) + "." + nameof(NctsArrivalMovementHeaderLookups.LocationOfGoodsCodeList))]
		public override ZString BM_LocationOfGoodsCode
		{
			get => base.BM_LocationOfGoodsCode;
			set
			{
				base.BM_LocationOfGoodsCode = value;
			}
		}

		[List(nameof(Lookups) + "." + nameof(NctsArrivalMovementHeaderLookups.ExpectedNextCustomsProcedureList))]
		[ResourceStringData("BF42B18D-A63D-479D-9963-641364AF22A2", Caption = "Expected Next Customs procedure", MediumCaption = "Expected Next Customs. Proc.", ShortCaption = "Expected Next Proc.")]
		public override ZString BM_ExpectedNextCustomsProcedure
		{
			get => base.BM_ExpectedNextCustomsProcedure;
			set => base.BM_ExpectedNextCustomsProcedure = value;
		}

		public override bool IsArrivalDetailsReadOnly => base.IsArrivalDetailsReadOnly || Header.Messages.Cast<EDIMessage>().Any(x => x.EM_MessageSubType == MessageSubTypeList.Codes.DT && x.EM_MessageType == "007" && x.EM_InterchangeStatus == EDIMessageStatusList.Codes.Sent);

		[ChildEditable(true)]
		public new NctsFrOfficeCodeCollection CustomsOffices => (NctsFrOfficeCodeCollection)base.CustomsOffices;

		protected override EU.NCTS.Business.NctsEuOfficeCodeCollection GetNewCustomsOffices() => new NctsFrOfficeCodeCollection(this);

		protected override IDictionary<ZString, Type> GetCusCodeDataTypesCore()
		{
			return new Dictionary<ZString, Type>
			{
				{ EU.Business.CusCodeDataTypeList.Codes.OfficeCode, typeof(NctsFrOfficeCode) },
			};
		}
	}
}
