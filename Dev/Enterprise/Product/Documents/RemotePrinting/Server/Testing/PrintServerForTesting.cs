using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using Enterprise.RemotePrinting.Server.JobPrinting;
using Enterprise.RemotePrinting.Server.RPSCore;

namespace Enterprise.RemotePrinting.Server.Testing;

public class PrintServerForTesting : PrintServer
{
	public PrintServerForTesting(DbConnection testConnection, IEmailSender emailSender = null)
	{
		this.testConnection = testConnection;
		this.emailSender = emailSender;
	}

	public List<ServerPrintJob> GetPrintJobsCore_Exposed(string printServerName)
	{
		return GetPrintJobsCore<ServerPrintJob>(printServerName, TestConnection);
	}

	public List<ServerPrintJobEx> GetPrintAndFaxJobs_Exposed(string printServerName)
	{
		return GetPrintJobsCore<ServerPrintJobEx>(printServerName, TestConnection);
	}

	public void SetPrintQueuesCore_Exposed(string printServerName, List<string> printQueueNames)
	{
		SetPrintQueuesCore(printServerName, printQueueNames.Select(name => new PrintQueueInfo { Name = name, IsSuspectedSurrogate = false }), TestConnection);
	}

	public List<ServerPrintQueue> GetChangedPrintQueuesCore_Exposed(string serverName, List<string> changedQueueNames)
	{
		return GetChangedPrintQueuesCore(serverName, changedQueueNames, TestConnection);
	}

	public ServerWatermark GetWatermarkCore_Exposed()
	{
		return GetWatermarkCore(TestConnection);
	}

	public void SetPrintJobSuccessCore_Exposed(List<Guid> jobPkList)
	{
		SetPrintJobSuccessCore(jobPkList, TestConnection);
	}

	public void SetPrintJobFailureCore_Exposed(List<PrintJobFailed> jobList)
	{
		SetPrintJobFailureCore(jobList, TestConnection);
	}

	protected override DbConnection NewConnection()
	{
		return TestConnection;
	}

	internal DbConnection TestConnection => testConnection ?? (testConnection = Db.NewExtraConnectionToMainDb());
	DbConnection testConnection;

	protected override IEmailSender CreateEmailSender(DbConnection connection)
	{
		return emailSender ?? base.CreateEmailSender(connection);
	}
	readonly IEmailSender emailSender;

	protected override int GetMaxBatchSize(DbConnection connection)
	{
		return MaxBatchSizeOverride >= 0 ? MaxBatchSizeOverride : base.GetMaxBatchSize(connection);
	}

	public int MaxBatchSizeOverride { get; set; } = -1;

	public int GetMaxBatchSizeBaseExposed(DbConnection connection)
	{
		return base.GetMaxBatchSize(connection);
	}

	public bool JobStatusHasChangedFromQUE_Exposed(DbConnection conn, Guid jobPk) => JobHasChangedFromQueOrPrn(conn, jobPk);

	protected override bool JobHasChangedFromQueOrPrn(DbConnection conn, Guid jobPk)
	{
		var sqlText = "";
		MarkFailedProcessedPrintJobsPK.ForEach(pk =>
		{
			sqlText += string.Format(@"
UPDATE dbo.StmPrintJob
SET SP_RetryAttempts = 3, SP_Status = 'FAL'
WHERE SP_PK = '{0}'", pk);
		});

		if (!string.IsNullOrEmpty(sqlText))
		{
			conn.ExecuteNonQuery(sqlText);
		}

		return base.JobHasChangedFromQueOrPrn(conn, jobPk);
	}

	public List<Guid> MarkFailedProcessedPrintJobsPK { get; } = new List<Guid>();

	public void UpdateExistingPrintQueuesDeleteStatus_Exposed(string printServerName, IEnumerable<PrintQueueInfo> printQueues, DbConnection conn) => UpdateExistingPrintQueuesDeleteStatus(printServerName, printQueues, conn);

	public void UpdateSuspectedSurrogateQueuesAllowPrintingStatus_Exposed(string printServerName, IEnumerable<PrintQueueInfo> printQueues, DbConnection conn) => UpdateSuspectedSurrogateQueuesAllowPrintingStatus(printServerName, printQueues, conn);

	public void CreateNewPrintQueues_Exposed(string printServerName, IEnumerable<PrintQueueInfo> printQueues, DbConnection conn) => CreateNewPrintQueues(printServerName, printQueues, conn);
}
