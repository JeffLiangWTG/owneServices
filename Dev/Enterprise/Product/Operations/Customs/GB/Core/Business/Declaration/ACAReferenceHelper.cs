using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public static class AcaHelper
	{
		public static string GetNextACA(string badge, string importsOrExport, BusinessObjectFactory factory)
		{
			var suffix = importsOrExport == "IMP" ? "M" : importsOrExport == "EXP" ? "X" : "Z";
			return badge + Env.NumberFountains.ACAReferenceNumber(badge, importsOrExport).GetNextFormatted(factory) + suffix;
		}

		public static void AllocateNewAcaIfNeededOnFirstSaving(JobDeclaration jobDeclaration)
		{
			// TODO - think about how to generate one ACA per export consolidation
			var company = jobDeclaration.GetCredentialCompanyFromDeclarationsBadge();
			if (!jobDeclaration.IsInDatabase && jobDeclaration.IsImport && !company.IsEmpty && jobDeclaration.ZG_Gateway == GatewayList.Codes.Pentant && jobDeclaration.JE_ACAReference.IsEmpty && jobDeclaration.JE_MasterUCR.IsEmpty)
			{
				jobDeclaration.JE_ACAReference = GetNextACA(company, jobDeclaration.JE_MessageType, jobDeclaration.Factory);
			}
		}

		public static void ForceAllocateNewAca(JobDeclaration jobDeclaration)
		{
			var company = jobDeclaration.GetCredentialCompanyFromDeclarationsBadge();
			if (!company.IsEmpty && jobDeclaration.ZG_Gateway == GatewayList.Codes.Pentant && jobDeclaration.JE_MasterUCR.IsEmpty)
			{
				jobDeclaration.JE_ACAReference = GetNextACA(company, jobDeclaration.JE_MessageType, jobDeclaration.Factory);
			}
		}
	}
}
