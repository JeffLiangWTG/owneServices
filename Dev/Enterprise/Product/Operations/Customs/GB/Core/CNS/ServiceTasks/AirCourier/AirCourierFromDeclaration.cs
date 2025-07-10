using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CNS.CnsAirCourier;
using Enterprise.Customs.GB.CNS.WebServices;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Customs.GB.CNS.ServiceTasks.AirCourier
{
	public class AirCourierFromDeclaration
	{
		public enum AirCourierMessageTypes
		{
			Unknown, Add, Delete
		}

		public AirCourierFromDeclaration(JobDeclaration dec, AirCourierMessageTypes how)
		{
			this.dec = dec;
			this.how = how;
		}
		public string DoEverything()
		{
			var manifest = CreateFromDeclaration();
			var result = UploadAndParseResult(manifest);
			SaveRequestAndRespionseAsMessages(manifest, result);
			return (result.IsSuccess ? "Success" : "Failure") + "\r\n"
				+ result.ResponseCode + "\r\n"
				+ result.WarningOrErrorText + "\r\n"
				+ (result.IsSuccess ? "New MUCR: " + dec.JE_MasterUCR : "");
		}

		void SaveRequestAndRespionseAsMessages(T_AirImportManifest manifest, ResponseHelper result)
		{
			var manifestXml = Serialize(manifest);
			var responseXml = Serialize(result.Result);
			var strategy = new GbMessageNumberStrategy(dec.Factory, EDIMessage.ApplicationCodes.GbCnsCompass);
			var requestMessage = dec.Messages.AddNew();
			var responseMessage = dec.Messages.AddNew();
			requestMessage.MessageNumberStrategy = strategy;
			responseMessage.MessageNumberStrategy = strategy;
			requestMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCnsCompass;
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCnsCompass;
			requestMessage.EM_MessageType = "COU";
			responseMessage.EM_MessageType = "COU";
			requestMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			requestMessage.EM_Status = result.IsSuccess ? EDIMessage.Status.Acknowledged : EDIMessage.Status.Rejected;
			responseMessage.EM_Status = EDIMessage.Status.ProcessedOK;
			requestMessage.EM_MessageText = manifestXml;
			responseMessage.EM_MessageText = responseXml;
			requestMessage.EM_ApplicationReference = "CNS/" + dec.JE_CustomsProfile;
			requestMessage.EM_MessageOwner = dec.JE_CustomsProfile.Left(requestMessage.EM_MessageOwnerInfo.MaxLength);
			if (result.IsSuccess)
			{
				if (how is AirCourierMessageTypes.Add)
				{
					dec.CalculateMasterUCR();
				}
				else
				{ // delete 
					dec.JE_MasterUCR = "";
				}
			}
		}

		public static ZString Serialize<T>(T dataObj)
		{
			var settings = new XmlWriterSettings();
			settings.OmitXmlDeclaration = false;
			settings.Indent = true;

			using (var stream = new StringWriter(CultureInfo.InvariantCulture))
			using (var xmlWritter = XmlWriter.Create(stream, settings))
			{
				var serializer = ZXmlSerializer.New(typeof(T));
				serializer.Serialize(xmlWritter, dataObj);
				return stream.ToString();
			}
		}

		T_AirImportManifest CreateFromDeclaration()
		{
			var origin = dec.Origin?.RL_IATA ?? ZString.Empty;
			var manifest = new T_AirImportManifest();
			manifest.CHIEFRole = dec.GetCredentialCompanyFromDeclarationsBadge().Left(3);

			manifest.Flight = new T_Flight()
			{
				AirportOfDestination = dec.PortOfArrival?.RL_IATA ?? ZString.Empty,
				AirportOfOrigin = origin,
				FlightDateTime = ToUtcSympatheticToDat((dec.JE_DateOfArrival.IsValid ? dec.JE_DateOfArrival : ZDateTime.UtcNow).ToDateTime()),
				FlightNo = dec.JE_VoyageFlightNo.Left(6)
			};

			manifest.Consignment = new T_Consignment()
			{
				Action = how.ToString().Substring(0, 1),
				CarrierPrefix = dec.CourierCarrierCode.Left(3),
				CourierRef1 = dec.CourierConsignmentReference.SubstringSafe(0, 8),
				CourierRef2 = dec.CourierConsignmentReference.SubstringSafe(8, 8),
				Shed = dec.CourierSiteId.Left(4),
				AgentRef = dec.JE_AgentsReference.Left(8),
				Packages = dec.JE_TotalNoOfPacks.ToString(),
				Weight = dec.JE_TotalWeight,
				Description = dec.JE_GoodsDescription.Left(20),
				ValueInd = dec.CourierConsignmentType.Left(1),
				BagRef = dec.CourierBagReference.Left(20),
				ConsignAOO = origin
			};
			return manifest;
		}

		DateTime ToUtcSympatheticToDat(DateTime source)
		{
			var offset = TimeZoneInfo.Local.GetUtcOffset(source); // Remember, .Local could be Aussie during DAT runs
			var newDt = source + offset;
			return newDt.ToUniversalTime();
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]  // Exceptions raised by the other end will be marshalled to us, could be lots of flavours of exception; usage is justified
		ResponseHelper UploadAndParseResult(T_AirImportManifest manifest)
		{
			var soapService = new AirImportManifestBindingQSService1();
			soapService.UserAgent = Chief.GenericMessagingHarness.CredentialsAndBadgeChecker.GetUserAgentForSoapRequests("");
			soapService.Url = "https://www.uat.cnsonline.net/ws/AirCourier/";
			soapService.Credentials = dec.GetCredentialFromDeclarationsBadge()?.GetCredential(null, null);
			soapService.Proxy = HttpWebRequest.DefaultWebProxy;
			CnsWebserviceSetterUpper.SetUrlCredentialsAndAcceptSslCertificate(soapService);
			try
			{
				var result = GetResult(manifest, soapService);
				return ParseResult(result);
			}
			catch (Exception x)
			{
				return new ResponseHelper(false, "", x.Message, null);
			}
		}

		protected virtual T_AirImportManifestResponse GetResult(T_AirImportManifest manifest, AirImportManifestBindingQSService1 soapService)
		{
			return soapService.AirImportManifest(manifest);
		}

		ResponseHelper ParseResult(T_AirImportManifestResponse result)
		{
			if (result != null)
			{
				var responseCode = result.AcknowledgementCode;
				var warningOrErrorText = result.ErrorText;
				var isSuccess = responseCode == "0000" || responseCode == "EA10";
				return new ResponseHelper(isSuccess, responseCode, warningOrErrorText, result);
			}
			return new ResponseHelper(false, "", "", result);
		}

		readonly JobDeclaration dec;
		readonly AirCourierMessageTypes how;

		class ResponseHelper
		{
			public bool IsSuccess { get; private set; }
			public string ResponseCode { get; private set; }
			public string WarningOrErrorText { get; private set; }
			public T_AirImportManifestResponse Result { get; private set; }
			public ResponseHelper(bool isSuccess, string responseCode, string warningOrErrorText, T_AirImportManifestResponse result)
			{
				this.IsSuccess = isSuccess;
				this.ResponseCode = responseCode;
				this.WarningOrErrorText = warningOrErrorText;
				this.Result = result;
			}
		}
	}
}

namespace Enterprise.Customs.GB.CNS.CnsAirCourier
{
	[XmlSerializerAssembly("Enterprise.Customs.GB.CNS.XmlSerializers")]
	public partial class T_AirImportManifest
	{
	}

	[XmlSerializerAssembly("Enterprise.Customs.GB.CNS.XmlSerializers")]
	public partial class T_AirImportManifestResponse
	{ }
}
