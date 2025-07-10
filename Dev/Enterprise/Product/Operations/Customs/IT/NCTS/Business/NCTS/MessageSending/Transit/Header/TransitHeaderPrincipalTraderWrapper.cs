using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.IT.Business.SADConstants;

namespace Enterprise.Customs.IT.NCTS.Business;

public class TransitHeaderPrincipalTraderWrapper : NctsSADHeaderPrincipalTraderWrapper
{
	public TransitHeaderPrincipalTraderWrapper(JobDocAddress principalTraderAddress, JobDocAddress representativeAddress) : base(principalTraderAddress)
	{
		representative = representativeAddress;
	}

	readonly JobDocAddress representative;

	protected override ZString TIRHolderIdentificationCore => ZString.Empty;

	protected override ZString RepresentativeNameCore => representative?.E2_CompanyName.Left(CustomsFieldMaxLength.Trader.Name) ?? ZString.Empty;

	protected override ZString RepresentativeGuaranteeTaxIdentificationNumberCore
	{
		get
		{
			var organisation = representative?.Organisation;
			if (organisation == null)
			{
				return ZString.Empty;
			}

			if (organisation.IsNaturalPersonIndividual())
			{
				var fiscalCode = organisation.GetFiscalCode();
				return !fiscalCode.IsEmpty ? fiscalCode : organisation.GetEoriCode();
			}
			else
			{
				var vatCode = organisation.GetVatCode();
				return !vatCode.IsEmpty ? vatCode : organisation.GetEoriCode();
			}
		}
	}
}
