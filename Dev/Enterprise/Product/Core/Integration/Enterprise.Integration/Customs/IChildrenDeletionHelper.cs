using CargoWise.EntityFramework;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface IChildrenDeletionHelper
		{
			void DeleteChildren(BusinessObject bizObj);
		}
	}
}
