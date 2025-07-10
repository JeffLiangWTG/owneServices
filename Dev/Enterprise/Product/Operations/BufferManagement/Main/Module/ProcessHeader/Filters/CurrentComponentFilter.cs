using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	public class CurrentComponentFilter : ComponentFilterBase
	{
		public CurrentComponentFilter(ZString description, ModuleIdentifier id, GetList listDelegate)
			: base(description, id, ProcessHeaderSchema.FH_FC_CurrentComponent, listDelegate,
				ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|CurrentComponent", ProcessHeader.ModuleFilterConstants.CurrentComponent))
		{
		}
	}
}
