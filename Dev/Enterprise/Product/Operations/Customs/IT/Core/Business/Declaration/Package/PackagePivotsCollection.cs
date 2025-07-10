using System;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

public class PackagePivotsCollection : BasePackagePivotsCollection<Customs.Business.InvoiceLinePackagePivot>
{
	public PackagePivotsCollection(Package package) : base(package)
	{
	}

	public override Type GetTypeOfElementsFromPK(ZGuid pk) => typeof(InvoiceLinePackagePivot);
}
