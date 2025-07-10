using System;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Environment
{
	public static class AccountingPeriodManager
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public static int GetPeriod(DateTime date, Guid companyPK)
		{
			int result = 0;
			string query = "select top 1 am_period from dbo.accperiodmanagement where am_startdate < @Date AND AM_EndDate > @Date AND AM_GC_Company = @Company order by AM_Period DESC";

			if (date != DateTime.MinValue && companyPK != Guid.Empty)
			{
				using (DbCommand command = Db.Connection.Command(query))
				{
					command.AddParameterBasedOnDbColumn("@Date", date, AccPeriodManagementSchema.AM_EndDate);
					command.AddParameterBasedOnDbColumn("@Company", companyPK, AccPeriodManagementSchema.AM_GC_Company);

					using (var reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							result = reader.GetInt32(0);
						}
					}
				}
			}

			return result;
		}
	}
}
