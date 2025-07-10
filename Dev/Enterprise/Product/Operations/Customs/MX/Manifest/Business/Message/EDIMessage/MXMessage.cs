using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.MX.Manifest.Business
{
	public class MXMessage : EDIMessage, Integration.Customs.MX.IMXMessage
	{
		public MXMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{ }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = EDIMessage.ApplicationCodes.MXCustoms;
		}

		protected override string GetMessageReferenceNumber() => Env.NumberFountains.GetOutgoingMXCustomsMessageNumber().GetNextFormatted(Factory);

		protected override string MessageNumberPlaceHolderOverride => MessageNumberPlaceHolderHtml;

		public override bool UsesPlaceHolders => true;

		protected override IStreamFormatter MessageStreamFormatter => new MXMessageStreamFormatter(new ZStringBuilder().ToStringWithNewLineBetweenAppends());
	}
}
