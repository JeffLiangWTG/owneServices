using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.ArchiveManager.Test.Actions
{
	class DummyWrapper : DocumentWrapper
	{
		public DummyWrapper()
			: base(null, new BusinessObjectFactory())
		{ }

		public override string ToString()
			=> "";
	}
}
