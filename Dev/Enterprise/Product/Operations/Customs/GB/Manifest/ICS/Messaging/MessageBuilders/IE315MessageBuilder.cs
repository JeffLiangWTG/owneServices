using System.Reflection;
using CargoWise.BrandManager;
using CargoWise.Customs.GB.MessageDefinitions.ICS.HMRC_ICS_IE315;
using CargoWise.Customs.GB.MessageDefinitions.ICS.HMRC_wsHeader_v1_1;
using CargoWise.Customs.GB.MessageDefinitions.ICS.oasis_200401_wss_wssecurity_secext_v1_0;
using CargoWise.Types;
using Enterprise.Customs.GB.Registry;

namespace Enterprise.Customs.GB.ICS.Messaging.IE315
{
	public class IE315MessageBuilder : MessageBuilder
	{
		public IE315MessageBuilder(IDeclaration inputDeclaration)
		{
			InputDeclaration = inputDeclaration;
			OutputMessageObject = new Envelope();
		}
		IDeclaration InputDeclaration { get; }
		Envelope OutputMessageObject { get; }

		protected override object GetPopulatedMessageObject()
		{
			PopulateHeader();
			PopulateBody();

			return OutputMessageObject;
		}

		void PopulateHeader()
		{
			var header = new EnvelopeHeader();

			void PopulateHeaderInfo()
			{
				var headerInfo = new Info();
				headerInfo.VendorName = new InfoVendorName { Uri = "http://www.wisetechglobal.com", Value = BrandingFactory.Instance.CompanyName };
				headerInfo.VendorId = "1601";
				headerInfo.VendorProduct = new InfoVendorProduct { Value = BrandingFactory.Instance.ProductName, Version = Assembly.GetExecutingAssembly().GetName().Version.ToString() };
				headerInfo.ServiceId = "1138";
				headerInfo.ServiceMessageType = "HMRC-ICS-IE315-DIRECT";
				headerInfo.SubmissionTimestamp = ZDateTime.UtcNow.ToDateTime();

				header.Info = headerInfo;
			}

			void PopulateHeaderSecurity()
			{
				var security = new EnvelopeHeaderSecurity
				{
					BinarySecurityToken = new BinarySecurityTokenType { Value = "BinarySecurityToken", ValueType = "http://www.hmrc.gov.uk#MarkToken" },
					UsernameToken = new UsernameTokenType
					{
						Username = new AttributedString { Value = GBCustomsDataRegistry.Instance.ICSUsername.Value },
						Password = new PasswordString { Value = GBCustomsDataRegistry.Instance.ICSPasssword.Value }
					}
				};
				header.Security = security;
			}

			PopulateHeaderInfo();
			PopulateHeaderSecurity();

			OutputMessageObject.Header = header;
		}

		void PopulateBody()
		{
			var cc315Builder = new CC315AMessageBuilder_v10(InputDeclaration);
			cc315Builder.PopulateMessageObject();

			var messageBody = new EnvelopeBody();
			messageBody.Cc315A = cc315Builder.OutputMessageObject;

			OutputMessageObject.Body = messageBody;
		}
	}
}
