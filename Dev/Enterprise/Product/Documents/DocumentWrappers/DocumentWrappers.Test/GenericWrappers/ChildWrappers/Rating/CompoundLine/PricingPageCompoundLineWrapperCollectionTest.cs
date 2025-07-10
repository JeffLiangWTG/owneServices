using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PricingPageCompoundLineWrapperCollection))]
	sealed class PricingPageCompoundLineWrapperCollectionTest : GenericWrapperCollectionTest<PricingPageCompoundLineWrapperCollection>
	{
		#region Implementation

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new PricingPageCompoundLineWrapper(Factory);
		}

		protected override PricingPageCompoundLineWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new PricingPageCompoundLineWrapperCollection(Factory);
		}

		#endregion
	}
}
