using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.eNett.Testing
{
	[TestedType(typeof(ComPayRegisteredOrganisationDataSource))]
	internal sealed class ComPayRegisteredOrganisationDataSourceTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ComPayRegisteredOrganisationDataSource(Factory);
		}

		#endregion
	}
}
