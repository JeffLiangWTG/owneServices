using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.ServiceTasks.XT.Subscribers
{
	public class LicenceEnterpriseSubscriber : LicenceBaseAuditSubscriber
	{
		public override ITableSchema Table => LicenceEnterpriseSchema.Instance;

		public override IEnumerable<SchemaColumn> SpecificColumns => new SchemaColumn[]
		{
			LicenceEnterpriseSchema.LE_EnterpriseCode
		};

		public override bool NotifyInsert => false;

		public override bool NotifyUpdate => true;

		public override bool NotifyDelete => false;

		public override Action<DataRow> CustomFilter => null;

		public override string Code => "XTE";

		public override string Description => "Enterprise Code Change Subscriber";

		public override bool IsRequired()
		{
			return true;
		}

		public override void ProcessChanges(ILogger logger, DataTable changeTable)
		{
			var shouldSave = false;
			logger.Information($"LicenceEnterprise Received DataRows: {changeTable.Rows.Count}");

			foreach (DataRow changeRow in changeTable.Rows)
			{
				if (changeRow.RowState != DataRowState.Modified)
				{
					throw new AggregateException("Add nor Delete operation is not expected.");
				}

				var dataRowVersion = DataRowVersion.Current;

				var enterprisePK = changeRow[LicenceEnterpriseSchema.PK.Name, dataRowVersion].ToStringSafe().Trim();
				var newEnterpriseCode = changeRow[LicenceEnterpriseSchema.Constants.LE_EnterpriseCode, dataRowVersion].ToStringSafe().Trim();
				var oldEnterpriseCode = changeRow[LicenceEnterpriseSchema.Constants.LE_EnterpriseCode, DataRowVersion.Original].ToStringSafe().Trim();

				if (string.IsNullOrEmpty(oldEnterpriseCode) && !string.IsNullOrEmpty(newEnterpriseCode))
				{
					logger.Information($"LicenceEnterprise Subscriber Processing DataRow, Change State: {changeRow.RowState}, Old EnterpriseCode '{oldEnterpriseCode}' - New EnterpriseCode '{newEnterpriseCode}'");
					shouldSave = true;

					try
					{
						SearchLicenceDatabaseRecords(newEnterpriseCode, enterprisePK);
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						var message = $"Error processing LicenceEnterprise changes: Old EnterpriseCode: {oldEnterpriseCode} - New EnterpriseCode {newEnterpriseCode}";
						throw new AggregateException(message, ex);
					}
				}
			}

			if (shouldSave)
			{
				DataFactory.Save();
			}
		}

		void SearchLicenceDatabaseRecords(string newEnterpriseCode, string enterprisePK)
		{
			ZDateTime changeDateTime = ZDateTime.UtcNow;

			var licenceDatabases = GetLicenceDatabaseRecords(enterprisePK);

			foreach (var licenceDatabase in licenceDatabases)
			{
				var transportType = licenceDatabase.LD_LicenceType == DatabaseTypes.Codes.Production ? EDIInterchangeTransportTypeList.Codes.xT : EDIInterchangeTransportTypeList.Codes.tXT;
				var xmlBody = CreateXMLMessage(licenceDatabase.LD_ServerCode, licenceDatabase.LD_LicenceType, licenceDatabase.LD_Password, licenceDatabase.LD_Password, "Add", newEnterpriseCode, changeDateTime, licenceDatabase.LD_DatabaseNumber.ToStringSafe());

				CreateInterchangeMessage(transportType, xmlBody, changeDateTime);
			}
		}

		protected virtual LicenceDatabase[] GetLicenceDatabaseRecords(string enterprisePK)
		{
			ZGuid.TryParse(enterprisePK, out ZGuid pk);
			var filter = new ZQuery(LicenceDatabaseSchema.LD_LE, pk);

			return DataFactory.Load<LicenceDatabase>(filter);
		}
	}
}
