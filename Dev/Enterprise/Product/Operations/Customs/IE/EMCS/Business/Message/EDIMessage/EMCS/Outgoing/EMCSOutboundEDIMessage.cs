using System.Data;
using System.Net;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class EMCSOutboundEDIMessage : IE.Business.OutboundEDIMessage
	{
		public EMCSOutboundEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new EMCSOutboundEDIMessageLookups Lookups => (EMCSOutboundEDIMessageLookups)base.Lookups;

		protected override EDIMessageLookups GetNewLookups() => new EMCSOutboundEDIMessageLookups(this);

		protected override string GetMessageReferenceNumber() => EM_MessageNum.IsEmpty ? Env.NumberFountains.IEMessageControlNumber(EDIMessage.ApplicationCodes.IECustomsEMCS).GetNextFormatted(Factory) : (string)EM_MessageNum;

		protected override void GetNumberFountainNumbersAndFillInPlaceHolders()
		{
			base.GetNumberFountainNumbersAndFillInPlaceHolders();

			EM_MessageText = EM_MessageText.Replace(EDIMessage.MessageNumberPlaceHolderHtml, EM_MessageNum);
			EM_MessageInterpretation = EncodeToHTMLFormat(EM_MessageText);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsEMCS;
		}

		string EncodeToHTMLFormat(string xmlText)
		{
			var encodedHtml = WebUtility.HtmlEncode(xmlText.Trim());

			const string template = @"
						<html>
							</head>
							<body>
								<pre>{0}<pre>
							</body>
						</html>";

			return string.Format(template, encodedHtml);
		}
	}
}
