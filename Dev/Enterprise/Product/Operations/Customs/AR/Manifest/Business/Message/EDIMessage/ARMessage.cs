using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AR.Manifest.Business
{
	public class ARMessage : EDIMessage, Integration.Customs.AR.IARMessage
	{
		public ARMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = EDIMessage.ApplicationCodes.ARCustoms;
		}

		protected override string GetMessageReferenceNumber() => Env.NumberFountains.GetOutgoingARCustomsMessageNumber().GetNextFormatted(Factory);

		protected override IStreamFormatter MessageStreamFormatter => new EDIMessageStreamFormatterForXml();

		protected override string MessageNumberPlaceHolderOverride => MessageNumberPlaceHolderHtml;

		public override bool UsesPlaceHolders => true;
	}
}
