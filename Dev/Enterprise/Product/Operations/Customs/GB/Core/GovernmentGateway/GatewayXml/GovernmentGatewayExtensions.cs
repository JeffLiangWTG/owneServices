using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using CargoWise.Customs.GB.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Customs.GB.GovernmentGateway
{
	public static class GovernmentGatewayExtensions
	{
		public static GBCustomsRequest NewGBCustomsRequest(EDIMessage message)
		{
			GBCustomsRequest request = null;

			var dataProvider = GetRequestDataProvider(message);
			if (dataProvider != null)
			{
				switch (message.EM_ApplicationCode)
				{
					case EDIInterchange.ApplicationCodes.GbCommonTransitConvention:
						request = new GBCustomsRequest
						{
							JobNumber = dataProvider.JobNumber,
							Provider = ProviderType.CTCGB,
							Credentials = dataProvider.Credentials,
							Service = GetServiceType(message),
							ServiceReference = dataProvider.ServiceReference,
							ContentType = "XML",
							Version = "1.0"
						};
						break;
					case EDIInterchange.ApplicationCodes.GbCustomsNCTS:
						var useApi21 = IsInFinalState(message) && IsUseVersion21ApiForAccept;
						request = new GBCustomsRequest
						{
							JobNumber = dataProvider.JobNumber,
							Provider = ProviderType.CTCGB,
							Credentials = dataProvider.Credentials,
							Service = GetServiceType(message),
							ServiceReference = dataProvider.ServiceReference,
							ContentType = "XML",
							Version = "2.0",
							Accept = $"application/vnd.hmrc.2.{(useApi21 ? '1' : '0')}+json"
						};
						break;
				}
			}
			return request;
		}

		public static bool IsInFinalState(EDIMessage message) => message.EM_LinkedObject switch
		{
			NctsDepartureMovementHeader movementHeader => !movementHeader.IsInPhase5TransitionPeriod,
			NctsHeader nctsHeader => !nctsHeader.IsInPhase5TransitionPeriod,
			_ => true,
		};

		public static bool IsUseVersion21ApiForAccept => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.Ncts5UseApi21, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today);

		static GatewayXml.ServiceTask.Ncts.CTCRequestDataProvider GetRequestDataProvider(EDIMessage message)
		{
			return message.EM_LinkedObject switch
			{
				NctsDepartureMovementHeader movementHeader => new GatewayXml.ServiceTask.Ncts.CTCRequestDataProvider((NctsHeader)movementHeader.Header, message),
				NctsHeader header => new GatewayXml.ServiceTask.Ncts.CTCRequestDataProvider(header, message),
				_ => null,
			};
		}

		public static ZString GetMessageTypeBasedOnApplicationCode(EDIMessage message)
		{
			return message.EM_ApplicationCode.Equals(EDIInterchange.ApplicationCodes.GbCommonTransitConvention) ? message.EM_MessageSubType :
							message.EM_ApplicationCode.Equals(EDIInterchange.ApplicationCodes.GbCustomsNCTS) ? message.EM_MessageType : ZString.Empty;
		}

		static ServiceType GetServiceType(EDIMessage message)
		{
			var messageType = GetMessageTypeBasedOnApplicationCode(message);
			switch (messageType)
			{
				case "015":
					return ServiceType.Depart;
				case "013":
				case "014":
				case "170":
					return ServiceType.UpdateDepart;
				case "007":
					return ServiceType.Arrive;
				case "044":
					return ServiceType.UpdateArrival;
				default:
					throw new NotSupportedException(FormattableString.Invariant($"Message type {messageType} is not supported."));
			}
		}
		static ZString Serialize<T>(T dataObj)
		{
			var settings = new XmlWriterSettings();
			settings.OmitXmlDeclaration = true;
			settings.Indent = true;

			using (var stream = new StringWriter(CultureInfo.InvariantCulture))
			using (var xmlWritter = XmlWriter.Create(stream, settings))
			{
				var serializer = ZXmlSerializer.New(typeof(T));
				serializer.Serialize(xmlWritter, dataObj);
				return stream.ToString();
			}
		}

		public static ZString Serialize(this GBCustomsRequest requestData) => Serialize<GBCustomsRequest>(requestData);

		public static GlbExternalPassword_GB GetValidAccessToken(this NctsHeader header)
		{
			var eori = header.Branch?.OrgProxy.GetEuIdentificationNumber() ?? ZString.Empty;
			var collection = GBGlbCompanyWrapper.GetWrapper<GBGlbCompanyWrapper>(header.Company).GBBPasswordCollection;
			var token = collection.OfType<GlbExternalPassword_GB>()
				.Where(x => x.Status == PasswordStatusList.Codes.Valid && x.EORI == eori && x.IsTokenForNCTS)
				.OrderBy(x => x.Badge)
				.ThenByDescending(x => x.GP_ExpiryDate)
				.FirstOrDefault();

			return token;
		}

		public static bool HasValidAccessToken(this NctsHeader header) => header.GetValidAccessToken() != null;
	}
}
