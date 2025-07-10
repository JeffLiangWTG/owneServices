using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class InlandTransportCollection : CusCodeDataCollection<InlandTransport>
	{
		public InlandTransportCollection(BusinessObject parent) : base(parent, NctsConstants.CusCodeDataTypes.TransportInland)
		{
			MaxCountValidationEnable(999);
		}

		public ReadOnlyCollection<(ZString, ZString)> DataAndCodeList
		{
			get => Factory.GetValue(ref dataAndCodeListCached, () =>
			{
				var list = new List<(ZString, ZString)>();
				this.Cast<InlandTransport>().OrderBy(x => x.CY_Code).ForEach(x => list.Add((x.CY_Code, x.CY_Data)));
				return list.AsReadOnly();
			});
			set
			{
				RemoveAndDeleteAll();
				foreach ((ZString code, ZString data) in value)
				{
					var item = AddNew();
					item.CY_Code = code;
					item.CY_Data = data;
				}
			}
		}
		CachedProperty<ReadOnlyCollection<(ZString, ZString)>> dataAndCodeListCached;
	}
}
