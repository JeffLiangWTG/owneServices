using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class InlandTransportCollection : CusCodeDataCollection<InlandTransport>
	{
		public InlandTransportCollection(JobDeclaration master) : base(master, CusCodeDataTypeList.Codes.TransportInland)
		{
		}

		public new JobDeclaration Master => (JobDeclaration)base.Master;

		public ReadOnlyCollection<(ZString, ZString)> DataAndCodeList
		{
			get => Factory.GetValue(ref dataAndCodeListCached, () =>
			{
				var list = new List<(ZString, ZString)>();
				this.Cast<InlandTransport>().OrderBy(x => x.Nationality).ForEach(x => list.Add((x.Nationality, x.CY_Data)));
				return list.AsReadOnly();
			});
			set
			{
				RemoveAndDeleteAll();
				foreach (var (nationality, data) in value)
				{
					var item = AddNew();
					item.Nationality = nationality;
					item.CY_Data = data;
				}
			}
		}
		CachedProperty<ReadOnlyCollection<(ZString, ZString)>> dataAndCodeListCached;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var inlandTransport = (InlandTransport)child;
			Master.InlandTransportLineNumberGenerator.RecalculateWhenAdded(inlandTransport);
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			Master.InlandTransportLineNumberGenerator.ReCalculateAll();
		}
	}
}
