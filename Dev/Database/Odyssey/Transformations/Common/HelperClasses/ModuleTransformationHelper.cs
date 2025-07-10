using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.IO;

namespace Enterprise.DbUpgrader.Transformation.Common
{
	[SuppressMessage("Microsoft.Design", "CA1815")]
	public static class ModuleTransformationHelper
	{
		public static IEnumerable<FilterUpdate> GetFilterUpdates(string selectModuleFiltersSql, IDictionary<string, string> filterMappings)
		{
			var moduleFilterUpdates = new List<FilterUpdate>();

			using (var command = Db.Connection.Command(selectModuleFiltersSql))
			{
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var pk = (Guid)reader["S9_PK"];
						var filterData = (byte[])reader["S9_FilterData"];
						var needsUpdate = false;

						string newFilterDataXml;
						try
						{
							newFilterDataXml = Compressor.UncompressAsString(filterData);
						}
						catch (Exception e) when (!e.IsCriticalException())
						{
							continue;
						}

						foreach (var mapping in filterMappings)
						{
							var stringToReplace = string.Format(CultureInfo.InvariantCulture, "<FilterDescription>{0}</FilterDescription>", mapping.Key);

							if (newFilterDataXml.Contains(stringToReplace))
							{
								newFilterDataXml = newFilterDataXml.Replace(stringToReplace, string.Format(CultureInfo.InvariantCulture, "<FilterDescription>{0}</FilterDescription>", mapping.Value));
								needsUpdate = true;
							}
						}

						if (needsUpdate)
						{
							var newFilterData = Compressor.Compress(Encoding.ASCII.GetBytes(newFilterDataXml));
							moduleFilterUpdates.Add(new FilterUpdate(pk, newFilterData));
						}
					}
				}
			}

			return moduleFilterUpdates;
		}

		public static void SaveChanges(IEnumerable<FilterUpdate> filterUpdates)
		{
			foreach (var update in filterUpdates)
			{
				using (var command = Db.Connection.Command("UPDATE dbo.StmModuleFilter SET S9_FilterData = @S9_FilterData WHERE S9_PK = @S9_PK"))
				{
					command.AddParameter("@S9_FilterData", SqlDbType.VarBinary, update.FilterData());
					command.AddParameter("@S9_PK", SqlDbType.UniqueIdentifier, update.PK);
					command.ExecuteNonQuery();
				}
			}
		}

		public class FilterUpdate
		{
			public FilterUpdate(Guid pk, byte[] filterData)
			{
				PK = pk;
				this.filterData = filterData;
			}

			public readonly Guid PK;

			public byte[] FilterData()
			{
				return filterData;
			}
			readonly byte[] filterData;
		}
	}
}
