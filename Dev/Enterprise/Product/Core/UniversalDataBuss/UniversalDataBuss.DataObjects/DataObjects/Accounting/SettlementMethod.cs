using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public class SettlementMethod : IDataObject
	{
		public CodeDescriptionPair Method { get; set; }
		[MaxLength(35)]
		public ZString? AuthorizationReference { get; set; }
		public ZDateTime? AuthorizationExpiryDate { get; set; }
	}
}