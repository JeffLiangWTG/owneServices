using System;
using System.Collections;
using System.Data;
using System.IO;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.FaxRouter.EventLogging;

namespace Enterprise.FaxRouter
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClassesAnalyzer", Justification = "This assembly has no need to change SqlConnection")]
	public class FaxDataModule : BaseDataModule
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		public void TiffFileDataTransform()
		{
			using (SqlConnection conn = GetEDIFaxDBConnection())
			{
				try
				{
					if (IsImageColumnType(conn))
					{
						if (IsNewColumnNotExist(conn))
						{
							using (SqlTransaction createColumnTransaction = conn.BeginTransaction())
							{
								try
								{
									string addNewColumnSql = @"ALTER TABLE FaxJobs ADD NewTiffFile varbinary(max) NULL";
									ExecuteSqlWithTransaction(addNewColumnSql, conn, createColumnTransaction);

									createColumnTransaction.Commit();
								}
								catch (Exception ex)
								{
									createColumnTransaction?.Rollback();
									EventLog.AddErrorEntry("Add NewTiffFile Column Error", ex);
									return;
								}
							}
						}

						using (SqlTransaction transformTransaction = conn.BeginTransaction())
						{
							try
							{
								string copyDataSql = @"UPDATE FaxJobs SET NewTiffFile = TiffFile";
								string dropOldColumnSql = @"ALTER TABLE FaxJobs DROP COLUMN TiffFile";
								string renameColumnSql = @"EXEC sp_rename 'FaxJobs.NewTiffFile', 'TiffFile', 'COLUMN'";

								ExecuteSqlWithTransaction(copyDataSql, conn, transformTransaction);
								ExecuteSqlWithTransaction(dropOldColumnSql, conn, transformTransaction);
								ExecuteSqlWithTransaction(renameColumnSql, conn, transformTransaction);

								transformTransaction.Commit();
							}
							catch (Exception ex)
							{
								transformTransaction?.Rollback();
								EventLog.AddErrorEntry("TiffFile DataTransform Error", ex);
							}
						}
					}
				}
				catch (Exception ex)
				{
					EventLog.AddErrorEntry("Select DataType Error", ex);
				}
				finally
				{
					conn.Close();
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		bool IsImageColumnType(SqlConnection connection)
		{
			string columnTypeQuery = @"SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'FaxJobs' AND COLUMN_NAME = 'TiffFile'";
			SqlCommand typeCmd = new SqlCommand(columnTypeQuery, connection);
			string columnType = (string)typeCmd.ExecuteScalar();

			return columnType == "image";
		}

		bool IsNewColumnNotExist(SqlConnection connection)
		{
			string checkColumnExistsQuery = @"SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'FaxJobs' AND COLUMN_NAME = 'NewTiffFile'";
			SqlCommand checkColumnCmd = new SqlCommand(checkColumnExistsQuery, connection);
			int columnCount = (int)checkColumnCmd.ExecuteScalar();

			return columnCount == 0;
		}

		void ExecuteSqlWithTransaction(string sql, SqlConnection connection, SqlTransaction transaction)
		{
			using (SqlCommand command = new SqlCommand(sql, connection, transaction))
			{
				command.ExecuteNonQuery();
			}
		}

		public void InsertFaxJob(EDIFaxDBJobDataLine aRecord)
		{
			using (SqlConnection conn = GetEDIFaxDBConnection())
			{
				string sqlCmdWithFaxJobId = @"INSERT INTO FaxJobs(FaxJobId, ChargeCode, ReceivedDateTime, Sender, TiffFile, PageCount, SysFaxJobId, SysId, EnterpriseCode, CompanyCode, ServerCode)
													VALUES(@FaxJobId, @ChargeCode, @ReceivedDateTime, @Sender, NULL, @PageCount, @SysFaxJobId, @SysId, @EnterpriseCode, @CompanyCode, @ServerCode)";
				string sqlCmdWithoutFaxJobId = @"INSERT INTO FaxJobs(FaxJobId, ChargeCode, ReceivedDateTime, Sender, TiffFile, PageCount, SysId, EnterpriseCode, CompanyCode, ServerCode) 
													VALUES(@FaxJobId, @ChargeCode, @ReceivedDateTime, @Sender, NULL, @PageCount, @SysId, @EnterpriseCode, @CompanyCode, @ServerCode)";
				string cmdString = sqlCmdWithFaxJobId;
				if (string.IsNullOrWhiteSpace(aRecord.SysFaxJobId))
				{
					cmdString = sqlCmdWithoutFaxJobId;
				}

				SqlCommand sqlCmd = new SqlCommand(cmdString, conn);

				SqlParameter[] parameters = {
												new SqlParameter("@FaxJobId"        , SqlDbType.UniqueIdentifier),
												new SqlParameter("@Sender"          , SqlDbType.VarChar),
												new SqlParameter("@ReceivedDateTime", SqlDbType.DateTime),
												new SqlParameter("@ChargeCode"      , SqlDbType.VarChar),
												new SqlParameter("@PageCount"       , SqlDbType.Int),
												new SqlParameter("@SysFaxJobId"     , SqlDbType.VarChar),
												new SqlParameter("@SysId"           , SqlDbType.VarChar),
												new SqlParameter("@EnterpriseCode"  , SqlDbType.VarChar),
												new SqlParameter("@CompanyCode"     , SqlDbType.VarChar),
												new SqlParameter("@ServerCode"      , SqlDbType.VarChar),
				};

				parameters[0].Value = aRecord.FaxJobId;
				parameters[1].Value = aRecord.Sender;
				parameters[2].Value = aRecord.ReceivedDateTime;
				parameters[3].Value = aRecord.ChargeCode;
				parameters[4].Value = aRecord.PageCount;
				parameters[5].Value = aRecord.SysFaxJobId;
				parameters[6].Value = aRecord.SysId;
				parameters[7].Value = aRecord.EnterpriseCode;
				parameters[8].Value = aRecord.CompanyCode;
				parameters[9].Value = aRecord.ServerCode;

				foreach (SqlParameter param in parameters)
				{
					sqlCmd.Parameters.Add(param);
				}

				sqlCmd.ExecuteNonQuery();
				conn.Close();
			}
		}

		public void SetTiffFile(Guid faxJobId, String tiffFilename)
		{
			FileStream fs = File.OpenRead(tiffFilename);
			byte[] tiffData = new Byte[fs.Length];
			try
			{
				fs.Read(tiffData, 0, (int)fs.Length);
			}
			finally
			{
				fs.Close();
			}
			SetTiffFile(faxJobId, tiffData);
		}

		public void SetTiffFile(Guid faxJobId, byte[] tiffData)
		{
			using (SqlConnection conn = GetEDIFaxDBConnection())
			{
				SqlCommand sqlCmd = new SqlCommand("UPDATE FaxJobs SET TiffFile = @TiffFile WHERE FaxJobId = @FaxJobId", conn);

				SqlParameter[] parameters = {
												new SqlParameter("@FaxJobId"        ,SqlDbType.UniqueIdentifier),
												new SqlParameter("@TiffFile"        ,SqlDbType.VarBinary),
				};

				parameters[0].Value = faxJobId;
				parameters[1].Value = tiffData;

				foreach (SqlParameter param in parameters)
				{
					sqlCmd.Parameters.Add(param);
				}

				sqlCmd.CommandTimeout = 240;
				sqlCmd.ExecuteNonQuery();
				conn.Close();
			}
		}

		public static byte[] GetTiffFile(Guid faxJobId)
		{
			byte[] result = Array.Empty<byte>();
			using (SqlConnection conn = GetEDIFaxDBConnection())
			{
				SqlCommand sqlCmd = new SqlCommand("SELECT TiffFile FROM FaxJobs WHERE FaxJobId = @FaxJobId", conn);

				SqlParameter[] parameters = {
												new SqlParameter("@FaxJobId"        ,SqlDbType.UniqueIdentifier),
				};

				parameters[0].Value = faxJobId;

				foreach (SqlParameter param in parameters)
				{
					sqlCmd.Parameters.Add(param);
				}

				SqlDataReader dr = sqlCmd.ExecuteReader();

				if (dr.Read() && dr["TiffFile"] != DBNull.Value && dr["TiffFile"] != null)
				{
					result = (byte[])dr["TiffFile"];
				}

				conn.Close();
			}
			return result;
		}

		public void InsertFaxRecipient(Guid aFaxJobId, EDIFaxDBRecipientDataLine aRecord)
		{
			using (SqlConnection conn = GetEDIFaxDBConnection())
			{
				SqlCommand sqlCmd = new SqlCommand(@"INSERT INTO FaxRecipients(FaxRecipientId , FaxJobId , SentDateTime , FaxNumber , Company , AttentionName , IsAcknowledged) 
												                    VALUES(@FaxRecipientId, @FaxJobId, @SentDateTime, @FaxNumber, @Company, @AttentionName, @IsAcknowledged)", conn);

				SqlParameter[] parameters = {
												new SqlParameter("@FaxRecipientId"      , SqlDbType.UniqueIdentifier),
												new SqlParameter("@FaxJobId"            , SqlDbType.UniqueIdentifier),
												new SqlParameter("@FaxNumber"           , SqlDbType.VarChar),
												new SqlParameter("@Company"             , SqlDbType.VarChar),
												new SqlParameter("@AttentionName"       , SqlDbType.VarChar),
												new SqlParameter("@SentDateTime"        , SqlDbType.DateTime),
												new SqlParameter("@IsAcknowledged"      , SqlDbType.Int),
				};

				parameters[0].Value = aRecord.FaxRecipientId;
				parameters[1].Value = aFaxJobId;
				parameters[2].Value = aRecord.FaxNumber;
				parameters[3].Value = aRecord.Company;
				parameters[4].Value = aRecord.AttentionName;
				if (!aRecord.SentDateTime.Equals(new DateTime(1900, 1, 1)))
				{
					parameters[5].Value = aRecord.SentDateTime;
				}
				else
				{
					parameters[5].Value = DBNull.Value;
				}

				if (aRecord.IsConfirmed)
				{
					parameters[6].Value = 1;
				}
				else
				{
					parameters[6].Value = 0;
				}

				foreach (SqlParameter param in parameters)
				{
					sqlCmd.Parameters.Add(param);
				}

				sqlCmd.ExecuteNonQuery();
				conn.Close();
			}
		}

		public static string GetSysFaxJobId(string aFaxRecipientId)
		{
			object result = null;

			using (SqlConnection conn = GetEDIFaxDBConnection())
			{
				SqlCommand sqlCmd = new SqlCommand(@"SELECT SysFaxJobId FROM
													FaxRecipients
													JOIN	
													FaxJobs ON FaxRecipients.FaxJobId = FaxJobs.FaxJobId
													WHERE FaxRecipients.FaxRecipientId = '" + aFaxRecipientId + "'", conn);
				result = sqlCmd.ExecuteScalar();
				conn.Close();
			}
			if (result != null)
			{
				return result.ToString();
			}
			return "";
		}

		public void MarkRecipientJobAsSent(Guid recipientId)
		{
			using (SqlConnection conn = GetEDIFaxDBConnection())
			{
				SqlCommand sqlcmd = new SqlCommand("update faxrecipients set sentdatetime = getdate() where faxrecipientid = '" + recipientId.ToString() + "'", conn);
				sqlcmd.ExecuteNonQuery();
				conn.Close();
			}
		}

		public static void UpdateFaxRecipientWithAck(String aRecipientId, int aAckSuccessStatus)
		{
			using (SqlConnection conn = GetEDIFaxDBConnection())
			{
				SqlCommand sqlcmd = new SqlCommand(@"UPDATE FaxRecipients 					
					SET AckDateTime = GETDATE(),
					IsAcknowledged = 1,
					AckSuccess = " + aAckSuccessStatus + " WHERE FaxRecipientid = '" + aRecipientId + "'", conn);
				sqlcmd.ExecuteNonQuery();
				conn.Close();
			}
		}

		public static ArrayList GetFaxManagerDataTable(String aSearchParameter)
		{
			ArrayList result = new ArrayList();
			using (SqlConnection conn = GetEDIFaxDBConnection())
			{
				SqlCommand sqlCmd = new SqlCommand(@"SELECT 
													FaxJobs.FaxJobId As FaxJobId,
													ReceivedDateTime,
													ChargeCode,
													PageCount,
													FaxRecipientId,
													SentDateTime,
													AttentionName,
													FaxNumber,
													Company,
													AckDateTime,
													AckSuccess
													FROM FaxJobs
													LEFT JOIN 
													FaxRecipients ON FaxJobs.FaxJobId = FaxRecipients.FaxJobId
													WHERE LTRIM(FaxJobs.ChargeCode) = '" + aSearchParameter +
													"' OR FaxJobs.FaxJobId like '%" + aSearchParameter + "%' OR FaxRecipientId like '%" + aSearchParameter + "%' ORDER BY ReceivedDateTime DESC", conn);

				SqlDataReader dr = sqlCmd.ExecuteReader();

				while (dr.Read())
				{
					MailDBItemDataLine aMailDBItemDataLine = new MailDBItemDataLine();
					aMailDBItemDataLine.PrimaryKey = (Guid)dr["FaxJobId"];
					aMailDBItemDataLine.ReceivedDateTime = (DateTime)dr["ReceivedDateTime"];
					aMailDBItemDataLine.ChargeCode = dr["ChargeCode"].ToString();
					aMailDBItemDataLine.PageCount = Int32.Parse(dr["PageCount"].ToString());
					aMailDBItemDataLine.FaxRecipientId = (Guid)dr["FaxRecipientId"];
					aMailDBItemDataLine.FaxRecipientAttentionName = dr["AttentionName"].ToString().Trim();
					aMailDBItemDataLine.FaxRecipientNumber = dr["FaxNumber"].ToString().Trim();
					aMailDBItemDataLine.FaxRecipientCompany = dr["Company"].ToString();

					if (dr["AckDateTime"] != DBNull.Value)
					{
						aMailDBItemDataLine.FaxRecipientAckDateTime = (DateTime)dr["AckDateTime"];
					}

					if (dr["AckSuccess"] != DBNull.Value)
					{
						aMailDBItemDataLine.FaxRecipientAckSuccess = dr["AckSuccess"].ToString();
					}

					if (dr["SentDateTime"] != DBNull.Value)
					{
						aMailDBItemDataLine.FaxRecipientSentDateTime = (DateTime)dr["SentDateTime"];
					}

					result.Add(aMailDBItemDataLine);
				}

				dr.Close();
				conn.Close();
			}

			return result;
		}

		public static ArrayList GetFaxManagerDataTable()
		{
			ArrayList result = new ArrayList();
			using (SqlConnection conn = GetEDIFaxDBConnection())
			{
				SqlCommand sqlCmd = new SqlCommand(@"SELECT TOP 100
													FaxJobs.FaxJobId As FaxJobId,
													ReceivedDateTime,
													ChargeCode,
													PageCount,
													FaxRecipientId,
													SentDateTime,
													AttentionName,
													FaxNumber,
													Company,
													AckDateTime,
													AckSuccess
													FROM FaxJobs
													LEFT JOIN 
													FaxRecipients ON FaxJobs.FaxJobId = FaxRecipients.FaxJobId
													ORDER BY ReceivedDateTime DESC", conn);

				SqlDataReader dr = sqlCmd.ExecuteReader();

				while (dr.Read())
				{
					MailDBItemDataLine aMailDBItemDataLine = new MailDBItemDataLine();
					aMailDBItemDataLine.PrimaryKey = (Guid)dr["FaxJobId"];
					aMailDBItemDataLine.ReceivedDateTime = (DateTime)dr["ReceivedDateTime"];
					aMailDBItemDataLine.ChargeCode = dr["ChargeCode"].ToString();
					aMailDBItemDataLine.PageCount = Int32.Parse(dr["PageCount"].ToString());
					aMailDBItemDataLine.FaxRecipientId = (Guid)dr["FaxRecipientId"];
					aMailDBItemDataLine.FaxRecipientAttentionName = dr["AttentionName"].ToString().Trim();
					aMailDBItemDataLine.FaxRecipientNumber = dr["FaxNumber"].ToString().Trim();
					aMailDBItemDataLine.FaxRecipientCompany = dr["Company"].ToString();

					if (dr["AckDateTime"] != DBNull.Value)
					{
						aMailDBItemDataLine.FaxRecipientAckDateTime = (DateTime)dr["AckDateTime"];
					}

					if (dr["AckSuccess"] != DBNull.Value)
					{
						aMailDBItemDataLine.FaxRecipientAckSuccess = dr["AckSuccess"].ToString();
					}

					if (dr["SentDateTime"] != DBNull.Value)
					{
						aMailDBItemDataLine.FaxRecipientSentDateTime = (DateTime)dr["SentDateTime"];
					}

					result.Add(aMailDBItemDataLine);
				}

				dr.Close();
				conn.Close();
			}

			return result;
		}

		public static ArrayList GetNextFaxManagerDataTable(String aMinDateTime)
		{
			ArrayList result = new ArrayList();
			using (SqlConnection conn = GetEDIFaxDBConnection())
			{
				SqlCommand sqlCmd = new SqlCommand(@"SELECT TOP 100	FaxJobs.FaxJobId As FaxJobId,
													ReceivedDateTime,
													ChargeCode,
													PageCount,
													FaxRecipientId,
													SentDateTime,
													AttentionName,
													FaxNumber,
													Company,
													AckDateTime,
													AckSuccess
													FROM FaxJobs
													LEFT JOIN 
													FaxRecipients ON FaxJobs.FaxJobId = FaxRecipients.FaxJobId
													WHERE ReceivedDateTime < '" + aMinDateTime + "' ORDER BY ReceivedDateTime DESC", conn);

				SqlDataReader dr = sqlCmd.ExecuteReader();

				while (dr.Read())
				{
					MailDBItemDataLine aMailDBItemDataLine = new MailDBItemDataLine();
					aMailDBItemDataLine.PrimaryKey = (Guid)dr["FaxJobId"];
					aMailDBItemDataLine.ReceivedDateTime = (DateTime)dr["ReceivedDateTime"];
					aMailDBItemDataLine.ChargeCode = dr["ChargeCode"].ToString();
					aMailDBItemDataLine.PageCount = Int32.Parse(dr["PageCount"].ToString());
					aMailDBItemDataLine.FaxRecipientId = (Guid)dr["FaxRecipientId"];
					aMailDBItemDataLine.FaxRecipientAttentionName = dr["AttentionName"].ToString().Trim();
					aMailDBItemDataLine.FaxRecipientNumber = dr["FaxNumber"].ToString().Trim();
					aMailDBItemDataLine.FaxRecipientCompany = dr["Company"].ToString();

					if (dr["AckDateTime"] != DBNull.Value)
					{
						aMailDBItemDataLine.FaxRecipientAckDateTime = (DateTime)dr["AckDateTime"];
					}

					if (dr["AckSuccess"] != DBNull.Value)
					{
						aMailDBItemDataLine.FaxRecipientAckSuccess = dr["AckSuccess"].ToString();
					}

					if (dr["SentDateTime"] != DBNull.Value)
					{
						aMailDBItemDataLine.FaxRecipientSentDateTime = (DateTime)dr["SentDateTime"];
					}

					result.Add(aMailDBItemDataLine);
				}

				dr.Close();
				conn.Close();
			}

			return result;
		}

		public static ArrayList GetNextFaxManagerDataTable(String aMinDateTime, String aMaxDateTime)
		{
			ArrayList result = new ArrayList();
			using (SqlConnection conn = GetEDIFaxDBConnection())
			{
				SqlCommand sqlCmd = new SqlCommand(@"SELECT TOP 100	FaxJobs.FaxJobId As FaxJobId,
													ReceivedDateTime,
													ChargeCode,
													PageCount,
													FaxRecipientId,
													SentDateTime,
													AttentionName,
													FaxNumber,
													Company,
													AckDateTime,
													AckSuccess
													FROM FaxJobs
													LEFT JOIN 
													FaxRecipients ON FaxJobs.FaxJobId = FaxRecipients.FaxJobId
													WHERE ReceivedDateTime BETWEEN '" + aMinDateTime + "' AND '" + aMaxDateTime + "' ORDER BY ReceivedDateTime DESC", conn);

				SqlDataReader dr = sqlCmd.ExecuteReader();

				while (dr.Read())
				{
					MailDBItemDataLine aMailDBItemDataLine = new MailDBItemDataLine();
					aMailDBItemDataLine.PrimaryKey = (Guid)dr["FaxJobId"];
					aMailDBItemDataLine.ReceivedDateTime = (DateTime)dr["ReceivedDateTime"];
					aMailDBItemDataLine.ChargeCode = dr["ChargeCode"].ToString();
					aMailDBItemDataLine.PageCount = Int32.Parse(dr["PageCount"].ToString());
					aMailDBItemDataLine.FaxRecipientId = (Guid)dr["FaxRecipientId"];
					aMailDBItemDataLine.FaxRecipientAttentionName = dr["AttentionName"].ToString().Trim();
					aMailDBItemDataLine.FaxRecipientNumber = dr["FaxNumber"].ToString().Trim();
					aMailDBItemDataLine.FaxRecipientCompany = dr["Company"].ToString();

					if (dr["AckDateTime"] != DBNull.Value)
					{
						aMailDBItemDataLine.FaxRecipientAckDateTime = (DateTime)dr["AckDateTime"];
					}

					if (dr["AckSuccess"] != DBNull.Value)
					{
						aMailDBItemDataLine.FaxRecipientAckSuccess = dr["AckSuccess"].ToString();
					}

					if (dr["SentDateTime"] != DBNull.Value)
					{
						aMailDBItemDataLine.FaxRecipientSentDateTime = (DateTime)dr["SentDateTime"];
					}

					result.Add(aMailDBItemDataLine);
				}

				dr.Close();
				conn.Close();
			}

			return result;
		}

		public static bool IsValidFaxRecipientJob(String aFaxRecipientId)
		{
			int result = 0;
			if (IsValidGuid(aFaxRecipientId))
			{
				using (SqlConnection conn = GetEDIFaxDBConnection())
				{
					SqlCommand sqlCmd = new SqlCommand(@"SELECT COUNT(*) FROM FaxRecipients WHERE FaxRecipientId = '" + aFaxRecipientId + "'", conn);

					result = Int32.Parse(sqlCmd.ExecuteScalar().ToString());
					conn.Close();
				}
			}
			return result > 0;
		}

		static internal bool IsValidGuid(string potentialGuid)
		{
			bool result;
			try
			{
				Guid faxRecipientGuid = new Guid(potentialGuid);
				result = true;
			}
			catch (FormatException)
			{
				result = false;
			}
			return result;
		}

		public static void InsertFaxAcknowledgement(FaxAcknowledgementDataLine aRecord)
		{
			using (SqlConnection conn = GetEDIFaxDBConnection())
			{
				SqlCommand sqlCmd = new SqlCommand(@"INSERT INTO FaxAcknowledgements
													(AckId,
													AckBody,
													AckChargeCode,
													AckReceivedDateTime,
													FaxReciepientId)
													VALUES
													(@AckId, 
													@AckBody, 
													@AckChargeCode, 
													GetDate(),													
													@FaxReciepientId)", conn);

				SqlParameter[] parameters = {
												new SqlParameter("@AckId"           , SqlDbType.VarChar),
												new SqlParameter("@AckBody"         , SqlDbType.Text),
												new SqlParameter("@AckChargeCode"   , SqlDbType.VarChar),
												new SqlParameter("@FaxReciepientId" , SqlDbType.VarChar),
											};

				parameters[0].Value = aRecord.AckId;
				parameters[1].Value = aRecord.AckBody;
				parameters[2].Value = aRecord.AckChargeCode;
				parameters[3].Value = aRecord.FaxReciepientId;

				foreach (SqlParameter param in parameters)
				{
					sqlCmd.Parameters.Add(param);
				}

				sqlCmd.ExecuteNonQuery();
				conn.Close();
			}
		}

		public static ArrayList GetOverDueAckFaxes(bool aIsAckWarningSent, int aDays, int aHours)
		{
			ArrayList result = new ArrayList();
			using (SqlConnection conn = GetEDIFaxDBConnection())
			{
				SqlCommand sqlCmd = new SqlCommand(@"SELECT 
													FaxJobs.ChargeCode As TrackingNumber,
													FaxRecipientId,
													SentDateTime,
													AttentionName,
													FaxNumber,
													Company
													FROM FaxRecipients 
													JOIN FaxJobs ON FaxRecipients.FaxJobId = FaxJobs.FaxJobId
													WHERE 
													SentDateTime BETWEEN DATEADD(DAY, - @Days, GETDATE()) AND DATEADD(HOUR, - @Hours, GETDATE())
													AND
													AckSuccess IS NULL
													AND 
													IsAckWarningSent = @IsAckWarningSent ORDER BY SentDateTime", conn);

				SqlParameter[] parameters =
				{
					new SqlParameter("@Days"                , SqlDbType.Int),
					new SqlParameter("@Hours"               , SqlDbType.Int),
					new SqlParameter("@IsAckWarningSent"    , SqlDbType.Int),
				};

				parameters[0].Value = aDays;
				parameters[1].Value = aHours;
				parameters[2].Value = 0;

				if (aIsAckWarningSent)
				{
					parameters[2].Value = 1;
				}

				foreach (SqlParameter param in parameters)
				{
					sqlCmd.Parameters.Add(param);
				}

				SqlDataReader dr = sqlCmd.ExecuteReader();

				while (dr.Read())
				{
					EDIFaxDBRecipientDataLine aEDIFaxDBRecipientDataLine = new EDIFaxDBRecipientDataLine();
					aEDIFaxDBRecipientDataLine.ChargeCode = dr["TrackingNumber"].ToString();
					aEDIFaxDBRecipientDataLine.FaxRecipientId = (Guid)dr["FaxRecipientId"];
					aEDIFaxDBRecipientDataLine.SentDateTime = (DateTime)dr["SentDateTime"];
					aEDIFaxDBRecipientDataLine.AttentionName = dr["AttentionName"].ToString();
					aEDIFaxDBRecipientDataLine.FaxNumber = dr["FaxNumber"].ToString();
					aEDIFaxDBRecipientDataLine.Company = dr["Company"].ToString();
					result.Add(aEDIFaxDBRecipientDataLine);
				}
			}

			return result;
		}

		public static void OverDueAckFaxWarningSent(Guid aRecipientId)
		{
			using (SqlConnection conn = GetEDIFaxDBConnection())
			{
				SqlCommand sqlcmd = new SqlCommand(@"UPDATE FaxRecipients 					
					SET IsAckWarningSent = 1 WHERE FaxRecipientid = '" + aRecipientId.ToString() + "'", conn);
				sqlcmd.ExecuteNonQuery();
				conn.Close();
			}
		}

		public static MailDBItemDataLine GetFaxRecord(String aFaxRecipientId)
		{
			MailDBItemDataLine result = new MailDBItemDataLine();

			using (SqlConnection conn = GetEDIFaxDBConnection())
			{
				SqlCommand sqlCmd = new SqlCommand(@"SELECT 
													FaxJobs.FaxJobId As FaxJobId,
													ReceivedDateTime,
													Sender,
													ChargeCode,
													PageCount,
													FaxRecipientId,
													SentDateTime,
													AttentionName,
													FaxNumber,
													Company,
													SysFaxJobId,
													SysId,
													ReportedLicenceHeader,
													GETDATE() AS AckDateTime											
													FROM FaxRecipients
													JOIN FaxJobs ON FaxJobs.FaxJobId = FaxRecipients.FaxJobId
													WHERE FaxRecipientId = '" + aFaxRecipientId + "'", conn);

				SqlDataReader dr = sqlCmd.ExecuteReader();

				if (dr.Read())
				{
					result.PrimaryKey = (Guid)dr["FaxJobId"];
					result.ReceivedDateTime = (DateTime)dr["ReceivedDateTime"];
					result.From = dr["Sender"].ToString();
					result.ChargeCode = dr["ChargeCode"].ToString();
					result.PageCount = Int32.Parse(dr["PageCount"].ToString());
					result.FaxRecipientId = (Guid)dr["FaxRecipientId"];
					if (dr["SentDateTime"] is DateTime)
					{
						result.FaxRecipientSentDateTime = (DateTime)dr["SentDateTime"];
					}

					result.FaxRecipientAttentionName = dr["AttentionName"].ToString().Trim();
					result.FaxRecipientNumber = dr["FaxNumber"].ToString().Trim();
					result.FaxRecipientCompany = dr["Company"].ToString();
					result.FaxRecipientAckDateTime = (DateTime)dr["AckDateTime"];
					result.SysFaxJobId = dr["SysFaxJobId"].ToString();
					result.SysId = dr["SysId"].ToString();
					if (dr["ReportedLicenceHeader"] is Guid)
					{
						result.ReportedLicenceHeader = (Guid)dr["ReportedLicenceHeader"];
					}
				}

				dr.Close();
				conn.Close();
			}

			return result;
		}

		public static bool ExceptionCausedByNetworkProblems(Exception ex)
		{
			SqlException sqlEx = ex as SqlException;
			if (sqlEx != null)
			{
				DbErrorHandler errorHandler = new DbErrorHandler(sqlEx, null);
				try
				{
					DbErrorType errorType = errorHandler.ExceptionType;
					return errorType == DbErrorType.GeneralNetworkError;
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}
	}
}
