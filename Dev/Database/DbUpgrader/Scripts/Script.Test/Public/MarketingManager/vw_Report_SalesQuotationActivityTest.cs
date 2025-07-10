using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MarketingManager;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MarketingManager
{
	[TestedType(typeof(vw_Report_SalesQuotationActivity))]
	class vw_Report_SalesQuotationActivityTest : DbCreateScriptTest
	{
		public void TestAllQuotationCasesWithSalesReps()
		{
			var companyPk = Guid.NewGuid();
			InsertCompany(companyPk, "TST", "AU", "AUD");

			var branchPk = Guid.NewGuid();
			InsertBranch(branchPk, companyPk);

			var departmentPk = Guid.Parse("57F778C1-DAF6-46E0-B7DC-AF01C161C936");

			var orgPk = Guid.NewGuid();
			var emptyOrgPk = Guid.NewGuid();
			InsertOrg(orgPk, "TSTORG");
			InsertOrg(emptyOrgPk, "EMPORG");

			InsertSalesRep(Guid.NewGuid(), "TP", "Test Person", true);
			InsertSalesRep(Guid.NewGuid(), "AP", "All Person", false);

			InsertOrgStaffAssignment(Guid.NewGuid(), "FES", companyPk, "TP", orgPk, "SAL");
			InsertOrgStaffAssignment(Guid.NewGuid(), "ALL", companyPk, "AP", orgPk, "SAL");

			DateTime? GetEndDate(int i)
			{
				switch (i)
				{
					case 5:
						return DateTime.Today;
					case 6:
						return DateTime.Today.AddHours(-26);
					case 7:
						return DateTime.Today.AddHours(1);

					default:
						return null;
				}
			}

			var ratingHeaderPks = new Guid[8];
			for (int i = 0; i < 8; i++)
			{
				ratingHeaderPks[i] = Guid.NewGuid();
				var endDate = GetEndDate(i);
				if (endDate.HasValue)
				{
					InsertRatingHeader(ratingHeaderPks[i], string.Format("{0:00000000}", i + 1), companyPk, endDate.Value);
				}
				else
				{
					InsertRatingHeader(ratingHeaderPks[i], string.Format("{0:00000000}", i + 1), companyPk);
				}
			}

			var orgAddressPk = Guid.NewGuid();
			var emptyOrgAddressPk = Guid.NewGuid();
			InsertOrgAdress(orgAddressPk, orgPk, "1 Test Rd");
			InsertOrgAdress(emptyOrgAddressPk, emptyOrgPk, "2 Empty Rd");

			InsertOrgCompanyData(Guid.NewGuid(), orgPk, companyPk);
			InsertOrgCompanyData(Guid.NewGuid(), emptyOrgPk, companyPk);

			InsertJobDocAddress(Guid.NewGuid(), orgAddressPk, ratingHeaderPks[0], "TH");
			InsertJobDocAddress(Guid.NewGuid(), orgAddressPk, ratingHeaderPks[1], "TH");
			InsertJobDocAddress(Guid.NewGuid(), orgAddressPk, ratingHeaderPks[2], "TH");
			InsertJobDocAddress(Guid.NewGuid(), orgAddressPk, ratingHeaderPks[3], "TH");
			InsertJobDocAddress(Guid.NewGuid(), emptyOrgAddressPk, ratingHeaderPks[4], "TH");
			InsertJobDocAddress(Guid.NewGuid(), orgAddressPk, ratingHeaderPks[5], "TH");
			InsertJobDocAddress(Guid.NewGuid(), orgAddressPk, ratingHeaderPks[6], "TH");
			InsertJobDocAddress(Guid.NewGuid(), orgAddressPk, ratingHeaderPks[7], "TH");

			InsertQuote(Guid.NewGuid(), "00000001", "TP", departmentPk, branchPk, companyPk, ratingHeaderPks[0]);
			InsertQuote(Guid.NewGuid(), "00000002", departmentPk, branchPk, companyPk, ratingHeaderPks[1]);
			InsertQuote(Guid.NewGuid(), "SEA", "SEA", "AUSYD", "USORD", ratingHeaderPks[2]);
			InsertQuote(Guid.NewGuid(), "ROA", "ROA", "AUSYD", "AUBNE", ratingHeaderPks[3]);
			InsertQuote(Guid.NewGuid(), "SEA", "SEA", "AUSYD", "USORD", ratingHeaderPks[4]);
			InsertQuote(Guid.NewGuid(), "SEA", "SEA", "AUSYD", "USLAX", ratingHeaderPks[5]);
			InsertQuote(Guid.NewGuid(), "ROA", "ROA", "AUPER", "AUMEL", ratingHeaderPks[6]);
			InsertQuote(Guid.NewGuid(), "ROA", "ROA", "AUPER", "AUSYD", ratingHeaderPks[7]);

			using (DbCommand command = TestConnection.Command("select * from dbo.vw_Report_SalesQuotationActivity where CompanyPK = @CompanyPK"))
			{
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyPk);
				DataTable result = new DataTable();
				result.Load(command.ExecuteReader());

				var rows = result.Rows;
				AssertEquals("Pre-condition: should return all quotes", 8, rows.Count);
				AssertSalesRep(rows, "00000001", "TP", "Has assigned Sales rep in Job Header");
				AssertSalesRep(rows, "00000002", "TP", "Has assigned Department in Job Header");
				AssertSalesRep(rows, "00000003", "TP", "No Job Header but Transport Mode and Origin/Destination can link it to a Department");
				AssertSalesRep(rows, "00000004", "AP", "Nothing to link it to a Department so defaults to Sales Rep for 'ALL' Department");
				AssertSalesRep(rows, "00000005", "", "Cannot find any valid Sales Rep");
				AssertQuoteStatus(rows, "00000006", "Active", "Quote End Date is Today and so this is not yet expired");
				AssertQuoteStatus(rows, "00000007", "Expired", "Quote has been expired yesterday");
				AssertQuoteStatus(rows, "00000008", "Active", "Quote End Date is Today and so this is not yet expired");
			}
		}

		static void AssertSalesRep(DataRowCollection rows, string quoteNumber, string expected, string message)
		{
			var wasRowMatched = false;

			foreach (var row in rows.Cast<DataRow>().Where(row => row["QuoteNumber"].ToString() == quoteNumber))
			{
				AssertEquals(message, expected, row["SalesRepInitials"]);
				wasRowMatched = true;
			}

			Assert("Row was expected but not found for " + quoteNumber, wasRowMatched);
		}

		static void AssertQuoteStatus(DataRowCollection rows, string quoteNumber, string expectedstatus, string message)
		{
			var wasRowMatched = false;

			foreach (var row in rows.Cast<DataRow>().Where(row => row["QuoteNumber"].ToString() == quoteNumber))
			{
				AssertEquals(message, expectedstatus, row["QuoteStatus"]);
				wasRowMatched = true;
			}

			Assert("Row was expected but not found for " + quoteNumber, wasRowMatched);
		}

		#region Implementation

		void InsertCompany(Guid gC_PK, string gC_Code, string gC_RN_NKCountryCode, string gC_RX_NKLocalCurrency)
		{
			string query = string.Format("INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}) VALUES ",
				GlbCompanySchema.Constants.TableName,
				GlbCompanySchema.PK.Name,
				GlbCompanySchema.GC_Code.Name,
				GlbCompanySchema.GC_Name.Name,
				GlbCompanySchema.GC_RN_NKCountryCode.Name,
				GlbCompanySchema.GC_RX_NKLocalCurrency.Name);

			using (DbCommand command = TestConnection.Command(query + "(@GC_PK, @GC_Code, 'Company', @GC_RN_NKCountryCode, @GC_RX_NKLocalCurrency)"))
			{
				command.AddParameter("@GC_PK", SqlDbType.UniqueIdentifier, gC_PK);
				command.AddParameter("@GC_Code", SqlDbType.VarChar, gC_Code);
				command.AddParameter("@GC_RN_NKCountryCode", SqlDbType.VarChar, gC_RN_NKCountryCode);
				command.AddParameter("@GC_RX_NKLocalCurrency", SqlDbType.VarChar, gC_RX_NKLocalCurrency);
				command.ExecuteNonQuery();
			}
		}

		void InsertBranch(Guid gB_PK, Guid gB_GC)
		{
			string query = string.Format("INSERT INTO {0} ({1}, {2}) VALUES ",
				GlbBranchSchema.Constants.TableName,
				GlbBranchSchema.PK.Name,
				GlbBranchSchema.GB_GC.Name);

			using (DbCommand command = TestConnection.Command(query + "(@GB_PK, @GB_GC)"))
			{
				command.AddParameter("@GB_PK", SqlDbType.UniqueIdentifier, gB_PK);
				command.AddParameter("@GB_GC", SqlDbType.UniqueIdentifier, gB_GC);
				command.ExecuteNonQuery();
			}
		}

		void InsertOrg(Guid oH_PK, string oH_Code)
		{
			string query = string.Format("INSERT INTO {0} ({1}, {2}) VALUES ",
				OrgHeaderSchema.Constants.TableName,
				OrgHeaderSchema.PK.Name,
				OrgHeaderSchema.OH_Code.Name);

			using (DbCommand command = TestConnection.Command(query + "(@OH_PK, @OH_Code)"))
			{
				command.AddParameter("@OH_PK", SqlDbType.UniqueIdentifier, oH_PK);
				command.AddParameter("@OH_Code", SqlDbType.VarChar, oH_Code);
				command.ExecuteNonQuery();
			}
		}

		void InsertSalesRep(Guid gS_PK, string gS_Code, string gS_FullName, bool gS_IsActive)
		{
			var personPk = Guid.NewGuid();
			var queryPerson = string.Format("insert into dbo.GlbPerson (PER_PK, PER_FullName) values (@PER_PK, 'name')");
			using (DbCommand command = TestConnection.Command(queryPerson))
			{
				command.AddParameterBasedOnDbColumn("@PER_PK", personPk, GlbPersonSchema.PK);
				command.ExecuteNonQuery();
			}

			string query = string.Format("INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}) VALUES ",
				GlbStaffSchema.Constants.TableName,
				GlbStaffSchema.PK.Name,
				GlbStaffSchema.GS_Code.Name,
				GlbStaffSchema.GS_LoginName.Name,
				GlbStaffSchema.GS_FullName.Name,
				GlbStaffSchema.GS_IsActive.Name,
				GlbStaffSchema.GS_PER.Name,
				GlbStaffSchema.Constants.GS_SystemCreateTimeUtc,
				GlbStaffSchema.Constants.GS_SystemCreateUser,
				GlbStaffSchema.Constants.GS_SystemLastEditTimeUtc,
				GlbStaffSchema.Constants.GS_SystemLastEditUser);

			using (DbCommand command = TestConnection.Command(query + "(@GS_PK, @GS_Code, @GS_LoginName, @GS_FullName, @GS_IsActive, @GS_PER, GetUtcDate(), '~BP', GetUtcDate(), '~BP')"))
			{
				command.AddParameter("@GS_PK", SqlDbType.UniqueIdentifier, gS_PK);
				command.AddParameter("@GS_Code", SqlDbType.VarChar, gS_Code);
				command.AddParameter("@GS_LoginName", SqlDbType.VarChar, gS_Code);
				command.AddParameter("@GS_FullName", SqlDbType.VarChar, gS_FullName);
				command.AddParameter("@GS_IsActive", SqlDbType.Bit, gS_IsActive ? 1 : 0);
				command.AddParameter("@GS_PER", SqlDbType.UniqueIdentifier, personPk);
				command.ExecuteNonQuery();
			}
		}

		void InsertOrgStaffAssignment(Guid o8_PK, string o8_Department, Guid o8_GC, string o8_GS_NKPersonResponsible, Guid o8_OH, string o8_Role)
		{
			string query = string.Format("INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}, {6}) VALUES ",
				OrgStaffAssignmentsSchema.Constants.TableName,
				OrgStaffAssignmentsSchema.PK.Name,
				OrgStaffAssignmentsSchema.O8_Department.Name,
				OrgStaffAssignmentsSchema.O8_GC.Name,
				OrgStaffAssignmentsSchema.O8_GS_NKPersonResponsible.Name,
				OrgStaffAssignmentsSchema.O8_OH.Name,
				OrgStaffAssignmentsSchema.O8_Role.Name);

			using (DbCommand command = TestConnection.Command(query + "(@O8_PK, @O8_Department, @O8_GC, @O8_GS_NKPersonResponsible, @O8_OH, @O8_Role)"))
			{
				command.AddParameter("@O8_PK", SqlDbType.UniqueIdentifier, o8_PK);
				command.AddParameter("@O8_Department", SqlDbType.VarChar, o8_Department);
				command.AddParameter("@O8_GC", SqlDbType.UniqueIdentifier, o8_GC);
				command.AddParameter("@O8_GS_NKPersonResponsible", SqlDbType.VarChar, o8_GS_NKPersonResponsible);
				command.AddParameter("@O8_OH", SqlDbType.UniqueIdentifier, o8_OH);
				command.AddParameter("@O8_Role", SqlDbType.VarChar, o8_Role);
				command.ExecuteNonQuery();
			}
		}

		void InsertQuote(Guid jH_PK, string jH_JobNum, string jH_GS_NKRepSales, Guid jH_GE, Guid jH_GB, Guid jH_GC, Guid jH_ParentID)
		{
			string query = string.Format("INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}) VALUES ",
				JobHeaderSchema.Constants.TableName,
				JobHeaderSchema.PK.Name,
				JobHeaderSchema.JH_JobNum.Name,
				JobHeaderSchema.JH_GS_NKRepSales.Name,
				JobHeaderSchema.JH_GE.Name,
				JobHeaderSchema.JH_GB.Name,
				JobHeaderSchema.JH_GC.Name,
				JobHeaderSchema.JH_ParentID.Name,
				JobHeaderSchema.JH_ParentTableCode.Name,
				JobHeaderSchema.JH_Status.Name);

			using (DbCommand command = TestConnection.Command(query + "(@JH_PK, @JH_JobNum, @JH_GS_NKRepSales, @JH_GE, @JH_GB, @JH_GC, @JH_ParentID, @JH_ParentTableCode, @JH_Status)"))
			{
				command.AddParameter("@JH_PK", SqlDbType.UniqueIdentifier, jH_PK);
				command.AddParameter("@JH_JobNum", SqlDbType.VarChar, jH_JobNum);
				command.AddParameter("@JH_GS_NKRepSales", SqlDbType.VarChar, jH_GS_NKRepSales);
				command.AddParameter("@JH_GE", SqlDbType.UniqueIdentifier, jH_GE);
				command.AddParameter("@JH_GB", SqlDbType.UniqueIdentifier, jH_GB);
				command.AddParameter("@JH_GC", SqlDbType.UniqueIdentifier, jH_GC);
				command.AddParameter("@JH_ParentID", SqlDbType.UniqueIdentifier, jH_ParentID);
				command.AddParameter("@JH_ParentTableCode", SqlDbType.VarChar, "TH");
				command.AddParameter("@JH_Status", SqlDbType.VarChar, "WRK");
				command.ExecuteNonQuery();
			}
		}

		void InsertQuote(Guid jH_PK, string jH_JobNum, Guid jH_GE, Guid jH_GB, Guid jH_GC, Guid jH_ParentID)
		{
			string query = string.Format("INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}) VALUES ",
				JobHeaderSchema.Constants.TableName,
				JobHeaderSchema.PK.Name,
				JobHeaderSchema.JH_JobNum.Name,
				JobHeaderSchema.JH_GE.Name,
				JobHeaderSchema.JH_GB.Name,
				JobHeaderSchema.JH_GC.Name,
				JobHeaderSchema.JH_ParentID.Name,
				JobHeaderSchema.JH_ParentTableCode.Name,
				JobHeaderSchema.JH_Status.Name);

			using (DbCommand command = TestConnection.Command(query + "(@JH_PK, @JH_JobNum, @JH_GE, @JH_GB, @JH_GC, @JH_ParentID, @JH_ParentTableCode, @JH_Status)"))
			{
				command.AddParameter("@JH_PK", SqlDbType.UniqueIdentifier, jH_PK);
				command.AddParameter("@JH_JobNum", SqlDbType.VarChar, jH_JobNum);
				command.AddParameter("@JH_GE", SqlDbType.UniqueIdentifier, jH_GE);
				command.AddParameter("@JH_GB", SqlDbType.UniqueIdentifier, jH_GB);
				command.AddParameter("@JH_GC", SqlDbType.UniqueIdentifier, jH_GC);
				command.AddParameter("@JH_ParentID", SqlDbType.UniqueIdentifier, jH_ParentID);
				command.AddParameter("@JH_ParentTableCode", SqlDbType.VarChar, "TH");
				command.AddParameter("@JH_Status", SqlDbType.VarChar, "WRK");
				command.ExecuteNonQuery();
			}
		}

		void InsertQuote(Guid tT_PK, string tT_TransportMode, string tT_ContainerMode, string tT_RL_NKReceivalLocation, string tT_RL_NKDeliveryLocation, Guid tT_TH)
		{
			string query = string.Format("INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}) VALUES ",
				RateOneOffShipmentSchema.Constants.TableName,
				RateOneOffShipmentSchema.PK.Name,
				RateOneOffShipmentSchema.TT_TransportMode.Name,
				RateOneOffShipmentSchema.TT_ContainerMode.Name,
				RateOneOffShipmentSchema.TT_RL_NKReceivalLocation.Name,
				RateOneOffShipmentSchema.TT_RL_NKDeliveryLocation.Name,
				RateOneOffShipmentSchema.TT_TH.Name,
				RateOneOffShipmentSchema.TT_SystemLastEditTimeUtc.Name,
				RateOneOffShipmentSchema.TT_SystemLastEditUser.Name,
				RateOneOffShipmentSchema.TT_SystemCreateTimeUtc.Name,
				RateOneOffShipmentSchema.TT_SystemCreateUser.Name);

			using (DbCommand command = TestConnection.Command(query + "(@TT_PK, @TT_TransportMode, @TT_ContainerMode, @TT_RL_NKReceivalLocation, @TT_RL_NKDeliveryLocation, @TT_TH, @TT_SystemLastEditTimeUtc, @TT_SystemLastEditUser, @TT_SystemCreateTimeUtc, @TT_SystemCreateUser)"))
			{
				command.AddParameter("@TT_PK", SqlDbType.UniqueIdentifier, tT_PK);
				command.AddParameter("@TT_TransportMode", SqlDbType.VarChar, tT_TransportMode);
				command.AddParameter("@TT_ContainerMode", SqlDbType.VarChar, tT_ContainerMode);
				command.AddParameter("@TT_RL_NKReceivalLocation", SqlDbType.VarChar, tT_RL_NKReceivalLocation);
				command.AddParameter("@TT_RL_NKDeliveryLocation", SqlDbType.VarChar, tT_RL_NKDeliveryLocation);
				command.AddParameter("@TT_TH", SqlDbType.UniqueIdentifier, tT_TH);
				command.AddParameter("@TT_SystemLastEditTimeUtc", SqlDbType.Date, DateTime.UtcNow);
				command.AddParameter("@TT_SystemLastEditUser", SqlDbType.VarChar, "~BP");
				command.AddParameter("@TT_SystemCreateTimeUtc", SqlDbType.Date, DateTime.UtcNow);
				command.AddParameter("@TT_SystemCreateUser", SqlDbType.VarChar, "~BP");
				command.ExecuteNonQuery();
			}
		}

		void InsertRatingHeader(Guid tH_PK, string tH_QuoteNumber, Guid tH_GC)
		{
			string query = string.Format("INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}) VALUES ",
				RatingHeaderSchema.Constants.TableName,
				RatingHeaderSchema.PK.Name,
				RatingHeaderSchema.TH_QuoteNumber.Name,
				RatingHeaderSchema.TH_GC.Name,
				RatingHeaderSchema.TH_RateType.Name,
				RatingHeaderSchema.TH_QuoteDate.Name,
				RatingHeaderSchema.TH_SystemLastEditTimeUtc.Name,
				RatingHeaderSchema.TH_SystemLastEditUser.Name,
				RatingHeaderSchema.TH_SystemCreateTimeUtc.Name,
				RatingHeaderSchema.TH_SystemCreateUser.Name);

			using (DbCommand command = TestConnection.Command(query + "(@TH_PK, @TH_QuoteNumber, @TH_GC, @TH_RateType, @TH_QuoteDate, @TH_SystemLastEditTimeUtc, @TH_SystemLastEditUser, @TH_SystemCreateTimeUtc, @TH_SystemCreateUser)"))
			{
				command.AddParameter("@TH_PK", SqlDbType.UniqueIdentifier, tH_PK);
				command.AddParameter("@TH_QuoteNumber", SqlDbType.VarChar, tH_QuoteNumber);
				command.AddParameter("@TH_GC", SqlDbType.UniqueIdentifier, tH_GC);
				command.AddParameter("@TH_RateType", SqlDbType.VarChar, "QTE");
				command.AddParameter("@TH_QuoteDate", SqlDbType.Date, DateTime.Today.AddMonths(-6));
				command.AddParameter("@TH_SystemLastEditTimeUtc", SqlDbType.Date, DateTime.UtcNow);
				command.AddParameter("@TH_SystemLastEditUser", SqlDbType.VarChar, "~BP");
				command.AddParameter("@TH_SystemCreateTimeUtc", SqlDbType.Date, DateTime.UtcNow);
				command.AddParameter("@TH_SystemCreateUser", SqlDbType.VarChar, "~BP");
				command.ExecuteNonQuery();
			}
		}

		void InsertRatingHeader(Guid tH_PK, string tH_QuoteNumber, Guid tH_GC, DateTime quoteEndDate)
		{
			string query = string.Format("INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}) VALUES ",
				RatingHeaderSchema.Constants.TableName,
				RatingHeaderSchema.PK.Name,
				RatingHeaderSchema.TH_QuoteNumber.Name,
				RatingHeaderSchema.TH_GC.Name,
				RatingHeaderSchema.TH_RateType.Name,
				RatingHeaderSchema.TH_QuoteDate.Name,
				RatingHeaderSchema.TH_QuoteEndDate.Name,
				RatingHeaderSchema.TH_SystemLastEditTimeUtc.Name,
				RatingHeaderSchema.TH_SystemLastEditUser.Name,
				RatingHeaderSchema.TH_SystemCreateTimeUtc.Name,
				RatingHeaderSchema.TH_SystemCreateUser.Name);

			using (DbCommand command = TestConnection.Command(query + "(@TH_PK, @TH_QuoteNumber, @TH_GC, @TH_RateType, @TH_QuoteDate, @TH_QuoteEndDate, @TH_SystemLastEditTimeUtc, @TH_SystemLastEditUser, @TH_SystemCreateTimeUtc, @TH_SystemCreateUser)"))
			{
				command.AddParameter("@TH_PK", SqlDbType.UniqueIdentifier, tH_PK);
				command.AddParameter("@TH_QuoteNumber", SqlDbType.VarChar, tH_QuoteNumber);
				command.AddParameter("@TH_GC", SqlDbType.UniqueIdentifier, tH_GC);
				command.AddParameter("@TH_RateType", SqlDbType.VarChar, "QTE");
				command.AddParameter("@TH_QuoteDate", SqlDbType.Date, DateTime.Today.AddMonths(-6));
				command.AddParameter("@TH_QuoteEndDate", SqlDbType.Date, quoteEndDate);
				command.AddParameter("@TH_SystemLastEditTimeUtc", SqlDbType.Date, DateTime.UtcNow);
				command.AddParameter("@TH_SystemLastEditUser", SqlDbType.VarChar, "~BP");
				command.AddParameter("@TH_SystemCreateTimeUtc", SqlDbType.Date, DateTime.UtcNow);
				command.AddParameter("@TH_SystemCreateUser", SqlDbType.VarChar, "~BP");
				command.ExecuteNonQuery();
			}
		}

		void InsertOrgAdress(Guid oA_PK, Guid oA_OH, string oA_Address1)
		{
			string query = string.Format("INSERT INTO {0} ({1}, {2}, {3}) VALUES ",
				OrgAddressSchema.Constants.TableName,
				OrgAddressSchema.PK.Name,
				OrgAddressSchema.OA_OH.Name,
				OrgAddressSchema.OA_Address1.Name);

			using (DbCommand command = TestConnection.Command(query + "(@OA_PK, @OA_OH, @OA_Address1)"))
			{
				command.AddParameter("@OA_PK", SqlDbType.UniqueIdentifier, oA_PK);
				command.AddParameter("@OA_OH", SqlDbType.UniqueIdentifier, oA_OH);
				command.AddParameter("@OA_Address1", SqlDbType.VarChar, oA_Address1);
				command.ExecuteNonQuery();
			}
		}

		void InsertOrgCompanyData(Guid oB_PK, Guid oB_OH, Guid oB_GC)
		{
			string query = string.Format("INSERT INTO {0} ({1}, {2}, {3}) VALUES ",
				OrgCompanyDataSchema.Constants.TableName,
				OrgCompanyDataSchema.PK.Name,
				OrgCompanyDataSchema.OB_OH.Name,
				OrgCompanyDataSchema.OB_GC.Name);

			using (DbCommand command = TestConnection.Command(query + "(@OB_PK, @OB_OH, @OB_GC)"))
			{
				command.AddParameter("@OB_PK", SqlDbType.UniqueIdentifier, oB_PK);
				command.AddParameter("@OB_OH", SqlDbType.UniqueIdentifier, oB_OH);
				command.AddParameter("@OB_GC", SqlDbType.UniqueIdentifier, oB_GC);
				command.ExecuteNonQuery();
			}
		}

		void InsertJobDocAddress(Guid e2_PK, Guid e2_OA_Address, Guid e2_ParentID, string e2_ParentTableCode)
		{
			string query = string.Format("INSERT INTO {0} ({1}, {2}, {3}, {4}) VALUES ",
				JobDocAddressSchema.Constants.TableName,
				JobDocAddressSchema.PK.Name,
				JobDocAddressSchema.E2_OA_Address.Name,
				JobDocAddressSchema.E2_ParentID.Name,
				JobDocAddressSchema.E2_ParentTableCode.Name);

			using (DbCommand command = TestConnection.Command(query + "(@E2_PK, @E2_OA_Address, @E2_ParentID, @E2_ParentTableCode)"))
			{
				command.AddParameter("@E2_PK", SqlDbType.UniqueIdentifier, e2_PK);
				command.AddParameter("@E2_OA_Address", SqlDbType.UniqueIdentifier, e2_OA_Address);
				command.AddParameter("@E2_ParentID", SqlDbType.UniqueIdentifier, e2_ParentID);
				command.AddParameter("@E2_ParentTableCode", SqlDbType.VarChar, e2_ParentTableCode);
				command.ExecuteNonQuery();
			}
		}
		#endregion
	}
}

