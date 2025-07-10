using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

class Ucc6ExportImporterJobDocAddressValidation : Ucc6TraderJobDocAddressValidation
{
	public Ucc6ExportImporterJobDocAddressValidation(AutoJobDocAddress parent, JobDeclaration declaration)
		: base(parent, GetImporterHumanReadableName(), declaration, isMandatory: false)
	{
		this.declaration = Argument.NotNull(declaration, nameof(declaration));
	}
	readonly JobDeclaration declaration;

	protected override void CheckOrganisationPK()
	{
		base.CheckOrganisationPK();

		var parent = Parent;
		new HeaderOrLineValueValidator<ZGuid>(
			headerValueProvider: () => parent.OrganisationPK,
			lineValuesProvider: () => declaration
										.InvoiceLines
										.Cast<JobComInvoiceLine>()
										.Select(x => x.JI_OA_ConsigneeAddress))
		{
			IsEmptyFunc = x => x.IsEmpty,
			FieldName = GetImporterHumanReadableName()
		}
		.ValidateHeader(parent.OrganisationPKInfo);
	}

	#region Implementation

	static string GetImporterHumanReadableName() => Res.GetString("044C8180-490B-4221-A774-D3B3B6E46CB5", "Importer");

	#endregion
}
