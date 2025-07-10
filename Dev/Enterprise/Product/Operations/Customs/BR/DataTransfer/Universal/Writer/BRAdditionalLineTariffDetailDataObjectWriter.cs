using Enterprise.Customs.BR.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.BR.DataTransfer.Universal
{
	public class BRAdditionalLineTariffDetailDataObjectWriter : Customs.DataTransfer.Universal.AdditionalLineTariffDetailDataObjectWriter
	{
		public BRAdditionalLineTariffDetailDataObjectWriter(IDataWritingManager writeManager) : base(writeManager)
		{
		}

		protected override AdditionalLineTariffDetail PopulateDataObject(Customs.Business.CusLineTariffDetail sourceBO)
		{
			var additionalLineTariff = base.PopulateDataObject(sourceBO);
			if (sourceBO is CusLineTariffDetail tariffDetail)
			{
				additionalLineTariff.Tariff = tariffDetail.ExNumber;
			}
			return additionalLineTariff;
		}
	}
}
