using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.Data;

namespace Enterprise.Client.EDI.ScavengingImportServiceTask
{
	public class SqlOrganizationScavengingRepository
	{
		public SqlOrganizationScavengingRepository() { }

		public SqlOrganizationScavengingRepository(INotifications notifier)
		{
			this.notifier = notifier;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		internal IEnumerable<ScavengingItem> Select(int count)
		{
			using (var command = Db.Connection.Command(string.Format("SELECT TOP {0} [IM_PK],[IM_ClientID],[IM_MessageTrackingID],[IM_MessageType],[IM_ApplicationCode],[IM_InsertUTC],[IM_Content] FROM dbo.ClientStatisticsXML ORDER BY IM_InsertUTC", count)))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					ScavengingItem item;
					var contentColumnIndex = reader.GetOrdinal("IM_Content");
					var itemID = new Guid(reader["IM_PK"].ToString());
					try
					{
						var contentBuffer = new byte[contentLengthLimit];
						var receivedLength = reader.GetBytes(contentColumnIndex, 0, contentBuffer, 0, contentLengthLimit);
						if (receivedLength == contentLengthLimit && reader.GetBytes(contentColumnIndex, contentLengthLimit, new byte[1], 0, 1) > 0)
						{
							notifier?.AddError(string.Format(CultureInfo.InvariantCulture, "Messages exceeded the length limit({0}): {1}. This message will be moved to archive", contentLengthLimit, itemID));
							continue;
						}

						var content = System.Text.Encoding.Unicode.GetString(contentBuffer).Trim('\0');

						item = new ScavengingItem(content, itemID)
						{
							Client = reader["IM_ClientID"].ToString(),
							Type = reader["IM_MessageType"].ToString(),
							Date = Convert.ToDateTime(reader["IM_InsertUTC"])
						};
					}
					catch (InvalidDataException ex)
					{
						reader.Close();
						reader.Dispose();
						if (ex.Message.StartsWith("Data found corrupted"))
						{
							Delete(itemID);
							throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture, "Data found corrupted when processing item: {0}. This message will be moved to archive", itemID), ex);
						}
						throw;
					}
					yield return item;
				}
			}
		}

		internal void ProcessItemAsDefect(ScavengingItem item)
		{
			item.IsProcessed = true;
			Delete(item.ID);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		internal void Delete(Guid itemID)
		{
			var sql = @"
DELETE dbo.ClientStatisticsXML
OUTPUT DELETED.*
INTO ClientStatisticsXMLArchive
WHERE IM_PK = @itemID";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@itemID", System.Data.SqlDbType.UniqueIdentifier, itemID);
				command.ExecuteNonQuery();
			}
		}

		internal int contentLengthLimit = 5000000;
		internal INotifications notifier;
	}
}
