using System.Linq;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.MasterFiles.Business.OrgPatternMatching;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgMatchings.Converters
{
	/// <summary>
	/// Convert Entity Object to OrgAddressMatching Object
	/// </summary>
	public class EntityToOrgAddressMatchingConverter
	{
		public void Convert(IMatchingAddress result, Entity source)
		{
			ConvertProperties(result, source);
			SetMainAddress(result, source);
			return;
		}

		void SetMainAddress(IMatchingAddress matching, IEntity entity)
		{
			var capabilities = entity.Children.Where(e => e.EntityName == "OrgAddressCapability");
			foreach (var capability in capabilities)
			{
				var addressType = (string)capability["AddressType"];
				bool isMainAddress = false;
				bool.TryParse((string)capability["IsMainAddress"], out isMainAddress);
				if (addressType == "OFC" && isMainAddress)
				{
					matching.SetMainAddress();
				}
			}
		}

		void ConvertProperties(IMatchingAddress result, IEntity entity)
		{
			foreach (var property in entity.Properties)
			{
				property.AssignedTo(result);
			}
		}
	}
}