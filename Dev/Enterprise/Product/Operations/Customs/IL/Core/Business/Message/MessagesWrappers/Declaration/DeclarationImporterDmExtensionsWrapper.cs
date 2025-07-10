using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;

namespace Enterprise.Customs.IL.Business
{
	public class DeclarationImporterDmExtensionsWrapper : IDeclarationImporterDmExtensions
	{
		DeclarationImporterDmExtensionsWrapper()
		{
		}

		public static DeclarationImporterDmExtensionsWrapper NewOrNull() => new DeclarationImporterDmExtensionsWrapper();

		public string Address => string.Empty;

		public ICodeType EntitlementTypeCode => null;

		public ITextType IssueLocation => null;

		public string Name => string.Empty;

		public ICodeType RoleCode => CodeTypeWrapper.NewOrNull("4");
	}
}
