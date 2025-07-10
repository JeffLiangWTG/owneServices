using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public class TransactionHeaderReference : IDataObject
	{
		[MaxLength(3), Mandatory]
		public ZString? Type { get; set; }
		[MaxLength(80)]
		public ZString? TypeDescription { get; set; }
		[MaxLength(120)]
		public ZString? Reference { get; set; }
		[MaxLength(80)]
		public ZString? ReferenceDescription { get; set; }
	}
}
