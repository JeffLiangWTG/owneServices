namespace Enterprise.StlAnalysis.Load
{
	using System;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Threading;
	using System.Xml;
	using CargoWise.Common;
	using Enterprise.ZArchitecture.Core.Encryption;

	class EtlController
	{
		public EtlController(EtlLogger logger)
		{
			this.logger = logger;
		}

		public void LoadClientSafe(string serverName, string databaseName, DateTime loadEndDateExclusive, CancellationToken token)
		{
			try
			{
				if (String.IsNullOrWhiteSpace(serverName) || String.IsNullOrWhiteSpace(databaseName))
				{
					throw new ArgumentException("Provide the server and database of client");
				}
				else
				{
					PopulateDateDimension(loadEndDateExclusive);
					token.ThrowIfCancellationRequested();

					var client = LoadClientDetails(serverName, databaseName);
					token.ThrowIfCancellationRequested();

					LoadStlDataFromClient(client, loadEndDateExclusive);
					token.ThrowIfCancellationRequested();
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				logger.ShowError(String.Format(CultureInfo.InvariantCulture, "Failed to run ETL scripts => {0}", ex.Message), ex);
				throw;
			}
		}

		public static DateTime GetFirstAnalysisDate()
		{
			int lastYear = DateTime.UtcNow.Year - 1;
			return new DateTime(Math.Max(lastYear, 2015), 1, 1);
		}

		readonly IEtlLogger logger;

		public static string GetLoadClientScript(string scriptResourceName)
		{
			string result = null;
			string resourceFullName = "Enterprise.StlAnalysis.Load.Scripts.ETL.Dimensions." + scriptResourceName;

			using (var scriptContentsStream = typeof(EtlController).Assembly.GetManifestResourceStream(resourceFullName))
			using (var contentsReader = new StreamReader(scriptContentsStream))
			{
				result = contentsReader.ReadToEnd();
			}

			return result;
		}

		#region Load Data

		HostedClient LoadClientDetails(string serverName, string databaseName)
		{
			logger.StartTask("# Load client from database");
			var xmlText = GetLicenseXml(serverName, databaseName);

			var xml = new XmlDocument();
			xml.LoadXml(xmlText);

			var companyNode = xml.SelectNodes("LicenceKey/Company");
			var enterpriseCode = companyNode[0].Attributes["EnterpriseCode"].InnerText;
			var serverCode = companyNode[0].Attributes["PhysicalServerID"].InnerText;

			if (string.IsNullOrEmpty(enterpriseCode))
			{
				throw new Exception("Missing EnterpriseCode from licence.");
			}
			if (string.IsNullOrEmpty(serverCode))
			{
				throw new Exception("Missing PhysicalServerID from licence.");
			}

			var result = new HostedClientLoader().LoadClientFromSpecificDatabase(enterpriseCode, serverCode, serverName, databaseName);
			logger.StartSubtask("## Completed ## Load client from database");

			return result;
		}

		string GetLicenseXml(string serverName, string databaseName)
		{
			var dbInfo = SqlServerInfo.NewClientSpecificSqlServerInfo(serverName, databaseName);

			using (var connection = DbManager.NewConnectionFromServerInfo(dbInfo))
			{
				connection.Open();

				var sqlText = @"
					SELECT TOP 1
						convert(nvarchar(max), sd.SD_BinaryValue) StringValue
					FROM
						dbo.StmData sd
						INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = sd.SD_Owner
					WHERE
						sd.SD_Name = 'FreightNotesLengthNew'
						AND gc.GC_Code <> 'DEM'
						AND convert(char(1), gc.GC_IsActive) in ('1', 'Y')";

				using (var command = DbManager.NewSqlCommand(sqlText, connection))
				{
					string encryptedKey = command.ExecuteScalar().ToString();

					if (String.IsNullOrWhiteSpace(encryptedKey))
					{
						throw new Exception("Missing licence information in the registry.");
					}
					else
					{
						var encryptionKey = Enterprise.Core.Constants.LicenceConstants.EncryptionKey;
						var encoder = new TwoWayEncoder(encryptionKey);
						return encoder.Decrypt(encryptedKey);
					}
				}
			}
		}

		void PopulateDateDimension(DateTime loadEndDateExclusive)
		{
			logger.StartTask("# Populate Date Dimension");
			new TimeDimensionLoader(loadEndDateExclusive).PopulateDates();
			logger.StartSubtask("## Completed ## Populate Date Dimension");
		}

		void LoadStlDataFromClient(HostedClient client, DateTime loadEndDateExclusive)
		{
			logger.StartTask("# Load Billing Transactions from client");

			var billableFeatures = LicensedFeatureCollection.Instance.Where(f => !String.IsNullOrWhiteSpace(f.BillingQuery));
			var clientStlLoader = new ClientStlDataLoader(logger, billableFeatures, loadEndDateExclusive);
			clientStlLoader.LoadStlDataForClientSafe(client);

			logger.StartSubtask("## Completed ## Load Billing Transactions from client");
		}

		#endregion // Load Data
	}
}
