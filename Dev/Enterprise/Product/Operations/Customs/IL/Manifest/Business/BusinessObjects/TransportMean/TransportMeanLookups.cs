
using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class TransportMeanLookups : ASYCUDA.Business.TransportMeanLookups
	{
		public TransportMeanLookups(ASYCUDA.Business.TransportMean parent) : base(parent)
		{
		}

		protected override ICollection TruckKindListCore
		{
			get
			{
				var dataGrouping = Core.Constants.CountryCodes.Israel;
				if (Parent is TransportMean transport
					&& transport.Parent is AsycudaManifestHeader header
					&& !header.AMA_RN_NKCountry.IsEmpty)
				{
					dataGrouping = header.AMA_RN_NKCountry;
				}

				var result = new CodeDescriptionPairList();
				var list = ZZRefCusCodeListCombined.Loader.Load(
					Factory,
					dataGrouping,
					 Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.IsraelTransportMeans,
					ZDateTime.Today);

				result.AddRange(list);
				result.Sort();
				return result;
			}
		}
	}
}
