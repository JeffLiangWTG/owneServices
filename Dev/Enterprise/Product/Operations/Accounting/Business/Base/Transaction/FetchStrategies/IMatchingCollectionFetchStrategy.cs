using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public class IMatchingCollectionFetchStrategy : BusinessObjectCollectionFetchStrategy
	{
		public IMatchingCollectionFetchStrategy(IMatchingCollection iMatchingCollection)
			: base(iMatchingCollection)
		{
		}

		protected override void FetchForViewCore(BusinessObject[] businessObjects, TableColumn[] columns)
		{
			if (businessObjects.Any() && columns.FirstOrDefault(x => x.ColumnName == "VoyageVesselOrFlightDate") != null)
			{
				foreach (var bizO in businessObjects)
				{
					BusinessObjectFactory readonlyFactory = bizO.Factory.GetCachedReadOnlyFactory();
					var transactionHeader = bizO as AccTransactionHeader;
					if (transactionHeader != null && transactionHeader.AH_JH.IsValid)
					{
						readonlyFactory.AddFetchHint(JobHeaderSchema.Constants.TableName, transactionHeader.AH_JH);
						if (!readonlyFactory.HasContext(BusinessContext.MatchingReadOnly))
						{
							readonlyFactory.SetContext(BusinessContext.MatchingReadOnly);
						}
					}
				}
			}
		}
	}
}