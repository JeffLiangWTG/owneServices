using CargoWise.Types;
using Enterprise.Customs.GB.Registry;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Business.Declaration
{
	/// <summary>
	/// For exports, location of goods is port of loading, e.g. GBFXT
	/// For imports, port of arrival
	/// ONLY sets if the substyle is a 'goods arrived' style
	/// </summary>
	public class Box30LocationOfGoodsValueSetter
	{
		public Box30LocationOfGoodsValueSetter(JobDeclaration dec)
		{
			this.dec = dec;
		}

		public void SetBox30()
		{
			if (ShouldCalculateLocationOfGoods())
			{
				var sourcePort = ZString.Empty;
				if (dec.IsExport)
				{
					sourcePort = dec.JE_RL_NKPortOfLoading;
				}
				else if (dec.IsImport)
				{
					sourcePort = dec.JE_RL_NKPortOfArrival;
				}
				if (sourcePort.Length == 5)
				{
					var newLocationOfGoods = PortConverter.UnlocoToChief(sourcePort, dec.Factory, dec.JE_TransportMode).Left(dec.JE_LocationOfGoodsInfo.MaxLength);
					if (((CodeDescriptionPairList)dec.Lookups.Locations).ContainsCode(newLocationOfGoods))
					{
						dec.JE_LocationOfGoods = newLocationOfGoods;
					}
				}
			}
		}

		bool ShouldCalculateLocationOfGoods()
		{
			return dec.IsGoodsArrivedSubStyle || GBCustomsDataRegistry.Instance.AllowLocationOfGoodsCalculationForNotArrivedGoods.Value;
		}

		readonly JobDeclaration dec;
	}
}
