using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageBuildingBlocks;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	class DLMMessage : EDIMessage
	{
		public DLMMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = MessageTypeList.Codes.DataLoadingModule;
			EM_MessageSubType = MessageTypeList.Codes.DataLoadingModule;
		}

		protected override CodeDescriptionPairList MessageSubTypeList
		{
			get { return new MessageTypeList(); }
		}

		protected override ZString TransmitMessageInterpretation
		{
			get
			{
				try
				{
					var messageBlock = new MessageBlockGenerator();
					messageBlock.Deserialise(EM_MessageText);
					return messageBlock.Serialise(true);
				}
				catch (InvalidMessageFormatException ex)
				{
					return ex.Message + "\r\n\r\n" + EM_MessageText;
				}
			}
		}
	}
}
