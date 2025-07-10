using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	public class CusTempStorageContainerLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCYCodeList()
		{
			var cusTempStorageContainerLookup = Factory.New<CusTempStorageContainer>().Lookups;
			AssertEquals("CYCode list lookup content should match container types of RefContainer_List", GetCY_CodeList().CodesAsString, cusTempStorageContainerLookup.CY_CodeList.CodesAsString);
		}

		public CodeDescriptionPairList GetCY_CodeList()
		{
			var result = new CodeDescriptionPairList();

			if (RefContainer_List == null)
			{
				return result;
			}
			foreach (var refContainer in RefContainer_List)
			{
				if (refContainer == null || string.IsNullOrWhiteSpace(refContainer.RC_Code))
				{
					continue;
				}
				result.Add(new CodeDescriptionPair(refContainer.RC_Code.ToString(), refContainer.RC_Code.ToString()));
			}
			return result;
		}

		public RefContainerCollection RefContainer_List
		{
			get { return refContainer_List ?? (refContainer_List = new RefContainerCollection(Factory)); }
		}
		RefContainerCollection refContainer_List;
	}
}
