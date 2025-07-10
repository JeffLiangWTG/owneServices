using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;

namespace CargoWise.EntityFramework
{
	public class ZSaveCommand : Disposable
	{
		public ZSaveCommand(DbCommand standardCommand, List<ILargeColumnSaver> largeColumnSaverList)
		{
			Argument.NotNull(standardCommand, "standardCommand");

			this.StandardCommand = standardCommand;
			this.StandardCommand.IsSaveCommand = true;
			this.LargeColumnSaverList = largeColumnSaverList;
		}

		public void Execute()
		{
			using (StandardCommand.DbConnection.SuspendAuditTriggers())
			{
				ExecuteStandardPart(SaveBigBlobAndTextColumns);
				SaveBigBlobAndTextColumns();
			}
		}

		void ExecuteStandardPart(Action<List<Guid>> recoveryAction)
		{
			try
			{
				StandardCommand.ExecuteNonQuery();
			}
			catch (SqlException e)
			{
				var errorDetails = new SqlExceptionReformatter().Reformat(e);

				if (recoveryAction != null && LargeColumnSaverList is { Count: > 0 })
				{
					throw new ZSaveCommandRecoverableException(e, errorDetails.PrimaryKey, errorDetails.IsConcurrencyError, recoveryAction);
				}
				else
				{
					throw new ZSaveCommandException(e, errorDetails.PrimaryKey, errorDetails.IsConcurrencyError, errorDetails.IsConcurrencyTriggerError);
				}
			}
			catch (Exception e)
			{
				throw new ZSaveCommandException(e, Guid.Empty, false);
			}
		}

		void SaveBigBlobAndTextColumns(List<Guid> pks = null)
		{
			var currentPk = Guid.Empty;

			try
			{
				var largeColumnSaverListToProcess = pks != null ? LargeColumnSaverList.Where(x => pks.Contains(x.RowPk)) : LargeColumnSaverList;

				foreach (ILargeColumnSaver zLargeColumnSaver in largeColumnSaverListToProcess)
				{
					currentPk = zLargeColumnSaver.RowPk;
					zLargeColumnSaver.Save(StandardCommand.DbConnection);
				}
			}
			catch (Exception e)
			{
				throw new ZSaveCommandException(e, currentPk, false);
			}
		}

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				StandardCommand.Dispose();
			}
		}

		internal bool ShouldReclaimMemory
		{
			get
			{
				foreach (var largeColumnSaver in LargeColumnSaverList)
				{
					if (largeColumnSaver is ZBinarySaver && ((ZBinarySaver)largeColumnSaver).Source is MemoryStream)
					{
						return true;
					}
					if (largeColumnSaver is ZTextSaver && ((ZTextSaver)largeColumnSaver).Source is StringReader)
					{
						return true;
					}
				}
				return false;
			}
		}

#if DEBUG

		public string StandardCommandText
		{
			get { return StandardCommand.CommandText; }
		}

#endif

		readonly DbCommand StandardCommand;
		readonly List<ILargeColumnSaver> LargeColumnSaverList;
	}
}
