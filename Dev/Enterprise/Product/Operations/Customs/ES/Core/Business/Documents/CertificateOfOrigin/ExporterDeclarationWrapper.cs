using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Documents.CertificateOfOrigin;

public class ExporterDeclarationWrapper : IExporterDeclaration
{
	public ExporterDeclarationWrapper(JobDeclaration declaration)
	{
		this.declaration = Argument.NotNull(declaration, nameof(declaration));
		exporter = declaration.DeclarantOrgAddress?.Header ?? declaration.Supplier;
	}
	readonly JobDeclaration declaration;
	readonly OrgHeader exporter;

	ZDate IExporterDeclaration.ReferenceDate => referenceDate ?? (referenceDate = ZDate.Today).Value;
	ZDate? referenceDate;

	ZString IExporterDeclaration.Place => declaration.DeclarantOrgAddress?.OA_City ?? ZString.Empty;

	ZString IExporterDeclaration.ExporterDetails
	{
		get
		{
			var supplierDetails = new ZStringBuilder();

			if (exporter != null)
			{
				supplierDetails.Append(exporter.OH_FullName);

				var orgCusCode = (OrgCusCode)exporter.CustomsCodes.FirstOrDefault(x => ((OrgCusCode)x).OK_CodeType == OrgCusCode.SpainCodeTypes.NIF);
				var regNo = orgCusCode?.OK_CustomsRegNo ?? ZString.Empty;

				supplierDetails.Append($"{NIFString} {regNo}");
			}

			return supplierDetails.ToStringWithNewLineBetweenAppends();
		}
	}

	const string NIFString = "NIF:";
}
