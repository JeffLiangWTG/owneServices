using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentEngine.Testing.UtilityClasses
{
	class DummyDocWrapper : DocumentWrapper
	{
		public DummyDocWrapper()
			: base(new DocumentWrapperForTesting(""), new BusinessObjectFactory())
		{
		}

		public override string ToString()
		{
			return "My ID";
		}
	}
}
