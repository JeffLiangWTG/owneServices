using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Org
{
	class Report_ListofEXVDocumentsIntegrationTest : TransactionedTestCase
	{
		JobRequiredDocument SetupData(string docReceivedDate)
		{
			var factory = new BusinessObjectFactory();
			var orgHeader = factory.NewWithValidTestData<OrgHeader>();
			var jobRequiredDocument = factory.NewWithValidTestData<JobRequiredDocument>();
			jobRequiredDocument.EQ_ParentID = orgHeader.PK;
			jobRequiredDocument.EQ_ParentTableCode = orgHeader.TablePrefix;
			jobRequiredDocument.ParentType = typeof(OrgHeader);
			jobRequiredDocument.EQ_DocCategory = "CSR";
			jobRequiredDocument.EQ_DocType = "EXV";
			jobRequiredDocument.EQ_ValidToDate = new ZDateTime(2018, 1, 1);

			var attribute1 = factory.NewWithValidTestData<JobRequiredDocAttrib>();
			attribute1.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CompanyCode;
			attribute1.D0_AttribValue = GlbCompany.CurrentCompany.PK.ToString();
			attribute1.D0_EQ = jobRequiredDocument.PK;

			var attribute2 = factory.NewWithValidTestData<JobRequiredDocAttrib>();
			attribute2.D0_AttribName = JobRequiredDocAttribTypeList.Codes.DocumentReceivedDate;
			attribute2.D0_AttribValue = docReceivedDate;
			attribute2.D0_EQ = jobRequiredDocument.PK;

			factory.Save();
			return jobRequiredDocument;
		}

		void RunTest(string docReceivedDate, DateTime? expectedDateTime)
		{
			var jobRequiredDocument = SetupData(docReceivedDate);

			using (var command = Db.Connection.Command(
				@"SELECT DOCRECEIVEDDATE FROM 
Report_ListofEXVDocuments(@CurrentCompany, @DocumentExpiryDateFrom, @DocumentExpiryDateTo) 
ORDER BY OH_Code, EQ_ValidToDate"))
			{
				command.AddParameter("@CurrentCompany", System.Data.SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK.ToGuid());
				command.AddParameter("@DocumentExpiryDateFrom", System.Data.SqlDbType.DateTime, jobRequiredDocument.EQ_ValidToDate.ToDateTime());
				command.AddParameter("@DocumentExpiryDateTo", System.Data.SqlDbType.DateTime, jobRequiredDocument.EQ_ValidToDate.ToDateTime());

				var totalCount = 0;

				AssertNoExceptionThrown(() =>
				{
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							totalCount++;
							if (expectedDateTime != null)
							{
								AssertEquals(expectedDateTime, (DateTime)reader["DOCRECEIVEDDATE"]);
							}
							else
							{
								AssertType(typeof(DBNull), reader["DOCRECEIVEDDATE"]);
							}
						}
					}
				});

				AssertEquals(1, totalCount);
			}
		}

		public void TestNoErrorOccurWhenDocumentReceivedDateLargerThan2080()
		{
			RunTest("1-1-2080", new DateTime(2080, 1, 1));
		}

		public void TestWrongResultOnUncleanedEUDocReceivedDate()
		{
			// Test shows that without cleaning D0_AttribValue the SQL will convert under the assumption that the date is a US one
			RunTest("6/11/2018", new DateTime(2018, 6, 11));
		}

		public void TestNullResultOnUncleanedEUDocReceivedDate()
		{
			RunTest("27/12/2018", null); // Because without cleaning the data first, SQL assumes US datetime format
		}

		public void TestNullResultOnGarbageDateValue()
		{
			RunTest("this is gibberish and not a valid date", null);
		}

		public void TestNoErrorOnValidatedDocReveivedDate()
		{
			RunTest("27 DEC 18 00:00", new DateTime(2018, 12, 27));
		}
	}
}

