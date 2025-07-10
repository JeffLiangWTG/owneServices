using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.DE.Business.DocumentWrappers
{
	public class DocEntryFeeCollection : DocumentWrapperCollection<DocEntryHeaderFee>
	{
		public DocEntryFeeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocEntryFeeCollection(IEnumerable<EntryFee> cumulatedFees, BusinessObjectFactory factoryToWrap)
			: base(cumulatedFees, factoryToWrap)
		{
		}
	}
}

