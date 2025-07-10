using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
#pragma warning disable CW1108

namespace Enterprise.Scheduler.GraphEngine
{
	public class StmQueueState : IEquatable<StmQueueState>, IQueueState
	{
		public const string Delimiter = ",";
		public const char DelimiterAsChar = ',';

		public StmQueueState(IDataReader reader)
		{
			int col = 0;
			Identifier = reader.GetGuid(col++);
			ParentID = reader.GetGuid(col++);
			TableCode = reader.GetString(col++);
			ParentMessageNumber = reader.GetString(col++);
			Status = reader.GetString(col++);
			Keys = reader.GetString(col++).Split(DelimiterAsChar);
			ChainID = reader.GetGuid(col++);

			IsInDatabase = true;
		}

		public StmQueueState(IBusiness bizo, IList<string> keys, ZString bizoMessageNumber)
		{
			Identifier = Guid.NewGuid();
			ParentID = bizo.Identifier.ToGuid();
			TableCode = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(bizo.TableName);
			ParentMessageNumber = bizoMessageNumber;
			Status = string.Empty;
			Keys = keys;
		}

		internal static IEnumerable<SchemaColumn> Columns()
		{
			yield return StmQueueStateSchema.PK;
			yield return StmQueueStateSchema.SQS_ParentID;
			yield return StmQueueStateSchema.SQS_ParentTableCode;
			yield return StmQueueStateSchema.SQS_ParentMessageNumber;
			yield return StmQueueStateSchema.SQS_Status;
			yield return StmQueueStateSchema.SQS_Keys;
			yield return StmQueueStateSchema.SQS_ChainID;
		}

		public bool IsInDatabase { get; private set; }
		public Guid Identifier { get; }
		public Guid ParentID { get; }
		public Guid ChainID { get; private set; }
		public bool ChainIdChanged { get; private set; }
		public string TableCode { get; }
		public string Status { get; private set; }
		public bool StatusChanged { get; private set; }
		public bool HasChanges => StatusChanged || ChainIdChanged;
		public IEnumerable<string> Keys { get; }
		public string ParentMessageNumber { get; }

		string IQueueState.OrderInfo => ParentMessageNumber;
		string IQueueState.GetParentDetails() => ParentMessageNumber;
		string IQueueState.GetFrontQueueDetails() => ChainID == Guid.Empty ? ParentMessageNumber : $"{ParentMessageNumber} {ChainID}";

		public void SetChainId(Guid id)
		{
			ChainID = id;
			ChainIdChanged = true;
		}

		public void UpdateStatus(string newStatus)
		{
			if (newStatus != Status)
			{
				if (Status == QueueStatusCodes.Codes.Queued)
				{
					ErrorReporter.Instance.Report((NoResString)"Status should never change after a row is queued", FormattableString.Invariant($"StmQueueState.Status should never change after a row is queued: ParentID:{ParentID}, ChainID:{ChainID}"), null);
				}
				Status = newStatus;
				StatusChanged = true;
			}
		}

		public void SetAsDatabaseSynced()
		{
			StatusChanged = false;
			ChainIdChanged = false;
			IsInDatabase = true;
		}

		[Conditional("DEBUG")]
		public void UpdateStatusForTesting(string newStatus) => UpdateStatus(newStatus);

		const int BETTER_LOAD_ESTIMATE = 200;

		internal static List<StmQueueState> Load(DbCommand command, int rowsEstimate)
		{
			var result = new List<StmQueueState>(Math.Min(BETTER_LOAD_ESTIMATE, rowsEstimate));
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					result.Add(new StmQueueState(reader));
				}
			}
			return result;
		}

		#region IEquatable<StmQueueState>

		public virtual bool Equals(StmQueueState other)
		{
			return other.Identifier == Identifier;
		}

		public override bool Equals(object obj)
		{
			return (obj as StmQueueState)?.Equals(this) ?? base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return Identifier.GetHashCode();
		}

		#endregion
	}
}
