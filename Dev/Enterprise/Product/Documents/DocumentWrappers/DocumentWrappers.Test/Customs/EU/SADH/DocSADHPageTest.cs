using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.DocumentWrappers.Customs.EU.Testing
{
	public class DocSADHPageTest : DocumentWrappers.Testing.DocBaseWrapperTest
	{
		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return DocSADHPage.New(Factory, null);
		}

		public void TestBox47TaxesTotals()
		{
			DocSADHPage page = DocSADHPage.New(Factory, entryLine);
			AssertEquals("Box47TaxesTotals has no taxes", 0, page.Box47TaxesTotals.Count);
		}

		public void TestBox47TotalAmount()
		{
			DocSADHPage page = DocSADHPage.New(Factory, entryLine);
			AssertEquals("Box47TotalAmount has no value", ZString.Empty, page.Box47TotalAmount);
		}

		public void TestBox47TotalMethodOfPayment()
		{
			DocSADHPage page = DocSADHPage.New(Factory, entryLine);
			AssertEquals("Box47TotalMethodOfPayment has no value", ZString.Empty, page.Box47TotalMethodOfPayment);
		}

		protected override void SetUp()
		{
			Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.UnitedKingdom);
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryLine = entryHeader.MergedLines.AddNew();
		}

		CusEntryLine entryLine;
	}
}
