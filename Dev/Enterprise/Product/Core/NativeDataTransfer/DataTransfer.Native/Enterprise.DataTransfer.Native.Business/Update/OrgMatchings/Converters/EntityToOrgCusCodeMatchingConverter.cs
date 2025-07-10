using Enterprise.DataTransfer.Native.Common;
using Enterprise.MasterFiles.Business.OrgPatternMatching;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgMatchings.Converters
{
	/// <summary>
	/// Convert Entity Object to OrgCusCodeMatching
	/// </summary>
	public class EntityToOrgCusCodeMatchingConverter
	{
		public void Convert(IMatchingCusCode result, Entity source)
		{
			ConvertProperties(result, source);
		}

		void ConvertProperties(IMatchingCusCode result, IEntity entity)
		{
			foreach (var property in entity.Properties)
			{
				property.AssignedTo(result);
			}
		}
	}
}