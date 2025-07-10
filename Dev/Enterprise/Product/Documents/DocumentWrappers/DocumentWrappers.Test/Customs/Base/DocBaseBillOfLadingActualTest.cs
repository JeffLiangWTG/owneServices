using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.Base.Testing
{
	[TestedType(typeof(DocBaseBillOfLading))]
	sealed class DocBaseBillOfLadingActualTest : DocBaseBillOfLadingTest
	{
		protected override DocBaseBillOfLading CreateBillOfLading()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var bill = declaration.Bills.CreatePrimaryBill(BillTypeList.Codes.MasterBill);
			return new DocBaseBillOfLading(bill);
		}
	}
}
