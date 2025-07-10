using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business.XmlMessaging;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class JPAFRMessage : XmlEDIMessage
	{
		public JPAFRMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			this.MessageNumberStrategy = new AFRMessageNumberGenerator(factory);
		}

		[BusinessObjectTestExclude]
		public override ZString EM_MessageInterpretation
		{
			get
			{
				if (messageInterpretation.IsEmpty)
				{
					messageInterpretation = MessageInterpretationGenerator.GetInterpretatedHTML(this);
				}
				return messageInterpretation;
			}
		}
		ZString messageInterpretation;
	}
}
