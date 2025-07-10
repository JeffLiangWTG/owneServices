using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business.Testing
{
	sealed class DummyRegistryBusinessObjectCollectionWithCaseIgnoring : DummyRegistryBusinessObjectCollection
	{
		public DummyRegistryBusinessObjectCollectionWithCaseIgnoring(ReadOnlyCodeDescriptionPairList list) : base(list) { }

		protected override bool IgnoreCaseInCodes { get { return IgnoreCaseInCodesForTest; } }
		public bool IgnoreCaseInCodesForTest { get; set; }
	}
}
