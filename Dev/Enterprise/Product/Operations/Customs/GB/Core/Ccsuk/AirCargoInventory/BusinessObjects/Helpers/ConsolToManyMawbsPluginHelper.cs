using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class ConsolToManyMawbsPluginHelper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public event EventHandler MawbsChanged;
		public ConsolToManyMawbsPluginHelper(ForwardingConsol consol) : base(consol.Factory)
		{
			this.consol = consol;
		}

		CusMAWBCollection mawbs;
		public CusMAWBCollection Mawbs
		{
			get
			{
				if (mawbs == null)
				{
					mawbs = LoadMawbs();
					OnMawbsChanged();
				}
				return mawbs;
			}
		}

		CusMAWBCollection LoadMawbs()
		{
			var retMawbs = new CusMAWBCollection(consol.Factory);
			var queryForeignKeyAndApp = new ZQuery(CusMAWBSchema.CM_JK, consol.PK);
			queryForeignKeyAndApp.AddToFilter(CusMAWBSchema.CM_ApplicationCode, ApplicationCodeList.Codes.GbCcsuk);
			var queryMawbNumberAndDateAndApplication = new ZQuery();
			if (!consol.JK_MasterBillNum.IsEmpty)
			{
				queryMawbNumberAndDateAndApplication.AddToFilter(CusMAWBSchema.CM_MAWB, consol.JK_MasterBillNum);
				queryMawbNumberAndDateAndApplication.AddToFilter(CusMAWBSchema.CM_JK, DBNull.Value);
				queryMawbNumberAndDateAndApplication.AddToFilter(CusMAWBSchema.CM_ApplicationCode, ApplicationCodeList.Codes.GbCcsuk);
				queryMawbNumberAndDateAndApplication.AddToFilter(CusMAWBSchema.CM_IsActive, true);
				var dateQuery = new ZQuery(CusMAWBSchema.CM_ArrivalDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.Now.AddMonths(-1 * Math.Abs(12)));
				dateQuery.AddToFilter(JoinCondition.Or, CusMAWBSchema.CM_ArrivalDate, ZDateTime.Empty);
				queryMawbNumberAndDateAndApplication.AddToFilter(dateQuery);
			}

			var overallQuery = new ZQuery(queryForeignKeyAndApp);
			overallQuery.AddToFilter(queryMawbNumberAndDateAndApplication, JoinCondition.Or);
			overallQuery.OrderBy = CusMAWBSchema.CM_SystemCreateTimeUtc.Name + " DESC";
			retMawbs.Load(overallQuery);
			return retMawbs;
		}

		protected virtual void OnMawbsChanged()
		{
			MawbsChanged?.Invoke(this, default);
		}

		public int ResetReloadAndCount()
		{
			mawbs = LoadMawbs();
			OnMawbsChanged();
			return Mawbs.Count;
		}

		public void MakeNewMawbAndPurgeCache()
		{
			CusMAWB.CreateNew(consol);
			mawbs = LoadMawbs();
			OnMawbsChanged();
		}

		public void RegisterChildMawbsToConsol()
		{
			foreach (CusMAWB m in Mawbs)
			{
				consol.RegisterEditableChildObject(m);
				m.CM_JK = consol.PK;
			}
		}

		readonly ForwardingConsol consol;
	}
}
