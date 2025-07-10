using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public class CusTempStorageContainerLookups : CusCodeDataLookups
	{
		public CusTempStorageContainerLookups(CusCodeData parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList CY_CodeList => GetCY_CodeList();

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
