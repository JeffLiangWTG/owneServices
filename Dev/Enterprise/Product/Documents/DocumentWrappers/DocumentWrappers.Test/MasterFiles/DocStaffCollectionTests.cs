using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocStaffCollection))]
	sealed class DocStaffCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocStaffCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var glbStaff = Factory.New<GlbStaff>();
			return DocStaff.New(glbStaff, Factory);
		}

		protected override DocStaffCollection GetCollectionToTest()
		{
			return new DocStaffCollection(Factory);
		}
	}
}
