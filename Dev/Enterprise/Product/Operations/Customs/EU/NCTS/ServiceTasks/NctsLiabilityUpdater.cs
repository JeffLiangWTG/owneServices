using System;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.ServiceTasks
{
	public class NctsLiabilityUpdater
	{
		public NctsLiabilityUpdater(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;
		const int BatchSize = 100;

		public void Run(ILogger serviceLogger, CancellationToken token)
		{
			var reader = new FilteredBusinessObjectReader<NctsHeader>(CreateMovementFilter(), factory)
			{
				BatchSize = BatchSize,
				SaveBeforeLoadNextEnabled = true
			};
			var totalCount = reader.ApproximateCount;
			var processed = 0;
			NctsHeader lastHeader = null;
			NctsHeader[] headers;
			while ((headers = (NctsHeader[])reader.LoadNextBatchInANewFactory(lastHeader)).Length > 0)
			{
				foreach (var header in headers)
				{
					try
					{
						UpdateGoodsItemsLiability(header);
					}
					catch (Exception ex)
					{
						serviceLogger.Log(LogType.Error, $"Error when processing {header.BH_JobReference}: {ex.Message}");
					}
				}
				lastHeader = headers.LastOrDefault();
				processed += headers.Length;

				if (processed % BatchSize == 0 || headers.Length != BatchSize)
				{
					serviceLogger.Log(LogType.Information, $"Processed {processed}/{totalCount} NCTS movements");
				}
				if (token.IsCancellationRequested)
				{
					break;
				}
			}
		}

		ZQuery CreateMovementFilter()
		{
			return new ZQuery(CusInBondHeaderSchema.BH_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.Now.AddYears(-2))
				.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, new[] { CusInBondApplicationCodeList.Codes.NCTS4, CusInBondApplicationCodeList.Codes.NCTS5 })
				.AddToFilter(CusInBondHeaderSchema.BH_HeaderType, new[] { NctsMovementType.Codes.Departure, NctsMovementType.Codes.DepartureAndArrival });
		}

		void UpdateGoodsItemsLiability(NctsHeader header)
		{
			foreach (var goodsItem in header.DepartureGoodsItems)
			{
				if (goodsItem.Fees.Count == 0)
				{
					goodsItem.UpdateAllFeesFromTariffRates();
				}
			}
		}
	}
}
