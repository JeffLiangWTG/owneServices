using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	#region LicenceHeader Collection

	[ModuleID("LicenceHeader")]
	public class LicenceHeaderCollection : BusinessObjectCollection<LicenceHeader>
	{
		public LicenceHeaderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public LicenceHeaderCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return new LicenceHeaderFindBoxListProvider(this); }
		}

		public LicenceHeader FindById(string databaseCode, string companyCode)
		{
			LicenceHeader result = null;

			foreach (LicenceHeader header in this)
			{
				if (header.DatabaseCode == databaseCode && header.CompanyCode == companyCode)
				{
					result = header;
					break;
				}
			}

			return result;
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}

	#endregion

	#region FindBox List Provider

	public class LicenceHeaderFindBoxListProvider : FindBoxListProvider
	{
		public LicenceHeaderFindBoxListProvider(LicenceHeaderCollection collection)
			: base(collection)
		{
		}

		public override string CodeFromPrimaryKey(ZGuid pK)
		{
			LicenceHeader header = List.Factory.Load<LicenceHeader>(pK);
			return header != null ? header.LicenceCode : ZString.Empty;
		}

		protected override IEnumerable<BusinessObject> BizObjsFromCodeWithCompleteFilter(string code)
		{
			return List.Factory.Load<LicenceHeader>(GetQuery(code));
		}

		protected override void AddCodeEqualsFilter(ZQuery query, string code)
		{
			query.AddToFilter(GetQuery(code));
		}

		protected override void AddCodeStartsWithFilter(ZQuery query, string code)
		{
			query.AddToFilter(GetQuery(code));
		}

		ZQuery GetQuery(string code)
		{
			code = code.PadRight(9, ' ');
			ZString enterpriseCode = code.Substring(0, 3);
			ZString companyCode = code.Substring(3, 3);
			ZString serverCode = code.Substring(6, 3);

			if (enterpriseCode.IsEmpty)
			{
				return ZQuery.NoResultQuery;
			}

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(LicenceHeader));
			ZDBOnlySubQuery companyQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), LicenceHeaderSchema.LA_LC);
			if (!companyCode.IsEmpty)
			{
				companyQuery.AddToFilter(LicenceCompanySchema.LC_CompanyCode, SQLComparisonOperator.StartsWith, companyCode);
			}
			ZDBOnlySubQuery enterpriseQuery = new ZDBOnlySubQuery(typeof(LicenceEnterprise), LicenceCompanySchema.LC_LE);
			enterpriseQuery.AddToFilter(LicenceEnterpriseSchema.LE_EnterpriseCode, SQLComparisonOperator.StartsWith, enterpriseCode);
			companyQuery.AddSubQuery(enterpriseQuery, JoinCondition.And);
			query.AddSubQuery(companyQuery, JoinCondition.And);

			if (!serverCode.IsEmpty)
			{
				ZDBOnlySubQuery dBQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceHeaderSchema.LA_LD);
				dBQuery.AddToFilter(LicenceDatabaseSchema.LD_ServerCode, SQLComparisonOperator.StartsWith, serverCode);
				query.AddSubQuery(dBQuery, JoinCondition.And);
			}

			return query;
		}
	}

	#endregion

	#region Company To Database Collection

	[ModuleID("LicenceDatabase")]
	public class LicenceCompanyLicenceDatabaseCollection : ManyToManyBusinessObjectCollection
	{
		public LicenceCompanyLicenceDatabaseCollection(LicenceCompany licCompany)
			: base(licCompany)
		{
			Parent = licCompany;
		}

		protected override Type TypeOfRelationshipBusinessObject
		{
			get { return typeof(LicenceHeader); }
		}

		protected override CargoWise.Schema.SchemaGuidColumn PivotTableFKToAssociatedBusinessObject
		{
			get { return LicenceHeaderSchema.LA_LC; }
		}

		protected override CargoWise.Schema.SchemaGuidColumn PivotTableFKToCollectionBusinessObjects
		{
			get { return LicenceHeaderSchema.LA_LD; }
		}

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			base.SetCollectionRelationships(child);
			using (child.SuspendSettingHasChanges())
			{
				var db = (LicenceDatabase)child;
				db.LD_LE = Parent.LC_LE;
				if (Parent.LicEnterprise != null)
				{
					Parent.LicEnterprise.DatabaseCollectionRequiresReload = true;
				}
			}
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			LicenceDatabase db = (LicenceDatabase)bizOAdded;
			db.LD_IsActiveInfo.ValueChanged += new EventHandler(ActiveValueChanged);
			var licHeader = (LicenceHeader)GetRelationshipBusinessObject(db);
			if (licHeader != null)
			{
				licHeader.LA_IsActiveInfo.ValueChanged += new EventHandler(ActiveValueChanged);
			}

			ActiveValueChanged(null, null);
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			LicenceDatabase db = (LicenceDatabase)bizO;
			db.LD_IsActiveInfo.ValueChanged -= new EventHandler(ActiveValueChanged);
			ActiveValueChanged(null, null);
		}

		public event EventHandler ActiveCountChanged;

		void ActiveValueChanged(object sender, EventArgs e)
		{
			if (ActiveCountChanged != null)
			{
				ActiveCountChanged(this, EventArgs.Empty);
			}
		}

		#region Implementation

		public LicenceCompany Parent { get; private set; }

		public LicenceDatabase this[int index]
		{
			get { return (LicenceDatabase)Elements[index]; }
		}

		public new LicenceDatabase AddNew()
		{
			return (LicenceDatabase)base.AddNew();
		}

		#endregion
	}

	#endregion
}

