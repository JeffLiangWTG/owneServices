using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.Accounting.GUI.Res;

namespace Enterprise.Accounting.Registry.GUI
{
	public class PaymentRequisitionStatusesRegistryItemEditor : CodeDescriptionBoolWithExtraBoolRegistryItemEditor
	{
		public PaymentRequisitionStatusesRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory, Res.GetString("e8b2315c-6851-42cd-add9-edf71c75c930", "Create Separate Payments"))
		{
		}
	}
}
