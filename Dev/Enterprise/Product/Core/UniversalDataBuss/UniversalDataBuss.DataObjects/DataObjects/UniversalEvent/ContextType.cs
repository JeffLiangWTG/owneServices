using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName), FlattenedIntoAttributes("Type", true)]
	public class ContextType : IDataObject
	{
		[Mandatory, MaxLength(50)] // Check this length. Not sure it's right! It might be though...
		public ZString? Type { get; set; }
		[MaxLength(50)]
		public ZString? Description { get; set; }

		public static implicit operator ContextType(string type)
		{
			return new ContextType { Type = type };
		}

		public static implicit operator string(ContextType contextType)
		{
			return contextType?.Type;
		}
	}
}
