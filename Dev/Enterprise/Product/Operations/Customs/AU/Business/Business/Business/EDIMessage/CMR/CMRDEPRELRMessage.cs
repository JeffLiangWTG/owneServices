using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRDEPRELRMessage : CMRCUSRESMessage
	{
		public CMRDEPRELRMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessageTypes.DEPREL;
		}

		[BusinessObjectTestExclude]
		public override ZString EM_MessageInterpretation
		{
			get
			{
				if (fEM_MessageInterpretation.IsEmpty && CUSRES != null)
				{
					fEM_MessageInterpretation = GetReport();
				}
				return fEM_MessageInterpretation;
			}
		}
		ZString fEM_MessageInterpretation;
	}
}
