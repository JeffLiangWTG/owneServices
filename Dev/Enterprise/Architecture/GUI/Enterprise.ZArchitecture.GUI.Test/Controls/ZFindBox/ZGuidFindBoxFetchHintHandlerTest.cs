using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	class ZGuidFindBoxFetchHintHandlerTest : ZCodeFindBoxFetchHintHandlerTest
	{
		protected override ZCodeFindBox GetNewFindBox()
		{
			return new ZGuidFindBox();
		}

		protected override string GetBindToProperty()
		{
			return DummyDependentBizoSchema.ZD1_Z0.Name;
		}

		protected override ZCodeFindBoxFetchHintHandler GetNewFetcher(IBindToList control, IBusiness dataSource)
		{
			return new ZGuidFindBoxFetchHintHandler(control, dataSource);
		}
	}
}
