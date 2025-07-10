using CargoWise.EntityFramework;
namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CTOCusHAWBAIRCRAmendmentGenerator : CusHAWBBaseAIRCRAmendmentGenerator
	{
		public CTOCusHAWBAIRCRAmendmentGenerator(CTOCusHAWB hAWB)
			: base(hAWB)
		{
		}

		protected internal override CMRMessageBuilder GetBuilder(BusinessObject bizo) => new AIRCRMessageBuilder(bizo as CTOCusHAWB);
	}
}
