using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.CN.DataTransfer.Universal
{
	public class CNContainerDataObjectReader : CustomsContainerDataObjectReader<BaseJobDeclaration, BaseCusContainer>
	{
		public CNContainerDataObjectReader(Container containerDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, BaseJobDeclaration declaration, ILandedCostDataReader landedCostDataReader = null) : base(containerDataObject, logger, helper, declaration, landedCostDataReader)
		{
		}
	}
}
