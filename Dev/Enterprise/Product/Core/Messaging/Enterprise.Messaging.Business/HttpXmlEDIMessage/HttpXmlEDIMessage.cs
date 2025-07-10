using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Messaging.Integration;

namespace Enterprise.Messaging.Business.HttpXmlMessaging
{
	public class HttpXmlEDIMessage : EDIMessage, IHttpXmlEDIMessage
	{
		public HttpXmlEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		protected override string GetMessageReferenceNumber()
		{
			return MessageNumberStrategy != null ? MessageNumberStrategy.GetMessageReferenceNumber() : Env.NumberFountains.HttpXmlEDIMessageNumber.GetNextFormatted(Factory);
		}

		public override bool UsesPlaceHolders => false;

		[BusinessObjectTestExclude]
		public override ZString EM_MessageText
		{
			get
			{
				ErrorReporter.ReportOnce(AutoEDIMessage.Schema.EM_MessageText + " should not be accessed. HttpXmlEDIMessage should be accessed by streamed properties only.");
				return base.EM_MessageText;
			}
			set
			{
				ErrorReporter.ReportOnce(AutoEDIMessage.Schema.EM_MessageText + " should not be accessed. HttpXmlEDIMessage should be accessed by streamed properties only.");
				base.EM_MessageText = value;
			}
		}

		[BusinessObjectTestExclude]
		public override ZString EM_MessageInterpretation
		{
			get
			{
				ErrorReporter.ReportOnce(EDIMessage.Schema.EM_MessageInterpretation + " should not be accessed. HttpXmlEDIMessage should be accessed by streamed properties only.");
				return base.EM_MessageInterpretation;
			}
			set
			{
				ErrorReporter.ReportOnce(EDIMessage.Schema.EM_MessageInterpretation + " should not be accessed. HttpXmlEDIMessage should be accessed by streamed properties only.");
				base.EM_MessageInterpretation = value;
			}
		}

		public override bool ShouldShowInterpretation => false;

		public override void OnSaving()
		{
			base.OnSaving();
			if (!IsInDatabase && EM_MessageNum.IsEmpty)
			{
				EM_MessageNum = GetMessageReferenceNumber();
			}
		}

		public void Save()
		{
			Factory.Save();
		}

		public void SetMessageTextSource(SubStreamableStream source)
		{
			SetEM_MessageTextSource(new StreamReaderSource(source));
		}

		string IHttpXmlRequestResponse.Status
		{
			get { return EM_Status; }
			set { EM_Status = value; }
		}

		SubStreamableStream IHttpXmlRequestResponse.GetMessageStream()
		{
			// TODO: EDIMessage stream is not a substream, when this changes
			// we should remove this wrapper.
			return new CargoWise.IO.Shim.SubStreamableStream(base.GetMessageStream());
		}
	}
}
