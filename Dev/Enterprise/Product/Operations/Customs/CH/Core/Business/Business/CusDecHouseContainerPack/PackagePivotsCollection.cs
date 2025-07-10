using System;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class PackagePivotsCollection : BasePackagePivotsCollection<Customs.Business.InvoiceLinePackagePivot>
{
	public PackagePivotsCollection(BasePackage package)
		: base(package)
	{
	}

	public override Type GetTypeOfElementsFromPK(ZGuid pk)
	{
		return typeof(InvoiceLinePackagePivot);
	}
}
