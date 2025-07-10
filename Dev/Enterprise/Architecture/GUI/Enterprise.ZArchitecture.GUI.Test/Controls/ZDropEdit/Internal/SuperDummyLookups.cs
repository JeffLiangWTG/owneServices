using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class SuperDummyLookups : DummyLookups
	{
		public SuperDummyLookups(SuperDummy parent) : base(parent)
		{
		}

		public CodeDescriptionPairList SortedInvoiceList => Huge?.SortedInvoiceList;
		HugeDummy Huge => ((SuperDummy)Parent)?.Huge;
	}
}
