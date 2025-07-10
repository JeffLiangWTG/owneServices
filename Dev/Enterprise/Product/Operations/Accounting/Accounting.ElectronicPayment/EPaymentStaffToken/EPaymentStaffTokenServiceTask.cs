using System;
using System.Globalization;
using System.Threading;
using CargoWise.Common;
using Enterprise.Accounting.ElectronicPayment.EPaymentStaffToken;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(EPaymentStaffTokenServiceTask.Code,
	"Process Electronic Payment Provider Staff Token",
	"ACC",
	typeof(EPaymentStaffTokenServiceTask),
	IsMandatory = true,
	IsScheduleReadOnly = true,
	CanRunInAnyBranch = true,
	MinimumPeriod = "5minutes",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding("EPT",
	EDIInterchangeSchema.Constants.TableName,
	new[] { EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
				 EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
				 EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
				 EDIInterchangeSchema.Constants.EI_InterchangeType + "=" + EDIInterchangeTypeList.Codes.Configuration,
				 EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.OFX },
	null)]

namespace Enterprise.Accounting.ElectronicPayment.EPaymentStaffToken
{
	public class EPaymentStaffTokenServiceTask : ServiceProviderImpl
	{
		public const string Code = "EPT";

		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			ServiceLogger.Log(LogType.Information, "Process EPayment Provider Staff Token service task started.");
			try
			{
				RunTaskCore(youMustReactToThisToken);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ServiceLogger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "Process EPayment Provider Staff Token service task ended abruptly.\r\nException: {0}\r\nException Message: {1}.", ex.GetType(), ex.Message));
				if (ex is InvalidOperationException || ex is NullReferenceException)
				{
					ErrorReporter.ReportOnce("5945E914-0DC9-41CE-AC5D-9946E0ED2442", "EPT Service Task Invalid Behavior Error", ex);// string of the exception message
				}
			}
			ServiceLogger.Log(LogType.Information, "Process EPayment Provider Staff Token service task completed.");
		}

		void RunTaskCore(CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			new EPaymentStaffTokenProcessor(Logger).ExecuteBatch(token);
		}

		protected LoggingInformation Logger
		{
			get
			{
				if (logger == null)
				{
					logger = new LoggingInformation();
					logger.OnLogInfoAdded += new LoggingInformation.LogInfoAdded(logger_OnLogInfoAdded);
				}
				return logger;
			}
		}
		LoggingInformation logger;

		void logger_OnLogInfoAdded(string log, LogType logType)
		{
			ServiceLogger.Log(logType, log);
		}
	}
}
