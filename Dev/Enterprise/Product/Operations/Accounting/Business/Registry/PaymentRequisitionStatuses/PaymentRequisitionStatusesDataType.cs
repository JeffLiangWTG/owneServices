using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Registry.Business
{
	[RegistryEditor("Enterprise.Accounting.Registry.GUI.PaymentRequisitionStatusesRegistryItemEditor, Enterprise.Accounting.GUI")]
	class PaymentRequisitionStatusesDataType : CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryDataType
	{
		public PaymentRequisitionStatusesDataType(ReadOnlyCodeDescriptionPairList systemDefinedList)
			: base(systemDefinedList, true)
		{
		}
	}
}
