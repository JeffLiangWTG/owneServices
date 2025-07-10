using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business
{
	public sealed class CFCPEDHeaderProvider : MonthlyClosingDecHeaderProvider, ICFCPEDHeader
	{
		public CFCPEDHeaderProvider(CusReconDeclaration declaration, string messageRole)
			: base(declaration, messageRole)
		{
		}

		public bool InputTaxDeductionFlag => Factory.GetValue(ref inputTaxDeductionFlag, () =>
		{
			var orgImpAddInfo = (DEOrgImpAddInfo)Declaration.DeclarantAddress?.Header.GetCountryData(Core.Constants.CountryCodes.Germany).ImpAddInfo;
			return orgImpAddInfo != null && YesNoList.IsYes(orgImpAddInfo.ZO_VATClaimBack);
		});
		CachedProperty<bool> inputTaxDeductionFlag;

		public string TaxOffice => DeclarationSender?.Header.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.TaxOffice);

		public string MandateReference => ClearanceAuthorization?.CusAuthorisationRules.FirstOrDefault(r => r.CPR_RuleCode == CusAuthorisationRuleTypeList.Codes.MandateReference)?.CPR_ValueFrom;

		public IReadOnlyCollection<ICFCPEDBody> Bodies => bodies ?? (bodies = Declaration.CusReconEntries.Cast<CusReconEntry>().Select(e => new CFCPEDBodyProvider(e, IsModificationMessage)).Where(p => p.Lines.Any()).ToArray());
		IReadOnlyCollection<ICFCPEDBody> bodies;

		OrgAddress DeclarationSender => CachedValueHelper.GetValue(ref declarationSender, () => Declaration.CRD_DeclarantType == RepresentationTypeList.Codes._2Direct ? Declaration.RepresentativeAddress : Declaration.DeclarantAddress);
		CachedValue<OrgAddress> declarationSender;
	}
}
