using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Web.Business
{
	public class OrgHeaderAutoCompleteHelper : AutoCompleteHelper
	{
		public OrgHeaderAutoCompleteHelper(BusinessObjectFactory factory)
			: base(factory) { }

		protected override Type BusinessObjectType
		{
			get { return typeof(OrgHeader); }
		}

		protected override SchemaColumn TextColumn
		{
			get { return OrgHeaderSchema.OH_FullName; }
		}

		protected override SchemaColumn KeyColumn
		{
			get { return OrgHeaderSchema.PK; }
		}

		protected override SchemaColumn CodeColumn
		{
			get { return OrgHeaderSchema.OH_Code; }
		}

		#region Overriden Methods

		protected override SchemaColumn[] ColumnsToSelectForListFilter()
		{
			return new SchemaColumn[] {
				OrgHeaderSchema.OH_Code
			};
		}

		ZDBOnlyQuery GetOrgRestrictionQuery()
		{
			ZDBOnlyQuery filter = null;
			if (WebEnv.CurrentUser != null)
			{
				ZGuid currectUserOrgPK = ((OrgContact)WebEnv.CurrentUser).OC_OH;

				filter = new ZDBOnlyQuery(BusinessObjectType);
				filter.AddToFilter(OrgHeaderSchema.PK, currectUserOrgPK);

				if (IsConsignor)
				{
					ZDBOnlySubQuery supplierLinkQuery = new ZDBOnlySubQuery(typeof(OrgSupplierBuyerLink), OrgSupplierBuyerLinkSchema.OL_OH_Supplier);
					supplierLinkQuery.AddToFilter(OrgSupplierBuyerLinkSchema.OL_OH_Buyer, currectUserOrgPK);
					filter.AddSubQuery(OrgHeaderSchema.PK, supplierLinkQuery, JoinCondition.Or);
				}

				if (IsConsignee)
				{
					ZDBOnlySubQuery buyerLinkQuery = new ZDBOnlySubQuery(typeof(OrgSupplierBuyerLink), OrgSupplierBuyerLinkSchema.OL_OH_Buyer);
					buyerLinkQuery.AddToFilter(OrgSupplierBuyerLinkSchema.OL_OH_Supplier, currectUserOrgPK);
					filter.AddSubQuery(OrgHeaderSchema.PK, buyerLinkQuery, JoinCondition.Or);
				}
			}
			return filter;
		}

		protected override ZQuery GetListFilter(ZString key)
		{
			ZDBOnlyQuery filter = GetOrgRestrictionQuery();

			if (filter == null)
			{
				return ZQuery.NoResultQuery;
			}

			ZQuery cnorCneeQuery = new ZQuery();

			if (IsConsignor)
			{
				cnorCneeQuery.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsConsignor, true);
			}
			if (IsConsignee)
			{
				cnorCneeQuery.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsConsignee, true);
			}

			if (!cnorCneeQuery.IsEmpty)
			{
				filter.AddToFilter(cnorCneeQuery);
			}
			filter.AddToFilter(TextColumn, SQLComparisonOperator.StartsWith, GetTextColumnFilterValue(key));
			filter.AddToFilter(JoinCondition.And, OrgHeaderSchema.OH_IsActive, ZBool.True);

			return filter;
		}

		protected override ZQuery GetKeyFilter(ZString text)
		{
			ZQuery filter = GetOrgRestrictionQuery();
			filter.AddToFilter(TextColumn, GetTextColumnFilterValue(text));
			return filter;
		}

		public override string SerializeAdditionalParamsToString()
		{
			return string.Format("{0},{1},{2}", IsConsignor, IsConsignee, NewOrgRelationType);
		}

		public override void RestoreAdditionalParamsFromSerializedString(string serializedParamsString)
		{
			string[] args = serializedParamsString.Split(new char[] { ',' });
			IsConsignor = bool.Parse(args[0]);
			IsConsignee = bool.Parse(args[1]);
			NewOrgRelationType = (NewOrgRelationTypes)Enum.Parse(typeof(NewOrgRelationTypes), args[2]);
		}

		#endregion

		#region Additional Parameters

		public bool IsConsignor;
		public bool IsConsignee;
		public NewOrgRelationTypes NewOrgRelationType = NewOrgRelationTypes.Unknown;

		#endregion

	}
}
