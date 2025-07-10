using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicPayment.Common
{
	internal class PartialBeneficiaryRequestMessageBuilder : BeneficiaryRequestMessageBuilder
	{
		internal PartialBeneficiaryRequestMessageBuilder(AccEPaymentBeneficiaryRequest request, int startPageNumber)
			: base(request)
		{
			StartPageNumber = startPageNumber;
		}

		int StartPageNumber { get; }

		protected override GlobalElectronicPayment.GlobalElectronicPayment GetGEPMessageCore()
		{
			var converter = new AccEPaymentBeneficiaryRequestToGEPConverter
			{
				StartPageNumber = StartPageNumber
			};
			return converter.ConvertBeneficiaryRequestToGEP(BeneficiaryRequest);
		}
	}
}
