using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName), FlattenedIntoAttributes(nameof(MessageNumber.Value), flattenEvenWithOldNamespace: true)]
	public class MessageNumber : IMessageNumber
	{
		[Mandatory, MaxLength(32)]
		public MessageNumberType? Type { get; set; }

		[Mandatory, MaxLength(256)]
		public ZString? Value { get; set; }
	}
}
