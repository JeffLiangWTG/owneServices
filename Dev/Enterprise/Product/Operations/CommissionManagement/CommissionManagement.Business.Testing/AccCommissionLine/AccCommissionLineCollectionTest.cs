using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(AccCommissionLineCollection))]
	internal class AccCommissionLineCollectionTest : ActiveBusinessObjectCollectionTestCase<AccCommissionLineCollection>
	{
		#region New

		public void TestDefaultsForNewElement()
		{
			var commissionHeader = Factory.New<AccCommissionHeader>();
			var directLine = commissionHeader.Lines.AddNew();
			AssertEquals(commissionHeader.TablePrefix, directLine.CL0_ParentTableCode);

			var commissionLineGroup = commissionHeader.LineGroups.AddNew();
			var groupLine = commissionLineGroup.Lines.AddNew();
			AssertEquals(commissionLineGroup.TablePrefix, groupLine.CL0_ParentTableCode);
		}

		#endregion
	}
}
