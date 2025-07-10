using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(PaymentRequisitionStatusesDataType))]
	class PaymentRequisitionStatusesDataTypeTest : CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryDataTypeTest
	{
		protected override string ExpectedEditorName
		{
			get { return "PaymentRequisitionStatusesRegistryItemEditor"; }
		}

		protected override CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryDataType GetNewDataType()
		{
			return new PaymentRequisitionStatusesDataType(null);
		}
	}
}
