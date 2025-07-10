using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.eNett.Testing
{
	[TestedType(typeof(ComPayRegisteredOrganisation))]
	internal sealed class ComPayRegisteredOrganisationTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ComPayRegisteredOrganisation();
		}

		#endregion
	}
}
