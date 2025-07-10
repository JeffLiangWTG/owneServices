using Enterprise.DataTransfer.Native.Common;
using Enterprise.MasterFiles.Business.OrgPatternMatching;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgMatchings.Converters
{
	/// <summary>
	/// Convert an Entity Object to OrgBrandOrRelatedNameForMatching Object
	/// </summary>
	public class EntityToOrgBrandMatchingConverter
	{
		public void Convert(IMatchingBrandOrRelatedName result, Entity source)
		{
			ConvertProperties(result, source);
		}

		void ConvertProperties(IMatchingBrandOrRelatedName result, IEntity entity)
		{
			foreach (var property in entity.Properties)
			{
				property.AssignedTo(result);
			}
		}
	}
}