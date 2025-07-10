using CargoWise.Customs.FR.MessageDefinitions.TP5.CC043C;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class CC043CMessagePrettier : NCTSMessagePrettier<Cc043CType>
	{
		public CC043CMessagePrettier(NCTSMessageDataObject<Cc043CType> messageDataObject) : base(messageDataObject)
		{
		}

		protected override ZString GetMessageInterpretationCore(Cc043CType messageObject)
		{
			return ToKeyValuePairSection(new (ZString key, ZString value)[]
			{
				((NoResString)"Status", TP5ResponseMessageSubTypeList.Descriptions.UnloadingPermission ?? ZString.Empty),
				("MRN", messageObject.TransitOperation?.Mrn ?? ZString.Empty),
				((NoResString)"Continue unloading", messageObject?.CtlControl?.ContinueUnloading ?? ZString.Empty)
			});
		}
	}
}
