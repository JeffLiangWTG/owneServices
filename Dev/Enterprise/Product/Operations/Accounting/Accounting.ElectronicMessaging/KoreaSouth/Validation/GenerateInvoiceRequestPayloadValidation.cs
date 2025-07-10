using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth
{
	class GenerateInvoiceRequestPayloadValidation : XsdValidation
	{
		public GenerateInvoiceRequestPayloadValidation() : base(
				"Enterprise.Accounting.ElectronicMessaging.KoreaSouth.Xsd.CustomizedSchema_1.0.xsd",
				"Enterprise.Accounting.ElectronicMessaging.KoreaSouth.Xsd.TaxInvoiceSchemaModule_1.0.xsd",
				"Enterprise.Accounting.ElectronicMessaging.KoreaSouth.Xsd.CodeListSchemaModule_1.0.xsd",
				"Enterprise.Accounting.ElectronicMessaging.KoreaSouth.Xsd.ISO_ISO3AlphaCurrencyCode_20081112.xsd",
				"Enterprise.Accounting.ElectronicMessaging.KoreaSouth.Xsd.QualifiedDataTypesSchemaModule_1.0.xsd",
				"Enterprise.Accounting.ElectronicMessaging.KoreaSouth.Xsd.ReusableAggregateBusinessInformationEntitySchemaModule_1.0.xsd",
				"Enterprise.Accounting.ElectronicMessaging.KoreaSouth.Xsd.UNECE_MeasurementUnitCommonCode_5.xsd")
		{
		}
	}
}
