using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(NFEImportObjectItem))]
	public class NFEImportObjectItemTest : NonPersistentBusinessObjectTestCase
	{
		#region Overrides of BusinessObjectBaseTestCase

		protected override BusinessObject GetNewBusinessObject()
		{
			return new NFEImportObjectItem(Factory);
		}

		#endregion
	}
}
