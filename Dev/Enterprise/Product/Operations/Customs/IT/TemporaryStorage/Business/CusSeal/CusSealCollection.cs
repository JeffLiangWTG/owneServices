using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

public class CusSealCollection : EU.Business.Declaration.CusSealCollection
{
	public CusSealCollection(BusinessObject master) : base(master)
	{
	}

	public new CusSeal AddNew() => (CusSeal)base.AddNew();

	public new CusSeal this[int index] => (CusSeal)base[index];
}

