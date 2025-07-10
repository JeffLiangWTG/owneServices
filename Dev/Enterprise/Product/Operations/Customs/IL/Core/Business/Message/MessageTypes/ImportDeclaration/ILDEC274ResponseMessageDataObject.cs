using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP.DF_MSG10000_ImportDeclaration;
namespace Enterprise.Customs.IL.Business
{
	public class ILDEC274ResponseMessageDataObject : MessageDataObject<DfNg2754Msg10004ImportDeclarationResponse>
	{
		public ILDEC274ResponseMessageDataObject(ILEDIMessage message) : base(message)
		{
		}

		public ImportDeclarationResponseAdaptor GetImportDeclarationResponseAdaptor()
			=> new(this.MessageData);

		protected override ILEDIMessagePrettierBase CreatePrettierCore() => new ILDEC274ResponseMessagePrettier(this);

		protected override bool CanSetMessageInterpretationCore => false;
	}
}
