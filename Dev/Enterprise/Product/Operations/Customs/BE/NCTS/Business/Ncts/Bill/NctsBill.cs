using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.NCTS.Business;

public class NctsBill : EU.NCTS.Business.NctsBill, IInlandTransportParent
{
	public NctsBill(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	[ChildEditable]
	public InlandTransportCollection InlandTransports
	{
		get
		{
			if (inlandTransports == null)
			{
				inlandTransports = new InlandTransportCollection(this);
				inlandTransports.Load();
				RegisterEditableChildObject(inlandTransports);
			}

			return inlandTransports;
		}
	}

	InlandTransportCollection inlandTransports;

	public IDictionary<ZString, Type> GetCusCodeDataTypes() => new Dictionary<ZString, Type>
	{
		{ Constants.CusCodeDataTypes.TransportInland, typeof(InlandTransport) }
	};

	public IEnumerable<IBusinessObjectFetchStrategy> GetFetchStrategies()
	{
		yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
	}

	#region InlandTransportLineNumberGenerator

	public IEnumerable<IShortSequenceNumberLine> InlandTransportLines => new TypedEnumerable<IShortSequenceNumberLine>(InlandTransports);

	public ShortSequenceNumberGenerator InlandTransportLineNumberGenerator => inlandTransportLineNumberGenerator ?? (inlandTransportLineNumberGenerator = new ShortSequenceNumberGenerator(() => InlandTransportLines));
	ShortSequenceNumberGenerator inlandTransportLineNumberGenerator;

	#endregion

	[ChildEditable(true)]
	public new EU.NCTS.Business.INctsDepartureCargoDescCollection<NctsDepartureCargoDesc> GoodsItems => (EU.NCTS.Business.INctsDepartureCargoDescCollection<NctsDepartureCargoDesc>)base.GoodsItems;

	protected override EU.NCTS.Business.INctsDepartureCargoDescCollection<EU.NCTS.Business.NctsDepartureCargoDesc> GetNewGoodsItems() => new EU.NCTS.Business.NctsDepartureCargoDescCollection<NctsDepartureCargoDesc>(this);

	[ChildEditable(true)]
	public new EU.NCTS.Business.IArrivalCusTransportMeansCollection<ArrivalCusTransportMeans> ArrivalTransportInfos => (EU.NCTS.Business.IArrivalCusTransportMeansCollection<ArrivalCusTransportMeans>)base.ArrivalTransportInfos;

	protected override EU.NCTS.Business.IArrivalCusTransportMeansCollection<EU.NCTS.Business.ArrivalCusTransportMeans> GetNewArrivalTransportInfos() => new EU.NCTS.Business.ArrivalCusTransportMeansCollection<ArrivalCusTransportMeans>(this);
}
