using System;
using System.IO;
using System.Xml.Serialization;
using CargoWise.eHub.Products.TWCustoms.TWCustomsGatewayAdapter;
using CargoWise.eHub.Products.TWCustoms.TWCustomsGatewayAdapter.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ILogger = Serilog.ILogger;

namespace CargoWise.eHub.Products.TWCustoms.TWCustomsGateway.Controllers
{
	[ApiController]
	[Produces("application/xml")]
	[Consumes("application/xml")]
	public class TWCustomsGatewayController : ControllerBase
	{
		private readonly ILogger Logger;
		private readonly IGatewayAdapter Adapter;

		public TWCustomsGatewayController(IConfiguration configuration, IPluginManager pluginManager, IFileManager fileManager, IHttpClient httpClient, ILogger logger)
		{
			Logger = logger.ForContext<TWCustomsGatewayController>();
			Adapter = new GatewayAdapter(configuration, pluginManager, fileManager, httpClient, logger);
		}

		[HttpPost("/ReceiveMessage")]
		public TWCustomsGatewayResponse ReceiveMessage([FromBody] TWCustomsGatewayReceiveRequest request)
		{
			return ProcessRequest(request);
		}

		[HttpPost("/SendMessage")]
		public TWCustomsGatewayResponse SendMessage([FromBody] TWCustomsGatewaySendRequest request)
		{
			return ProcessRequest(request);
		}

		private TWCustomsGatewayResponse ProcessRequest(ITWCustomsRequest request)
		{
			Logger.Verbose($"received {nameof(request)}: {ToXml(request)}");

			try
			{
				var result = request is TWCustomsGatewaySendRequest ? Adapter.SendMessage(request) : Adapter.ReceiveMessage(request);

				if (result.HasError)
					return new TWCustomsGatewayResponse(result);

				return TWCustomsGatewayResponse.OK;
			}
			catch (Exception e)
			{
				Logger.Error(e, "An error occur when processing request. {0}");
				return new TWCustomsGatewayResponse(e);
			}
		}

		private string ToXml(object toSerialize)
		{
			if (toSerialize == null) return "null";
			var type = toSerialize.GetType();
			XmlSerializer xmlSerializer = new XmlSerializer(type);

			using (StringWriter textWriter = new StringWriter())
			{
				xmlSerializer.Serialize(textWriter, toSerialize);
				return textWriter.ToString();
			}
		}
	}
}