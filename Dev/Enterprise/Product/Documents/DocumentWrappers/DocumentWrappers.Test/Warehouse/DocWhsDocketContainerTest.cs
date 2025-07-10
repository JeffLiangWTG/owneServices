using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocWhsDocketContainer))]
	sealed class DocWhsDocketContainerTest : DocumentWrapperTestCase
	{
		#region Properties

		#region ZString Fields

		public void TestIsChargeable()
		{
			DocketContainer.WC_IsChargeable = true;
			AssertEquals("DocWrapper IsChargeable property is incorrect", "Yes", DocWrapper.IsChargeable);
			DocketContainer.WC_IsChargeable = false;
			AssertEquals("DocWrapper IsChargeable property is incorrect", "No", DocWrapper.IsChargeable);
		}

		#endregion

		#region ZInt Fields

		public void TestItemCount()
		{
			DocketContainer.WC_ItemCount = 10;
			AssertEquals("DocWrapper ItemCount property is incorrect", 10, DocWrapper.ItemCount);
		}

		public void TestPalletCount()
		{
			DocketContainer.WC_PalletCount = 20;
			AssertEquals("DocWrapper PalletCount property is incorrect", 20, DocWrapper.PalletCount);
		}

		#endregion

		#endregion

		#region IDocSimpleContainer members

		public void TestContainerNumber()
		{
			DocketContainer.WC_ContainerNum = "Container 1";
			AssertEquals("DocWrapper ContainerNumber property is incorrect", "Container 1", DocWrapper.ContainerNumber);
			DocketContainer.WC_ContainerNum = "Container 2";
			AssertEquals("DocWrapper ContainerNumber property is incorrect", "Container 2", DocWrapper.ContainerNumber);
		}

		public void TestSealNumber()
		{
			DocketContainer.WC_SealNum = "Seal 1";
			AssertEquals("DocWrapper SealNumber property is incorrect", "Seal 1", DocWrapper.SealNumber);
			DocketContainer.WC_SealNum = "Seal 2";
			AssertEquals("DocWrapper SealNumber property is incorrect", "Seal 2", DocWrapper.SealNumber);
		}

		public void TestType()
		{
			RefContainer docRefContainer = Factory.New<RefContainer>();
			DocketContainer.WC_RC = docRefContainer.PK;
			docRefContainer.RC_Code = "AAA";
			AssertEquals("DocWrapper Type property is incorrect", "AAA", DocWrapper.Type);
			docRefContainer.RC_Code = "BBB";
			AssertEquals("DocWrapper Type property is incorrect", "BBB", DocWrapper.Type);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			DocketContainer = Factory.New<WhsDocketContainer>();
			DocWrapper = DocWhsDocketContainer.New(DocketContainer, Factory);
			AssertNotNull("Wrapper not null", DocWrapper);
			base.SetUp();
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocWrapper };
		}

		WhsDocketContainer DocketContainer;
		DocWhsDocketContainer DocWrapper;

		#endregion
	}
}
