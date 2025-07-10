using System;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Business.Requests;
using Enterprise.DataTransfer.Native.Business.Responses;
using Enterprise.DataTransfer.Native.Business.Xml.Deserializers;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Logging;
using Enterprise.DataTransfer.Native.Common.Stat;
using Enterprise.Integration;

namespace Enterprise.DataTransfer.Native.Business.Update
{
	public class UpdateHandler : IHandler
	{
		public UpdateHandler(INativeFactoryProvider factoryProvider)
		{
			Retries = 3;
			Parser = new EntitySetXmlDeserializer();
			this.factoryProvider = factoryProvider;
		}
		readonly INativeFactoryProvider factoryProvider;

		public Response Execute(Request request, IResponseFactory responseFactory)
		{
			var responseData = Execute(request);
			return BuildResponse(responseFactory, responseData);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		public UpdateResponse Execute(Request request)
		{
			var updateRequest = new UpdateRequest(request);

			UpdateContext context = null;
			var logger = new MemoryLogger();
			AncillaryImportServices sessionServices = null;
			try
			{
				ZExceptionReporting.ProcessWithConcurrencyHandling(() =>
				{
					sessionServices = new AncillaryImportServices(logger);
					var requestConverter = new UpdateContextConverter(sessionServices, factoryProvider);
					context = requestConverter.Convert(updateRequest.Settings);
					using (context)
					{
						var validateResult = updateRequest.Validate();
						if (!validateResult.IsSuccess)
						{
							logger.Error(validateResult.Message);
						}
						else
						{
							var entitySet = Deserialize(updateRequest.Body, sessionServices);

							context.Import(entitySet);

							var messages = context.Statistics.Summary();
							logger.Informations(messages);
						}
					}
				},
				() => logger.Warning("A data concurrency issue has occurred. Attempting to retry."),
				Retries);
			}
			catch (ZDataConcurrencyException concurrencyException)
			{
				logger.Log(LogType.Error,
					FormattableString.Invariant($"A data concurrency issue has occurred multiple times.\r\nThis request has been rejected."),
					concurrencyException);
			}
			catch (Exception exception) when (!exception.IsCriticalException())
			{
				var body = updateRequest.Body;
				logger.LogOrReportException(exception, body?.ToString() ?? "body was null", sessionServices);
			}
			return new UpdateResponse(context?.EntityInfo ?? new EntityInfo(), logger.Buffer);
		}

		int Retries
		{
			get;
			set;
		}

#if DEBUG
		public void OverrideRetriesForTest(int retries) => Retries = retries;
#endif

		IEntitySet Deserialize(XElement inputXml, AncillaryImportServices sessionServices)
		{
			if (inputXml == null)
			{
				return null;
			}
			var entitySets = Parser.Deserialize(inputXml, sessionServices);

			return entitySets;
		}

		static Response BuildResponse(IResponseFactory responseFactory, UpdateResponse responseData)
		{
			var response = responseFactory.GetNewResponse();
			response.EntityInfo = responseData.EntityInfo;
			response.Status = responseData.LogBuffer.GetStatus();
			response.Informations = responseData.LogBuffer.Logs().Select(l => l.ToString()).ToArray();
			return response;
		}

		// Dependency will be injected by Spring.NET
		#region Dependency

		public EntitySetXmlDeserializer Parser;

		#endregion
	}
}
