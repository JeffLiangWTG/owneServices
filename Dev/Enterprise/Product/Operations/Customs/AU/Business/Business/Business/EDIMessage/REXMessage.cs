using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// EDIMessage.ApplicationCodes.NEXDOCS  'NEX'
	/// </summary>
	public class REXMessage : EDIMessage, Integration.Customs.AU.INEXDOCMessage
	{
		public REXMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[BusinessObjectTestExclude]
		public override ZString EM_MessageInterpretation
		{
			get => MessageInterpretation;
			set => base.EM_MessageInterpretation = value;
		}

		protected ZString MessageInterpretation => !messageInterpretation.IsEmpty ? messageInterpretation : (messageInterpretation = REXMessageInterpretationGenerator.GetInterpretedHTML(this));
		ZString messageInterpretation;
	}
}
