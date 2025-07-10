using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.CA.Business
{
	public class IIDUniversalShipmentMessage : EDIMessage
	{
		public IIDUniversalShipmentMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[BusinessObjectTestExclude]
		public override ZString EM_MessageInterpretation
		{
			get
			{
				if (messageInterpretation.IsEmpty)
				{
					messageInterpretation = IIDUniversalShipmentMessageInterpretationGenerator.GetInterpretatedHTML(this);
				}
				return messageInterpretation;
			}
		}
		ZString messageInterpretation;

		protected override ZBool ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressedOverride()
		{
			return true;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodeList.Codes.CAIMP;
			EM_MessageType = MessageTypeList.Codes.IntegratedImportDeclaration;
			EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
		}
	}
}
