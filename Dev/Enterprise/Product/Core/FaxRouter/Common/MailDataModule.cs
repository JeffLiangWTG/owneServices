using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;

namespace Enterprise.FaxRouter
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClassesAnalyzer", Justification = "This assembly has no need to change SqlConnection")]
	public class MailDataModule : BaseDataModule
	{
		string GetWhereClauseForMailItemsBySubjectString(string stringContainedInSubject)
		{
			return "MI_Subject LIKE '%" + stringContainedInSubject + "%'";
		}

		string GetWhereClauseForMailItemsBySender(string semicolonSeparatedEmailSenderList)
		{
			String[] emailAddresses = semicolonSeparatedEmailSenderList.Split(';');
			StringBuilder sQLLikeEmailBuilder = new StringBuilder();
			foreach (string email in emailAddresses)
			{
				sQLLikeEmailBuilder.Append(" MI_FROM LIKE '%" + email + "%' OR");
			}
			return sQLLikeEmailBuilder.ToString().Substring(0, sQLLikeEmailBuilder.ToString().Length - 3);
		}

		public int GetTotalNewReceivedMailItemsBySubjectSubstring(string stringContainedInSubject)
		{
			return GetTotalNewReceivedMailItemsWithAdditionalWhereClause(GetWhereClauseForMailItemsBySubjectString(stringContainedInSubject));
		}

		public IEnumerable<List<MailDBItemDataLine>> GetNewReceivedMailItemsBySubjectSubstringInBatches(string stringContainedInSubject, int maxMailItems)
		{
			return GetNewReceivedMailItemsWithAdditionalWhereClauseInBatches(GetWhereClauseForMailItemsBySubjectString(stringContainedInSubject), maxMailItems);
		}

		public int GetTotalNewReceivedMailItemsBySender(string semicolonSeparatedEmailSenderList)
		{
			return GetTotalNewReceivedMailItemsWithAdditionalWhereClause(GetWhereClauseForMailItemsBySender(semicolonSeparatedEmailSenderList));
		}

		public IEnumerable<List<MailDBItemDataLine>> GetNewReceivedMailItemsBySenderInBatches(string semicolonSeparatedEmailSenderList, int maxMailItems)
		{
			return GetNewReceivedMailItemsWithAdditionalWhereClauseInBatches(GetWhereClauseForMailItemsBySender(semicolonSeparatedEmailSenderList), maxMailItems);
		}

		int GetTotalNewReceivedMailItemsWithAdditionalWhereClause(string additionalWhereClause)
		{
			int total = 0;

			using (SqlConnection conn = GetMailDBConnection())
			{
				using (var sqlCmd = new SqlCommand(@"SELECT COUNT(*) FROM dbo.MailDBItems WHERE MI_Status = 'QUE' AND MI_DIRECTION = 'RCV' AND (" + additionalWhereClause + ")", conn))
				{
					total = (int)sqlCmd.ExecuteScalar();
				}
			}
			return total;
		}

		public IEnumerable<List<MailDBItemDataLine>> GetNewReceivedMailItemsWithAdditionalWhereClauseInBatches(string additionalWhereClause, int batchSize)
		{
			Guid? lastGuidProcessed = null;
			List<MailDBItemDataLine> mailItemsBatch = null;

			do
			{
				mailItemsBatch = new List<MailDBItemDataLine>();

				using (SqlConnection conn = GetMailDBConnection())
				using (SqlCommand sqlCmd = new SqlCommand(@"
	SELECT TOP " + batchSize + @" 
		MI_PK                  ,
		MI_Direction           ,
		MI_ReceivedDateTime    ,
		MI_SendDateTime        ,
		MI_LastAttemptDateTime ,
		MI_From                ,
		MI_Header              ,
		MI_Body
	FROM
		dbo.MailDBItems
	WHERE
		MI_Status = 'QUE' AND
		MI_Direction = 'RCV' AND 
		(" + additionalWhereClause + ")" +
		(lastGuidProcessed.HasValue ? " AND MI_PK > '" + lastGuidProcessed.Value + "'" : "") +
	"ORDER BY MI_PK ASC", conn))
				{
					using (SqlDataReader dr = sqlCmd.ExecuteReader())
					{
						while (dr.Read())
						{
							MailDBItemDataLine newItem = new MailDBItemDataLine();
							newItem.PrimaryKey = (Guid)dr["MI_PK"];
							newItem.Direction = dr["MI_Direction"].ToString();
							if (dr["MI_ReceivedDateTime"] != DBNull.Value)
							{
								newItem.ReceivedDateTime = (DateTime)dr["MI_ReceivedDateTime"];
							}

							if (dr["MI_SendDateTime"] != DBNull.Value)
							{
								newItem.SentDateTime = (DateTime)dr["MI_SendDateTime"];
							}

							if (dr["MI_LastAttemptDateTime"] != DBNull.Value)
							{
								newItem.LastAttemptDateTime = (DateTime)dr["MI_LastAttemptDateTime"];
							}

							newItem.From = dr["MI_From"].ToString();
							newItem.Header = dr["MI_Header"].ToString();
							newItem.Body = dr["MI_Body"].ToString();

							mailItemsBatch.Add(newItem);
						}
					}
				}

				if (mailItemsBatch.Count > 0)
				{
					lastGuidProcessed = mailItemsBatch.Last().PrimaryKey;
					yield return mailItemsBatch;
				}
			} while (mailItemsBatch.Count > 0);
		}

		public void MarkMailItemAsProcessed(MailDBItemDataLine aMailDBItemDataLine)
		{
			using (SqlConnection conn = GetMailDBConnection())
			{
				SqlCommand sqlCmd = new SqlCommand("UPDATE dbo.MailDBItems SET MI_Status = 'PRS' WHERE MI_PK = '" + aMailDBItemDataLine.PrimaryKey + "'", conn);
				sqlCmd.ExecuteNonQuery();

				conn.Close();
			}
		}

		public void UpdateLastAttemptDateTime(MailDBItemDataLine mailDBItem)
		{
			using (SqlConnection conn = GetMailDBConnection())
			using (SqlCommand sqlCmd = new SqlCommand("UPDATE dbo.MailDBItems SET MI_LastAttemptDateTime = @LastAttempt WHERE MI_PK = @MI_PK", conn))
			{
				sqlCmd.Parameters.AddWithValue("@LastAttempt", mailDBItem.LastAttemptDateTime);
				sqlCmd.Parameters.AddWithValue("@MI_PK", mailDBItem.PrimaryKey);
				sqlCmd.CommandTimeout = 240;
				sqlCmd.ExecuteNonQuery();
				conn.Close();
			}
		}

		public void GetMailDBItemTIFFAttachment(MailDBItemDataLine aMailDBItemDataLine)
		{
			using (SqlConnection conn = GetMailDBConnection())
			{
				SqlCommand sqlCmd = new SqlCommand(@"SELECT
                                                    MA_PK,
                                                    MA_FileName,
                                                    MA_Data,
                                                    MA_Encoding,
                                                    MA_MI
                                                FROM
                                                    dbo.MailDBAttachments
                                                WHERE
                                                    MA_MI = '" + aMailDBItemDataLine.PrimaryKey + "'", conn);

				SqlDataReader dr = sqlCmd.ExecuteReader(CommandBehavior.SequentialAccess);
				while (dr.Read())
				{
					MailDBAttachmentDataLine attachment = new MailDBAttachmentDataLine();
					attachment.PrimaryKey = (Guid)dr["MA_PK"];
					attachment.FileName = dr["MA_FileName"].ToString();
					long bytesize = dr.GetBytes(2, 0, null, 0, 0);
					attachment.Data = new byte[bytesize];
					long bytesread = 0;
					int chunkSize = 1024;
					int curpos = 0;
					while (bytesread < bytesize)
					{
						// chunkSize is an arbitrary application defined value 
						bytesread += dr.GetBytes(2, curpos, attachment.Data, curpos, Math.Min(chunkSize, (int)(bytesize - bytesread)));
						curpos += chunkSize;
					}
					attachment.Data = (byte[])ZCompressor.GetUncompressedVersion(attachment.Data, "MA_Data");
					attachment.Encoding = dr["MA_Encoding"].ToString();
					attachment.MailItemsPK = (Guid)dr["MA_MI"];
					aMailDBItemDataLine.Attachments.Add(attachment);
				}
				dr.Close();
				conn.Close();
			}
		}
	}
}
