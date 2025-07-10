using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class EmcsEDIMessage : DEEDIMessage
	{
		public EmcsEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[BusinessObjectTestExclude]
		public override ZString EM_MessageInterpretation
		{
			get => base.EM_MessageInterpretation.EncodeToHTMLFormat();
			set => base.EM_MessageInterpretation = value;
		}

		protected override string GetMessageReferenceNumber() => Env.NumberFountains.DEMessageControlNumber(GlbCompany.CurrentCompany.PK.ToGuid(), GlbCompany.CurrentCompany.GC_Code, 11).GetNextFormatted(Factory);

		protected override void GetNumberFountainNumbersAndFillInPlaceHolders()
		{
			base.GetNumberFountainNumbersAndFillInPlaceHolders();

			var interchangeControlReference = Env.NumberFountains.DEEMCSInterchangeControlReference.GetNextFormatted(Factory);

			EM_MessageText = EM_MessageText
				.Replace(EDIMessage.SendersReferencePlaceHolderHtml, EM_MessageNum)
				.Replace(EDIInterchange.InterchangeNumberPlaceHolderHtml, interchangeControlReference);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodes.DECustomsEmcsSystem;
			EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			EM_MessageType = DE.Messaging.EDIMessageTypeList.Codes.EMCS;
		}
	}
}
