using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Data;

namespace Enterprise.ZArchitecture.Environment
{
	class TimeZoneOffsetCache
	{
		#region Properties

		public IReadOnlyCollection<TimeSpan> TimeZoneOffsets
		{
			get
			{
				if (timeZoneOffsets == null)
				{
					timeZoneOffsets = GetAllUniqueTimeZoneOffsets();
				}
				return timeZoneOffsets;
			}
		}
		SortedSet<TimeSpan> timeZoneOffsets;

		#endregion

		#region Implementation

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		SortedSet<TimeSpan> GetAllUniqueTimeZoneOffsets()
		{
			var uniqueTimeZoneOffsets = new SortedSet<TimeSpan>();
			var sqlText = uniqueZoneOffsetsFromUTCSqlText;

			using (var cmd = Db.Connection.Command(sqlText))
			{
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var offsetInMinutes = (short)reader["UtcOffset"];
						uniqueTimeZoneOffsets.Add(TimeSpan.FromMinutes(offsetInMinutes));
					}
				}
			}
			return uniqueTimeZoneOffsets;
		}

		public bool Contains(TimeSpan offset)
		{
			return timeZoneOffsets.Contains(offset);
		}

		#endregion

		#region SQL Scripts

		const string uniqueZoneOffsetsFromUTCSqlText = @"SELECT DISTINCT R2_OffsetMinutesFromUTC AS UtcOffset FROM dbo.RefTimeZone";

		#endregion
	}
}
