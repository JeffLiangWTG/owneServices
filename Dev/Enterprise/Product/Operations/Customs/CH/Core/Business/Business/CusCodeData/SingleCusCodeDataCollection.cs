using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public abstract class SingleCusCodeDataCollection<T> : CusCodeDataCollection<T>
												where T : CusCodeData
{
	public SingleCusCodeDataCollection(BusinessObject parent, ZString cY_Type) : base(parent, cY_Type)
	{
	}

	public T FindOrCreate() => this.Cast<T>().OrderByDescending(x => x.CY_SystemCreateTimeUtc).FirstOrDefault() ?? AddNew();
}
