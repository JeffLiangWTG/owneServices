using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using CargoWise.Bi.Common;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.ServiceTasks.XT.Subscribers
{
	public class LicenceDatabaseSubscriber : LicenceBaseAuditSubscriber
	{
		public override ITableSchema Table => LicenceDatabaseSchema.Instance;

		public override IEnumerable<SchemaColumn> SpecificColumns => new SchemaColumn[]
		{
			LicenceDatabaseSchema.LD_IsActive,
			LicenceDatabaseSchema.LD_LE,
			LicenceDatabaseSchema.LD_LicenceType,
			LicenceDatabaseSchema.LD_Password,
			LicenceDatabaseSchema.LD_ServerCode,
			LicenceDatabaseSchema.LD_DatabaseNumber
		};

		public override bool NotifyInsert => true;

		public override bool NotifyUpdate => true;

		public override bool NotifyDelete => true;

		public override Action<DataRow> CustomFilter => null;

		public override string Code => "XTD";

		public override string Description => "Database Code Change Subscriber";

		public override bool IsRequired()
		{
			return true;
		}

		public override void ProcessChanges(ILogger logger, DataTable changeTable)
		{
			var shouldSave = false;
			logger.Information($"LicenceDatabase Received DataRows: {changeTable.Rows.Count}");

			var branch = GlbBranch.GetOneActiveBranchPerCompany()[0];
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid(), reportInactive: false))
			{
				foreach (DataRow changeRow in changeTable.Rows)
				{
					var dataRowVersion = changeRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;

					var changeRowValue = GetDataRowDetails(changeRow, changeRow.RowState); // CA2208 requires the argument to the ArgumentOutOfRangeException constructor to be an argument of the method in which the exception is thrown
					logger.Information($"LicenceDatabase Subscriber Processing DataRow, Change State: {changeRow.RowState}; Value: {changeRowValue}");

					var isActive = bool.Parse(changeRow[LicenceDatabaseSchema.Constants.LD_IsActive, dataRowVersion].ToStringSafe().Trim());
					if (changeRow.RowState == DataRowState.Added && !isActive)
					{
						continue;
					}
					shouldSave = true;

					try
					{
						CreateInterchange(logger, changeRow, dataRowVersion);
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						var message = "Error processing LicenceDatabase changes: " + GetDataRowDetailsCore(changeRow, dataRowVersion);
						throw new AggregateException(message, ex);
					}
				}
			}

			if (shouldSave)
			{
				DataFactory.Save();
			}
		}

		void CreateInterchange(ILogger logger, DataRow changeRow, DataRowVersion rowVersion)
		{
			ZDateTime changeDateTime = ZDateTime.UtcNow;
			var licenceType = changeRow[LicenceDatabaseSchema.Constants.LD_LicenceType, rowVersion].ToStringSafe().Trim();
			var enterprisePK = changeRow[LicenceDatabaseSchema.Constants.LD_LE, rowVersion].ToStringSafe().Trim();
			var originalEnterprisePK = changeRow.RowState == DataRowState.Added ? enterprisePK : changeRow[LicenceDatabaseSchema.Constants.LD_LE, DataRowVersion.Original].ToStringSafe().Trim();
			var enterpriseCode = GetEnterpriseCode(new Guid(enterprisePK), changeRow.RowState == DataRowState.Deleted, logger);

			if (!string.IsNullOrEmpty(enterpriseCode))
			{
				var transportType = licenceType == DatabaseTypes.Codes.Production ? EDIInterchangeTransportTypeList.Codes.xT : EDIInterchangeTransportTypeList.Codes.tXT;

				var dataRowState = changeRow.RowState;

				if (changeRow.RowState == DataRowState.Modified && enterprisePK != originalEnterprisePK)
				{
					var originalEnterpriseCode = GetEnterpriseCode(new Guid(originalEnterprisePK), false, logger);
					dataRowState = DataRowState.Deleted;
					rowVersion = DataRowVersion.Original;
					var xmlBodyOriginal = GetXMLMessage(changeRow, rowVersion, originalEnterpriseCode, changeDateTime, dataRowState);
					CreateInterchangeMessage(transportType, xmlBodyOriginal, changeDateTime);
					dataRowState = DataRowState.Added;
					rowVersion = DataRowVersion.Current;
				}

				var xmlBody = GetXMLMessage(changeRow, rowVersion, enterpriseCode, changeDateTime, dataRowState);

				CreateInterchangeMessage(transportType, xmlBody, changeDateTime);
			}
		}

		protected virtual string GetEnterpriseCode(Guid enterprisePK, bool isDeleted, ILogger logger)
		{
			var licenceEnterprise = DataFactory.Load<LicenceEnterprise>(enterprisePK);
			string enterpriseCode = licenceEnterprise?.LE_EnterpriseCode;

			if (enterpriseCode == null && isDeleted)
			{
				enterpriseCode = GetEnterpriseCodeFromAuditServer(enterprisePK);
			}

			if (enterpriseCode == null)
			{
				var message = $"Licence Enterprise Code was not found PK {enterprisePK}";
				throw new ApplicationException(message);
			}

			return enterpriseCode;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		string GetEnterpriseCodeFromAuditServer(Guid enterprisePK)
		{
			var enterpriseCode = "";
			var auditServer = BiServers.LoadAuditServerUsingCacheIfPossible(Db.Connection);

			var enterpriseCodeQuery = $@" SELECT TOP 1 LE_EnterpriseCode FROM dbo.LicenceEnterprise
				WHERE LE_PK = @enterprisePK";

			using (var auditConnection = Db.NewExtraConnectionWithMainDbCredentials(auditServer, Db.AuditDatabaseName))
			using (var command = auditConnection.Command(enterpriseCodeQuery))
			{
				command.AddParameter("@enterprisePK", SqlDbType.UniqueIdentifier, enterprisePK);

				enterpriseCode = command.ExecuteScalar()?.ToString();
			}
			return enterpriseCode;
		}

		protected virtual string GetXMLMessage(DataRow changeRow, DataRowVersion rowVersion, string enterpriseCode, ZDateTime changeDateTime, DataRowState dataRowState)
		{
			string changeType;
			switch (dataRowState)
			{
				case DataRowState.Added:
					changeType = "Add";
					break;
				case DataRowState.Deleted:
					changeType = "Delete";
					break;
				case DataRowState.Modified:
					changeType = "Update";
					var newIsActive = bool.Parse(changeRow[LicenceDatabaseSchema.Constants.LD_IsActive, rowVersion].ToStringSafe().Trim());
					var oldIsActive = bool.Parse(changeRow[LicenceDatabaseSchema.Constants.LD_IsActive, DataRowVersion.Original].ToStringSafe().Trim());
					if (newIsActive != oldIsActive)
					{
						changeType = newIsActive ? "Add" : "Delete";
					}
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(dataRowState), $"{dataRowState} contains an invalid value");
			}

			var databaseCode = changeRow[LicenceDatabaseSchema.Constants.LD_ServerCode, rowVersion].ToStringSafe().Trim();
			var databaseNumber = changeRow[LicenceDatabaseSchema.Constants.LD_DatabaseNumber, rowVersion].ToStringSafe().Trim();
			var licenceType = changeRow[LicenceDatabaseSchema.Constants.LD_LicenceType, rowVersion].ToStringSafe().Trim();
			var password = changeRow[LicenceDatabaseSchema.Constants.LD_Password, rowVersion].ToStringSafe().Trim();
			var oldPassword = dataRowState == DataRowState.Modified ? changeRow[LicenceDatabaseSchema.Constants.LD_Password, DataRowVersion.Original].ToStringSafe().Trim() : "";

			return CreateXMLMessage(databaseCode, licenceType, password, oldPassword, changeType, enterpriseCode, changeDateTime, databaseNumber);
		}

		string GetDataRowDetails(DataRow changeRow, DataRowState changeRowState)
		{
			switch (changeRowState)
			{
				case DataRowState.Added:
					return GetDataRowDetailsCore(changeRow, DataRowVersion.Current);
				case DataRowState.Deleted:
					return GetDataRowDetailsCore(changeRow, DataRowVersion.Original);
				case DataRowState.Modified:
					return $"Original Version: {GetDataRowDetailsCore(changeRow, DataRowVersion.Original)} \r\n Current Version: {GetDataRowDetailsCore(changeRow, DataRowVersion.Current)}";
				default:
					throw new ArgumentOutOfRangeException(nameof(changeRowState), $"{nameof(changeRowState)} contains an invalid value");
			}
		}

		string GetDataRowDetailsCore(DataRow changeRow, DataRowVersion dataRowVersion)
		{
			var message = new StringBuilder();
			message.Append("Data row details");

			foreach (DataColumn column in changeRow.Table.Columns)
			{
				var value = changeRow[column, dataRowVersion];
				var valueAsString = value is Array arr
					? FormattableString.Invariant($"{arr.GetType().Name} length {arr.Length}")
					: value?.ToString();

				message.AppendLine();
				message.Append(FormattableString.Invariant($"{column.ColumnName}: {valueAsString}"));
			}

			return message.ToString();
		}
	}
}
