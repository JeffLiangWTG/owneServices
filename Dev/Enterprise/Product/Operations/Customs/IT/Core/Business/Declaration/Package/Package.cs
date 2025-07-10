using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

public class Package : EU.Business.Declaration.Package, Integration.Customs.IT.IPackage
{
	public Package(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	protected override CusDecHouseContainerPackValidation GetNewValidation() => new PackageValidation(this);

	[MaxLength(nameof(MarksAndNosMaxLength))]
	public override ZString CW_MarksAndNos { get => base.CW_MarksAndNos; set => base.CW_MarksAndNos = value; }

	int MarksAndNosMaxLength => SADConstants.CustomsFieldMaxLength.Package.MarksAndNos;

	protected override BasePackagePivotsCollection<Customs.Business.InvoiceLinePackagePivot> GetNewInvoiceLinePivotCollectionCore()
		=> new PackagePivotsCollection(this);
}
