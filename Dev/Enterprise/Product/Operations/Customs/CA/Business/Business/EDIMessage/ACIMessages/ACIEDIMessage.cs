using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business.MessageProcessors;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Edifact.D00A.Messages.CUSRES;
using Enterprise.Environment;

namespace Enterprise.Customs.CA.Business
{
	public class ACIEDIMessage : EDIMessage
	{
		public ACIEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool ShouldShowInterpretation
		{
			get { return true; }
		}

		protected override string GetMessageReferenceNumber()
		{
			return Env.NumberFountains.EDIFACTNumberFountain("M", "IMP", ApplicationCodes.CAACI).GetNextFormatted(Factory);
		}

		protected override bool ShouldUseUnformattedMessageText
		{
			get { return false; }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodes.CAACI;
		}

		protected CUSRESMessage CUSRES
		{
			get { return cusres ?? (cusres = (CUSRESMessage)GetAutoEdifactMessageUsingNamedFactory(new CaEdifactMessageFactory(), new CACharSet())); }
		}
		CUSRESMessage cusres;
	}
}
