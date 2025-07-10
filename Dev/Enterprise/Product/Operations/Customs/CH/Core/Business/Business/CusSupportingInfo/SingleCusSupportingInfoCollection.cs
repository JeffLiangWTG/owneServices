using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public abstract class SingleCusSupportingInfoCollection<T> : CusSupportingInfoCollection<T>
												where T : CusSupportingInfo
{
	public SingleCusSupportingInfoCollection(BusinessObject parent, ZString cSI_Type) : base(parent, cSI_Type)
	{
	}

	public T FindOrCreate() => this.Cast<T>().OrderByDescending(x => x.CSI_SystemCreateTimeUtc).FirstOrDefault() ?? AddNew();
}
