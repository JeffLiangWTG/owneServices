using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Module
{
	public class JobDeclarationModule : Customs.Module.JobDeclarationModule
	{
		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new JobDeclarationFilterBusinessObject();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Broker; }
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new AUJobDeclarationFilterControl(this, GridCollection, (JobDeclarationFilterBusinessObject)FilterBusinessObject);
		}

		protected override Customs.GUI.MultiJobDeclarationForm GetNewMultiJobDeclarationHeader(Business.MultiJobDeclarationHeader header)
		{
			return new MultiJobDeclarationForm(header);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			SetGuiProviders(Factory);
			return new JobDeclarationCollection(Factory);
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewActionMenuItems());
			if (Env.CurrentUser.IsDeveloper && Env.Registry.EnableCustomsDiagnostics)
			{
				result.Add(new ZMenuItem("-"));
				result.Insert(result.Count, new ZMenuItem("Run one cycle of Message Processor Service Task", new EventHandler(RunOneMessageProcessCycle_Click)));
				result.Insert(result.Count, new ZMenuItem("Run one cycle of CTL Message Processor Service Task", new EventHandler(RunOneCTLMessageProcessCycle_Click)));
				result.Insert(result.Count, new ZMenuItem("Run one cycle of CRS Message Processor Service Task", new EventHandler(RunOneCRSMessageProcessCycle_Click)));
				result.Insert(result.Count, new ZMenuItem("Run one cycle of Interchange Sender Service Task", new EventHandler(RunOneInterchangeSenderCycle_Click)));
				result.Insert(result.Count, new ZMenuItem("Run one cycle of Interchange Receiver Service Task", new EventHandler(RunOneInterchangeReceiverCycle_Click)));
				result.Insert(result.Count, new ZMenuItem("Run one cycle of Interchange Processor Service Task", new EventHandler(RunOneInterchangeProcessorCycle_Click)));
				result.Insert(result.Count, new ZMenuItem("Run one cycle of Late and Pending Cargo Report", new EventHandler(RunOneLateAndPendingCargoReportCycle_Click)));
				result.Insert(result.Count, new ZMenuItem("Run one cycle of Reference File Updater", new EventHandler(RunOneReferenceFileUpdateCycle_Click)));
			}
			return result.ToArray();
		}

		CancellationTokenSource CancellationTokenSource => cancellationTokenSource ?? (cancellationTokenSource = new CancellationTokenSource());
		CancellationTokenSource cancellationTokenSource;

		void RunOneMessageProcessCycle_Click(object sender, EventArgs e)
		{
			using (var messageProcessor = new AUCMessageProcessor())
			{
				messageProcessor.ExecuteBatch(CancellationTokenSource.Token);
				DisplayLog(messageProcessor);
			}
		}

		void RunOneCTLMessageProcessCycle_Click(object sender, EventArgs e)
		{
			using (var messageProcessor = new AUCCTLMessageProcessor())
			{
				messageProcessor.ExecuteBatch(CancellationTokenSource.Token);
				DisplayLog(messageProcessor);
			}
		}

		void RunOneCRSMessageProcessCycle_Click(object sender, EventArgs e)
		{
			using (var messageProcessor = new AUCCRSMessageProcessor())
			{
				messageProcessor.ExecuteBatch(CancellationTokenSource.Token);
				DisplayLog(messageProcessor);
			}
		}

		void RunOneInterchangeReceiverCycle_Click(object sender, EventArgs e)
		{
			using (var interchangeRetriever = new AUCInterchangeRetriever())
			{
				interchangeRetriever.ExecuteBatch(CancellationTokenSource.Token);
				DisplayLog(interchangeRetriever);
			}
		}

		void RunOneInterchangeSenderCycle_Click(object sender, EventArgs e)
		{
			using (var interchangeSender = new AUCInterchangeSender())
			{
				interchangeSender.ExecuteBatch(CancellationTokenSource.Token);
				DisplayLog(interchangeSender);
			}
		}

		void RunOneInterchangeProcessorCycle_Click(object sender, EventArgs e)
		{
			using (var interchangeProcessor = new AUCInboundInterchangeProcessor(new BatchProcessor.LoggingInformation()))
			{
				interchangeProcessor.ExecuteBatch(CancellationTokenSource.Token);
				DisplayLog(interchangeProcessor);
			}
		}

		void RunOneLateAndPendingCargoReportCycle_Click(object sender, EventArgs e)
		{
			using (var lateAndPendingCargoReport = new CargoReportWorkflow(new BusinessObjectFactory()))
			{
				lateAndPendingCargoReport.Logger = new BatchProcessor.LoggingInformation();
				lateAndPendingCargoReport.ExecuteBatch(CancellationTokenSource.Token);
				DisplayLog(lateAndPendingCargoReport);
			}
		}

		void RunOneReferenceFileUpdateCycle_Click(object sender, EventArgs e)
		{
			Type type = Type.GetType("Enterprise.Customs.AU.ServiceTasks.ReferenceFilesServiceTask, Enterprise.Customs.AU.ServiceTasks");
			object o = Activator.CreateInstance(type, Array.Empty<object>());
			System.Reflection.MethodInfo methodInfo = type.GetMethod("RunTaskForDebugging", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
			methodInfo.Invoke(o, null);
		}
	}
}
