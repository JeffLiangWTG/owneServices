using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	[TestedType(typeof(APTransferCreator))]
	public class APTransferCreatorTestCase : TransferCreatorTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new APTransferCreator(Factory);
		}

		public override void TestCreateTransfer()
		{
			base.TestCreateTransfer();
			Assert("Transfer should be an APTransfer", CreatedTransfer is APTransfer);
			AssertEquals("Descriptions should be descriptive",
				"AP TRANSFER FROM TSTORG TO TSTORG2 (SYSTEM GENERATED)",
				CreatedTransfer.TransferFrom.AH_Desc);
			AssertEquals("Descriptions should be descriptive",
				"AP TRANSFER FROM TSTORG TO TSTORG2 (SYSTEM GENERATED)",
				CreatedTransfer.TransferTo.AH_Desc);
		}
	}
}
