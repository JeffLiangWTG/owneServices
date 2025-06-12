using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using CargoWise.eHub.Common;
using Common.Logging;

namespace CargoWise.eHub.Gateway
{
	public class eHubErrorHandler : BehaviorExtensionElement, IErrorHandler, IServiceBehavior
	{
		#region Overrides

		public override Type BehaviorType
		{
			get { return typeof(eHubErrorHandler); }
		}

		protected override object CreateBehavior()
		{
			return new eHubErrorHandler();
		}

		#endregion

		#region IErrorHandler Members

		public bool HandleError(Exception error)
		{
			return true;
		}

		public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
		{
			if (error.GetType().IsAssignableFrom(typeof(FaultException)) || error is SystemThrottleException)
			{
				var faultexception = new FaultException<ExceptionDetail>(new ExceptionDetail(error), AddExceptionIDToError(error));
				fault = Message.CreateMessage(version, faultexception.CreateMessageFault(), StreamedServiceConstants.FaultContractAction);
			}
			else
			{
				var applicationException = (error is FaultException<ApplicationFault>) ? new FaultException<ApplicationFault>(new ApplicationFault() { ErrorMessage = AddExceptionIDToError(error), MessageExceptionDictionary = (error as FaultException<ApplicationFault>).Detail.MessageExceptionDictionary }) : new FaultException<ApplicationFault>(new ApplicationFault() { ErrorMessage = "Internal error, refer to Reason for details." }, AddExceptionIDToError(error));

				fault = Message.CreateMessage(version, applicationException.CreateMessageFault(), StreamedServiceConstants.FaultContractAction);
			}
		}

		#endregion

		#region IServiceBehavior Members

		public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
		{
			return;
		}

		public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
			foreach (var ChannelDispatcher in serviceHostBase.ChannelDispatchers.Cast<ChannelDispatcher>())
			{
				ChannelDispatcher.ErrorHandlers.Add(this);
			}
		}

		public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
			return;
		}

		#endregion

		#region Implementation

		public void SetLogger(ILog log)
		{
			logger = log;
		}

		public string AddExceptionIDToError(Exception error)
		{
			var exceptionID = Guid.NewGuid().ToString();
			logger.ThreadVariablesContext.Set(nameof(LogEvent.ExceptionID), exceptionID);
			logger.Error(error);
			logger.ThreadVariablesContext.Remove(nameof(LogEvent.ExceptionID));
			
			string errorMessage;
			if (IsDatabaseErrorNotCausedByApplication(error))
			{
				errorMessage = defaultMessage;
			}
			else
			{
				errorMessage = (error is FaultException<ApplicationFault>) ? (error as FaultException<ApplicationFault>).Detail.ErrorMessage : error.Message;
			}

			return LimitExceptionMessageSize(errorMessage, exceptionID);
		}

		public string LimitExceptionMessageSize(string errorMessage, string exceptionID)
		{
			var exceptionReportTextSizeLimit = int.TryParse(ConfigurationManager.AppSettings["ExceptionReportTextSizeLimit"], out var textLimit) ? textLimit : 307200;
			var suffix = string.Format($"... Batch error text limit reached. Error text truncated\r\n ExceptionID: {exceptionID}.");

			var message = (errorMessage.Length > exceptionReportTextSizeLimit) ? errorMessage.Substring(0, exceptionReportTextSizeLimit - suffix.Length) + suffix : $"{errorMessage} ExceptionID: {exceptionID}.";
			return message;
		}

		public bool IsDatabaseErrorNotCausedByApplication(Exception error)
		{
			return error is SqlException sqlException && !SqlExceptionHandler.IsApplicationException((sqlException).Number);
		}
			

		private static ILog logger = LogManager.GetLogger(typeof(eHubErrorHandler));

		private const string defaultMessage = "eHub Gateway under maintenance. Messages will be resubmitted to eHub automatically on next service task run. Please ignore the following message: Server under maintenance and have not a valid ediEnterprise licence code.\r\n";

		#endregion
	}
}
