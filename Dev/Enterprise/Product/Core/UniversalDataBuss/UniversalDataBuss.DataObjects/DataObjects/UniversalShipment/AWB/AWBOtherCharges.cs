using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.AWB
{
	[XsdSchema(Placement.Inner)]
	public class AWBOtherCharges : IDataObject
	{
		public CodeDescriptionPair2Char ChargeCode { get; set; }
		public CodeDescriptionPair1Char EntitlementDue { get; set; }
		public CodeDescriptionPair PrepaidCollect { get; set; }
		[MaxLength(35)]
		public ZString? Description { get; set; }
		public ZDecimal? Amount { get; set; }
	}
}
