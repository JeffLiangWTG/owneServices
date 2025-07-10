using System.Collections.Generic;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business
{
	public interface IAdditionalInfoCollectionProvider
	{
		IEnumerable<AdditionalInfo> AdditionalInfos { get; }
	}
}
