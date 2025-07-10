using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class NationalAdditionalCodeProvider : ICcQualifierNationalAdditionalCode
	{
		public NationalAdditionalCodeProvider(int sequenceNumber, CusLineTariffDetail tariffDetail)
		{
			SequenceNumber = sequenceNumber.ToString();
			this.tariffDetail = Argument.NotNull(tariffDetail, nameof(tariffDetail));
		}

		readonly CusLineTariffDetail tariffDetail;

		public string CcQualifier => null;

		public string NationalAdditionalCode => tariffDetail.BZ_Tariff;

		public string SequenceNumber { get; }
	}
}
