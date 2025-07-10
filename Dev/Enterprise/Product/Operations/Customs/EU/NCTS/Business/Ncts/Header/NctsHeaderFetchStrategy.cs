using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsHeaderFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public NctsHeaderFetchStrategy(NctsHeader header)
			: base(header)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, BusinessObject.PK);
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			var nctsHeader = (NctsHeader)BusinessObject;

			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case nameof(NctsHeader.Job) + "+" + nameof(NctsHeader.Job.JH_Status):
					case nameof(NctsHeader.Job) + "+" + nameof(NctsHeader.Job.JH_HoldReason):
						AddFetchHintJobHeader(nctsHeader);
						break;
					case NctsHeader.Schema.ContactFullName:
					case NctsHeader.Schema.ContactPhone:
					case NctsHeader.Schema.ContactEmail:
						Factory.AddFetchHint(typeof(CusInBondPerson), CusInBondPerson.GetFilter(nctsHeader.PK, CusInBondPerson.LocationContactType));
						break;
				}
			}
		}

		void AddFetchHintJobHeader(NctsHeader nctsHeader)
		{
			var query = new ZQuery(JobHeaderSchema.JH_ParentID, nctsHeader.Shipment?.PK ?? nctsHeader.PK)
				.AddToFilter(JobHeaderSchema.JH_GC, nctsHeader.Company.PK)
				.AddToFilter(JobHeaderSchema.JH_IsActive, true);

			Factory.AddFetchHint(JobHeaderSchema.Instance, query);
		}
	}
}
