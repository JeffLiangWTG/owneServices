using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	public class UNDGSubstanceWrapperSummaryProviderForTest : IUNDGSubstanceWrapperSummaryProviderForTest
	{
		public string GetSummary(IUNDGDataItem undgDataItem, BusinessObjectFactory factory)
		{
			if (undgDataItem is UNDGDataItem undgDataItemBizObj)
			{
				var wrapper = new UNDGSubstanceWrapper(undgDataItemBizObj, factory);
				return wrapper.Summary;
			}

			return string.Empty;
		}
	}
}
