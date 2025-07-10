using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.AccStatement.Testing
{
	[TestedType(typeof(BankStatementFormat))]
	public class BankStatementFormatTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidation()
		{
			AssertEquals(typeof(BankStatementFormatValidation), TestBankStatementFormat.Validation.GetType());
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new BankStatementFormat(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestBankStatementFormat = new BankStatementFormat(Factory);
		}

		public void TestStatementFileFormats_ListHasCorrectXMLFormatAndDescription()
		{
			var bankStatementFormat = (BankStatementFormat)GetNewBusinessObject();
			var fileFormatList = bankStatementFormat.StatementFileFormats_List;
			AssertEquals("XML", fileFormatList[0].Code);
			AssertEquals("ediDataInterface XML format", fileFormatList[0].Description);
		}

		BankStatementFormat TestBankStatementFormat;

		#endregion
	}
}
