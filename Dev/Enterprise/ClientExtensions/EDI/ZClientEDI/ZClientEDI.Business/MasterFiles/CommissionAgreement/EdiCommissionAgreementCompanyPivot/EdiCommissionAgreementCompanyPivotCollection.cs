using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EdiCommissionAgreementCompanyPivotCollection : ActiveBusinessObjectCollection<EdiCommissionAgreementCompanyPivot>
	{
		public EdiCommissionAgreementCompanyPivotCollection(EdiCommissionAgreementCustomization commissionAgreementCustomization)
			: base(commissionAgreementCustomization)
		{
		}

		public EdiCommissionAgreementCompanyPivot AddNew(ClientCompany clientCompany)
		{
			Argument.NotNull(clientCompany, "clientCompany");

			return AddNew(clientCompany.PK);
		}

		public EdiCommissionAgreementCompanyPivot AddNew(ZGuid clientCompanyPk)
		{
			var result = AddNew();
			using (result.SuspendSettingHasChanges())
			using (result.GetValidationSuspender())
			{
				result.EPY_LCC = clientCompanyPk;
			}

			return result;
		}
	}
}

