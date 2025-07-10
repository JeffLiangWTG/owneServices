using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Rating.Business.DocumentPrinting.DocAmount;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PricingPageColumnWrapperCollection))]
	sealed class PricingPageColumnWrapperCollectionTest : GenericWrapperCollectionTest<PricingPageColumnWrapperCollection>
	{
		#region Implementation

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new PricingPageColumnWrapper("heading", DocAmount.Create((NoResString)"value"), Factory);
		}

		protected override PricingPageColumnWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new PricingPageColumnWrapperCollection(Factory);
		}

		#endregion
	}
}
