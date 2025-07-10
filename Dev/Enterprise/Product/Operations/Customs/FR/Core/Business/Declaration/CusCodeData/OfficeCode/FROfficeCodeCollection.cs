using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.FR.Business.Declaration;

public class FROfficeCodeCollection : EuOfficeCodeCollection
{
	public FROfficeCodeCollection(BusinessObject master) : base(master)
	{
	}
	public override Type GetTypeOfElementsFromPK(ZGuid pk) => typeof(OfficeCode);

	public new OfficeCode AddNew() => (OfficeCode)base.AddNew();

	public new OfficeCode this[int index] => (OfficeCode)base[index];
}
