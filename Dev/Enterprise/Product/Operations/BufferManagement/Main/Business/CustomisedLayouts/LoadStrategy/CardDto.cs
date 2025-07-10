using System.Collections.Generic;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.BufferManagement.Business
{
	public class CardDto : CustomBusinessObject
	{
		public CardDto(ICardContent card, IEnumerable<ControlCustomisationLinePropertyCache> lineCaches)
			: base(null, CustomPropertyCollectionBuilder.GetCustomProperties(GetCustomisationFields(card, lineCaches)))
		{
		}

		static IEnumerable<ICustomProperty> GetCustomisationFields(ICardContent content, IEnumerable<ControlCustomisationLinePropertyCache> lineCaches)
		{
			return lineCaches.Select(l => l.GetCustomProperty(content)).DistinctBy(x => x.Identifier).ToArray();
		}
	}
}
