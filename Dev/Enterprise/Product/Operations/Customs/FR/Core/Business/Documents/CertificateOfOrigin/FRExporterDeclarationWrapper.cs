using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Documents
{
	public class FRExporterDeclarationWrapper : ExporterDeclarationWrapper
	{
		public FRExporterDeclarationWrapper(JobDeclaration declaration)
		{
			Argument.NotNull(declaration, nameof(declaration));
			shipper = declaration.Supplier;
		}
		readonly OrgHeader shipper;

		protected override ZString GetPlace() => shipper?.MainAddress?.OA_City ?? ZString.Empty;
	}
}
