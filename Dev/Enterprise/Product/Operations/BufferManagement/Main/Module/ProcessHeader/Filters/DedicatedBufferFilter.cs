using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	public class DedicatedBufferFilter : ComponentFilterBase
	{
		public DedicatedBufferFilter(ZString description, ModuleIdentifier id, GetList listDelegate)
			: base(description, id, ProcessHeaderSchema.FH_FC_DedicatedBuffer, listDelegate,
				ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|DedicatedBuffer", ProcessHeader.ModuleFilterConstants.DedicatedBuffer))
		{
		}
	}
}
