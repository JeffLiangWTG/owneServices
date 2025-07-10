using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.eRouter.Business
{
	public class eRouterEdiEnterpriseCommunicationCollection : BusinessObjectCollection<eRouterEdiEnterpriseCommunication>
	{
		public eRouterEdiEnterpriseCommunicationCollection(EDIOrgHeader orgHeader)
			: this(orgHeader.Factory)
		{
			this.OrgHeader = orgHeader;
		}

		public eRouterEdiEnterpriseCommunicationCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			SetReadOnlyIncludingChildren(true);
		}

		public readonly EDIOrgHeader OrgHeader;

		protected override BusinessObject AddNewCore(Type bizOType)
		{
			throw new NotSupportedException("AddNew eRouterEdiEnterpriseCommunication is not supported");
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			if (OrgHeader != null)
			{
				result.AddToFilter(eRouterEdiEnterpriseCommunicationSchema.EC_EnterpriseCode, OrgHeader.LicenceEnterpriseCode);
			}
			return result;
		}

		//public override void Load(ZQuery alternativeAdditionalFilter)
		//{
		//	using (SuspendListChanged())
		//	{
		//		RemoveAllButLeaveRelationshipsIntact();

		//		ZQuery filter = GetCompleteLoadFilter(alternativeAdditionalFilter);
		//		filter.FetchOnlyFromLocalCache = false;
		//		DataSet dataSet = new DataSet();
		//		var conInfo = new ZSqlConnectionInfo(Db.Connection, "[syderouter.db.corporate.cargowise.com].eRouter");
		//		var dataQuery = new ZDataQuery(conInfo, eRouterEdiEnterpriseCommunicationSchema.Constants.TableName, filter);
		//		ZSqlLoader loader = new ZSqlLoader(dataSet, conInfo);
		//		var response = loader.LoadPersistentRowsIntoDataSet(dataQuery).DataRowLoadResponses[0];
		//		var rows = response.AllRows;
		//		foreach (var row in rows)
		//		{
		//			var item = new eRouterEdiEnterpriseCommunication(Factory, row);
		//			Add(item);
		//		}

		//		ReSort();
		//	}
		//}
	}
}
