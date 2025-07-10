using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.ReportWriter
{
	public class DataSourceCollection : NonPersistentBusinessObjectCollection<DataSource>
	{
		public DataSourceCollection(ReportBizObj parent)
			: base(parent.Factory)
		{
			this.parent = parent;
		}

		public readonly ReportBizObj parent;

		public void AddOrUpdate(string name, string sql)
		{
			var result = this.Cast<DataSource>().FirstOrDefault(x => x.Name == name);
			if (result == null)
			{
				result = AddNew();
				result.Name = name;
			}
			result.SQL = sql;
		}

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DataSource(parent);
		}

		#endregion
	}
}
