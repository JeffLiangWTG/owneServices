using System;
#if NETFRAMEWORK
using System.Data.Objects;
using System.ServiceModel;
using System.ServiceModel.Channels;
#else
using System.Data.Entity.Core.Objects;
#endif
using CargoWise.Common;
using CargoWise.Services.Common.Model;
using Enterprise.ZArchitecture.Environment;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Services.Common
{
	[CodeAlive("This code is still being used in eServices repo.")]
	// Code Location : https://devops.wisetechglobal.com/wtg/eServices/_git/eServices?path=%2FDistanceCalculation%2FService%2FDistanceCalculationService.svc.cs&version=GBmaster&line=30&lineEnd=30&lineStartColumn=1&lineEndColumn=163&lineStyle=plain&_a=contents
	public static class RequestAuditLogger
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		public static Guid CreateRequestAudit(ServiceRequestConfigurationData configuration, string transactionType, string transactionSubType)
		{
			Argument.NotNull(configuration, nameof(configuration)); // Suggested By ReviewBot 
			var request = new eHubAuditRequest();
			request.B0_PK = Guid.NewGuid();
			request.B0_LicenceCode = configuration.LicenceCode;
			request.B0_ClientSpecifiedIdentifier = configuration.ClientSpecifiedID;
			request.B0_ClientSpecifiedIdentifierType = string.IsNullOrEmpty(configuration.ClientSpecifiedIDType) ? "" : configuration.ClientSpecifiedIDType;
			request.B0_TransactionIdentifier = configuration.TransactionID;
			request.B0_TransactionType = transactionType;
			request.B0_TransactionSubType = transactionSubType;
			request.B0_UserName = configuration.UserName;
			request.B0_RequestUTC = DateTime.UtcNow;
#if NETFRAMEWORK
			if (OperationContext.Current != null)
			{
				request.B0_RequestIP = ((RemoteEndpointMessageProperty)OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name]).Address;
			}
#endif

			var entities = new Entities(ConnectionProvider.GetCommonConnectionString());
			entities.AddToeHubAuditRequest(request);

#if DEBUG
			if (!Globals.IsTest)
#endif
			{
				entities.SaveChanges(SaveOptions.AcceptAllChangesAfterSave);
			}

			return request.B0_PK;
		}
	}
}
