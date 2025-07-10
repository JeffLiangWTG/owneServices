using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocExportStatementSetting : NonPersistentBusinessObject, IObsoleteValidation
	{
		public DocExportStatementSetting(ExportStatementSetting statementSetting)
		{
			fStatementSetting = statementSetting;
		}

		//public static DocExportStatementSetting New(ExportStatementSetting StatementSetting)
		//{
		//    return (StatementSetting == null) ? null : new DocExportStatementSetting(StatementSetting);
		//}

		public ZString CountryCode
		{
			get { return (StatementSetting.Parent == null) ? ZString.Empty : StatementSetting.Parent.CountryCode; }
		}

		public ZString Name
		{
			get { return StatementSetting.Code; }
		}

		public ZString Statement
		{
			get { return StatementSetting.Statement; }
		}

		public ZBool UseOnHawb
		{
			get { return StatementSetting.UseOnHawb; }
		}

		public ZBool UseOnDirectIATAMawb
		{
			get { return StatementSetting.UseOnDirectIATAMawb; }
		}

		public ZBool UseOnConsolidationMawb
		{
			get { return StatementSetting.UseOnConsolidationMawb; }
		}

		public ZBool UseOnHouseBillOfLading
		{
			get { return StatementSetting.UseOnHouseBillOfLading; }
		}

		public ZBool UseOnDirectMasterBillOfLading
		{
			get { return StatementSetting.UseOnDirectMasterBillOfLading; }
		}

		public ZBool UseOnConsolidationMasterBillOfLading
		{
			get { return StatementSetting.UseOnConsolidationMasterBillOfLading; }
		}

		public override string ToString()
		{
			return Statement;
		}

		#region StatementSetting
		protected ExportStatementSetting StatementSetting
		{
			get
			{
				if (fStatementSetting == null)
				{
					fStatementSetting = new ExportStatementSetting();
				}
				return fStatementSetting;
			}
		}
		ExportStatementSetting fStatementSetting;
		#endregion
	}
}
