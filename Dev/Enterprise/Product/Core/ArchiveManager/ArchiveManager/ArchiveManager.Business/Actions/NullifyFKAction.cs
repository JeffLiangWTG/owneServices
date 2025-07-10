using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.Data;
using CargoWise.Integration;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ArchiveManager.Business.Actions
{
	public class NullifyFKAction : IArchiveAction
	{
		public void Add(SchemaGuidColumn fKColumnToNullify, Guid fKValueToBeReplacedWithNull)
		{
			if (FKDictionary.TryGetValue(fKColumnToNullify, out List<Guid> fkList))
			{
				fkList.Add(fKValueToBeReplacedWithNull);
			}
			else
			{
				fkList = new List<Guid>
				{
					fKValueToBeReplacedWithNull
				};
				FKDictionary.Add(fKColumnToNullify, fkList);
			}
		}

		readonly Dictionary<SchemaGuidColumn, List<Guid>> FKDictionary = new();

		#region IArchiveAction Members

		public ITransactionManager BeginTransactionWithManager()
			=> Db.Connection.BeginTransactionWithManager();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "faster to do one single database hit.")]
		public void Execute()
		{
			if (FKDictionary.Count == 0)
			{
				return;
			}

			var sqlBuilder = new StringBuilder(70 + (FKDictionary.Count * 15));
			foreach (var key in FKDictionary.Keys)
			{
				var parameterName = (NoResString)"@" + key.Name + (NoResString)"Values";
				_ = sqlBuilder.Append($@"
					UPDATE {key.TableName}
					SET {key.Name} = null,
						{key.ColumnPrefix}_SystemLastEditTimeUtc = GETUTCDATE(),
						{key.ColumnPrefix}_SystemLastEditUser = '~BP'
					WHERE {key.Name} IN (SELECT value FROM {parameterName})");
			}

			using var command = Db.Connection.Command(sqlBuilder.ToString());
			foreach (var key in FKDictionary.Keys)
			{
				var parameterName = (NoResString)"@" + key.Name + (NoResString)"Values";
				command.AddTableValuedParameter(parameterName, key, FKDictionary[key]);
			}

			_ = command.ExecuteNonQuery();
		}

		#endregion
	}
}
