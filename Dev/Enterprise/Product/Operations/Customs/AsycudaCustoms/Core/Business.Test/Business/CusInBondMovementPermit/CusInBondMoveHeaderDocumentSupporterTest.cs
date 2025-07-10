using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(CusInBondMoveHeaderDocumentSupporter))]
	class CusInBondMoveHeaderDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestBusinessContext()
		{
			var moveHeader = Factory.New<CusInBondMoveHeader>();
			AssertEquals(CargoWise.Definitions.BusinessContext.CusInBondHeader, moveHeader.DocumentSupporter.BusinessContext);
		}

		public void TestSupportedDataContexts()
		{
			var moveHeader = Factory.New<CusInBondMoveHeader>();
			Assert(moveHeader.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.CusInBondHeader)));
		}

		public void TestStorageDocsAreEditableIfInRelated()
		{
			var moveHeader = Factory.New<CusInBondMoveHeader>();
			AssertEquals("should be editable as child", true, moveHeader.DocumentSupporter.StorageDocsAreEditableIfInRelated);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_GB = GlbBranch.CurrentBranch.PK;
			var moveHeader = Factory.New<CusInBondMoveHeader>();
			moveHeader.BM_BH = header.PK;
			return moveHeader;
		}
	}
}
