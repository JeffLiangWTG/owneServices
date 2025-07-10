using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(CustomsDisbursementCharge))]
	sealed class CustomsDisbursementChargeTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CustomsDisbursementCharge();
		}

		#endregion
	}
}
