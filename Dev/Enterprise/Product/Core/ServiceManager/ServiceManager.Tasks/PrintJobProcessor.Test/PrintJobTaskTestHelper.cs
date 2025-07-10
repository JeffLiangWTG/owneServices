using System;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor.Testing;

public class PrintJobTaskTestHelper
{
	public PrintJobTaskTestHelper(BusinessObjectFactory factory)
	{
		this.factory = factory;
	}

	readonly BusinessObjectFactory factory;

	public StmPrintJob CreateTestPrintJobAndAddToQueue(ZString jobType, ZGuid? deliveryGroupGuid = null, ZGuid? parentGuid = null, long queueSequence = 0, ZGuid? printQueueGuid = null)
	{
		var printJob = CreateTestPrintJobOnly(jobType, deliveryGroupGuid, parentGuid, printQueueGuid);
		AddPrintJobToQueue(printJob, queueSequence);

		return printJob;
	}

	public StmPrintJob CreateTestPrintJobOnly(ZString jobType, ZGuid? deliveryGroupGuid = null, ZGuid? parentGuid = null, ZGuid? printQueueGuid = null)
	{
		return NewTestPrintJobWithValidData(jobType, deliveryGroupGuid, parentGuid, printQueueGuid);
	}

	StmPrintJob NewTestPrintJobWithValidData(ZString jobType, ZGuid? deliveryGroupGuid = null, ZGuid? parentGuid = null, ZGuid? printQueueGuid = null)
	{
		if (deliveryGroupGuid == null)
		{
			var deliveryGroup = factory.New<StmDeliveryGroup>();
			deliveryGroup.SB_IsProcessed = true;

			deliveryGroupGuid = deliveryGroup.PK;
		}

		var printJob = factory.New<StmPrintJob>();
		printJob.SP_JobType = jobType;
		printJob.SP_EmailAttachmentFormat = "XLS";
		printJob.SP_Destination = "example@example.com";
		printJob.SP_DocumentName = "TestDocument";
		printJob.SP_CustomProperties = new byte[] { 1, 2, 3, 4, 5 };
		printJob.SP_EmailAttachments = "test.XLS";
		printJob.SP_EmailSubjectLine = "PrintJobTaskTest";
		printJob.SP_ParentTableName = "JobShipment";
		printJob.SP_ParentGuid = parentGuid ?? ZGuid.NewZGuid();
		printJob.SP_RelatedBusinessContext = "SHP";
		printJob.SP_RunDateTime = ZDateTime.UtcNow.AddMinutes(-1);
		printJob.SP_SB_DeliveryGroup = deliveryGroupGuid.Value;
		printJob.SP_EDocsProcessed = ZBool.False;

		if (printQueueGuid != null)
		{
			printJob.SP_SQ = printQueueGuid.Value;
		}

		return printJob;
	}

	public StmPrintJobQueue AddPrintJobToQueue(StmPrintJob printJob, long queueSequence = 0)
	{
		var printJobQueue = PrintJobSchedulingTask.QueuePrintJob(printJob, printJob.Factory);
		printJobQueue.SPQ_Sequence = queueSequence;

		return printJobQueue;
	}

	public StmPrintQueue CreateTestPrintQueueAndServer()
	{
		var printServer = factory.New<StmPrintServer>();
		printServer.SPS_ServerName = "Server1";

		var printQueue = factory.New<StmPrintQueue>();
		printQueue.SQ_QueueName = "Queue1";
		printQueue.SQ_DisplayName = "QueueDisplay1";
		printQueue.SQ_WebPrintServiceAddress = "ServiceAddress1";
		printQueue.SQ_SPS_Server = printServer.PK;

		return printQueue;
	}

	public int CountMailDBItems(string subject)
	{
		return (int)Db.Connection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.{MailDBItemsSchema.Constants.TableName} WHERE {MailDBItemsSchema.Constants.MI_Subject} = '{subject}'");
	}

	public int CountMailDBRecipients(string address)
	{
		return (int)Db.Connection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.{MailDBRecipientsSchema.Constants.TableName} WHERE {MailDBRecipientsSchema.Constants.MR_RecipientMailAddress} = '{address}'");
	}

	public static IDisposable ClearUserContext()
	{
		var userContext = EnvProxy.Instance.CurrentUserContext;
		EnvProxy.Instance.ClearUserContext();
		(EnvProxy.Instance as IEnvironmentForTest)?.ResetSecurityForTest();

		return new DisposableAction(() =>
		{
			EnvProxy.Instance.SetUserContext(userContext);
		});
	}
}
