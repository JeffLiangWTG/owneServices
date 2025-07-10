using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocAccountingVoucherLineCollection))]
	public class DocAccountingVoucherLineCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocAccountingVoucherLineCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			VoucherLine voucherLine = new VoucherLine();
			return DocAccountingVoucherLine.New(voucherLine, Factory);
		}

		protected override DocAccountingVoucherLineCollection GetCollectionToTest()
		{
			return new DocAccountingVoucherLineCollection(Factory);
		}
	}
}
