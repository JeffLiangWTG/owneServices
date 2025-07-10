using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Build.Database.Script.TestFramework
{
	/// <summary>
	/// Class for inserting accounting data into DB for tests.
	/// </summary>
	/// <remarks>
	/// It is usually preferable to test scripts from the Accouting solution \Enterprise\Product\Operations\Accounting\Business\ScriptTests so
	/// that you can use the BusinessObjectFactory to more easily create the data. If only I had known...
	/// </remarks>
	public class TestDbHelper : TestDbHelperBase
	{
		public TestDbHelper(DbConnection dbConnection) : base(dbConnection) { }

		public static Guid DefaultCompanyPK => new Guid("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC");
		public static Guid OtherCompanyPK => new Guid("22C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061");
		public static string DefaultCompanyCountryCode => "AU";

		public static Guid DepartmentBrnPK => new Guid("86BB1C22-0865-4685-996E-D56CBD136491");
		public static Guid DepartmentBrnPK2 => new Guid("432D350D-037E-4620-AEC8-A422DCC2FC7D");
		public static Guid BranchBrnPK => new Guid("27A55065-AC88-4EC3-8BED-E575E79172CB");
		public static Guid BranchBrnPK2 => new Guid("2FDBA7FB-60BA-4A03-8336-0DEFAC4F9673");
		public static Guid UserStaffPK => new Guid("70EFA270-3F0F-479C-9AED-0009455622E2");
		public static string UserStaffCode => "C";
		public static Guid glAccountPkAAD = new Guid("A303D530-B8D8-4CBF-A29F-F22F9D05BC7B");
		public static Guid glAccountPkADA = new Guid("AE6D15CE-63EC-41BF-905C-A95894A08095");
		public static Guid glAccountPkAFA = new Guid("C9C886AD-611E-4C29-8B2B-85020E0F5E2A");

		public static Guid DefaultCompanyOrgProxyPK => new Guid("4F1F6B5D-F65F-4B9F-A769-8C170A7A8642");

		public Guid GetCompanyPKByCode(string code)
			=> DbConnection.ExecuteScalar<Guid>($"SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = '{code}'");
		public Guid GetCreditor(Guid company)
			=> (Guid)RunSQL(new { OB_GC = company }, "SELECT TOP 1 OH_PK FROM dbo.OrgHeader INNER JOIN dbo.OrgCompanyData ON OB_OH = OH_PK WHERE OB_GC = @OB_GC AND OB_IsDebtor = 0 AND OB_IsCreditor = 1", executionType: SQLExecutionTypes.ExecuteScalar);

		public Guid InsertOrgCusCode(string customsRegNo, string codeType, string rn_NKCodeCountry, Guid oa_PremisesAddress, Guid oh_PK)
		{
			Insert(OrgCusCodeSchema.Constants.TableName, new
			{
				OK_PK = FreshPK(),
				OK_CustomsRegNo = customsRegNo,
				OK_CodeType = codeType,
				OK_RN_NKCodeCountry = rn_NKCodeCountry,
				OK_OA_PremisesAddress = oa_PremisesAddress,
				OK_OH = oh_PK
			});
			return lastPK;
		}

		public Guid InsertOrgAddress(Guid oh_PK, string address1, string code, string country = "")
		{
			Insert(OrgAddressSchema.Constants.TableName, new
			{
				OA_PK = FreshPK(),
				OA_OH = oh_PK,
				OA_Address1 = address1,
				OA_Code = code,
				OA_RN_NKCountryCode = country
			});
			return lastPK;
		}

		public Guid InsertChargeCode(Guid? companyPK, string code, string chargeType = "MRG", string chargeGroup = "ORG", decimal marginPercentage = 100, Guid? revenueAccount = null, Guid? wIPAccount = null, Guid? costAccount = null, Guid? accrualAccount = null, Guid? gSTRate = null)
		{
			Insert(AccChargeCodeSchema.Constants.TableName, new
			{
				AC_PK = FreshPK(),
				AC_Code = code,
				AC_ChargeType = chargeType,
				AC_AG_RevenueAccount = revenueAccount,
				AC_AG_WIPAccount = wIPAccount,
				AC_AG_CostAccount = costAccount,
				AC_AG_AccrualAccount = accrualAccount,
				AC_AT_GSTRate = gSTRate,
				AC_MarginPercentage = marginPercentage,
				AC_ChargeGroup = chargeGroup,
				AC_GC = companyPK
			});
			return lastPK;
		}

		public Guid InsertAccChargeCreditorOverride(Guid chargeCodePK, string jobType, string direction = "", string transportMode = "", string paymentTerm = "", string defaultingRule = "SCA", Guid? departmentPK = null, string creditorRole = "", Guid? creditorPK = null)
		{
			Insert(AccChargeCreditorOverrideSchema.Constants.TableName, new
			{
				ACC_PK = FreshPK(),
				ACC_AC_ChargeCode = chargeCodePK,
				ACC_JobType = jobType,
				ACC_Direction = direction,
				ACC_TransportMode = transportMode,
				ACC_PaymentTerm = paymentTerm,
				ACC_DefaultingRule = defaultingRule,
				ACC_GE_Department = departmentPK,
				ACC_CreditorRole = creditorRole,
				ACC_OH_Creditor = creditorPK,
			});
			return lastPK;
		}

		public Guid InsertAccChargeBranchOverride(Guid chargeCodePK, string jobType, string direction = "", string transportMode = "", string defaultingRule = "SBA", Guid? branchPK = null)
		{
			Insert(AccChargeBranchOverrideSchema.Constants.TableName, new
			{
				YA_PK = FreshPK(),
				YA_AC_ChargeCode = chargeCodePK,
				YA_JobType = jobType,
				YA_Direction = direction,
				YA_TransportMode = transportMode,
				YA_DefaultingRule = defaultingRule,
				YA_GB_SpecificBranch = branchPK,
			});
			return lastPK;
		}

		#region GL Account

		public Guid InsertGLAccount(string accountNum, string description, string accountType = "P&L", string units = "", string cashFlowType = "", string debitCredit = "DR", Guid? altAccount = null, string agColumn = "AP")
		{
			Insert(AccGLHeaderSchema.Constants.TableName, new
			{
				AG_PK = FreshPK(),
				AG_AccountNum = accountNum,
				AG_Description = description,
				AG_AccountType = accountType,
				AG_StatisticalUnits = units,
				AG_DebitCredit = debitCredit,
				AG_TotalLevel = 0,
				AG_AG_AlternateNum = altAccount,
				AG_Column = agColumn,
				AG_PrintSequence = 0,
				AG_Notes = "",
				AG_AccountGroup = "",
				AG_IsActive = true,
				AG_ControlAccount = false,
				AG_DisallowDirectPosting = false,
				AG_CashFlowType = cashFlowType,
			});
			return lastPK;
		}

		public Guid GLAccountPK1
		{
			get
			{
				if (glAccountPK1 == Guid.Empty)
				{
					glAccountPK1 = InsertGLAccount("111.222.01", "TestGLAccount 1");
				}
				return glAccountPK1;
			}
		}
		Guid glAccountPK1;

		public Guid GLNoteAccountPK1
		{
			get
			{
				if (glNoteAccountPK1 == Guid.Empty)
				{
					glNoteAccountPK1 = InsertGLAccount("211.222.01", "TestGLNoteAccount 1", "NTE", "KWH");
				}
				return glNoteAccountPK1;
			}
		}
		Guid glNoteAccountPK1;

		#endregion

		public Guid InsertAccPeriod(int year, int month, Guid? companyPK = null, DateTime? startDate = null)
		{
			Insert(AccPeriodManagementSchema.Constants.TableName, new
			{
				AM_PK = FreshPK(),
				AM_Period = year * 100 + month,
				AM_Year = year,
				AM_StartDate = startDate ?? new DateTime(year, month, 1),
				AM_EndDate = (startDate ?? new DateTime(year, month, 1)).AddMonths(1).AddMinutes(-1),
				AM_GC_Company = companyPK ?? TestDbHelper.DefaultCompanyPK
			});
			return lastPK;
		}

		public Guid InsertJobBookedCtgMove(Guid cartagePK, Guid containerPK)
		{
			Insert(JobBookedCtgMoveSchema.Constants.TableName, new
			{
				EW_PK = FreshPK(),
				EW_JJ = cartagePK,
				EW_JC_Container = containerPK
			});
			return lastPK;
		}

		public Guid InsertJobCartage(string jJ_ConsignmentID, Guid branchPK, Guid? sailingPK = null, bool jJ_IsCancelled = false, string jJ_E3_NKJobType = "", Guid? jJ_ParentID = null, string jJ_ParentTableCode = "")
		{
			Insert(JobCartageSchema.Constants.TableName, new
			{
				JJ_PK = FreshPK(),
				JJ_ConsignmentID = jJ_ConsignmentID,
				JJ_GB = branchPK,
				JJ_JX_Sailing = sailingPK,
				JJ_IsCancelled = jJ_IsCancelled,
				JJ_E3_NKJobType = jJ_E3_NKJobType,
				JJ_ParentID = jJ_ParentID,
				JJ_ParentTableCode = jJ_ParentTableCode
			});
			return lastPK;
		}

		public Guid InsertJobCharge(Guid jobPK, Guid branchPK, Guid companyPK, Guid departmentPK, Guid chargeCodePK, Guid transactionLinePKRev, Guid transactionLinePKCst, decimal oSSellAmt = 0m, Guid? jR_OH_CostAccount = null, Guid? jR_OH_SellAccount = null, decimal jR_LocalCostAmt = 0, decimal jR_LocalSellAmt = 0, bool jR_ProFormaCost = false, bool jR_ProFormaRevenue = false, decimal jR_OSCostAmt = 0, decimal jR_OSCostGSTAmt = 0, decimal costExchangeRate = 1m, Guid? consolCostPK = null, string costCurrency = "AUD", string sellCurrency = "AUD", short displaySequence = 0)
		{
			Insert(JobChargeSchema.Constants.TableName, new
			{
				JR_PK = FreshPK(),
				JR_JH = jobPK,
				JR_GB = branchPK,
				JR_GC = companyPK,
				JR_GE = departmentPK,
				JR_AC = chargeCodePK,
				JR_AL_ARLine = transactionLinePKRev == Guid.Empty ? (Guid?)null : transactionLinePKRev,
				JR_AL_APLine = transactionLinePKCst == Guid.Empty ? (Guid?)null : transactionLinePKCst,
				JR_OSSellAmt = oSSellAmt,
				JR_OH_CostAccount = jR_OH_CostAccount,
				JR_LocalCostAmt = jR_LocalCostAmt,
				JR_ProFormaCost = jR_ProFormaCost,
				JR_OH_SellAccount = jR_OH_SellAccount,
				JR_LocalSellAmt = jR_LocalSellAmt,
				JR_ProFormaRevenue = jR_ProFormaRevenue,
				JR_OSCostAmt = jR_OSCostAmt,
				JR_OSCostGSTAmt = jR_OSCostGSTAmt,
				JR_OSCostExRate = costExchangeRate,
				JR_E6 = consolCostPK,
				JR_RX_NKCostCurrency = costCurrency,
				JR_RX_NKSellCurrency = sellCurrency,
				JR_DisplaySequence = displaySequence
			});
			return lastPK;
		}

		public Guid InsertJobChargeAndLines(string transactionNum, Guid jobPK, Guid chargeCodePK, Guid? glAccountPK, Guid branchPK, Guid companyPK, Guid departmentPK, Guid debtorOrgPK, Guid creditorOrgPK, decimal revenueOrWipAmount, decimal costOrAccrualAmount, bool isWipAccrual, DateTime? postDate, DateTime? reverseDate)
		{
			var transactionHeaderPK = isWipAccrual ? (Guid?)null : InsertTransactionHeader("AP", "INV", transactionNum, 0M, postDate, branchPK, departmentPK);
			var transactionLinePKRev = InsertTransactionLine(isWipAccrual ? null : transactionHeaderPK, jobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, debtorOrgPK, (isWipAccrual ? -1 : 1) * revenueOrWipAmount, isWipAccrual ? "WIP" : "REV", postDate, reverseDate);
			var transactionLinePKCst = InsertTransactionLine(isWipAccrual ? null : transactionHeaderPK, jobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, creditorOrgPK, (isWipAccrual ? 1 : -1) * revenueOrWipAmount, isWipAccrual ? "ACR" : "CST", postDate, reverseDate);
			var jobCharge = InsertJobCharge(jobPK, branchPK, companyPK, departmentPK, chargeCodePK, transactionLinePKRev, transactionLinePKCst);

			if (isWipAccrual && reverseDate != null)
			{
				// Create the revenue and cost lines that caused this reversal of WIP and ACR
				transactionHeaderPK = InsertTransactionHeader("AR", "INV", transactionNum, 0M, reverseDate, branchPK, departmentPK);
				transactionLinePKRev = InsertTransactionLine(transactionHeaderPK, jobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, debtorOrgPK, revenueOrWipAmount, "REV", reverseDate, reverseDate);
				transactionHeaderPK = InsertTransactionHeader("AP", "INV", transactionNum, 0M, reverseDate, branchPK, departmentPK);
				transactionLinePKCst = InsertTransactionLine(transactionHeaderPK, jobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, creditorOrgPK, -costOrAccrualAmount, "CST", reverseDate, reverseDate);
				jobCharge = InsertJobCharge(jobPK, branchPK, companyPK, departmentPK, chargeCodePK, transactionLinePKRev, transactionLinePKCst);
			}
			return jobCharge;
		}

		public Guid InsertJobContainer()
		{
			Insert(JobContainerSchema.Constants.TableName, new
			{
				JC_PK = FreshPK()
			});
			return lastPK;
		}

		public Guid InsertJobContainerLegs(Guid movePK)
		{
			Insert(JobContainerLegsSchema.Constants.TableName, new
			{
				JU_PK = FreshPK(),
				JU_EW = movePK
			});
			return lastPK;
		}

		public Guid InsertTransactionLine(Guid? transactionHeaderPK = null, Guid? jobPK = null, Guid? chargeCodePK = null, Guid? glAccountPK = null, Guid? branchPK = null, Guid? departmentPK = null, Guid? orgHeaderPK = null, decimal lineAmount = 10, string lineType = "REV", DateTime? postDate = null, DateTime? reverseDate = null, decimal taxAmount = 0m, decimal taxRecoverable = 1m, string taxBasis = "A", Guid? companyPK = null, decimal exchangeRate = 1m, bool postToGL = false, bool reverseToGL = false, Guid? taxRatePK = null, Guid? taxMessageIdPK = null, string transactionCurrency = "AUD")
		{
			Insert(AccTransactionLinesSchema.Constants.TableName, new
			{
				AL_PK = FreshPK(),
				AL_AH = transactionHeaderPK,
				AL_JH = jobPK,
				AL_GB = branchPK.HasValue ? branchPK : BranchBrnPK,
				AL_GE = departmentPK.HasValue ? departmentPK : DepartmentBrnPK,
				AL_AC = chargeCodePK,
				AL_AG = glAccountPK,
				AL_LineAmount = lineAmount,
				AL_LineType = lineType,
				AL_OH = orgHeaderPK,
				AL_PostDate = postDate,
				AL_ReverseDate = reverseDate,
				AL_GSTVAT = taxAmount,
				AL_RX_NKTransactionCurrency = transactionCurrency,
				AL_InputGSTVATRecoverable = taxRecoverable,
				AL_GSTVATBasis = taxBasis,
				AL_GC = companyPK.HasValue ? companyPK : TestDbHelper.DefaultCompanyPK,
				AL_ExchangeRate = exchangeRate,
				AL_PostToGL = postToGL ? "Y" : "N",
				AL_ReverseToGL = reverseToGL ? "Y" : "N",
				AL_AT = taxRatePK,
				AL_A9_VATCLASS = taxMessageIdPK
			});
			return lastPK;
		}

		public Guid InsertTransactionHeader(string ledger, string type, string transactionNum, decimal invoiceAmount, DateTime? postDate, Guid? branchPK = null, Guid? departmentPK = null, Guid? bankAccountPK = null, DateTime? dueDate = null, int count = 1, string category = "", Guid? glAccountPK = null, bool postToGL = true, string receiptBatchNo = "",
			Guid? job = null, Guid? companyPK = null, Guid? org = null, decimal gstAmount = 0m, string currency = "AUD", decimal exchangeRate = 1m, decimal outstandingAmount = 0m, decimal localOtherTaxAmount = 0m, DateTime? invoiceDate = null, bool isCancelled = false, DateTime? fullyPaidDate = null, DateTime? documetReceivedDate = null, string consolidatedInvoiceRef = "", decimal osAmount = 0m, Guid? cashAdvanceHeader = null, string receiptType = "CSH")
		{
			Insert(AccTransactionHeaderSchema.Constants.TableName, new
			{
				AH_PK = FreshPK(),
				AH_Ledger = ledger,
				AH_TransactionType = type,
				AH_TransactionNum = transactionNum,
				AH_TransactionCount = count,
				AH_TransactionReference = "",
				AH_Desc = ledger + " " + type + " " + transactionNum,
				AH_InvoiceDate = invoiceDate ?? postDate,
				AH_TransactionCategory = category,
				AH_DueDate = dueDate.HasValue ? dueDate : postDate,
				AH_InvoiceAmount = invoiceAmount,
				AH_GSTAmount = gstAmount,
				AH_WithholdingTax = 0,
				AH_OSTotal = osAmount != 0 ? osAmount : invoiceAmount,
				AH_RX_NKTransactionCurrency = currency,
				AH_ExchangeRate = exchangeRate,
				AH_AgePeriod = 0,
				AH_PostPeriod = 0,
				AH_PostDate = postDate,
				AH_ChequeOrReference = "CASH",
				AH_ReceiptType = receiptType,
				AH_CashBasisGSTIndicator = 0,
				AH_CashBasisGSTRealisedToGL = 0,
				AH_ChequeDrawer = "CASH",
				AH_DrawerBank = "",
				AH_InvoiceApproved = 0,
				AH_ConsolidatedInvoiceRef = consolidatedInvoiceRef,
				AH_InvoicePrinted = 0,
				AH_IsCancelled = isCancelled ? 1 : 0,
				AH_NotAllocated = 0,
				AH_OutstandingAmount = outstandingAmount,
				AH_PostedToEFT = 0,
				AH_PostToGL = postToGL ? "Y" : "N",
				AH_ReceiptBatchNo = receiptBatchNo,
				AH_TransactionCreatedByMatching = 0,
				AH_InvoiceTerm = "",
				AH_InvoiceTermDays = 0,
				AH_POST1 = 0,
				AH_POST2 = 0,
				AH_POST3 = 0,
				AH_POST4 = 0,
				AH_GB = branchPK.HasValue ? branchPK : BranchBrnPK,
				AH_GE = departmentPK.HasValue ? departmentPK : DepartmentBrnPK,
				AH_AB = bankAccountPK,
				AH_PostedInternal = 0,
				AH_GC = companyPK.HasValue ? companyPK : DefaultCompanyPK,
				AH_AG = glAccountPK,
				AH_JH = job,
				AH_OH = org,
				AH_LocalTaxAmountOtherTaxes = localOtherTaxAmount,
				AH_FullyPaidDate = fullyPaidDate,
				AH_DocumentReceivedDate = documetReceivedDate,
				AH_CAH_CashAdvanceRequestHeader = cashAdvanceHeader
			});
			return lastPK;
		}

		public Guid InsertTransactionMatchLink(Guid transactionPK, string groupCode, DateTime? matchDate = null, decimal paidAmount = 100m)
		{
			Insert(AccTransactionMatchLinkSchema.Constants.TableName, new
			{
				AP_PK = FreshPK(),
				AP_AH = transactionPK,
				AP_MatchGroupNum = groupCode,
				AP_MatchDate = matchDate ?? DateTime.Today,
				AP_Amount = paidAmount,
			});
			return lastPK;
		}

		public Guid InsertOrgHeader(string code, string fullName)
		{
			Insert(OrgHeaderSchema.Constants.TableName, new
			{
				OH_PK = FreshPK(),
				OH_Code = code,
				OH_FullName = fullName 
			});
			return lastPK;
		}

		public Guid InsertJobChargeRevRecognition(string recognitionType, DateTime recognitionDate, Guid jobPK)
		{
			Insert(JobChargeRevRecognitionSchema.Constants.TableName, new
			{
				D3_PK = FreshPK(),
				D3_RecognitionType = recognitionType,
				D3_RecognitionDate = recognitionDate,
				D3_JH = jobPK
			});
			return lastPK;
		}

		public Guid InsertCashAdvanceHeader(string transactionNum, string ledger, Guid company, Guid org, Guid job, string currency, decimal osAmount, decimal localAmount, decimal osPaidAmount = 0M, decimal localPaidAmount = 0M, string status = "REQ", bool isPrinted = false)
		{
			Insert(AccCashAdvanceRequestHeaderSchema.Constants.TableName, new
			{
				CAH_PK = FreshPK(),
				CAH_RequestReferenceNumber = transactionNum,
				CAH_Status = status,
				CAH_Ledger = ledger,
				CAH_GC_Company = company,
				CAH_OH_Organization = org,
				CAH_JH_Job = job,
				CAH_RX_NKTransactionCurrency = currency,
				CAH_OSAmount = osAmount,
				CAH_LocalAmount = localAmount,
				CAH_OSPaidAmount = osPaidAmount,
				CAH_LocalPaidAmount = localPaidAmount,
				CAH_Printed = isPrinted
			});
			return lastPK;
		}

		#region JobRole

		public Guid InsertJobRole(string title, string description)
		{
			Insert(HRJobRoleSchema.Constants.TableName, new
			{
				HJ_PK = FreshPK(),
				HJ_JobTitle = title,
				HJ_JobRoleDescription = description
			});
			return lastPK;
		}

		#endregion

		#region Staff

		public Guid InsertStaff(string code, string loginName = null)
		{
			Insert(GlbStaffSchema.Constants.TableName, new
			{
				GS_PK = FreshPK(),
				GS_Code = code,
				GS_LoginName = loginName
			});
			return lastPK;
		}

		#endregion

		#region Branch

		public Guid InsertBranch(string code, Guid? companyPK = null, string branchName = null)
		{
			branchName = branchName ?? string.Empty;
			Insert(GlbBranchSchema.Constants.TableName, new
			{
				GB_PK = FreshPK(),
				GB_Code = code,
				GB_GC = companyPK ?? DefaultCompanyPK,
				GB_BranchName = branchName
			});
			return lastPK;
		}

		public Guid DefaultBranchPK
		{
			get
			{
				if (defaultBranchPK == Guid.Empty)
				{
					defaultBranchPK = InsertBranch("ZZB", TestDbHelper.DefaultCompanyPK);
				}
				return defaultBranchPK;
			}
		}
		Guid defaultBranchPK;

		#endregion

		#region Department

		public Guid InsertDepartment(string code, string descripton = null)
		{
			descripton = descripton ?? string.Empty;
			Insert(GlbDepartmentSchema.Constants.TableName, new
			{
				GE_PK = FreshPK(),
				GE_Code = code,
				Ge_Desc = descripton
			});
			return lastPK;
		}

		public Guid DefaultDepartmentPK
		{
			get
			{
				if (defaultDepartmentPK == Guid.Empty)
				{
					defaultDepartmentPK = InsertDepartment("ZZD");
				}
				return defaultDepartmentPK;
			}
		}
		Guid defaultDepartmentPK;

		#endregion

		#region EmployementHistory

		public Guid InsertEmploymentHistory(Guid staffPk, string startTime, string jobTitle, Guid jobRolePk, string jobFamily, string jobDesc, string companyName, string depReason, string depComment, string employmentType = "PER", Guid? geh_pk = null)
		{
			Insert(GlbEmploymentHistorySchema.Constants.TableName, new
			{
				GEH_PK = geh_pk ?? FreshPK(),
				GEH_GS_Staff = staffPk,
				GEH_EffectiveDate = startTime,
				GEH_JobTitle = jobTitle,
				GEH_HJ_JobRole = jobRolePk,
				GEH_JobFamily = jobFamily,
				GEH_JobDescription = jobDesc,
				GEH_EmploymentType = employmentType,
				GEH_CompanyName = companyName,
				GEH_DepartureReason = depReason,
				GEH_DepartureComments = depComment,
				GEH_SystemCreateTimeUtc = DateTime.Now,
				GEH_SystemCreateUser = "E",
				GEH_SystemLastEditTimeUtc = DateTime.Now,
				GEH_SystemLastEditUser = "E"
			});
			return lastPK;
		}

		#endregion

		#region EmploymentTeam

		public Guid InsertEmploymentTeam(Guid staffPk, string startTime, string teamName, Guid? get_pk = null)
		{
			Insert(GlbEmploymentTeamSchema.Constants.TableName, new
			{
				GET_PK = get_pk ?? FreshPK(),
				GET_GS_Staff = staffPk,
				GET_EffectiveDate = startTime,
				GET_GST_NKTeamCode = teamName,
				GET_SystemCreateTimeUtc = DateTime.Now,
				GET_SystemCreateUser = "E",
				GET_SystemLastEditTimeUtc = DateTime.Now,
				GET_SystemLastEditUser = "E"
			});
			return lastPK;
		}

		#endregion

		#region HomeBranchDepartment

		public Guid InsertHomeBranchDepartment(Guid staffPk, string startTime, Guid departmentPk, Guid branchPk, Guid? ghb_pk = null)
		{
			Insert(GlbEmployingBranchDepartmentSchema.Constants.TableName, new
			{
				GHB_PK = ghb_pk ?? FreshPK(),
				GHB_GS_Staff = staffPk,
				GHB_EffectiveDate = startTime,
				GHB_GE_Department = departmentPk,
				GHB_GB_Branch = branchPk,
				GHB_SystemCreateTimeUtc = DateTime.Now,
				GHB_SystemCreateUser = "E",
				GHB_SystemLastEditTimeUtc = DateTime.Now,
				GHB_SystemLastEditUser = "E"
			});
			return lastPK;
		}

		#endregion

		#region BranchDepartment

		public Guid InsertBeneficiaryBranchDepartment(Guid staffPk, string startTime, Guid departmentPk, Guid branchPk, Guid? gbb_pk = null)
		{
			Insert(GlbBeneficiaryBranchDepartmentSchema.Constants.TableName, new
			{
				GBB_PK = gbb_pk ?? FreshPK(),
				GBB_GS_Staff = staffPk,
				GBB_EffectiveDate = startTime,
				GBB_GE_Department = departmentPk,
				GBB_GB_Branch = branchPk,
				GBB_SystemCreateTimeUtc = DateTime.Now,
				GBB_SystemCreateUser = "E",
				GBB_SystemLastEditTimeUtc = DateTime.Now,
				GBB_SystemLastEditUser = "E"
			});
			return lastPK;
		}

		#endregion

		#region EmploymentLocation

		public Guid InsertEmploymentLocation(Guid staffPk, string startTime, string address1, string address2, string city, string state, string postCode, string countryCode, string validationStatus, Guid? gel_pk = null)
		{
			Insert(GlbEmploymentLocationSchema.Constants.TableName, new
			{
				GEL_PK = gel_pk ?? FreshPK(),

				GEL_GS_Staff = staffPk,
				GEL_EffectiveDate = startTime,
				GEL_Address1 = address1,
				GEL_Address2 = address2,
				GEL_City = city,
				GEL_State = state,
				GEL_PostCode = postCode,
				GEL_RN_NKCountryCode = countryCode,
				GEL_ValidationStatus = validationStatus,
				GEL_SystemCreateTimeUtc = DateTime.Now,
				GEL_SystemCreateUser = "E",
				GEL_SystemLastEditTimeUtc = DateTime.Now,
				GEL_SystemLastEditUser = "E"
			});
			return lastPK;
		}

		#endregion

		public Guid InsertShipment(string uniqueConsignRef, DateTime createdTime)
		{
			return InsertShipment(uniqueConsignRef, true, createdTime, string.Empty, string.Empty, string.Empty, string.Empty, DateTime.Now, DateTime.Now.AddDays(1), 0, null, null, string.Empty, false);
		}

		public Guid InsertShipment(string uniqueConsignRef, bool isForwardRegistered)
		{
			return InsertShipment(uniqueConsignRef, isForwardRegistered, DateTime.Now, string.Empty, string.Empty, string.Empty, string.Empty, DateTime.Now, DateTime.Now.AddDays(1), 0, null, null, string.Empty, false);
		}

		public Guid InsertShipment(string uniqueConsignRef, string transportMode, string packingMode, string origin, string destination, DateTime departure, DateTime arrival, decimal chargeable)
		{
			return InsertShipment(uniqueConsignRef, true, DateTime.Now, transportMode, packingMode, origin, destination, departure, arrival, chargeable, null, null, string.Empty, false);
		}

		public Guid InsertShipment(string uniqueConsignRef, bool isForwardRegistered, DateTime createdTime, string transportMode, string packingMode, string origin, string destination, DateTime departure, DateTime arrival, decimal chargeable, Guid? sailingPk, Guid? principalPk, string shipmentStatus, bool isShipping)
		{
			Insert(JobShipmentSchema.Constants.TableName, new
			{
				JS_PK = FreshPK(),
				JS_UniqueConsignRef = uniqueConsignRef,
				JS_IsForwardRegistered = isForwardRegistered,
				JS_SystemCreateTimeUtc = createdTime,
				JS_TransportMode = transportMode,
				JS_PackingMode = packingMode,
				JS_RL_NKOrigin = origin,
				JS_RL_NKDestination = destination,
				JS_E_DEP = departure,
				JS_E_ARV = arrival,
				JS_ActualChargeable = chargeable,
				JS_JX = sailingPk,
				JS_OH_DeliveryAgent = principalPk,
				JS_ShipmentStatus = shipmentStatus,
				JS_IsShipping = isShipping
			});
			return lastPK;
		}

		public Guid InsertJobDocsAndCartage(Guid parentID, string parentTableCode, DateTime lclAvailableDate)
		{
			Insert(JobDocsAndCartageSchema.Constants.TableName, new
			{
				JP_PK = FreshPK(),
				JP_Isvalid = true,
				JP_ParentID = parentID,
				JP_ParentTableCode = parentTableCode,
				JP_LCLAvailable = lclAvailableDate
			});
			return lastPK;
		}

		public Guid InsertJobSailing(Guid originPK, Guid destinationPK, DateTime depotAvailablilityDate)
		{
			Insert(JobSailingSchema.Constants.TableName, new
			{
				JX_PK = FreshPK(),
				JX_JA = originPK,
				JX_JB = destinationPK,
				JX_DepotAvailabilityDate = depotAvailablilityDate
			});
			return lastPK;
		}

		public Guid InsertJobVoyOrigin(string portOfLoading, Guid voyagePK)
		{
			Insert(JobVoyOriginSchema.Constants.TableName, new
			{
				JA_PK = FreshPK(),
				JA_RL_NKPortOfLoading = portOfLoading,
				JA_JV = voyagePK
			});
			return lastPK;
		}

		public Guid InsertJobVoyDestination(string portOfDischarge, Guid voyagePK, DateTime availablityDate)
		{
			Insert(JobVoyDestinationSchema.Constants.TableName, new
			{
				JB_PK = FreshPK(),
				JB_RL_NKPortOfDischarge = portOfDischarge,
				JB_JV = voyagePK,
				JB_AvailabilityDate = availablityDate
			});
			return lastPK;
		}

		public Guid InsertJobVoyage(string voyageFlight)
		{
			Insert(JobVoyageSchema.Constants.TableName, new
			{
				JV_PK = FreshPK(),
				JV_VoyageFlight = voyageFlight
			});
			return lastPK;
		}

		public Guid InsertJobVoyage(string voyageFlight, string vessel)
		{
			Insert(JobVoyageSchema.Constants.TableName, new
			{
				JV_PK = FreshPK(),
				JV_RV_NKVessel = vessel,
				JV_VoyageFlight = voyageFlight
			});
			return lastPK;
		}

		public Guid InsertJobConsolTransport(Guid parentGuid, string parentType, Guid sailingPK, DateTime etd, DateTime eta, int legOrder)
		{
			Insert(JobConsolTransportSchema.Constants.TableName, new
			{
				JW_PK = FreshPK(),
				JW_ParentGUID = parentGuid,
				JW_ParentType = parentType,
				JW_JX = sailingPK,
				JW_ETD = etd,
				JW_ETA = eta,
				JW_LegOrder = legOrder
			});
			return lastPK;
		}

		public Guid InsertConsol(string uniqueConsignRef, string loadPort, string dischargePort)
		{
			Insert(JobConsolSchema.Constants.TableName, new
			{
				JK_PK = FreshPK(),
				JK_UniqueConsignRef = uniqueConsignRef,
				JK_RL_NKLoadPort = loadPort,
				JK_RL_NKDischargePort = dischargePort
			});
			return lastPK;
		}

		public Guid InsertConsolCost(Guid chargeCode, string invoiceNum, DateTime invoiceDate, string costCurrency, decimal oSCostAmount, decimal oSCostGSTAmount, decimal costExchangeRate, decimal localCostAmount, string apportionmentMethod, Guid? creditor, Guid? aPInvoicePK, Guid? aRInvoicePK, Guid? consol, Guid company, Guid? taxId = null)
		{
			Insert(JobConsolCostSchema.Constants.TableName, new
			{
				E6_PK = FreshPK(),
				E6_AC_ChargeCode = chargeCode,
				E6_InvoiceNum = invoiceNum,
				E6_InvoiceDate = invoiceDate,
				E6_RX_NKCurrency = costCurrency,
				E6_OSCostAmount = oSCostAmount,
				E6_OSGSTAmount = oSCostGSTAmount,
				E6_ExchangeRate = costExchangeRate,
				E6_LocalCostAmount = localCostAmount,
				E6_ApportionmentMethod = apportionmentMethod,
				E6_OH_Creditor = creditor.HasValue ? (object)creditor : GetCreditor(company),
				E6_AH_APInvoice = aPInvoicePK,
				E6_AH_ARInvoice = aRInvoicePK,
				E6_ParentID = consol,
				E6_ParentTableCode = "JK",
				E6_GC = company,
				E6_AT_TaxRate = taxId
			});
			return lastPK;
		}

		public Guid InsertJobConShipLink(Guid consolPK, Guid shipmentPK)
		{
			Insert(JobConShipLinkSchema.Constants.TableName, new
			{
				JN_PK = FreshPK(),
				JN_JK = consolPK,
				JN_JS = shipmentPK
			});
			return lastPK;
		}

		public Guid InsertJobStorage(string jobNunumber, DateTime billingDate, Guid clientOrgPK)
		{
			Insert(JobStorageSchema.Constants.TableName, new
			{
				ET_PK = FreshPK(),
				ET_StorageJobNumber = jobNunumber,
				ET_StorageType = "WHS",
				ET_BillingDate = billingDate,
				ET_StorageFromDate = ToDate("2012-01-01"),
				ET_StorageToDate = ToDate("2012-12-31"),
				ET_OH_Client = clientOrgPK
			});
			return lastPK;
		}

		public Guid InsertJobContainerDetention(string jobNumber, Guid companyPK, Guid principalOrgPK, Guid clientOrgPK)
		{
			Insert(JobContainerDetentionSchema.Constants.TableName, new
			{
				NC_PK = FreshPK(),
				NC_JobNumber = jobNumber,
				NC_GC = companyPK,
				NC_OH_Principal = principalOrgPK,
				NC_OH_Client = clientOrgPK,
				NC_SystemCreateTimeUtc = "2020-01-01",
				NC_SystemCreateUser = "US1"
			});

			return lastPK;
		}

		public Guid InsertJob(string jobNumber, Guid companyPK, Guid branchPK, Guid departmentPK,
			string parentTableCode, Guid? parentId, string status, DateTime createdTime)
		{
			Insert(JobHeaderSchema.Constants.TableName, new
			{
				JH_PK = FreshPK(),
				JH_JobNum = jobNumber,
				JH_GB = branchPK,
				JH_GC = companyPK,
				JH_GE = departmentPK,
				JH_ParentID = parentId.HasValue ? (object)parentId : Guid.NewGuid(),
				JH_Status = status,
				JH_ParentTableCode = parentTableCode,
				JH_SystemCreateTimeUtc = createdTime
			});
			return lastPK;
		}

		public Guid InsertJobChargeAttrib(Guid jobChargePK, string name, string value, double amount)
		{
			Insert(JobChargeAttribSchema.Constants.TableName, new
			{
				EC_PK = FreshPK(),
				EC_JR = jobChargePK,
				EC_Name = name,
				EC_Value = value,
				EC_Amount = amount
			});
			return lastPK;
		}

		public Guid InsertJobConsolCostAttrib(Guid costPK, string name, string value, double amount)
		{
			Insert(JobConsolCostAttribSchema.Constants.TableName, new
			{
				E6A_PK = FreshPK(),
				E6A_Name = name,
				E6A_Value = value,
				E6A_Amount = amount,
				E6A_E6_JobConsolCost = costPK
			});
			return lastPK;
		}

		public Guid InsertCashBasisVAT(Guid linePK, DateTime? postDate = null, Guid? companyPK = null, decimal taxAmount = 1)
		{
			Insert(AccCashBasisVATSchema.Constants.TableName, new
			{
				YC_PK = FreshPK(),
				YC_AL_TransactionLine = linePK,
				YC_GC = companyPK ?? DefaultCompanyPK,
				YC_TaxAmount = taxAmount,
				YC_PostDate = (postDate.HasValue ? postDate : DateTime.Now),
				YC_SystemCreateTimeUtc = DateTime.Now,
				YC_SystemCreateUser = "E"
			});
			return lastPK;
		}

		public Guid InsertOrgContact(string name, string email, bool webAccessEnabled, Guid orgHeader, string notifyMode = "", string attachmentType = "", bool isActive = true, string lastEditUser = "A")
		{
			Insert(OrgContactSchema.Constants.TableName, new
			{
				OC_PK = FreshPK(),
				OC_ContactName = name,
				OC_Email = email,
				OC_IsActive = isActive,
				OC_WebAccessEnabled = webAccessEnabled,
				OC_OH = orgHeader,
				OC_NotifyMode = notifyMode,
				OC_AttachmentType = attachmentType,
				OC_SystemLastEditUser = lastEditUser
			});
			return lastPK;
		}

		public Guid InsertOrgDocument(string documentGroup, Guid? menuItem, Guid orgContact, string deliveryBy, string attachmentType = "")
		{
			Insert(OrgDocumentSchema.Constants.TableName, new
			{
				OD_PK = FreshPK(),
				OD_DocumentGroup = documentGroup,
				OD_SU_MenuItem = menuItem,
				OD_DeliverBy = deliveryBy,
				OD_AttachmentType = attachmentType,
				OD_FilterShipmentMode = "ALL",
				OD_FilterDirection = "ALL",
				OD_IsValid = 1,
				OD_OC = orgContact
			});
			return lastPK;
		}

		public Guid InsertStmMenuItem(string menuName, string businessContext, string menuType, bool isSystemDefined = true)
		{
			Insert(StmMenuItemSchema.Constants.TableName, new
			{
				SU_PK = FreshPK(),
				SU_MenuName = menuName,
				SU_BusinessContext = businessContext,
				SU_MenuType = menuType,
				SU_IsSystemDefined = isSystemDefined ? 1 : 0
			});
			return lastPK;
		}

		public Guid InsertOrgSecurity(bool granted, string securityName, Guid orgHeader, Guid? stmMenuItem = null)
		{
			Insert(OrgSecuritySchema.Constants.TableName, new
			{
				OX_PK = FreshPK(),
				OX_Granted = granted,
				OX_SecurityItemName = securityName,
				OX_OH = orgHeader,
				OX_SU = stmMenuItem
			});
			return lastPK;
		}

		public Guid InsertOrgSecurityContacts(bool granted, string securityName, Guid orgContact, Guid orgSecurity)
		{
			Insert(OrgSecurityContactsSchema.Constants.TableName, new
			{
				OZ_PK = FreshPK(),
				OZ_Granted = granted,
				OZ_OC = orgContact,
				OZ_OX = orgSecurity
			});
			return lastPK;
		}

		public Guid InsertComplianceReport(string code, DateTime dateFrom, DateTime dateTo, Guid? companyPK = null)
		{
			Insert(AccComplianceReportSchema.Constants.TableName, new
			{
				ACR_PK = FreshPK(),
				ACR_ReportType = code,
				ACR_GC_Company = companyPK ?? DefaultCompanyPK,
				ACR_Periodicity = "RNG",
				ACR_DateFrom = dateFrom,
				ACR_DateTo = dateTo,
				ACR_Status = "ADD",
				ACR_SystemCreateTimeUtc = dateTo,
				ACR_SystemLastEditTimeUtc = dateTo
			});
			return lastPK;
		}

		public Guid InsertComplianceReportQueue(string reportType, Guid parentPK, string parentTableCode, DateTime date, Guid? companyPK = null, Guid? branchPK = null, string subCode = "")
		{
			Insert(AccTransactionComplianceReportQueueSchema.Constants.TableName, new
			{
				ACQ_PK = FreshPK(),
				ACQ_ReportType = reportType,
				ACQ_GC_Company = companyPK ?? DefaultCompanyPK,
				ACQ_GB_Branch = branchPK ?? DefaultBranchPK,
				ACQ_ParentID = parentPK,
				ACQ_ParentTableCode = parentTableCode,
				ACQ_Date = date,
				ACQ_ReportSubCode = subCode,
			});
			return lastPK;
		}

		public Guid InsertBankAccount(string code, Guid glAccountPK, string description = null, Guid? companyPK = null, Guid? branchPK = null, string accountNum = null, string bSB = null, string currency = null)
		{
			Insert(AccBankAccountSchema.Constants.TableName, new
			{
				AB_PK = FreshPK(),
				AB_Code = code,
				AB_Desc = description ?? code + " DESCRIPTION",
				AB_AccountNum = accountNum ?? code,
				AB_BSB = bSB ?? code,
				AB_RX_NKAccountCurrency = currency ?? "AUD",
				AB_AG = glAccountPK,
				AB_GC = companyPK.GetValueOrDefault(DefaultCompanyPK),
				AB_GB = branchPK == null ? (object)DBNull.Value : branchPK,
			});
			return lastPK;
		}

		public Guid InsertCompany(string code, string name, string currency, string country, bool isReciprocal, bool isGSTRegistered)
		{
			Insert(GlbCompanySchema.Constants.TableName, new
			{
				GC_PK = FreshPK(),
				GC_Code = code,
				GC_Name = name,
				GC_RX_NKLocalCurrency = currency,
				GC_RN_NKCountryCode = country,
				GC_IsReciprocal = isReciprocal,
				GC_IsGSTRegistered = isGSTRegistered
			});
			return lastPK;
		}

		public Guid InsertChequeBook(string code, decimal startNo, decimal currentNo, decimal lastNo, Guid bankAccount, Guid branchPK)
		{
			Insert(AccChequeBookSchema.Constants.TableName, new
			{
				AK_PK = FreshPK(),
				AK_Code = code,
				AK_StartNo = startNo,
				AK_CurrentNo = currentNo,
				AK_LastNo = lastNo,
				AK_AB = bankAccount,
				AK_GB = branchPK
			});
			return lastPK;
		}

		public Guid InsertHotCheque(Guid chequeBook, Guid? job = null)
		{
			Insert(AccHotChequeSchema.Constants.TableName, new
			{
				AQ_PK = FreshPK(),
				AQ_AK = chequeBook,
				AQ_JH = job,
				AQ_ActualOrMaxIndicator = "ACT"
			});
			return lastPK;
		}

		public Guid InsertQueryClaim(string claimNum, string claimStatus, decimal claimAmount, Guid transactionHeaderPK, Guid branchPK, Guid orgContactPK, Guid orgHeaderPK)
		{
			Insert(AccQueryClaimSchema.Constants.TableName, new
			{
				AY_PK = FreshPK(),
				AY_QueryClaimReference = claimNum,
				AY_QueryClaimType = "CLC",
				AY_QueryClaimStatus = claimStatus,
				AY_QueryClaimReasonCode = "DMG",
				AY_QueryClaimAmount = claimAmount,
				AY_ShortDescriptionOfClaim = "DAMAGED GOODS",
				AY_AH = transactionHeaderPK,
				AY_GB = branchPK,
				AY_OC = orgContactPK,
				AY_OH_Debtor = orgHeaderPK,
				AY_HoldOption = "DNM"
			});
			return lastPK;
		}

		public static void AddTVPParameters(DbCommand command, string paramName, string parameterTypeName, string[] values)
		{
			using (var table = new DataTable())
			{
				table.Locale = CultureInfo.InvariantCulture;
				table.Columns.Add("Value", typeof(string)); // Part of SQL code

				if (values != null)
				{
					foreach (var value in values)
					{
						table.Rows.Add(value);
					}
				}

				command.AddTableValuedParameter(paramName, parameterTypeName, table);
			}
		}

		public Guid InsertAccTransactionHeaderSubAccount(Guid transactionHeaderPK, string subClassParentTableCode, Guid subClassParentId)
		{
			Insert(AccTransactionHeaderSubAccountSchema.Constants.TableName, new
			{
				AHS_PK = FreshPK(),
				AHS_AH = transactionHeaderPK,
				AHS_SubClassParentTableCode = subClassParentTableCode,
				AHS_SubClassParentId = subClassParentId
			});
			return lastPK;
		}

		public Guid InsertAccGLHeaderSubAccount(Guid glHeaderPK, bool isSubClassValidationRuleMandatory, string subClass)
		{
			Insert(AccGLHeaderSubAccountSchema.Constants.TableName, new
			{
				ASA_PK = FreshPK(),
				ASA_AG = glHeaderPK,
				ASA_IsSubClassValidationRuleMandatory = isSubClassValidationRuleMandatory,
				ASA_SubClass = subClass
			});
			return lastPK;
		}

		public Guid InsertAccTransactionLineSubAccount(Guid transactionLinePK, string subClassParentTableCode, Guid subClassParentId)
		{
			Insert(AccTransactionLineSubAccountSchema.Constants.TableName, new
			{
				Al1_PK = FreshPK(),
				Al1_AL = transactionLinePK,
				AL1_SubClassParentTableCode = subClassParentTableCode,
				AL1_SubClassParentId = subClassParentId
			});
			return lastPK;
		}

		public Guid InsertGLAggregate(decimal amount, string transactionCategory, int period, Guid glAccountPK, Guid? branchPK = null, Guid? departmentPK = null, Guid? companyPK = null)
		{
			Insert(AccGLAggregateSchema.Constants.TableName, new
			{
				AA_PK = FreshPK(),
				AA_Amount = amount,
				AA_TransactionCategory = transactionCategory,
				AA_Period = period,
				AA_AG = glAccountPK,
				AA_GB = branchPK ?? DefaultBranchPK,
				AA_GE = departmentPK ?? DepartmentBrnPK,
				AA_GC = companyPK ?? DefaultCompanyPK,
			});
			return lastPK;
		}

		public Guid InsertGeneralLedgerData(int postPeriod, Guid glAccountPK, DateTime postdate, decimal debitAmount, string gLDType = "PST", string gLAccountType = "ARC", Guid? branchPK = null, Guid? departmentPK = null, Guid? companyPK = null)
		{
			Insert(AccGeneralLedgerDataSchema.Constants.TableName, new
			{
				GLD_PK = FreshPK(),
				GLD_GC_Company = companyPK ?? DefaultCompanyPK,
				GLD_PostDate = postdate,
				GLD_AG_GLAccount = glAccountPK,
				GLD_OSDebitAmount = debitAmount,
				GLD_LocalDebitAmount = debitAmount,
				GLD_GB_Branch = branchPK ?? DefaultBranchPK,
				GLD_GE_Department = departmentPK ?? DepartmentBrnPK,
				GLD_PostPeriod = postPeriod,
				GLD_Type = gLDType,
				GLD_GLAccountType = gLAccountType
			});
			return lastPK;
		}

		public Guid InsertGenExportBatchSequence(string type, int batchNumber, string parentTableCode, Guid parentID)
		{
			Insert(GenExportBatchSequenceSchema.Constants.TableName, new
			{
				XB_PK = FreshPK(),
				XB_Type = type,
				XB_BatchNumber = batchNumber,
				XB_ParentTableCode = parentTableCode,
				XB_ParentID = parentID
			});
			return lastPK;
		}

		public Guid InsertTaxRate(string code, bool isActive = true, string description = "", string rateType = "RAT", string extraRateType = "", string countryCode = "AU", short postingGroupId = 1, string referenceRateType = "ZZRAT", string referenceExtraRateType = "ZRAT", string rateSource = "TID")
		{
			Insert(AccTaxRateSchema.Constants.TableName, new
			{
				AT_PK = FreshPK(),
				AT_Code = code,
				AT_Description = description,
				AT_Type = rateType,
				AT_ExtraTaxRateType = extraRateType,
				AT_RN_NKCountry = countryCode,
				AT_IsActive = isActive,
				AT_PostingGroupId = postingGroupId,
				AT_ReferenceExtraRateType = referenceExtraRateType,
				AT_ReferenceRateType = referenceRateType,
				AT_RateSource = rateSource,
				AT_TaxSystemCode = "DNC"
			});

			return lastPK;
		}

		public Guid InsertInvMsg(string code, string description = "", bool isShownOnDocuments = true, bool isActive = true, string countryCode = "AU", string englishMsg = "", string localMsg = "AU",
			bool isTriggerExemptionMessage = false, string taxGroup = "TGR", int autoVersion = 1)
		{
			Insert(AccInvMsgSchema.Constants.TableName, new
			{
				A9_PK = FreshPK(),
				A9_Code = code,
				A9_Description = description,
				A9_IsShownOnDocuments = isShownOnDocuments,
				A9_IsActive = isActive,
				A9_RN_NKCountryCode = countryCode,
				A9_EnglishMsg = englishMsg,
				A9_LocalMsg = localMsg,
				A9_IsTriggerExemptionMessage = isTriggerExemptionMessage,
				A9_TaxGroupCode = taxGroup,
				A9_AutoVersion = autoVersion,
				A9_SystemCreateTimeUtc = DateTime.Now,
				A9_SystemCreateUser = "USR",
				A9_SystemLastEditTimeUtc = DateTime.Now,
				A9_SystemLastEditUser = "USR"
			});

			return lastPK;
		}

		public Guid InsertPackLine(Guid shipmentPk, string packLineDescription, int packs, string freightMode = "OUT")
		{
			Insert(JobPackLinesSchema.Constants.TableName, new
			{
				JL_PK = FreshPK(),
				JL_FreightMode = freightMode,
				JL_PackageCount = packs,
				JL_Description = packLineDescription,
				JL_JS = shipmentPk
			});
			return lastPK;
		}

		public Guid InsertUNDGDataItem(Guid packLinePK, decimal flashPoint, string imoClass, string parentCode = "JL")
		{
			Insert(UNDGDataItemSchema.Constants.TableName, new
			{
				DI_PK = FreshPK(),
				DI_DGFlashPoint = flashPoint,
				DI_ParentTableCode = parentCode,
				DI_ParentID = packLinePK,
				DI_IMOClass = imoClass
			});
			return lastPK;
		}

		public Guid InsertUNDGSubstance(Guid dataItemPK, string unno, string variant, string standard, string imoClass, string packingGroup, decimal flashPoint, string description, string parentCode = "DI")
		{
			Insert(ZZUNDGSubstanceSchema.Constants.TableName, new
			{
				DG_PK = FreshPK(),
				DG_IsActive = 1,
				DG_IsSystem = 1,
				DG_UNNO = unno,
				DG_Variation = ("Packing group" + packingGroup + "."),
				DG_Class = imoClass,
				DG_Standard = standard,
				DG_PSN = description,
				DG_PG = packingGroup,
				DG_Code = unno + variant,
				DG_Variant = variant,
				DG_FlashPoint = flashPoint
			});

			Insert(UNDGSubstancePivotSchema.Constants.TableName, new
			{
				DP_PK = FreshPK(),
				DP_ParentId = dataItemPK,
				DP_ParentTableCode = parentCode,
				DP_UNNO = unno,
				DP_Variant = variant,
				DP_Standard = standard,
				DP_IsDefault = 1
			});
			return lastPK;
		}

		public Guid InsertProcessTasks(Guid parentID, string parentTableCode, string type, DateTime actualDate)
		{
			Insert(ProcessTasksSchema.Constants.TableName, new
			{
				P9_PK = FreshPK(),
				P9_ParentID = parentID,
				P9_ParentTableCode = parentTableCode,
				P9_Type = type,
				P9_ActualDate = actualDate
			});
			return lastPK;
		}

		public Guid InsertAccGroup(string groupType, string code = "TESTGRP", string description = null, Guid? companyPK = null)
		{
			var gc_pk = companyPK ?? DefaultCompanyPK;
			Insert("AccGroup", new
			{
				GRO_PK = FreshPK(),
				GRO_GC = gc_pk == Guid.Empty ? (object)DBNull.Value : gc_pk,
				GRO_GroupType = groupType,
				GRO_Code = code,
				GRO_Description = description ?? (code + " Description")
			});
			return lastPK;
		}

		public Guid InsertOrgCreditorGroup(string code = "CRD", string description = null)
		{
			Insert("OrgCreditorGroup", new
			{
				OG_PK = FreshPK(),
				OG_Code = code,
				OG_Desc = description ?? (code + " Description")
			});
			return lastPK;
		}

		public Guid InsertOrgDebtorGroup(string code = "DEB", string description = null)
		{
			Insert("OrgDebtorGroup", new
			{
				OJ_PK = FreshPK(),
				OJ_Code = code,
				OJ_Desc = description ?? (code + " Description")
			});
			return lastPK;
		}

		public Guid InsertAccJobConfig(string configType, string ledger = "", string parentTableCode = "", Guid? parentId = null, string jobType = "ALL", string serviceDirection = "ALL", string transportMode = "ALL", string origin = "", string destination = "", string code = "", string code2 = "", string code3 = "", DateTime? startDate = null, DateTime? expiryDate = null, decimal percentage = 0m, decimal amount = 0m, int number = 0, bool flag = false, Guid? companyPK = null, string invoiceCurrencyType = "")
		{
			var jcf_gc = companyPK ?? DefaultCompanyPK;
			Insert("AccJobConfig", new
			{
				JCF_PK = FreshPK(),
				JCF_GC = jcf_gc == Guid.Empty ? (object)DBNull.Value : jcf_gc,
				JCF_ConfigType = configType,
				JCF_Ledger = ledger,
				JCF_ParentTableCode = parentTableCode,
				JCF_ParentId = parentId,
				JCF_JobType = jobType,
				JCF_ServiceDirection = serviceDirection,
				JCF_TransportMode = transportMode,
				JCF_RN_NKOriginCountry = origin,
				JCF_RN_NKDestinationCountry = destination,
				JCF_Code = code,
				JCF_Code2 = code2,
				JCF_Code3 = code3,
				JCF_StartDate = startDate,
				JCF_ExpiryDate = expiryDate,
				JCF_Percentage = percentage,
				JCF_Amount = amount,
				JCF_Number = number,
				JCF_Flag = flag,
				JCF_InvoiceCurrencyType = invoiceCurrencyType
			});
			return lastPK;
		}

		public Guid InsertAccJobConfigPivot(Guid accJobConfigPk, string code = "", string exRateType = "", Guid? parentId = null, string parentTableCode = "", DateTime? startDate = null, DateTime? expiryDate = null)
		{
			Insert("AccJobConfigPivot", new
			{
				JCT_PK = FreshPK(),
				JCT_JCF_JobConfig = accJobConfigPk,
				JCT_Code = code,
				JCT_ExRateType = exRateType,
				JCT_StartDate = startDate,
				JCT_ExpiryDate = expiryDate,
				JCT_ParentId = parentId,
				JCT_ParentTableCode = parentTableCode
			});
			return lastPK;
		}

		public Guid InsertAccPaymentApproval(string ledger, Guid orgPK, Guid parentPK, string status = "PST", Guid? branchPK = null, Guid? departmentPK = null, Guid? companyPK = null)
		{
			var guid = Guid.NewGuid();
			var gc_pk = companyPK ?? DefaultCompanyPK;
			Insert(AccPaymentApprovalSchema.Constants.TableName, new
			{
				AV_PK = guid,
				AV_PayRunNo = 0,
				AV_Status = status,
				AV_PaymentType = "CSH",
				AV_Ledger = ledger,
				AV_RX_NKPaymentCurrency = "AUD",
				AV_PayExRate = 1m,
				AV_Amount = 100m,
				AV_ExchangeDifference = 10m,
				AV_Discount = 5m,
				AV_ChequeOrReference = "CSH",
				AV_PaymentComment = "PAYMENT APPROVAL",
				AV_GS_NKApproval1st = "",
				AV_GS_NKApproval2nd = "",
				AV_GS_NKApproval3rd = "",
				AV_GB = branchPK ?? DefaultBranchPK,
				AV_OH = orgPK,
				AV_AH = parentPK,
				AV_RejectionReasonCode = "",
				AV_RejectionReasonDetails = "",
				AV_GC = gc_pk == Guid.Empty ? (object)DBNull.Value : gc_pk,
				AV_PaymentApprovalReference = "1234567"
			});
			return guid;
		}

		#region Tax Framework

		public Guid InsertOrgCompanyData(Guid orgHeaderPK, Guid? companyPK = null)
		{
			Insert(OrgCompanyDataSchema.Constants.TableName, new
			{
				OB_PK = FreshPK(),
				OB_GC = companyPK ?? DefaultCompanyPK,
				OB_OH = orgHeaderPK
			});

			return lastPK;
		}

		public Guid InsertOrgTaxConfiguration(Guid taxConfiguration, Guid orgPK)
		{
			Insert(AccOrgTaxConfigurationSchema.Constants.TableName, new
			{
				OTC_PK = FreshPK(),
				OTC_ETC = taxConfiguration,
				OTC_OB = orgPK,
				OTC_IsActive = true,
				OTC_RecoverTax = false,
				OTC_IsThresholdUsed = false
			});

			return lastPK;
		}

		public Guid InsertOrgTaxRate(Guid orgTaxConfiguration, DateTime? startDate = null, DateTime? endDate = null, string source = "MOV")
		{
			Insert(AccOrgTaxRateSchema.Constants.TableName, new
			{
				OTR_PK = FreshPK(),
				OTR_OTC = orgTaxConfiguration,
				OTR_RateNumerator = 1,
				OTR_RateDenominator = 10,
				OTR_Source = source,
				OTR_StartDate = startDate ?? DateTime.Now.AddDays(-1),
				OTR_EndDate = endDate ?? DateTime.Now.AddDays(1)
			});

			return lastPK;
		}

		public Guid InsertTaxConfiguration(string code, Guid companyPK, string cancellationPolicy = "NAL", string taxRealisationMethod = "PDT", string taxRecordCreationTrigger = "MDT", string taxAmountRounding = "STD", string recoveryMethod = "NOR", string ledger = "AR")
		{
			Insert(AccTaxConfigurationSchema.Constants.TableName, new
			{
				ETC_PK = FreshPK(),
				ETC_Code = code,
				ETC_Description = "bla blah",
				ETC_RN_NKCountry = "AU",
				ETC_TaxAuthorityCode = "DNC",
				ETC_TaxSystemCode = "DNC",
				ETC_Ledger = ledger,
				ETC_ParentId = companyPK,
				ETC_ParentTableCode = "GC",
				ETC_IsActive = true,
				ETC_TaxRealisationMethod = taxRealisationMethod,
				ETC_TaxRecordCreationTrigger = taxRecordCreationTrigger,
				ETC_CancellationPolicy = cancellationPolicy,
				ETC_RecoveryMethod = recoveryMethod,
				ETC_TaxAmountRounding = taxAmountRounding
			});

			return lastPK;
		}

		public Guid InsertTaxOverrideGroup(string code, string description = null)
		{
			Insert(AccTaxOverrideGroupSchema.Constants.TableName, new
			{
				AX_PK = FreshPK(),
				AX_Code = code,
				AX_Description = description ?? code + " Description",
				AX_RN_NKCountry = "AU",
				AX_SystemCreateTimeUtc = DateTime.Now,
				AX_SystemCreateUser = 'E',
				AX_SystemLastEditTimeUtc = DateTime.Now,
				AX_SystemLastEditUser = 'E'
			});

			return lastPK;
		}

		public Guid InsertTaxOverrideGroupTaxConfigurationPivot(Guid taxOverrideGroupPK, Guid taxConfigurationPK, Guid? taxIDPK = null, Guid? taxMessagePK = null)
		{
			Insert(AccTaxOverrideGroupTaxConfigurationPivotSchema.Constants.TableName, new
			{
				AXP_PK = FreshPK(),
				AXP_IsValid = 1,
				AXP_TaxAuthorityServiceCode = "ABC",
				AXP_TaxAuthorityServiceCodeDescription = "ABC Description",
				AXP_RateNumerator = 10,
				AXP_RateDenominator = 1,
				AXP_AX_TaxOverrideGroup = taxOverrideGroupPK,
				AXP_ETC_TaxConfiguration = taxConfigurationPK,
				AXP_AT_TaxID = taxIDPK,
				AXP_A9_DefaultVATClass = taxMessagePK,
				AXP_SystemCreateTimeUtc = DateTime.Now,
				AXP_SystemCreateUser = 'E',
				AXP_SystemLastEditTimeUtc = DateTime.Now,
				AXP_SystemLastEditUser = 'E'
			});

			return lastPK;
		}

		public Guid InsertTaxOverrideRule(string costSellAll, bool createTaxRecord, string destination, string direction, string incoTerm, string jobType, string origin, Guid taxOverrideGroupPK, string transportMode, Guid? taxRatePK = null)
		{
			Insert(AccChargeTaxOverrideSchema.Constants.TableName, new
			{
				AO_PK = FreshPK(),
				AO_CostSellAll = costSellAll,
				AO_CreateTaxRecord = createTaxRecord,
				AO_CustomsStatus = "ABC",
				AO_DebtorRole = "",
				AO_DefaultingRule = "NON",
				AO_Destination = destination,
				AO_Direction = direction,
				AO_HomeCountryOrZone = "AU",
				AO_IncoTerm = incoTerm,
				AO_JobType = jobType,
				AO_OrganisationCategory = "ALL",
				AO_Origin = origin,
				AO_ParentID = taxOverrideGroupPK,
				AO_ParentTableCode = "AX",
				AO_SupplyType = "",
				AO_SystemCreateTimeUtc = DateTime.Now,
				AO_SystemCreateUser = "USR",
				AO_SystemLastEditTimeUtc = DateTime.Now,
				AO_SystemLastEditUser = "USR",
				AO_TaxRegCntryOrGroup = "GRP",
				AO_TaxRegCntryOrZone = "ZON",
				AO_TransactionContext = "ALL",
				AO_TransportMode = transportMode,
				AO_AT = taxRatePK == null ? (object)DBNull.Value : taxRatePK
			});

			return lastPK;
		}

		public Guid InsertTaxTransaction(Guid transactionPK, Guid companyPK, Guid branchPK, Guid departmentPK, Guid taxConfigurationPK, Guid taxIdPK)
		{
			Insert(AccTaxTransactionSchema.Constants.TableName, new
			{
				ATT_PK = FreshPK(),
				ATT_AH = transactionPK,
				ATT_GC = companyPK,
				ATT_GB = branchPK,
				ATT_AffectsSourceTransactionTotal = false,
				ATT_Basis = "PST",
				ATT_ETC = taxConfigurationPK,
				ATT_Ledger = "AR",
				ATT_TaxSystemCode = "DNC",
				ATT_TaxDate = DateTime.Today,
				ATT_PostDate = DateTime.Today,
				ATT_RateNumerator = 10,
				ATT_RateDenominator = 1,
				ATT_TaxAuthorityServiceCode = "SAL",
				ATT_TaxAuthorityServiceCodeDescription = "SAL desc",
				ATT_AT_TaxID = taxIdPK,
				ATT_RX_NKOSTaxCurrency = "AUD",
				ATT_OSTaxBaseAmount = 100,
				ATT_LocalTaxBaseAmount = 100,
				ATT_OSTaxAmount = 10,
				ATT_LocalTaxAmount = 10,
				ATT_GE_Department = departmentPK,
				ATT_TaxSuperType = "SPR",
				ATT_SystemCreateTimeUtc = DateTime.Now,
				ATT_SystemCreateUser = "USR",
				ATT_SystemLastEditTimeUtc = DateTime.Now,
				ATT_SystemLastEditUser = "USR",
			});

			return lastPK;
		}

		public Guid InsertTaxTransactionPivot(Guid taxTransactionPK, Guid linePK)
		{
			Insert(AccTaxRecordTransactionLinePivotSchema.Constants.TableName, new
			{
				ATP_PK = FreshPK(),
				ATP_ATT = taxTransactionPK,
				ATP_AL_TransactionLine = linePK,
				ATP_IsTaxExpense = 0,
				ATP_LocalTaxAmount = 100,
			});

			return lastPK;
		}

		public Guid InsertTaxGLMovement(Guid taxTrasanctionPK, Guid debitGLAccountPK, Guid creditGLAccountPK, decimal amount = 10, string type = "PND", DateTime? date = null, int period = 202001)
		{
			Insert(AccTaxGLMovementSchema.Constants.TableName, new
			{
				ATM_PK = FreshPK(),
				ATM_ATT_TaxTransaction = taxTrasanctionPK,
				ATM_Type = type,
				ATM_AG_DebitAccount = debitGLAccountPK,
				ATM_AG_CreditAccount = creditGLAccountPK,
				ATM_Amount = amount,
				ATM_Date = (date ?? new DateTime(2020, 01, 05)).Date,
				ATM_Period = period,
				ATM_SystemCreateTimeUtc = DateTime.Now,
				ATM_SystemCreateUser = "USR",
				ATM_SystemLastEditTimeUtc = DateTime.Now,
				ATM_SystemLastEditUser = "USR",
			});

			return lastPK;
		}

		#endregion

		#region Electronic Invoicing

		public Guid InsertAccEInvoicingTransactionPivot(Guid parentPK, string parentTableCode = "AH", string status = "QUE", string actionType = "SUB", Guid? companyPK = null, string countryCode = "AU", Guid? batchPK = null)
		{
			var guid = Guid.NewGuid();
			var gc_pk = companyPK ?? DefaultCompanyPK;
			Insert(AccEInvoicingTransactionPivotSchema.Constants.TableName, new
			{
				AIP_PK = guid,
				AIP_GC = gc_pk,
				AIP_RN_NKCountryCode = countryCode,
				AIP_AIB = batchPK,
				AIP_ParentID = parentPK,
				AIP_ParentTableCode = parentTableCode,
				AIP_Status = status,
				AIP_ActionType = actionType,
			});
			return guid;
		}

		#endregion

		#region Registry

		public void SetRegistryPLAppropriationAccount(Guid value)
		{
			var name = "GL_PL_APPROPRIATION_ACCOUNT";
			DbConnection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, @"DELETE FROM dbo.StmData WHERE SD_Name ='{0}';
									INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_Type, SD_BinaryValue, SD_GuidValue) VALUES
									(NEWID(), '{0}', NULL, NULL, 'GID', CONVERT(varbinary(max), N'{1}'), '{1}')", name, value));
		}

		public void SetRegistryGLDLastProcessedDate(Guid companyPK, DateTime value)
		{
			var name = "JournalEntriesLastProcessedDate";
			DbConnection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, @"DELETE FROM dbo.StmData WHERE SD_Name ='{0}';
									INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_Type, SD_BinaryValue, SD_GuidValue) VALUES
									(NEWID(), '{0}', '{1}', NULL, 'DT', CONVERT(varbinary(max), N'{2}'), NULL)", name, companyPK, value));
		}

		#endregion

		public Guid InsertDtbBooking(string jobID, Guid bookingConsolidationPK, Guid branchPK, bool isActive = true)
		{
			var guid = Guid.NewGuid();
			Insert(DtbBookingSchema.Constants.TableName, new
			{
				KM_PK = guid,
				KM_JobID = jobID,
				KM_KB_Booking = bookingConsolidationPK,
				KM_GB_Branch = branchPK,
				KM_IsActive = isActive
			});
			return guid;
		}

		public Guid InsertDtbBookingConsolidation(string jobType = "BKG", string jobID = "")
		{
			var guid = Guid.NewGuid();
			Insert(DtbBookingConsolidationSchema.Constants.TableName, new
			{
				KB_PK = guid,
				KB_JobType = jobType,
				KB_JobID = jobID
			});
			return guid;
		}

		public Guid InsertDtbBookingInstruction(int sequence, Guid bookingPK)
		{
			var guid = Guid.NewGuid();
			Insert(DtbBookingInstructionSchema.Constants.TableName, new
			{
				KN_PK = guid,
				KN_Sequence = sequence,
				KN_KM_BookingMovement = bookingPK
			});
			return guid;
		}

		public Guid InsertCusExitHeader(string jobReference, Guid branchPK, Guid companyPK)
		{
			var guid = Guid.NewGuid();
			Insert(CusExitHeaderSchema.Constants.TableName, new
			{
				CXH_PK = guid,
				CXH_ApplicationCode = "XIT",
				CXH_JobReference = jobReference,
				CXH_GB_Branch = branchPK,
				CXH_GC_Company = companyPK,
				CXH_ClusterKey = 1
			});
			return guid;
		}

		public Guid InsertOrgOpportunity(Guid orgHeaderPK, Guid companyPK, Guid orgContactPK, string opportunityID)
		{
			var guid = Guid.NewGuid();
			Insert(OrgOpportunitySchema.Constants.TableName, new
			{
				P8_PK = FreshPK(),
				P8_OH = orgHeaderPK,
				P8_GC = companyPK,
				P8_OC = orgContactPK,
				P8_OpportunityID = opportunityID
			});
			return guid;
		}

		public Guid InsertAccDraftInvoiceHeader(Guid companyPK, Guid branchPK, Guid departmentPK, Guid? transactionHeaderPK)
		{
			Insert(AccDraftInvoiceHeaderSchema.Constants.TableName, new
			{
				AIH_PK = FreshPK(),
				AIH_GC_Company = companyPK,
				AIH_GB_Branch = branchPK,
				AIH_GE_Department = departmentPK,
				AIH_RX_NKTransactionCurrency = "AUD",
				AIH_TransactionType = "INV",
				AIH_Description = "AP Invoice",
				AIH_AH_PostedTransactionHeader = transactionHeaderPK,
			});
			return lastPK;
		}

		public static IEnumerable<decimal> YieldPrimes(int count)
		{
			yield return 2m;

			var next = 3;
			for (int i = 1; i < count; i++)
			{
				if (IsPrime(next))
				{
					yield return next;
				}
				++next;
			}

			bool IsPrime(int num)
			{
				for (int i = 2; i < num / 2; i++)
				{
					if (num % i == 0)
					{
						return false;
					}
				}
				return true;
			}
		}

		/// <summary>
		/// Returns a list of sequential Accounting periods in the specified range.
		/// An accounting period is YYYYMM.
		/// </summary>
		/// <param name="firstPeriod">Int in YYYYMM format</param>
		/// <param name="lastPeriod">Int in YYYYMM format</param>
		/// <remarks>
		/// Accounting Period Examples:
		/// 202101 is the first period in financial year 2021
		/// 200912 is the twelfth period in financial year 2009
		/// 200013 is an invalid accounting period
		/// Note that the first period might not be January (or July) as financial years start at different times of the year in different countries.
		/// </remarks>
		public static IEnumerable<int> YieldAccountingPeriods(int firstPeriod, int lastPeriod)
		{
			var year = int.Parse(firstPeriod.ToString().Substring(0, 4));
			var period = int.Parse(firstPeriod.ToString().Substring(4, 2));

			while ((year * 100) + period <= lastPeriod)
			{
				yield return (year * 100) + period;

				if (period == 12)
				{
					++year;
					period = 1;
				}
				else
				{
					++period;
				}
			}
		}

		public void CreateEDWCompany(string scriptDbName, AdminConnection edwConnection, string companyCode, Guid companyID, int companyKey)
		{
			string sql = string.Format(
				@"INSERT INTO [{0}].[Organization].[BAS__Company] (CompanyCode, CompanyID, CompanyKey) VALUES ('{1}', '{2}', {3})",
				scriptDbName, companyCode, companyID, companyKey);

			edwConnection.ExecuteNonQuery(sql);
		}

		public Guid CreateEDWOrganisation(string scriptDbName, AdminConnection edwConnection, string code, string name, int orgKey)
		{
			var orgID = Guid.NewGuid();

			string sql = string.Format(
				@"INSERT INTO [{0}].[Organization].[BAS__Organization] (Code, FullName, OrganizationKey, OrganizationID) VALUES ('{1}', '{2}', {3}, '{4}')",
				scriptDbName, code, name, orgKey, orgID);

			edwConnection.ExecuteNonQuery(sql);
			return orgID;
		}

		public Guid CreateEDWChargeCode(string scriptDbName, AdminConnection edwConnection, int companyKey, string code, int chargeCodeKey)
		{
			var chargeCodeID = Guid.NewGuid();

			string sql = string.Format(
				@"INSERT INTO [{0}].[Finance].[BAS__ChargeCode] (CompanyKey, Code, ChargeCodeKey, ChargeCodeID) VALUES ({1}, '{2}', {3}, '{4}')",
				scriptDbName, companyKey, code, chargeCodeKey, chargeCodeID);

			edwConnection.ExecuteNonQuery(sql);
			return chargeCodeID;
		}

		public Guid CreateEDWBranch(string scriptDbName, AdminConnection edwConnection, string code, int companyKey, int branchKey)
		{
			var branchID = Guid.NewGuid();

			string sql = string.Format(
				@"INSERT INTO [{0}].[Organization].[BAS__Branch] (CompanyKey, BranchCode, BranchKey, BranchID) VALUES ({1}, '{2}', {3}, '{4}')",
				scriptDbName, companyKey, code, branchKey, branchID);

			edwConnection.ExecuteNonQuery(sql);
			return branchID;
		}

		public Guid CreateEDWDepartment(string scriptDbName, AdminConnection edwConnection, string code, int departmentKey)
		{
			var departmentID = Guid.NewGuid();

			string sql = string.Format(
				@"INSERT INTO [{0}].[Organization].[BAS__Department] (Code, DepartmentKey, DepartmentID) VALUES ('{1}', {2}, '{3}')",
				scriptDbName, code, departmentKey, departmentID);

			edwConnection.ExecuteNonQuery(sql);
			return departmentID;
		}

		public Guid CreateEDWShipment(string scriptDbName, AdminConnection edwConnection, string uniqueConsignRef, string createdTime, int shipmentKey)
		{
			var shipmentID = Guid.NewGuid();

			string sql = string.Format(
				@"INSERT INTO [{0}].[InternationalLogistics].[BAS__Shipment] (JobNumber, ShipmentCreateDateTimeUtc, ShipmentKey, ShipmentID) VALUES ('{1}', '{2}', {3}, '{4}')",
				scriptDbName, uniqueConsignRef, createdTime, shipmentKey, shipmentID);

			edwConnection.ExecuteNonQuery(sql);
			return shipmentID;
		}

		public Guid CreateEDWShipment(string scriptDbName, AdminConnection edwConnection, string uniqueConsignRef, string createdTime, int isForwardRegistered, int shipmentKey)
		{
			var shipmentID = Guid.NewGuid();

			string sql = string.Format(
				@"INSERT INTO [{0}].[InternationalLogistics].[BAS__Shipment] (JobNumber, ShipmentCreateDateTimeUtc, ShipmentKey, IsForwardRegistered, ShipmentID) VALUES ('{1}', '{2}', {3}, '{4}', '{5}')",
				scriptDbName, uniqueConsignRef, createdTime, shipmentKey, isForwardRegistered, shipmentID);

			edwConnection.ExecuteNonQuery(sql);
			return shipmentID;
		}

		public Guid CreateEDWJob(string scriptDbName, AdminConnection edwConnection, string jobNumber, int companyKey, Guid companyID, Guid branchID, Guid departmentID, string parentTableCode, Guid parentID, string status, string createdTime, int jobHeaderKey, int isActive)
		{
			var jobHeaderID = Guid.NewGuid();

			string sql = string.Format(
				@"INSERT INTO [{0}].[Finance].[BAS__JobHeader] (JobNo, CompanyKey, CompanyID, BranchID, DepartmentID, ParentTableCode, ParentID, Status, CreateDateTimeUtc, JobHeaderKey, JobHeaderID, IsActive) VALUES ('{1}', {2}, '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', {10}, '{11}', {12})",
				scriptDbName, jobNumber, companyKey, companyID, branchID, departmentID, parentTableCode, parentID, status, createdTime, jobHeaderKey, jobHeaderID, isActive);

			edwConnection.ExecuteNonQuery(sql);
			return jobHeaderID;
		}

		public void CreateEDWJobCharge(string scriptDbName, AdminConnection edwConnection, string sellCurrency, string costCurrency, decimal sellAmt, decimal costAmt, Guid transactionLinePKRev, Guid transactionLinePKCst, int jobChargeKey)
		{
			var jobChargeID = Guid.NewGuid();

			string sql = string.Format(
				@"INSERT INTO [{0}].[Finance].[BAS__JobCharge] (JobSellCurrency, JobCostCurrency, JobOSSellAmt, JobOSCostAmt, JobALARLineID, JobALAPLineID, JobChargeKey, JobChargeID) VALUES ('{1}', '{2}', {3}, {4}, '{5}', '{6}', {7}, '{8}')",
				scriptDbName, sellCurrency, costCurrency, sellAmt, costAmt, transactionLinePKRev, transactionLinePKCst, jobChargeKey, jobChargeID);

			edwConnection.ExecuteNonQuery(sql);
		}

		public Guid CreateEDWTransactionLine(string scriptDbName, AdminConnection edwConnection, Guid jobHeaderID, Guid chargeCodeID, int companyKey, Guid companyID, Guid branchID, Guid departmentID, Guid orgID, decimal lineAmt, string lineType, string postDateTime, int accGLTransactionLineKey, string reverseDateTime = null)
		{
			var accGLTransactionLineID = Guid.NewGuid();

			string sql = string.Format(
				@"INSERT INTO [{0}].[Finance].[BAS__AccGLTransactionLine] (JobHeaderID, CompanyKey, CompanyID, BranchID, DepartmentID, OrganizationID, LineAmount, LineType, PostDateTime, AccGLTransactionLineKey, AccGLTransactionLineID, ChargeCodeID{1}) VALUES ('{2}', {3}, '{4}', '{5}', '{6}', '{7}', {8}, '{9}', '{10}', {11}, '{12}', '{13}'{14})",
				scriptDbName, reverseDateTime != null ? ", ReverseDateTime" : "", jobHeaderID, companyKey, companyID, branchID, departmentID, orgID, lineAmt, lineType, postDateTime, accGLTransactionLineKey, accGLTransactionLineID, chargeCodeID, reverseDateTime != null ? $", '{reverseDateTime}'" : "");

			edwConnection.ExecuteNonQuery(sql);
			return accGLTransactionLineID;
		}

		public Guid CreateEDWTransactionLine(string scriptDbName, AdminConnection edwConnection, Guid jobHeaderID, Guid chargeCodeID, int companyKey, Guid companyID, Guid branchID, Guid departmentID, Guid orgID, decimal lineAmt, string lineType, string postDateTime, int accGLTransactionLineKey, int jobHeaderKey)
		{
			var accGLTransactionLineID = Guid.NewGuid();

			string sql = string.Format(
				@"INSERT INTO [{0}].[Finance].[BAS__AccGLTransactionLine] (JobHeaderID, CompanyKey, CompanyID, BranchID, DepartmentID, OrganizationID, LineAmount, LineType, PostDateTime, AccGLTransactionLineKey, AccGLTransactionLineID, ChargeCodeID, JobHeaderKey) VALUES ('{1}', {2}, '{3}', '{4}', '{5}', '{6}', {7}, '{8}', '{9}', {10}, '{11}', '{12}', '{13}')",
				scriptDbName, jobHeaderID, companyKey, companyID, branchID, departmentID, orgID, lineAmt, lineType, postDateTime, accGLTransactionLineKey, accGLTransactionLineID, chargeCodeID, jobHeaderKey);

			edwConnection.ExecuteNonQuery(sql);
			return accGLTransactionLineID;
		}

		public void CreateEDWJobChargeAndLines(string scriptDbName, AdminConnection edwConnection, Guid jobHeaderID, Guid chargeCodeID, int companyKey, Guid companyID, Guid branchID, Guid departmentID, Guid orgDebtorID, Guid orgCreditorID, decimal sellAmt, string createdTime, string revereseTime = null)
		{
			var transactionLinePKRev = CreateEDWTransactionLine(scriptDbName, edwConnection, jobHeaderID, chargeCodeID, companyKey, companyID, branchID, departmentID, orgDebtorID, -sellAmt, "WIP", createdTime, 61, revereseTime);
			var transactionLinePKCst = CreateEDWTransactionLine(scriptDbName, edwConnection, jobHeaderID, chargeCodeID, companyKey, companyID, branchID, departmentID, orgCreditorID, sellAmt, "ACR", createdTime, 62, revereseTime);
			CreateEDWJobCharge(scriptDbName, edwConnection, "USD", "GBP", 100, -80, transactionLinePKRev, transactionLinePKCst, 71);
		}

		public void CreateEDWJobChargeAndLines(string scriptDbName, AdminConnection edwConnection, Guid jobHeaderID, Guid chargeCodeID, int companyKey, Guid companyID, Guid branchID, Guid departmentID, Guid orgDebtorID, Guid orgCreditorID, decimal sellAmt, string createdTime, int jobHeaderKey)
		{
			var transactionLinePKRev = CreateEDWTransactionLine(scriptDbName, edwConnection, jobHeaderID, chargeCodeID, companyKey, companyID, branchID, departmentID, orgDebtorID, -sellAmt, "WIP", createdTime, 61, jobHeaderKey);
			var transactionLinePKCst = CreateEDWTransactionLine(scriptDbName, edwConnection, jobHeaderID, chargeCodeID, companyKey, companyID, branchID, departmentID, orgCreditorID, sellAmt, "ACR", createdTime, 62, jobHeaderKey);
			CreateEDWJobCharge(scriptDbName, edwConnection, "USD", "GBP", 100, -80, transactionLinePKRev, transactionLinePKCst, 71);
		}

		public void AddTVP_uniqueidentifierAndIsEmptyParameters(DbCommand command, string paramName, string isEmptyParamName, Guid[] values)
		{
			var table = new DataTable();
			table.Columns.Add("Value", typeof(Guid)); // Part of SQL code

			if (values != null)
			{
				foreach (var value in values)
				{
					table.Rows.Add(value);
				}
			}

			command.AddTableValuedParameter(paramName, "dbo.TVP_uniqueidentifier", table);
			command.AddParameter(isEmptyParamName, SqlDbType.Bit, table.Rows.Count == 0);
		}

		public void ExecuteLoadAGGTables(string scriptDbName, AdminConnection edwConnection)
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture,
				"EXEC [{0}].[{1}].[{2}]",
				scriptDbName,
				"InternationalLogistics",
				"usp_IniLoad_AGG__JobParentsByControllingCustomerAndAgent2"
			);

			edwConnection.ExecuteNonQuery(sqlText);
		}

		string GetIniLoadSQLText1(string scriptDbName, AdminConnection edwConnection)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT [InitialLoadQuery] FROM [{0}].[biAdmin].[CustomTableConfiguration] WHERE  [ModelSchemaName] = 'Finance' AND [ModelTableName] = 'CUS__GLTransactionLineExtended'",
				scriptDbName
			);

			var resultTable = DataUtils.GetDataTableFromQuery(edwConnection, sqlText);
			var record = resultTable.Select().Single();
			var iniLoadSQLText = record.ItemArray[0].ToString();
			return iniLoadSQLText;
		}
		string GetIniLoadSQLText2(string scriptDbName, AdminConnection edwConnection)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT [InitialLoadQuery] FROM [{0}].[biAdmin].[CustomTableConfiguration] WHERE  [ModelSchemaName] = 'InternationalLogistics' AND [ModelTableName] = 'CUS__JobHeaderExtended'",
				scriptDbName
			);

			var resultTable = DataUtils.GetDataTableFromQuery(edwConnection, sqlText);
			var record = resultTable.Select().Single();
			var iniLoadSQLText = record.ItemArray[0].ToString();
			return iniLoadSQLText;
		}

		public void ExecuteLoadCUSTables(string scriptDbName, AdminConnection edwConnection)
		{
			var iniLoadSQLText1 = GetIniLoadSQLText1(scriptDbName, edwConnection);
			var iniLoadSQLText2 = GetIniLoadSQLText2(scriptDbName, edwConnection);

			edwConnection.ExecuteNonQuery(iniLoadSQLText1);
			edwConnection.ExecuteNonQuery(iniLoadSQLText2);
		}

		public Guid InsertAsset(Guid companyPK, Guid branchPK, Guid departmentPK, DateTime tempDate, int assetNumber, string isActive = "1")
		{
			var tempDateSTR = tempDate.AddDays(assetNumber).ToString("yyyyMMdd");
			var code = $"{assetNumber}";
			var desc = $"Description {assetNumber}";
			var serialNumber = $"SN{assetNumber}";
			var note = $"Note Asset {assetNumber}";

			var assetPK = Guid.NewGuid();

			DbConnection.ExecuteNonQuery("INSERT INTO AccAssetHeader (AAH_PK, AAH_AutoVersion, AAH_IsActive, AAH_Code, AAH_Description, AAH_SerialNumber, AAH_Note, AAH_PurchaseDate, AAH_ReceivedDate, AAH_SaleDisposalDate, AAH_GC_Company, AAH_GB_Branch, AAH_GE_Department, AAH_SystemCreateTimeUtc, AAH_SystemCreateUser, AAH_SystemLastEditTimeUtc, AAH_SystemLastEditUser )" +
				$" VALUES ('{assetPK}', 1, {isActive}, '{code}', '{desc}', '{serialNumber}', '{note}', '{tempDateSTR}', '{tempDateSTR}', '{tempDateSTR}', '{companyPK}', '{branchPK}', '{departmentPK}', '{tempDateSTR}', 'XXX', '{tempDateSTR}', 'XXX' )");

			DbConnection.ExecuteNonQuery("INSERT INTO AccAssetGLAccount (AAG_PK, AAG_AutoVersion, AAG_GLAccountType, AAG_AG_GLHeader, AAG_AAH_Asset, AAG_SystemCreateTimeUtc, AAG_SystemCreateUser, AAG_SystemLastEditTimeUtc, AAG_SystemLastEditUser)" +
				$" VALUES ('{Guid.NewGuid()}', 1, 'AAD', '{glAccountPkAAD}', '{assetPK}', '{tempDateSTR}', 'XXX', '{tempDateSTR}', 'XXX' )");
			DbConnection.ExecuteNonQuery("INSERT INTO AccAssetGLAccount (AAG_PK, AAG_AutoVersion, AAG_GLAccountType, AAG_AG_GLHeader, AAG_AAH_Asset, AAG_SystemCreateTimeUtc, AAG_SystemCreateUser, AAG_SystemLastEditTimeUtc, AAG_SystemLastEditUser)" +
				$" VALUES ('{Guid.NewGuid()}', 1, 'ADA', '{glAccountPkADA}', '{assetPK}', '{tempDateSTR}', 'XXX', '{tempDateSTR}', 'XXX' )");
			DbConnection.ExecuteNonQuery("INSERT INTO AccAssetGLAccount (AAG_PK, AAG_AutoVersion, AAG_GLAccountType, AAG_AG_GLHeader, AAG_AAH_Asset, AAG_SystemCreateTimeUtc, AAG_SystemCreateUser, AAG_SystemLastEditTimeUtc, AAG_SystemLastEditUser)" +
				$" VALUES ('{Guid.NewGuid()}', 1, 'AFA', '{glAccountPkAFA}', '{assetPK}', '{tempDateSTR}', 'XXX', '{tempDateSTR}', 'XXX' )");

			return assetPK;
		}

		public Guid InsertAssetTransaction(Guid assetPk, Guid companyPk, Guid branchPk, Guid departmentPk,
			string transactionType, string transactionNumber, string currency, DateTime postDate,
			DateTime? cancelledDate, DateTime? supplierReferenceDate, string supplierReferenceNumber)
		{
			var isCancelledStr = cancelledDate.HasValue ? "1" : "0";
			var cancelledDateStr = cancelledDate.HasValue ? $"'{cancelledDate.Value:yyyyMMdd}'" : "NULL";
			var postDateStr = postDate.ToString("yyyyMMdd");
			var supplierReferenceDateStr = supplierReferenceDate.HasValue ? $"'{supplierReferenceDate.Value:yyyyMMdd}'" : "NULL";
			var description = $"Description {transactionNumber}";
			var transactionPk = Guid.NewGuid();
			DbConnection.ExecuteNonQuery(
				"INSERT INTO AccAssetTransactionHeader (" +
				"ASH_PK, ASH_AutoVersion, ASH_IsCancelled, ASH_TransactionType, ASH_TransactionNumber, ASH_Description, " +
				"ASH_RX_NKTransactionCurrency, ASH_ExchangeRate, ASH_OSAmount, ASH_LocalAmount, ASH_PostDate, " +
				"ASH_CancelledDate, ASH_SupplierReferenceDate, ASH_SupplierReferenceNumber, ASH_AAH_Asset, ASH_GC_Company, " +
				"ASH_GB_Branch, ASH_GE_Department, ASH_SystemCreateTimeUtc, ASH_SystemCreateUser, ASH_SystemLastEditTimeUtc, " +
				"ASH_SystemLastEditUser)" +
				$" VALUES ('{transactionPk}', 1, {isCancelledStr}, '{transactionType}', '{transactionNumber}', '{description}', " +
				$"'{currency}', '{1.0}', 1000, 1000, '{postDateStr}', " +
				$"{cancelledDateStr}, {supplierReferenceDateStr}, '{supplierReferenceNumber}','{assetPk}', '{companyPk}', " +
				$"'{branchPk}', '{departmentPk}', '{postDateStr}', 'XXX', '{postDateStr}', 'XXX' )");
			return transactionPk;
		}

		public Guid InsertAssetTransactionLine(Guid companyPk, Guid branchPk, Guid departmentPk, string lineType,
			int sequence, string currency, DateTime postDate, Guid transactionPk, Guid glAccountPk, decimal amount = 1000)
		{
			var postDateStr = postDate.ToString("yyyyMMdd");
			var description = "Description";
			var linePk = Guid.NewGuid();
			DbConnection.ExecuteNonQuery(
				"INSERT INTO AccAssetTransactionLine (" +
				"ASL_PK, ASL_AutoVersion, ASL_LineType, ASL_Sequence, ASL_Description, " +
				"ASL_RX_NKTransactionCurrency, ASL_ExchangeRate, ASL_OSAmount, ASL_LocalAmount, ASL_PostDate, " +
				"ASL_ASH_AssetTransaction, ASL_AG_Account, ASL_GC_Company, ASL_GB_Branch, ASL_GE_Department, " +
				"ASL_SystemCreateTimeUtc, ASL_SystemCreateUser, ASL_SystemLastEditTimeUtc, ASL_SystemLastEditUser)" +
				$" VALUES ('{linePk}', 1, '{lineType}', {sequence}, '{description}', " +
				$"'{currency}', {1.0}, {amount}, {amount}, '{postDateStr}', " +
				$"'{transactionPk}', '{glAccountPk}', '{companyPk}', '{branchPk}', '{departmentPk}'," +
				$"'{postDateStr}', 'XXX', '{postDateStr}', 'XXX' )");
			return linePk;
		}
	}

	public static class TestHelperExtensions
	{
		public static DataRow GetRowByAccountCode(this DataTable result, string accountCode)
			=> (from DataRow row in result.Rows where row["AccountNumber"].ToString().StartsWith(accountCode) select row).FirstOrDefault();
	}
}

