using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(ImportLicenseResponseObject))]
	class ImportLicenseResponseObjectTest : NonPersistentBusinessObjectTestCase
	{
		#region Overrides of BusinessObjectBaseTestCase

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ImportLicenseResponseObject(Factory);
		}

		#endregion
	}
}
