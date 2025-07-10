using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.ICS
{
	public class IcsSsGreatBritainEDIMessage : GbEDIMessage
	{
		public const string MessageNumberPlaceHolderXml = "{{MSGNO PLACEHOLDER}}";

		public IcsSsGreatBritainEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			MessageNumberStrategy = new GbMessageNumberStrategy(Factory, EDIMessage.ApplicationCodes.GbMessageICSGreatBritain);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = EDIMessage.ApplicationCodes.GbMessageICSGreatBritain;
		}

		protected override string MessageNumberPlaceHolderOverride => MessageNumberPlaceHolderXml;
	}
}
