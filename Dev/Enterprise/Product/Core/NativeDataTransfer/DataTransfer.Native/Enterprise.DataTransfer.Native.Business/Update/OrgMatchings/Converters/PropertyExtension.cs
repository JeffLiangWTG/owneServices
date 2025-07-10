using Enterprise.DataTransfer.Native.Common;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgMatchings.Converters
{
	public static class PropertyExtension
	{
		public static void AssignedTo(this Property property, object result)
		{
			new PropertyConverter(property).AssignProperty(result);
		}
	}
}