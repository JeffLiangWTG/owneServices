#if DEBUG
using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.ClientSharedComponents;
using Enterprise.Environment;
using Enterprise.Registry.Business;

namespace Enterprise.Client.OIA
{
	internal class OIATestHelper : SharedTestHelper
	{
		public OIATestHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OIATestHelper()
			: base()
		{
		}

		#region Set Factory Items

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		internal object ExecuteScalar(string sQL)
		{
			DbCommand cmd = Db.Connection.Command(sQL);
			return cmd.ExecuteScalar();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		internal void ExecuteNonQuery(string sQL)
		{
			DbCommand cmd = Db.Connection.Command(sQL);
			cmd.ExecuteNonQuery();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		internal bool ExistsDbObject(string objectName)
		{
			DbCommand cmd = Db.Connection.Command("select name FROM sys.objects where name='" + objectName + "'");
			object result = cmd.ExecuteScalar();
			return (result != null);
		}

		#endregion

		#region Set Registry Items

		public override void SetValidRegistryAll()
		{
			SystemDataRegistry.Instance.GLTransactionsCSVExportDirectory.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Env.TempPath);
		}

		#endregion
	}
}
#endif
