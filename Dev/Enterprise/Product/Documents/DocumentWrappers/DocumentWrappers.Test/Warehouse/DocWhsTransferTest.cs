using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocWhsTransfer))]
	sealed class DocWhsTransferTest : DocWhsDocketTest<WhsTransfer, DocWhsTransfer>
	{
		#region Properties

		#region ZString Fields

		public void TestTransferConfimationDocumentTitle()
		{
			AssertEquals("Transfer Confirmation", DocketWrapper.TransferConfimationDocumentTitle);
			Docket.WD_DocketSubType = Enterprise.Warehouse.Transactions.CodeLists.TransferType.Codes.InterWhsSource;
			AssertEquals("Inter-Warehouse Transfer Confirmation", DocketWrapper.TransferConfimationDocumentTitle);
		}

		#endregion
		#endregion
		#region Implementation

		protected override DocWhsTransfer CreateWhsDocketWrapper(WhsDocketLabelControl docketLabel)
		{
			return DocWhsTransfer.New(docketLabel, Factory);
		}

		protected override DocWhsTransfer CreateWhsDocketWrapper(WhsTransfer docket)
		{
			return DocWhsTransfer.New(docket, Factory);
		}

		#endregion
	}
}
