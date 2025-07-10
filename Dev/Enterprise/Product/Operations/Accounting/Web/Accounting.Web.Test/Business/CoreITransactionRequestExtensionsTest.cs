using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Export.Business;
using Enterprise.Accounting.Web.Business;
using Enterprise.MasterFiles.Business.Testing;
using Moq;

namespace Enterprise.Accounting.Web.Testing.Business
{
	public class CoreITransactionRequestExtensionsTest : TestCaseWithFactory
	{
		public void TestGenerateCheckTransactionPaymentStatusQuery()
		{
			var connection = ((IDbConnectionInternals)base.TestConnection).ADOConnection;
			var transaction = ((IDbConnectionInternals)base.TestConnection).ADOTransaction;
			var dataAccess = new BaseDataAccess(connection, transaction);

			var mockRequest = new Mock<ITransactionNaturalKeys>();
			mockRequest.Setup(x => x.OrgCode).Returns("OH1");
			mockRequest.Setup(x => x.CompanyCode).Returns("GC1");
			mockRequest.Setup(x => x.AccLedger).Returns("AP");
			mockRequest.Setup(x => x.TransactionType).Returns("INV");
			mockRequest.Setup(x => x.TransactionNumber).Returns("Trans001");
			mockRequest.Setup(x => x.JobTransactionNumber).Returns("JobNum002");
			mockRequest.Setup(x => x.InternalReference).Returns("InvoiceRef003");

			var sqlCmd = mockRequest.Object.GenerateCheckTransactionPaymentStatusQuery(dataAccess);

			AssertEquals(sqlCmd.CommandText, "EXEC dbo.CheckTransactionPaymentStatus @OrgCode_0, @CompanyCode_0, @AccLedger, @TransactionType, @TransactionNumber, @JobTransactionNumber, @InternalReference, NULL");
			var paramsList = sqlCmd.Parameters.ToList<SqlParameter>();
			AssertSqlParameter("@OrgCode_0", SqlDbType.NVarChar, 254, "OH1");
			AssertSqlParameter("@CompanyCode_0", SqlDbType.Char, 3, "GC1");
			AssertSqlParameter("@AccLedger", SqlDbType.Char, 2, "AP");
			AssertSqlParameter("@TransactionType", SqlDbType.Char, 3, "INV");
			AssertSqlParameter("@TransactionNumber", SqlDbType.VarChar, 38, "Trans001");
			AssertSqlParameter("@JobTransactionNumber", SqlDbType.VarChar, 38, "JobNum002");
			AssertSqlParameter("@InternalReference", SqlDbType.VarChar, 38, "InvoiceRef003");

			void AssertSqlParameter(string typeName, SqlDbType sqlDbType, int size, object value)
			{
				AssertEquals(true, paramsList.Single(x => x.ParameterName == typeName && x.SqlDbType == sqlDbType && x.Size == size && x.Value == value) != null);
			}
		}
	}
}

