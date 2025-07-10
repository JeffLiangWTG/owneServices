using System;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;

namespace Enterprise.Accounting.GUI.ARAP.Testing
{
	class TestComplianceDocumentFormForSqlException : ComplianceDocumentForm
	{
		public TestComplianceDocumentFormForSqlException(AccComplianceDocumentHeader businessEntity) : base(businessEntity)
		{
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			var result = base.ValidateAndSave();
			CreateAndThrowSqlException();
			return result;
		}

		public void CreateAndThrowSqlException()
		{
			var table = new DataTable("BlahBlah");
			var col = new DataColumn("PK", typeof(Guid));
			table.Columns.Add(col);
			table.PrimaryKey = new DataColumn[] { col };
			var row = table.NewRow();
			var complianceDocumentHeader = (AccComplianceDocumentHeader)BusinessEntity;
			try
			{
				#pragma warning disable CW1116 // Test purpose
				var conn = new SqlConnection(@"Data Source=.;Database=GUARANTEED_TO_FAIL;Connection Timeout=1");  // On connection will be created, just to throw the exception
				conn.Open();
				#pragma warning restore CW1116 // Test purpose
			}
			catch (System.Data.Common.DbException ex)
			{
				var innerEx = new ZDataException(ex, row, Db.Connection);
				var saveException = new ZSaveException(innerEx, complianceDocumentHeader.Factory);
				throw saveException;
			}
		}
	}
}
