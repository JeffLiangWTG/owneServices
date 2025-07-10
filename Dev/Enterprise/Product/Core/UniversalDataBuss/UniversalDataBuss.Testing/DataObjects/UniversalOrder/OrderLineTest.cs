using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	[TestedType(typeof(OrderLine))]
	class OrderLineTest : DataObjectTestCase<OrderLine>
	{
		protected override List<string> ExpectedAllowLineControlWhiteSpaceAttributePropertiesCore() => new List<string>()
		{
			nameof(OrderLine.AdditionalInformation),
			nameof(OrderLine.SpecialInstructions),
			nameof(OrderLine.LineComment)
		};
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			return new Dictionary<string, int>()
			{
				{ nameof(OrderLine.PalletID), WhsDocketLineSchema.WE_PalletID.MaxLength },
				{ nameof(OrderLine.PackageGroupId), WhsDocketLineSchema.WE_PackageGroupId.MaxLength },
				{ nameof(OrderLine.CurrentHoldReason), WhsDocketLineSchema.WE_CurrentHoldReason.MaxLength },
				{ nameof(OrderLine.PartAttribute1), WhsDocketLineSchema.WE_PartAttrib1.MaxLength },
				{ nameof(OrderLine.PartAttribute2), WhsDocketLineSchema.WE_PartAttrib2.MaxLength },
				{ nameof(OrderLine.PartAttribute3), WhsDocketLineSchema.WE_PartAttrib3.MaxLength },
				{ nameof(OrderLine.SerialNumber), WhsDocketLineSchema.WE_SerialNumber.MaxLength },
				{ nameof(OrderLine.ConfirmationNumber), JobOrderLineSchema.JO_ConfirmationNum.MaxLength },
				{ nameof(OrderLine.AdditionalTerms), JobOrderLineSchema.JO_AdditionalTerms.MaxLength },
				{ nameof(OrderLine.LineComment), WhsDocketLineSchema.WE_LineComment.MaxLength },
				{ nameof(OrderLine.AdditionalInformation), JobOrderLineSchema.JO_AdditionalInformation.MaxLength },
				{ nameof(OrderLine.SpecialInstructions), JobOrderLineSchema.JO_SpecialInstructions.MaxLength },
				{ nameof(OrderLine.CrossDockOrderNumber), WhsDocketLineSchema.WE_ReceiveCrossDockOrderNo.MaxLength },
				{ nameof(OrderLine.CommercialInvoiceNumber), JobOrderLineSchema.JO_CommercialInvoiceNo.MaxLength },
				{ nameof(OrderLine.ContainerNumber), JobOrderLineSchema.JO_ContainerNumber.MaxLength },
				{ nameof(OrderLine.PutAwayArea), WhsAreaSchema.WA_Name.MaxLength },
				{ nameof(OrderLine.LineReference), JobOrderLineSchema.JO_LineReference.MaxLength },
				{ nameof(OrderLine.BatchNumber), PkgPackageOrderReferenceSchema.KPO_BatchNumber.MaxLength },
				{ nameof(OrderLine.HarmonisedCode), JobOrderLineSchema.JO_HSCode.MaxLength },
			};
		}
	}
}




