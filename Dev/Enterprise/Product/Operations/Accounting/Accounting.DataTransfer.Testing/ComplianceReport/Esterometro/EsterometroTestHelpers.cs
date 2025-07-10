using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.Esterometro.Testing
{
	/// <summary>
	/// Extensions to TestObjectCreator for Esterometro specific testing. 
	/// </summary>
	public static class EsterometroTestHelpers
	{
		internal static OrgHeader CreateEsterometroOrgProxyForCompany(this TestObjectCreator objectCreator)
		{
			var companyOrgProxy = objectCreator.CreateOrgHeader("FRATEL", false, false);
			companyOrgProxy.OH_FullName = "Fratelli Salvadori Srl";
			companyOrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.IVA, "98645152", Core.Constants.CountryCodes.Italy);
			companyOrgProxy.MainAddress.OA_Address1 = "Viale Peitro";
			companyOrgProxy.MainAddress.OA_Address2 = "Pietramellara 11";
			companyOrgProxy.MainAddress.OA_City = "Bologna";
			companyOrgProxy.MainAddress.OA_State = "BO";
			companyOrgProxy.MainAddress.OA_PostCode = "40121";
			companyOrgProxy.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
			return companyOrgProxy;
		}

		internal static OrgHeader CreateEsterometroOrgProxyForBranch(this TestObjectCreator objectCreator)
		{
			var branchOrgProxy = objectCreator.CreateOrgHeader("SRLSOC", false, false);
			branchOrgProxy.OH_FullName = "Italy SRL a socio unico";
			branchOrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.IVA, "12457898", Core.Constants.CountryCodes.Italy);
			branchOrgProxy.MainAddress.OA_Address1 = "Via Cassanese";
			branchOrgProxy.MainAddress.OA_Address2 = "224 Palazzo Caravaggio";
			branchOrgProxy.MainAddress.OA_City = "Milan";
			branchOrgProxy.MainAddress.OA_State = "MI";
			branchOrgProxy.MainAddress.OA_PostCode = "20090";
			branchOrgProxy.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
			return branchOrgProxy;
		}
	}
}
