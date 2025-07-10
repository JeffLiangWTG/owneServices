using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Business
{
	public class DeclarationImporterWrapper : IDeclarationImporter
	{
		DeclarationImporterWrapper(JobDeclaration jobDeclaration)
		{
			this.jobDeclaration = jobDeclaration;
		}
		readonly JobDeclaration jobDeclaration;

		public static DeclarationImporterWrapper NewOrNull(JobDeclaration jobDeclaration) => jobDeclaration == null ? null : new DeclarationImporterWrapper(jobDeclaration);

		public IDeclarationImporterDmExtensions DmExtensions => DeclarationImporterDmExtensionsWrapper.NewOrNull();

		public IIDType ID => IDTypeWrapper.NewOrNull(GetCustomsRegNo(), Constants.CustomsDeclaration.ImporterSchemeID);

		ZString GetCustomsRegNo()
		{
			return jobDeclaration.Importer?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.Israel) ?? ZString.Empty;
		}
	}
}
