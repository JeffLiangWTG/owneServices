using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(CusInBondMoveDetail))]
	public class CusInBondMoveDetailTest : EnterpriseBusinessObjectTestCase
	{
		public void TestICusInBondContainerTypeSupporter()
		{
			var supporter = moveDetail as ICusInBondContainerTypeSupporter;
			AssertEquals(typeof(CusInBondContainer), supporter.ContainerType);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetMovementDetail();
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			return GetMovementDetail();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetMovementDetail();
		}

		public void TestValidation()
		{
			AssertType<CusInBondMoveDetailValidation>(moveDetail.Validation);
		}

		public void TestContainerCollection()
		{
			AssertType<CusInBondContainerCollection>(moveDetail.Containers);
		}

		public void TestMoveHeader()
		{
			AssertEquals(moveDetail.MoveHeader.PK, moveDetail.B9_BM);
		}

		public void TestSaveAndDelete()
		{
			moveDetail.Containers.AddNew();
			moveDetail.Containers.AddNew();
			Factory.Save();

			AssertNotNull(Factory.Load<CusInBondMoveDetail>(moveDetail.PK));
			AssertEquals(2, Factory.Load<CusInBondContainer>(new ZQuery(CusInBondContainerSchema.BC_ParentID, moveDetail.PK)).Length);
			moveDetail.Delete();
			AssertEquals(0, Factory.Load<CusInBondContainer>(new ZQuery(CusInBondContainerSchema.BC_ParentID, moveDetail.PK)).Length);
		}

		protected override void SetUp()
		{
			base.SetUp();
			moveDetail = GetMovementDetail();
		}
		CusInBondMoveDetail moveDetail;

		CusInBondMoveDetail GetMovementDetail()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_GB = GlbBranch.CurrentBranch.PK;
			var moveHeader = Factory.New<CusInBondMoveHeader>();
			moveHeader.BM_BH = header.PK;
			return moveHeader.MovementDetails.AddNew();
		}
	}
}
