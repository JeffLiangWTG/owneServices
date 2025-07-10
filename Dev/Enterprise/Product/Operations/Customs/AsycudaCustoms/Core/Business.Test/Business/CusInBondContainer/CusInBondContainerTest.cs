using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(CusInBondContainer))]
	public class CusInBondContainerTest : Customs.Business.Testing.BaseCusInBondContainerTest<CusInBondContainer>
	{
		public void TestBC_ContainerNumCaption()
		{
			var container = Factory.New<CusInBondContainer>();
			AssertEquals("Container Number", DataBoundResourceStrings.GetDataForProperty(container.BC_ContainerNumInfo).Caption);
		}

		public void TestLookups()
		{
			var container = Factory.New<CusInBondContainer>();
			AssertType<CusInBondContainerLookups>(container.Lookups);
		}

		public void TestValidation()
		{
			var container = Factory.New<CusInBondContainer>();
			AssertType<CusInBondContainerValidation>(container.Validation);
		}

		public void TestMoveDetail()
		{
			var moveDetail = Factory.New<CusInBondMoveDetail>();
			var container = Factory.New<CusInBondContainer>();
			container.BC_ParentID = moveDetail.PK;
			AssertEquals(container.BC_ParentID, container.MoveDetail?.PK);
		}

		protected override BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var result = factory.New<CusInBondContainer>();
			var header = factory.New<CusInBondHeader>();
			header.BH_GB = GlbBranch.CurrentBranch.PK;
			var moveHeader = factory.New<CusInBondMoveHeader>();
			moveHeader.BM_BH = header.PK;
			var moveDetail = moveHeader.MovementDetails.AddNew();
			result.BC_ParentID = moveDetail.PK;
			result.BC_ParentTableCode = CusInBondMoveDetailSchema.Constants.Prefix;
			return result;
		}
	}
}
