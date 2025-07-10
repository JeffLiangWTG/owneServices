using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	[TestedType(typeof(ARTransferCreator))]
	public class ARTransferCreatorTestCase : TransferCreatorTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ARTransferCreator(Factory);
		}

		public override void TestCreateTransfer()
		{
			base.TestCreateTransfer();
			Assert("Created Transfer should be an ARTransfer", CreatedTransfer is ARTransfer);
			AssertEquals("Descriptions should be descriptive",
				"AR TRANSFER FROM TSTORG TO TSTORG2 (SYSTEM GENERATED)",
				CreatedTransfer.TransferFrom.AH_Desc);
			AssertEquals("Descriptions should be descriptive",
				"AR TRANSFER FROM TSTORG TO TSTORG2 (SYSTEM GENERATED)",
				CreatedTransfer.TransferTo.AH_Desc);
		}
	}
}
