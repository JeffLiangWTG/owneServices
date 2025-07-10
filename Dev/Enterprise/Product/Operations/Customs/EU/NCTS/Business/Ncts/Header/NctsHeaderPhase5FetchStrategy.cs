using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsHeaderPhase5FetchStrategy : NctsHeaderFetchStrategy
	{
		public NctsHeaderPhase5FetchStrategy(NctsHeader header)
			: base(header)
		{
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			var nctsHeader = (NctsHeader)BusinessObject;

			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case NctsHeader.Schema.Explanation:
					case NctsHeader.Schema.HeaderUnloadingNotes:
					case NctsHeader.Schema.UnloadedMeansOfTransportAtDepartureIdentity:
					case NctsHeader.Schema.UnloadedMeansOfTransportAtDepartureNationality:
						Factory.AddFetchHint(CusAddInfoSchema.B7_ParentID, nctsHeader.PK);
						break;
				}
			}
		}
	}
}
