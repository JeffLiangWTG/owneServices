using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.eNett.Testing
{
	[TestedType(typeof(ComPayRegisteredOrganisationCollection))]
	internal sealed class ComPayRegisteredOrganisationCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ComPayRegisteredOrganisationCollection>
	{
		#region Implementation

		protected override ComPayRegisteredOrganisationCollection GetCollectionToTest()
		{
			return new ComPayRegisteredOrganisationCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ComPayRegisteredOrganisation(Factory);
		}

		#endregion
	}
}
