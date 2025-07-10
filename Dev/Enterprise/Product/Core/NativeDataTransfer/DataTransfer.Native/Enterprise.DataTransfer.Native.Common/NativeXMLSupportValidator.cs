using System.Linq;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DataTransfer.Native.Common
{
	public static class NativeXMLSupportValidator
	{
		public static void CheckEntityIsSupported(string entitySetName, string entityRootName)
		{
			if (IsTableDeprecated(entityRootName))
			{
				throw new NativeXMLUserVisibleException(
					string.Format(@"The '{0}' Native XML dataset has been deprecated. Please use the Universal Shipment XML instead.",
					entitySetName)
				);
			}
		}

		public static bool IsTableDeprecated(string tableName)
			=> GetDeprecatedList_IgnoresRegistry().Contains(tableName) ||
				(!DataRegistry.Instance.IsNativeXMLSupported && GetDeprecateList().Contains(tableName));

		static string[] GetDeprecateList()
			=> new[]
			{
				"JobDeclaration",
				"JobShipment",
				"JobOrderHeader"
			};

		static string[] GetDeprecatedList_IgnoresRegistry() => new[] { "UNDGSubstance" };
	}
}
