using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(CusInBondHeader))]
	class CusInBondHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var header = Factory.New<CusInBondHeader>();
			AssertEquals(CusInBondHeader.AsycudaEntryInstructionType, header.BH_ApplicationCode);
		}

		public void TestICusInBondHeader()
		{
			AssertType<CusInBondHeader>(Factory.New<Integration.Customs.AsycudaCustoms.ICusInBondHeader>());
		}

		public void TestCorrectlyLoad()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_GB = GlbBranch.CurrentBranch.PK;
			var moveHeader = Factory.New<CusInBondMoveHeader>();
			moveHeader.BM_BH = header.PK;
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			AssertType<CusInBondHeader>(newFactory.Load<Customs.Business.BaseCusInBondHeader>(header.PK));
			AssertType<CusInBondMoveHeader>(newFactory.Load<Customs.Business.BaseCusInBondMoveHeader>(moveHeader.PK));
		}
	}
}
