using Enterprise.Customs.BR.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.BR.DataTransfer.Universal
{
	public class BRAdditionalLineTariffDetailDataObjectReader : AdditionalLineTariffDetailDataObjectReader
	{
		public BRAdditionalLineTariffDetailDataObjectReader(AdditionalLineTariffDetail dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, UniversalDataObjectReaderHelper helper, Customs.Business.IAdditionalLineTariffDetailParent parent) : base(dataObject, logger, factory, helper, parent)
		{
		}

		protected override void PopulateBusinessObject(Customs.Business.CusLineTariffDetail targetBO)
		{
			base.PopulateBusinessObject(targetBO);

			if (targetBO is CusLineTariffDetail tariffDetail)
			{
				tariffDetail.ExNumber = tariffDetail.BZ_Tariff;
			}
		}
	}
}

