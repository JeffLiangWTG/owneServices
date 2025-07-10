using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IL.Business
{
	public class DeclarationAgentWrapper : IDeclarationAgent
	{
		DeclarationAgentWrapper(JobDeclaration jobDeclaration)
		{
			this.jobDeclaration = jobDeclaration;
		}
		readonly JobDeclaration jobDeclaration;

		public static DeclarationAgentWrapper NewOrNull(JobDeclaration jobDeclaration) => jobDeclaration == null ? null : new DeclarationAgentWrapper(jobDeclaration);

		public IIDType ID =>  IDTypeWrapper.NewOrNull(jobDeclaration.DeclarantAddress?.Header?.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.VATCode, CountryCodes.Israel)?.OK_CustomsRegNo ?? ZString.Empty);

		public ICodeType RoleCode => CodeTypeWrapper.NewOrNull("1");
	}
}
