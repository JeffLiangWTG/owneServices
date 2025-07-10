using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.AuditDataServices.MDM.Subscribers
{
	public abstract class PatternMatchingSubscriber<T> : ActualDataChangesAuditSubscriber where T : BusinessObject
	{
		protected abstract string PKColumn { get; }

		public override bool IsRequired()
		{
			return true;
		}

		public override bool NotifyInsert
		{
			get { return true; }
		}

		public override bool NotifyUpdate
		{
			get { return true; }
		}

		public override bool NotifyDelete
		{
			get { return false; }
		}

		protected T BizO { get; set; }

		public override void ProcessChanges(ILogger logger, DataTable changeTable)
		{
			using (Environment.DisposableEnvironment.ForBranch(GlbBranch.GetFirstActiveBranch().PK.ToGuid()))
			{
				var factory = new BusinessObjectFactory();
				var shouldSave = false;

				foreach (DataRow changeRow in changeTable.Rows)
				{
					var patternMasterDetails = Enumerable.Empty<PatternMasterDetail>();

					if (ZGuid.TryParse(changeRow[PKColumn], out ZGuid pk))
					{
						BizO = factory.Load<T>(pk);

						if (BizO != null)
						{
							foreach (string columnToHash in ColumnsToHash)
							{
								var currentValuesToHash = GetValuesToHash(changeRow, columnToHash, DataRowVersion.Current);
								var currentHashedValues = GetHash(currentValuesToHash);

								for (int index = 0; index < currentValuesToHash.Count; index++)
								{
									if (changeRow.RowState == DataRowState.Added && currentHashedValues[index] != 0)
									{
										shouldSave |= CreateNewRecords(factory, index, currentHashedValues[index]);
									}
									else if (changeRow.RowState == DataRowState.Modified)
									{
										var originalValuesToHash = GetValuesToHash(changeRow, columnToHash, DataRowVersion.Original);
										var originalhashedValues = GetHash(originalValuesToHash);

										shouldSave |= PatternMatchingCountryCodeProcessor.UpdatePatternMatchingTables(BizO, GetCountryCodeColumns, changeRow, factory, GetMaster(factory), MasterType);

										if (!originalValuesToHash[index].Equals(currentValuesToHash[index], StringComparison.OrdinalIgnoreCase) && currentHashedValues[index] != 0)
										{
											shouldSave |= UpdateExistingRecords(factory, index, currentHashedValues[index], originalhashedValues[index]);
										}
										else if (currentHashedValues[index] == 0)
										{
											shouldSave |= DeleteExistingRecords(factory, index, originalhashedValues[index]);
										}
									}
								}
							}

							patternMasterDetails = ProcessPatternMatchingResults(factory);
						}
					}

					LogRow(changeRow, logger, patternMasterDetails);
				}

				if (shouldSave)
				{
					TrySave(factory);
				}
			}
		}

		void TrySave(BusinessObjectFactory factory)
		{
			var retryCounter = 3;

			while (--retryCounter >= 0)
			{
				try
				{
					factory.Save();
					return;
				}
				catch (ZSaveConcurrencyException)
				{
					Thread.Sleep(100);
				}
			}
		}

		protected virtual IEnumerable<SchemaColumn> GetCountryCodeColumns
		{
			get
			{
				return Enumerable.Empty<SchemaColumn>();
			}
		}

		protected virtual PatternMasterType MasterType => PatternMasterType.OrgHeader;

		protected abstract ICollection<string> ColumnsToHash { get; }

		protected abstract List<string> GetValuesToHash(DataRow changeRow, string changeRowField, DataRowVersion rowVersion);

		protected abstract bool CreateNewRecords(BusinessObjectFactory factory, int index, int hashedValue);

		protected abstract bool UpdateExistingRecords(BusinessObjectFactory factory, int index, int hashedValue, int originalHashedValue);

		protected abstract bool DeleteExistingRecords(BusinessObjectFactory factory, int index, int hashedValue);

		protected abstract string GetRowDetail(DataRow changeRow);

		protected abstract string GetSubscriberName();

		protected abstract IEnumerable<PatternMasterDetail> ProcessPatternMatchingResultsCore(BusinessObjectFactory factory);

		protected virtual BusinessObject GetMaster(BusinessObjectFactory factory)
		{
			return null;
		}

		void LogRow(DataRow changeRow, ILogger logger, IEnumerable<PatternMasterDetail> patternMasters)
		{
			var subscriberName = GetSubscriberName();
			var rowDetail = GetRowDetail(changeRow);

			if (patternMasters.Any())
			{
				var queuedPatternMasters = patternMasters.Where(x => x.Queued);
				if (queuedPatternMasters.Any())
				{
					logger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture,
						"Queued Deduplication in {0} for {1}. Row: {2}", // subscriber log
						subscriberName,
						string.Join(", ", queuedPatternMasters),
						rowDetail
					));
				}

				var skipPatternMasters = patternMasters.Where(x => !x.Queued);
				if (skipPatternMasters.Any())
				{
					logger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture,
						"Skip Queuing Deduplication in {0} for {1}. Row: {2}", // subscriber log
						subscriberName,
						string.Join(", ", skipPatternMasters),
						rowDetail
					));
				}
			}
			else
			{
				logger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture,
					"None Queuing Deduplication in {0}. Row: {1}", // subscriber log
					subscriberName,
					rowDetail
				));
			}
		}

		IEnumerable<PatternMasterDetail> ProcessPatternMatchingResults(BusinessObjectFactory factory)
		{
			return ProcessPatternMatchingResultsCore(factory).ToArray();
		}

		List<int> GetHash(List<string> valuesToHash)
		{
			var hashes = new List<int>();

			foreach (string valueToHash in valuesToHash)
			{
				var hash = !string.IsNullOrEmpty(valueToHash) ? TextStandardizerHelper.ComputeStringHashFast(valueToHash) : 0;
				hashes.Add(hash);
			}

			return hashes;
		}

		protected string GetValueToUpper(string value)
		{
			return value?.ToUpperInvariant();
		}
	}
}
