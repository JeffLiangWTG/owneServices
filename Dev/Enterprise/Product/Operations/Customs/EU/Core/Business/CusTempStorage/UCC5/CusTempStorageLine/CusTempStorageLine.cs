using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.EU;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	[SingleObjectAroundARow]
	[DependentBusinessObject(typeof(CusTempStorageDec), "CusTempStorageLines")]
	public class CusTempStorageLine : AutoCusTempStorageLine, IHugeSequenceNumberLine
		, ICusTempStorageLine
	{
		public CusTempStorageLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema 

		public new class Schema : AutoCusTempStorageLine.Schema
		{
			public const string CustodianOrgPK = "CustodianOrgPK";
			public const string GoodsOwnerOrgPK = "GoodsOwnerOrgPK";
		}

		#endregion

		#region Type Decider

		public static readonly CusTempStorageLineTypeDecider TypeDecider = new CusTempStorageLineTypeDecider();

		#endregion

		#region Properties

		#region TSL_STH

		[RelatedBusinessObject("Dec")]
		public override ZGuid TSL_STH
		{
			get { return base.TSL_STH; }
			set { base.TSL_STH = value; }
		}

		public CusTempStorageDec Dec => LoadDec();

		protected virtual CusTempStorageDec LoadDec() => Factory.Load<CusTempStorageDec>(TSL_STH);

		#endregion

		#region CustodianOrgPK

		[ResourceStringData("88CFDB2E-BA04-45A2-9850-8572947369B2", Caption = "Custodian")]
		public ZGuid CustodianOrgPK
		{
			get => TSL_OA_Custodian_ZAddress.OrgPK;
			set => TSL_OA_Custodian_ZAddress.OrgPK = value;
		}

		public ZPropertyInfo CustodianOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CustodianOrgPK, x => TSL_OA_Custodian_ZAddress.OrgPKInfo); }
		}

		[List(nameof(Lookups) + "." + nameof(CusTempStorageLineLookups.OrganizationsFindBoxList))]
		public override ZGuid TSL_OA_Custodian { get => base.TSL_OA_Custodian; set => base.TSL_OA_Custodian = value; }

		#endregion

		#region GoodsOwnerOrgPK

		public ZGuid GoodsOwnerOrgPK
		{
			get => TSL_OA_GoodsOwner_ZAddress.OrgPK;
			set => TSL_OA_GoodsOwner_ZAddress.OrgPK = value;
		}

		public ZPropertyInfo GoodsOwnerOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.GoodsOwnerOrgPK, x => TSL_OA_GoodsOwner_ZAddress.OrgPKInfo); }
		}

		[List(nameof(Lookups) + "." + nameof(CusTempStorageLineLookups.OrganizationsFindBoxList))]
		public override ZGuid TSL_OA_GoodsOwner { get => base.TSL_OA_GoodsOwner; set => base.TSL_OA_GoodsOwner = value; }

		#endregion

		#region ShouldRemoveReleatedToLinesWhenDeleting

		public bool ShouldDeleteReleatedToLinesWhenDeleting => ShouldDeleteReleatedToLinesWhenDeletingCore();

		protected virtual bool ShouldDeleteReleatedToLinesWhenDeletingCore() => true;

		#endregion

		#region SequenceNumberEnabled

		public bool SequenceNumberEnabled => SequenceNumberEnabledCore;

		protected virtual bool SequenceNumberEnabledCore => true;

		#endregion

		#region IHugeSequenceNumberLine

		public virtual ZInt SequenceNumber { get => TSL_LineNo; set => TSL_LineNo = value; }

		public ZGuid FKToHeader => TSL_STH;

		protected virtual bool SetLineNumOnSettingTSL_STHEnabled => true;

		#endregion

		#endregion

		#region CusTempStorageLineItems

		protected virtual ICusTempStorageLineItemCollection<CusTempStorageLineItem> NewCusTempStorageLineItemCollection()
		{
			return new CusTempStorageLineItemCollection<CusTempStorageLineItem>(this);
		}

		[ChildEditable(true)]
		public ICusTempStorageLineItemCollection<CusTempStorageLineItem> CusTempStorageLineItems
		{
			get
			{
				if (cusTempStorageLineItems == null)
				{
					cusTempStorageLineItems = NewCusTempStorageLineItemCollection();
					RegisterEditableChildObject(cusTempStorageLineItems);
				}
				return cusTempStorageLineItems;
			}
		}
		ICusTempStorageLineItemCollection<CusTempStorageLineItem> cusTempStorageLineItems;

		#endregion

		#region Validation

		protected override CusTempStorageLineValidation GetNewValidation() => new CusTempStorageLineValidation(this);

		#endregion

		#region Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			TSL_ReferenceNumberType = "T1";
		}

		public override void Delete()
		{
			base.Delete();
			this.DeleteChildren<CusTempStorageLineItem>(CusTempStorageLineItemSchema.TSI_TSL);
			this.DeleteChildren<CusTempStorageLinePivot>(CusTempStorageLinePivotSchema.SLR_TSL_FromLine);
		}

		#endregion
	}
}
