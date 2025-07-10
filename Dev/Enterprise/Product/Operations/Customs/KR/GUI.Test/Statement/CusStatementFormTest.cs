using System.Windows.Forms;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(Statements))]
	sealed class CusStatementFormTest : ZFormBasherTest
	{
		public void TestMenuIsNotNull()
		{
			var statement = Factory.New<CusStatementHeader>();
			using (Statements form = new Statements(statement))
			{
				AssertNotNull("Please check auto-generated codes, 'Menu=null;' and remove the line", form.Menu);
			}
		}

		public void TestFormCaption()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = StatementHeaderTypeList.Codes.NormalReport;
			statement.B2_StatementNumber = "030190079807";
			using (Statements form = new Statements(statement))
			{
				AssertEquals("Statement - 030-19-0079807", form.FormCaption);
			}

			statement.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
			statement.B2_StatementNumber = "0127030112200237050";
			using (Statements form = new Statements(statement))
			{
				AssertEquals("Customs Individual Disbursement Bills - 0127-030-11-22-0-023705-0", form.FormCaption);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var result = new Statements(Header);
			result.ControllerID = ControllerIDs.Customs.CustomsStatement;
			return result;
		}

		CusStatementHeader Header
		{
			get { return header ?? (header = Factory.New<CusStatementHeader>()); }
		}
		CusStatementHeader header;
	}
}
