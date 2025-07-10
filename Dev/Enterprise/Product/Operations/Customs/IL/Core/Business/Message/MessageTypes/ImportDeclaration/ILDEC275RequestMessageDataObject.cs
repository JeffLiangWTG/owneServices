using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP.DF_MSG10000_ImportDeclaration;
namespace Enterprise.Customs.IL.Business
{
	public class ILDEC275RequestMessageDataObject : MessageDataObject<DfMsg10000ImportDeclaration>
	{
		public ILDEC275RequestMessageDataObject(ILEDIMessage message) : base(message)
		{
		}

		protected override ILEDIMessagePrettierBase CreatePrettierCore() => null;

		protected override bool CanSetMessageInterpretationCore => false;
	}
}
