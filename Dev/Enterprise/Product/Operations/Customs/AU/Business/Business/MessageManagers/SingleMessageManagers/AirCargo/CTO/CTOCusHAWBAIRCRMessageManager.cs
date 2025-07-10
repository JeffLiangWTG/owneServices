using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CTOCusHAWBAIRCRMessageManager : CusHAWBBaseAIRCRManager
	{
		public CTOCusHAWBAIRCRMessageManager(CTOCusHAWB hAWB)
			: base(hAWB)
		{
		}

		internal override CMRAmendmentGenerator GetAmendmentManager(BusinessObject bizo) => new CTOCusHAWBAIRCRAmendmentGenerator(bizo as CTOCusHAWB);

		internal override CMRMessageBuilder[] GetBuilder(BusinessObject bizo) => new CMRMessageBuilder[] { new AIRCRMessageBuilder(bizo as CTOCusHAWB) };

		public override string MessageFriendlyName => "Air Cargo Report for MAWB: " + ((CTOCusHAWB)BusinessObject).CS_HAWB;
	}
}
