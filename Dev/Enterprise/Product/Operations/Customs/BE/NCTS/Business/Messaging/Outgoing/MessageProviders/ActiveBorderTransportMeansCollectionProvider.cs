using System.Collections;
using System.Collections.Generic;
using CargoWise.Customs.BE.MessageContracts.Interfaces;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class ActiveBorderTransportMeansCollectionProvider : IReadOnlyCollection<IActiveBorderTransportMeans>
	{
		public ActiveBorderTransportMeansCollectionProvider(NctsDepartureMovementHeader header)
		{
			var sequence = 0;

			if (!header.BM_ActiveBorderIdentificationType.IsEmpty && !header.BM_TOLCarrierID.IsEmpty && !header.BM_RN_NKTOLCarrierNationality.IsEmpty)
			{
				activeBorderTransportMeans.Add(new ActiveBorderTransportMeansProvider(header, ++sequence));
			}

			foreach (var additionalTransportAtBorder in header.AdditionalTransportAtBorderList)
			{
				activeBorderTransportMeans.Add(new ActiveBorderTransportMeansProvider(additionalTransportAtBorder, ++sequence));
			}
		}

		public int Count => activeBorderTransportMeans.Count;

		public IEnumerator<IActiveBorderTransportMeans> GetEnumerator() => activeBorderTransportMeans.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => activeBorderTransportMeans.GetEnumerator();

		readonly List<IActiveBorderTransportMeans> activeBorderTransportMeans = new();
	}
}
