using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;

namespace Enterprise.Customs.CA.Business.MessageManagers
{
	class CADStatusCalculator : EDIFACTMessageStatusCalculator
	{
		public override ZString MessageTypeDescription => MessageTypeList.Descriptions.CommercialAccountingDeclaration;

		public override ZString CalculatedJobStatus(IEDIFACTMessageAttachee linkedObject)
		{
			return ZString.Empty;
		}
	}
}
