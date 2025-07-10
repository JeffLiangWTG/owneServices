using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.DataTransfer.Universal;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.ShipmentProcessing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using UniversalCodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;

namespace Enterprise.Accounting.DataTransfer.Universal
{
	public class JobCostingAdapter : IJobCostingAdapter
	{
		BusinessObjectFactory Factory;
		JobManagement JobManagement;
		GlbCompany Company;
		ZString DataProvider;

		public JobCostingAdapter()
		{
			addMappings();
			addFilterMapping();
		}

#if DEBUG

		internal
#endif
		Dictionary<Enum, SchemaColumn> mapping = new Dictionary<Enum, SchemaColumn>();

		void addMappings()
		{
			mapping.Add(ChargeLineElementType.ChargeCode, JobChargeSchema.JR_AC);
			mapping.Add(ChargeLineElementType.Description, JobChargeSchema.JR_Desc);
			mapping.Add(ChargeLineElementType.Branch, JobChargeSchema.JR_GB);
			mapping.Add(ChargeLineElementType.Department, JobChargeSchema.JR_GE);
			mapping.Add(ChargeLineElementType.CostOSCurrency, JobChargeSchema.JR_RX_NKCostCurrency);
			mapping.Add(ChargeLineElementType.CostOSAmount, JobChargeSchema.JR_OSCostAmt);
			mapping.Add(ChargeLineElementType.CostLocalAmount, JobChargeSchema.JR_LocalCostAmt);
			mapping.Add(ChargeLineElementType.CostAPInvoiceNumber, JobChargeSchema.JR_APInvoiceNum);
			mapping.Add(ChargeLineElementType.CostInvoiceDate, JobChargeSchema.JR_APInvoiceDate);
			mapping.Add(ChargeLineElementType.CostDueDate, JobChargeSchema.JR_PaymentDate);
			mapping.Add(ChargeLineElementType.CostGSTVATID, JobChargeSchema.JR_AT_CostGSTRate);
			mapping.Add(ChargeLineElementType.CostOSGSTVATAmount, JobChargeSchema.JR_OSCostGSTAmt);
			mapping.Add(ChargeLineElementType.SellOSCurrency, JobChargeSchema.JR_RX_NKSellCurrency);
			mapping.Add(ChargeLineElementType.SellOSAmount, JobChargeSchema.JR_OSSellAmt);
			mapping.Add(ChargeLineElementType.SellLocalAmount, JobChargeSchema.JR_LocalSellAmt);
			mapping.Add(ChargeLineElementType.SellInvoiceType, JobChargeSchema.JR_InvoiceType);
			mapping.Add(ChargeLineElementType.SellGSTVATID, JobChargeSchema.JR_AT_SellGSTRate);
			mapping.Add(ChargeLineElementType.Debtor, JobChargeSchema.JR_OH_SellAccount);
			mapping.Add(ChargeLineElementType.Creditor, JobChargeSchema.JR_OH_CostAccount);
			mapping.Add(ChargeLineElementType.DisplaySequence, JobChargeSchema.JR_DisplaySequence);
			mapping.Add(ChargeLineElementType.SupplierReference, JobChargeSchema.JR_CostReference);
			mapping.Add(ChargeLineElementType.SellReference, JobChargeSchema.JR_SellReference);
			mapping.Add(ChargeLineElementType.GovernmentReportingCostChargeCode, JobChargeSchema.JR_CostGovtChargeCode);
			mapping.Add(ChargeLineElementType.GovernmentReportingSellChargeCode, JobChargeSchema.JR_SellGovtChargeCode);
			mapping.Add(ChargeLineElementType.CostExchangeRate, JobChargeSchema.JR_OSCostExRate);
			mapping.Add(ChargeLineElementType.SellExchangeRate, JobChargeSchema.JR_OSSellExRate);
			mapping.Add(CostPlaceOfSupplyElement.Location, JobChargeSchema.JR_CostPlaceOfSupply);
			mapping.Add(CostPlaceOfSupplyElement.LocationType, JobChargeSchema.JR_CostPlaceOfSupplyType);
			mapping.Add(SellPlaceOfSupplyElement.Location, JobChargeSchema.JR_SellPlaceOfSupply);
			mapping.Add(SellPlaceOfSupplyElement.LocationType, JobChargeSchema.JR_SellPlaceOfSupplyType);
			mapping.Add(ChargeLineElementType.CostSupplyType, JobChargeSchema.JR_CostSupplyType);
			mapping.Add(ChargeLineElementType.SellSupplyType, JobChargeSchema.JR_SellSupplyType);
			mapping.Add(ChargeLineElementType.ARCashAdvanceRequired, JobChargeSchema.JR_IsARCashAdvance);
			mapping.Add(ChargeLineElementType.APCashAdvanceRequired, JobChargeSchema.JR_IsAPCashAdvance);
		}

		delegate bool MatchingCriteriaDelegate(Charge charge, MatchingCriteria criteria);

		readonly Dictionary<Enum, MatchingCriteriaDelegate> filterMapping = new Dictionary<Enum, MatchingCriteriaDelegate>();

		void addFilterMapping()
		{
			filterMapping.Add(ChargeLineElementType.CostIsPosted, new MatchingCriteriaDelegate((x, c) => x.IsCostPosted == new ZBool(c.Value.Value)));
			filterMapping.Add(ChargeLineElementType.SellIsPosted, new MatchingCriteriaDelegate((x, c) => x.IsRevenuePosted == new ZBool(c.Value.Value)));
			filterMapping.Add(ChargeLineElementType.SellPostedTransactionNumber, new MatchingCriteriaDelegate((x, c) => x.JR_ARInvoiceNumber == c.Value.Value));
			filterMapping.Add(ChargeLineElementType.CostOSGSTVATAmount, new MatchingCriteriaDelegate((x, c) => x.JR_OSCostGSTAmt_Calc == new ZDecimal(c.Value.Value)));
		}

		internal enum CostPlaceOfSupplyElement
		{
			Location,
			LocationType
		}

		internal enum SellPlaceOfSupplyElement
		{
			Location,
			LocationType
		}

		#region Generate

		public JobCosting Generate(IJobHeaderParentCore jobHeaderParentCore, IDataObjectWriterStrategy writerStrategy)
		{
			var jobHeaderParent = jobHeaderParentCore as IJobHeaderParent;
			if (jobHeaderParent != null)
			{
				var job = new JobHeader.Loader(jobHeaderParent).Load();
				if (job != null)
				{
					return Generate(job.PK, writerStrategy);
				}
			}

			return null;
		}

#if DEBUG

		public JobCosting GenerateForTesting(ZGuid jobPK)
		{
			return Generate(jobPK, DefaultDataObjectWriterStrategy.TestInstance);
		}

#endif

		JobCosting Generate(ZGuid jobPK, IDataObjectWriterStrategy writerStrategy)
		{
			this.Factory = new BusinessObjectFactory();
			JobCosting jobCosting = null;
			JobManagement = Factory.Load<JobManagement>(jobPK);

			if (JobManagement == null)
			{
				return jobCosting;
			}

			jobCosting = new JobCosting(writerStrategy);

			GlbBranch branch = Factory.Load<GlbBranch>(JobManagement.JH_GB);
			GlbDepartment department = Factory.Load<GlbDepartment>(JobManagement.JH_GE);
			GlbStaff salesStaff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, JobManagement.JH_GS_NKRepSales);
			GlbStaff operationsStaff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, JobManagement.JH_GS_NKRepOps);

			if (branch != null)
			{
				jobCosting.Branch = new Branch();
				jobCosting.Branch.Code = branch != null ? branch.GB_Code : null;
				jobCosting.Branch.Name = branch != null ? branch.GB_BranchName : null;
			}
			if (department != null)
			{
				jobCosting.Department = new Department();
				jobCosting.Department.Code = department != null ? department.GE_Code : null;
				jobCosting.Department.Name = department != null ? department.GE_DescMultilingual : null;
			}
			if (salesStaff != null)
			{
				jobCosting.SalesStaff = new Staff();
				jobCosting.SalesStaff.Code = salesStaff.GS_Code;
				jobCosting.SalesStaff.Name = salesStaff.GS_FullName;
			}
			if (operationsStaff != null)
			{
				jobCosting.OperationsStaff = new Staff();
				jobCosting.OperationsStaff.Code = operationsStaff != null ? operationsStaff.GS_Code : null;
				jobCosting.OperationsStaff.Name = operationsStaff != null ? operationsStaff.GS_FullName : null;
			}
			if (JobManagement.RepOps != null && JobManagement.RepOps.HomeBranch != null)
			{
				jobCosting.HomeBranch = new Branch();
				jobCosting.HomeBranch.Code = JobManagement.RepOps != null ? JobManagement.RepOps.HomeBranch.GB_Code : ZString.Empty;
				jobCosting.HomeBranch.Name = JobManagement.RepOps != null ? JobManagement.RepOps.HomeBranch.GB_BranchName : ZString.Empty;
			}
			jobCosting.Currency = new Currency();
			jobCosting.Currency.Code = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			if (GlbCompany.CurrentCompany.LocalCurrency != null)
			{
				jobCosting.Currency.Description = GlbCompany.CurrentCompany.LocalCurrency.RX_DescMultilingual.GetUnresolvedString();
			}
			jobCosting.TotalRevenue = JobManagement.TotalRevenue;
			jobCosting.TotalCost = JobManagement.TotalCost;
			jobCosting.TotalAccrual = JobManagement.TotalAccrual;
			jobCosting.AccrualRecognized = JobManagement.AcrRecognized;
			jobCosting.AccrualNotRecognized = JobManagement.AcrNotRecognized;
			jobCosting.TotalWIP = JobManagement.TotalWIP;
			jobCosting.WIPRecognized = JobManagement.WipRecognized;
			jobCosting.WIPNotRecognized = JobManagement.WipNotRecognized;
			jobCosting.TotalJobProfit = JobManagement.ProfitLossNotRecognized + JobManagement.ProfitLossRecognized;
			jobCosting.AgentRevenue = GetAgentRevenue(jobPK);
			jobCosting.LocalClientRevenue = GetLocalClientRevenue(jobPK);
			jobCosting.OtherDebtorRevenue = GetOtherDebtorRevenue(jobPK);
			jobCosting.ClientContractNumber = JobManagement.JH_ClientContractNumber;

			var taxBranch = JobManagement.TaxBranch;
			if (taxBranch != null)
			{
				jobCosting.TaxBranch = new Branch()
				{
					Code = taxBranch.GB_Code,
					Name = taxBranch.GB_BranchName,
				};
			}

			PopulateChargeLineCollection(jobPK, jobCosting, writerStrategy);
			PopulateCashAdvanceHeaderCollection(jobPK, jobCosting, writerStrategy);

			JobManagement = null;
			this.Factory = null;

			return jobCosting;
		}

		void PopulateCashAdvanceHeaderCollection(ZGuid jobPK, JobCosting jobCosting, IDataObjectWriterStrategy writerStrategy)
		{
			Job job = Factory.Load<Job>(jobPK);
			if (job != null && job.CashAdvanceRequests.Count > 0)
			{
				jobCosting.SetCashAdvanceRequestHeaderCollection(() =>
				{
					var cashAdvanceRequestHeaderCollection = new List<CashAdvanceRequestHeader>();
					foreach (AccCashAdvanceRequestHeader requestHeader in job.CashAdvanceRequests)
					{
						var cashAdvanceRequestHeader = new CashAdvanceRequestHeader(writerStrategy);
						if (!requestHeader.CAH_RequestReferenceNumber.IsEmpty)
						{
							cashAdvanceRequestHeader.RequestReferenceNumber = requestHeader.CAH_RequestReferenceNumber;
						}
						cashAdvanceRequestHeader.Status = requestHeader.CAH_Status;
						cashAdvanceRequestHeader.Ledger = requestHeader.CAH_Ledger;
						if (requestHeader.Organization != null)
						{
							cashAdvanceRequestHeader.OrgHeader = new OrganizationReference();
							cashAdvanceRequestHeader.OrgHeader.Key = requestHeader.Organization.OH_Code;
							cashAdvanceRequestHeader.OrgHeader.Type = nameof(DataContextType.Organization);
						}
						if (requestHeader.TransactionCurrency != null)
						{
							var requestHeaderCurrency = new Currency();
							requestHeaderCurrency.Code = requestHeader.TransactionCurrency.RX_Code;
							requestHeaderCurrency.Description = requestHeader.TransactionCurrency.RX_DescMultilingual.GetUnresolvedString();
							cashAdvanceRequestHeader.Currency = requestHeaderCurrency;
						}
						cashAdvanceRequestHeader.LocalAmount = requestHeader.CAH_LocalAmount;
						cashAdvanceRequestHeader.LocalPaidAmount = requestHeader.CAH_LocalPaidAmount;
						cashAdvanceRequestHeader.OSAmount = requestHeader.CAH_OSAmount;
						cashAdvanceRequestHeader.OSPaidAmount = requestHeader.CAH_OSPaidAmount;

						cashAdvanceRequestHeaderCollection.Add(cashAdvanceRequestHeader);
					}
					return cashAdvanceRequestHeaderCollection;
				}
				);
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "This should be a single method, and it is not complex.")]
		[SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		void PopulateChargeLineCollection(ZGuid jobPK, JobCosting jobCosting, IDataObjectWriterStrategy writerStrategy)
		{
			Job job = Factory.Load<Job>(jobPK);
			if (job != null && job.Charges.Count > 0)
			{
				var placeOfSupplyHelper = new UniversalPlaceOfSupplyHelper(job.Company);

				jobCosting.SetChargeLineCollection(() =>
				{
					var chargeLineCollection = new List<ChargeLine>();

					foreach (Charge charge in job.Charges)
					{
						ChargeLine chargeLine = new ChargeLine(writerStrategy);
						if (!charge.JR_APInvoiceNum.IsEmpty)
						{
							chargeLine.CostAPInvoiceNumber = charge.JR_APInvoiceNum;
						}

						chargeLine.DisplaySequence = charge.JR_DisplaySequence;

						if (charge.ChargeCode != null)
						{
							chargeLine.ChargeCode = new ChargeCode();
							chargeLine.ChargeCode.Code = charge.ChargeCode.AC_Code;
							chargeLine.ChargeCode.Description = charge.ChargeCode.AC_Desc;

							ZString code = charge.ChargeCode.AC_ChargeGroup;
							if (!code.IsEmpty)
							{
								ChargeCodeGroupList chargeGroupList = new ChargeCodeGroupList();
								ZString description = chargeGroupList.GetDescriptionFromCode(code);

								chargeLine.ChargeCodeGroup = new UniversalCodeDescriptionPair() { Code = charge.ChargeCode.AC_ChargeGroup, Description = description };
							}
						}

						PopulateBranch(charge, chargeLine);
						PopulateDepartment(charge, chargeLine);

						chargeLine.Description = charge.JR_Desc;
						PopulateSellAccount(charge, chargeLine);
						PopulateCostAccount(charge, chargeLine);

						chargeLine.CostRatingBehaviour = new UniversalCodeDescriptionPair
						{
							Code = charge.JR_Calc_CostRatingBehavior,
							Description = JobChargeLookups.GetDescriptionForRatingBehaviourCode(charge.JR_Calc_CostRatingBehavior)
						};

						chargeLine.SellRatingBehaviour = new UniversalCodeDescriptionPair
						{
							Code = charge.JR_Calc_SellRatingBehavior,
							Description = JobChargeLookups.GetDescriptionForRatingBehaviourCode(charge.JR_Calc_SellRatingBehavior)
						};

						if (!charge.JR_APInvoiceDate.IsEmpty)
						{
							chargeLine.CostInvoiceDate = charge.JR_APInvoiceDate;
						}
						if (!charge.JR_PaymentDate.IsEmpty)
						{
							chargeLine.CostDueDate = charge.JR_PaymentDate;
						}
						chargeLine.CostIsPosted = charge.IsCostPosted;
						chargeLine.SellIsPosted = charge.IsRevenuePosted;
						chargeLine.CostLocalAmount = charge.JR_LocalCostAmt;
						chargeLine.SellLocalAmount = charge.JR_LocalSellAmt;
						chargeLine.CostOSAmount = charge.JR_OSCostAmt;
						chargeLine.SellOSAmount = charge.JR_OSSellAmt;
						chargeLine.CostExchangeRate = charge.JR_OSCostExRate;
						chargeLine.SellExchangeRate = charge.JR_OSSellExRate;
						if (charge.SellCurrency != null)
						{
							Currency chargeLineOSSellCurrency = new Currency();
							chargeLineOSSellCurrency.Code = charge.SellCurrency.RX_Code;
							chargeLineOSSellCurrency.Description = charge.SellCurrency.RX_DescMultilingual.GetUnresolvedString();
							chargeLine.SellOSCurrency = chargeLineOSSellCurrency;
						}
						if (charge.CostCurrency != null)
						{
							Currency chargeLineOSCostCurrency = new Currency();
							chargeLineOSCostCurrency.Code = charge.CostCurrency.RX_Code;
							chargeLineOSCostCurrency.Description = charge.CostCurrency.RX_DescMultilingual.GetUnresolvedString();
							chargeLine.CostOSCurrency = chargeLineOSCostCurrency;
						}

						if (charge.IsRevenuePosted && charge.ARLine != null && charge.ARLine.TransactionHeader != null)
						{
							chargeLine.SellPostedTransactionNumber = charge.ARLine.TransactionHeader.AH_TransactionNum;
							chargeLine.SellPostedTransactionType = charge.ARLine.TransactionHeader.AH_TransactionType;
							chargeLine.SellPostedTransaction = new TransactionInfo()
							{
								Number = charge.ARLine.TransactionHeader.AH_TransactionNum,
								TransactionType = new TransactionTypeConverter().ToEnumValue(charge.ARLine.TransactionHeader.AH_TransactionType),
								TransactionDate = charge.ARLine.TransactionHeader.AH_InvoiceDate,
								DueDate = charge.ARLine.TransactionHeader.AH_DueDate,
								OutstandingAmount = charge.ARLine.TransactionHeader.AH_OutstandingAmount,
								FullyPaidDate = charge.ARLine.TransactionHeader.AH_FullyPaidDate,
							};
						}

						chargeLine.SellInvoiceType = charge.JR_InvoiceType;
						if (charge.SellGSTRate != null)
						{
							chargeLine.SellGSTVATID = new TaxID();
							chargeLine.SellGSTVATID.TaxCode = charge.SellGSTRate.AT_Code;
							chargeLine.SellGSTVATID.Description = charge.SellGSTRate.AT_Description;
						}
						if (charge.CostGSTRate != null)
						{
							chargeLine.CostGSTVATID = new TaxID();
							chargeLine.CostGSTVATID.TaxCode = charge.CostGSTRate.AT_Code;
							chargeLine.CostGSTVATID.Description = charge.CostGSTRate.AT_Description;
						}
						chargeLine.SellOSGSTVATAmount = charge.JR_OSSellGSTAmt_Calc;
						chargeLine.CostOSGSTVATAmount = charge.JR_OSCostGSTAmt_Calc;

						if (!charge.JR_CostReference.IsEmpty)
						{
							chargeLine.SupplierReference = charge.JR_CostReference;
						}

						if (!charge.JR_SellReference.IsEmpty)
						{
							chargeLine.SellReference = charge.JR_SellReference;
						}

						if (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.Value)
						{
							if (!charge.JR_CostGovtChargeCode.IsEmpty)
							{
								chargeLine.GovernmentReportingCostChargeCode = charge.JR_CostGovtChargeCode;
							}

							if (!charge.JR_SellGovtChargeCode.IsEmpty)
							{
								chargeLine.GovernmentReportingSellChargeCode = charge.JR_SellGovtChargeCode;
							}
						}

						if (charge.JR_E6.IsValid && charge.ParentConsolCost != null && charge.ParentConsolCost.GenericConsolBizO != null)
						{
							var consolReference = new EntityReference();
							consolReference.Key = charge.ParentConsolCost.GenericConsolBizO.VX_Code;
							consolReference.Type = charge.ParentConsolCost.GenericConsolBizO.GetDataContextType.ToString();
							chargeLine.CostApportionmentConsolNumber = consolReference;
						}

						if (charge is IPaymentBasisViewCharge updateableCharge)
						{
							chargeLine.SetCostRatingBasisCollection(() => Helpers.PopulateUniversalPaymentBases(updateableCharge.CostPaymentBasesView));
							chargeLine.SetSellRatingBasisCollection(() => Helpers.PopulateUniversalPaymentBases(updateableCharge.SellPaymentBasesView));
						}

						var sellPlaceOfSupply = placeOfSupplyHelper.GetPlaceOfSupply(charge.JR_SellPlaceOfSupply, charge.JR_SellPlaceOfSupplyType);
						if (sellPlaceOfSupply != null)
						{
							chargeLine.SellPlaceOfSupply = sellPlaceOfSupply;
						}

						var costPlaceOfSupply = placeOfSupplyHelper.GetPlaceOfSupply(charge.JR_CostPlaceOfSupply, charge.JR_CostPlaceOfSupplyType);
						if (costPlaceOfSupply != null)
						{
							chargeLine.CostPlaceOfSupply = costPlaceOfSupply;
						}

						if (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value)
						{
							CargoWise.Integration.ICodeDescription costSupplyType = (Enterprise.Registry.Business.CodeDescriptionBool)AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesList.Value.FindByCode(charge.JR_CostSupplyType);
							chargeLine.CostSupplyType = new UniversalCodeDescriptionPair()
							{
								Code = costSupplyType?.Code ?? charge.JR_CostSupplyType,
								Description = costSupplyType?.Description ?? ZString.Empty
							};

							CargoWise.Integration.ICodeDescription sellSupplyType = (Enterprise.Registry.Business.CodeDescriptionBool)AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesList.Value.FindByCode(charge.JR_SellSupplyType);
							chargeLine.SellSupplyType = new UniversalCodeDescriptionPair()
							{
								Code = sellSupplyType?.Code ?? charge.JR_SellSupplyType,
								Description = sellSupplyType?.Description ?? ZString.Empty
							};
						}

						var costTaxBranch = charge.CostTaxBranch;

						if (costTaxBranch != null)
						{
							chargeLine.CostTaxBranch = new Branch()
							{
								Code = costTaxBranch.GB_Code,
								Name = costTaxBranch.GB_BranchName
							};
						}

						var sellTaxBranch = charge.SellTaxBranch;

						if (sellTaxBranch != null)
						{
							chargeLine.SellTaxBranch = new Branch()
							{
								Code = sellTaxBranch.GB_Code,
								Name = sellTaxBranch.GB_BranchName
							};
						}

						chargeLine.ARCashAdvanceRequired = charge.JR_IsARCashAdvance;
						var arCashAdvanceRequestLine = charge.ARCashAdvanceRequestLine;
						if (arCashAdvanceRequestLine != null)
						{
							chargeLine.ARCashAdvanceRequestLine = PopulateCashAdvanceRequestLine(arCashAdvanceRequestLine);
						}

						chargeLine.APCashAdvanceRequired = charge.JR_IsAPCashAdvance;
						var apCashAdvanceRequestLine = charge.APCashAdvanceRequestLine;
						if (apCashAdvanceRequestLine != null)
						{
							chargeLine.APCashAdvanceRequestLine = PopulateCashAdvanceRequestLine(apCashAdvanceRequestLine);
						}

						chargeLineCollection.Add(chargeLine);
					}

					return chargeLineCollection;
				});
			}

			CashAdvanceRequestLine PopulateCashAdvanceRequestLine(AccCashAdvanceRequestLine apCashAdvanceRequestLine)
			{
				var requestLine = new CashAdvanceRequestLine()
				{
					RequestReferenceNumber = apCashAdvanceRequestLine.RequestHeader.CAH_RequestReferenceNumber,
					Status = apCashAdvanceRequestLine.CAL_Status,
					OSAmount = apCashAdvanceRequestLine.CAL_OSAmount,
					OSPaidAmount = apCashAdvanceRequestLine.CAL_OSPaidAmount,
					LocalAmount = apCashAdvanceRequestLine.CAL_LocalAmount,
					LocalPaidAmount = apCashAdvanceRequestLine.CAL_LocalPaidAmount
				};
				return requestLine;
			}
		}

		static void PopulateCostAccount(Charge charge, ChargeLine chargeLine)
		{
			if (charge.CostAccount != null)
			{
				chargeLine.Creditor = new OrganizationReference();
				chargeLine.Creditor.Key = charge.CostAccount.OH_Code;
				chargeLine.Creditor.Type = nameof(DataContextType.Organization);
				if (!charge.CostAccount.CompanyData.OB_APExternalCreditorCode.IsEmpty)
				{
					chargeLine.ExternalCreditorCode = charge.CostAccount.CompanyData.OB_APExternalCreditorCode;
				}
			}
		}

		static void PopulateSellAccount(Charge charge, ChargeLine chargeLine)
		{
			if (charge.SellAccount != null)
			{
				chargeLine.Debtor = new OrganizationReference();
				chargeLine.Debtor.Key = charge.SellAccount.OH_Code;
				chargeLine.Debtor.Type = nameof(DataContextType.Organization);
				if (!charge.SellAccount.CompanyData.OB_ARExternalDebtorCode.IsEmpty)
				{
					chargeLine.ExternalDebtorCode = charge.SellAccount.CompanyData.OB_ARExternalDebtorCode;
				}
			}
		}

		static void PopulateDepartment(Charge charge, ChargeLine chargeLine)
		{
			if (charge.Department != null)
			{
				chargeLine.Department = new Department();
				chargeLine.Department.Code = charge.Department.GE_Code;
				chargeLine.Department.Name = charge.Department.GE_DescMultilingual;
			}
		}

		static void PopulateBranch(Charge charge, ChargeLine chargeLine)
		{
			if (charge.Branch != null)
			{
				chargeLine.Branch = new Branch();
				chargeLine.Branch.Code = charge.Branch.GB_Code;
				chargeLine.Branch.Name = charge.Branch.GB_BranchName;
			}
		}

		ZDecimal GetAgentRevenue(ZGuid jobPK)
		{
			ZDBOnlyQuery consolQuery = new ZDBOnlyQuery(typeof(ForwardingConsol));
			ZDBOnlySubQuery conShipLinkSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JK);
			ZDBOnlySubQuery shipmentSubQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobConShipLinkSchema.JN_JS);
			ZDBOnlySubQuery jobHeaderSubQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
			jobHeaderSubQuery.AddToFilter(JobHeaderSchema.PK, jobPK);
			shipmentSubQuery.AddSubQuery(jobHeaderSubQuery, JoinCondition.And);
			conShipLinkSubQuery.AddSubQuery(shipmentSubQuery, JoinCondition.And);
			consolQuery.AddSubQuery(conShipLinkSubQuery, JoinCondition.And);
			ForwardingConsol consol = Factory.LoadTop1<ForwardingConsol>(consolQuery);

			if (!JobManagement.AgentCollectPK.IsValid && consol == null)
			{
				return ZDecimal.Zero;
			}

			ZDBOnlyQuery agentRevenueQuery = new ZDBOnlyQuery(typeof(AccTransactionLines));
			agentRevenueQuery.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_JH, jobPK);
			agentRevenueQuery.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_LineType, TransactionLineTypes.Revenue);
			agentRevenueQuery.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_GC, GlbCompany.CurrentCompany.PK);

			List<ZGuid> pKList = new List<ZGuid>();
			if (JobManagement.AgentCollectPK != ZGuid.Empty)
			{
				pKList.Add(JobManagement.AgentCollectPK);
			}
			if (consol != null && consol.ReceivingForwarderPK.IsValid)
			{
				pKList.Add(consol.ReceivingForwarderPK);
			}
			if (consol != null && consol.SendingForwarderPK.IsValid)
			{
				pKList.Add(consol.SendingForwarderPK);
			}

			if (pKList.Count > 0)
			{
				agentRevenueQuery.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_OH, pKList);
			}

			AccTransactionLines[] lines = Factory.Load<AccTransactionLines>(agentRevenueQuery);
			ZDecimal agentRevenueTotal = lines.Sum(x => x.AL_LineAmount);

			return agentRevenueTotal;
		}

		ZDecimal GetLocalClientRevenue(ZGuid jobPK)
		{
			if (!JobManagement.LocalChargesPK.IsValid)
			{
				return ZDecimal.Zero;
			}
			ZDBOnlyQuery localClientRevenueQuery = new ZDBOnlyQuery(typeof(AccTransactionLines));
			localClientRevenueQuery.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_JH, jobPK);
			localClientRevenueQuery.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_LineType, TransactionLineTypes.Revenue);
			localClientRevenueQuery.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_OH, JobManagement.LocalChargesPK);
			localClientRevenueQuery.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_GC, GlbCompany.CurrentCompany.PK);

			AccTransactionLines[] lines = Factory.Load<AccTransactionLines>(localClientRevenueQuery);
			ZDecimal localClientRevenueTotal = lines.Sum(x => x.AL_LineAmount);

			return localClientRevenueTotal;
		}

		ZDecimal GetOtherDebtorRevenue(ZGuid jobPK)
		{
			ZDBOnlyQuery otherDebtorRevenueQuery = new ZDBOnlyQuery(typeof(AccTransactionLines));
			otherDebtorRevenueQuery.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_JH, jobPK);
			otherDebtorRevenueQuery.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_LineType, TransactionLineTypes.Revenue);
			if (JobManagement.AgentCollectPK.IsValid)
			{
				otherDebtorRevenueQuery.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_OH, SQLComparisonOperator.NotEqual, JobManagement.AgentCollectPK);
			}
			if (JobManagement.LocalChargesPK.IsValid)
			{
				otherDebtorRevenueQuery.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_OH, SQLComparisonOperator.NotEqual, JobManagement.LocalChargesPK);
			}
			otherDebtorRevenueQuery.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_GC, GlbCompany.CurrentCompany.PK);

			AccTransactionLines[] lines = Factory.Load<AccTransactionLines>(otherDebtorRevenueQuery);
			ZDecimal otherDebtorRevenueTotal = lines.Sum(x => x.AL_LineAmount);

			return otherDebtorRevenueTotal;
		}

		#endregion

		#region ImportCharges

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Logging message")]
		public void ImportCharges(BusinessObjectFactory factory, IXmlImportLogger logger, IJobCostingData shipment, ZGuid jobHeaderParentPK, ZString jobHeaderParentTablePrefix)
		{
			Factory = factory;

			if (shipment != null && shipment.JobCosting != null)
			{
				Company = null;

				if (shipment.DataContext != null && !shipment.DataContext.CompanyCodeToImportInto.IsEmpty)
				{
					Company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, shipment.DataContext.CompanyCodeToImportInto));
				}
				else if (logger.TopLevelDataContext != null && !logger.TopLevelDataContext.CompanyCodeToImportInto.IsEmpty)
				{
					Company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, logger.TopLevelDataContext.CompanyCodeToImportInto));
				}

				if (Company == null)
				{
					var errorMessage = Res.GetString("1a8551ss-9Eed5-4ae1-B6fe-599ed14ffe0c", "Cannot import {0} element with Invalid Company Code being specified in the Shipment’s DataContext.", nameof(JobCosting));
					logger.Log(Enterprise.Integration.LogType.Error, errorMessage);
					throw new DataObjectReadFailureException(errorMessage);
				}

				GlbBranch branch = null;
				GlbDepartment department = null;

				if (shipment.JobCosting != null && shipment.JobCosting.Branch != null && shipment.JobCosting.Branch.Code.HasValue && !shipment.JobCosting.Branch.Code.Value.IsEmpty)
				{
					branch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, shipment.JobCosting.Branch.Code));
				}
				if (shipment.JobCosting != null && shipment.JobCosting.Department != null && shipment.JobCosting.Department.Code.HasValue && !shipment.JobCosting.Department.Code.Value.IsEmpty)
				{
					department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, shipment.JobCosting.Department.Code));
				}

				// There always should be TopLevelDataContext to use DataProviderForCodeMapping for mapping MatchingCriteria codes, but it happens in Universal unit tests
				if (logger.TopLevelDataContext != null)
				{
					DataProvider = logger.TopLevelDataContext.DataProviderForCodeMapping;
				}

				var bizO = Factory.Load(jobHeaderParentTablePrefix, jobHeaderParentPK);
				var parent = bizO as IJobHeaderParent;
				Job job = null;

				if (parent != null)
				{
					var loader = new Job.Loader(parent);
					job = loader.Load(true, Company);
					if (job == null)
					{
						job = loader.TryCreateWithMutex(false);

						var jobTrackerService = Factory.ServiceContainer.GetService<JobTrackerService>() ?? Factory.ServiceContainer.AddService(new JobTrackerService(Factory));
						jobTrackerService.Add(job);
					}
					if (job == null)
					{
						var caption = Res.GetString("417d975b-b37d-4e2e-b52d-a862cefec605", "Failed to create Job.");
						var errorMessage = loader.GetJobCreationErrorForService();
						// MessageProcessingBusinessFailureException is the proper exception type for business logic exceptions.
						// In case of transient errors shouldRetry flag should be set to true.
						throw new MessageProcessingBusinessFailureException(errorMessage, caption, shouldRetry: true);
					}
					if (job.JH_Status == JobHeaderStatus.Closed.Code && (branch != null || department != null))
					{
						if (branch != null)
						{
							AddJobCannotBeUpdatedLog(logger, "Branch", job.JH_JobNum);
						}

						if (department != null)
						{
							AddJobCannotBeUpdatedLog(logger, "Department", job.JH_JobNum);
						}
					}

					else
					{
						if (branch != null)
						{
							if (job.Company != null && job.Company != branch.Company)
							{
								var caption = Res.GetString("2c8d6149-0ea6-412a-a524-3be0bce6c89c", "Failed to create Job.");
								var errorMessage = Res.GetString("ef89b0e5-7d75-4564-bb57-9fc18c062979", @"The branch '{0}' in the <JobCosting> does not belong to the system company '{1}' processing the XML import.
Please ensure that the correct <Company> data is specified in the <DataTargetCollection>.", branch.GB_Code, job.Company.GC_Code);
								throw new MessageProcessingBusinessFailureException(errorMessage, caption, shouldRetry: false);
							}
							job.JH_GB = branch.PK;
						}
						if (job.Branch == null)
						{
							var errorMessage = Res.GetString("9a72343e-2590-4077-a89d-4cd6f8e87d9c", "Job Costing Branch could not be defaulted.");
							logger.Log(Enterprise.Integration.LogType.Error, errorMessage);
							throw new DataObjectReadFailureException(errorMessage);
						}
						if (department != null)
						{
							job.JH_GE = department.PK;
						}
					}

					if (!ObjectFactory.Get<IElectronicProcessingChargeProvider>().HasElectronicProcessingChargeCurrencyExchangeRate(job))
					{
						logger.Log(Enterprise.Integration.LogType.Error, AccountingUtils.GetElectronicProcessingChargeCurrencyNoExchangeRateErrorMessage(job));
						throw new DataObjectReadFailureException(AccountingUtils.GetElectronicProcessingChargeCurrencyNoExchangeRateErrorMessage(job));
					}

					job.SetValue(JobHeaderSchema.JH_ClientContractNumber, shipment.JobCosting.ClientContractNumber, logger);

					if (shipment.JobCosting.ChargeLineCollection != null)
					{
						if (job.JH_Status != JobHeaderStatus.Closed.Code && job.JH_Status != JobHeaderStatus.JobReadyForFinancialClosure.Code)
						{
							var postingQueueCreator = new JobChargePostingQueueCreator(factory, jobHeaderParentPK, jobHeaderParentTablePrefix);

							using (Factory.SetTempContext(BusinessContext.JobChargeImportingFromEDIMessage))
							using (Factory.SetTempContext(BusinessContext.JobChargeInvalidDebtorWarningInsteadOfError))
							{
								ProcessCharges(shipment, job, logger, postingQueueCreator);
							}

							if (!logger.HasErrors())
							{
								postingQueueCreator.CreateJobChargePostingQueueRecords();
							}
						}
						else if (job.JH_Status == JobHeaderStatus.Closed.Code)
						{
							logger.Log(Enterprise.Integration.LogType.Warning, Res.GetString("bd375456-9ae6-4194-aec6-8a491dfca12b", "The Job Charges cannot be inserted/updated as the Job {0} is closed.", job.JH_JobNum));
						}
						else if (job.JH_Status == JobHeaderStatus.JobReadyForFinancialClosure.Code)
						{
							logger.Log(Enterprise.Integration.LogType.Error, AccountingConstants.JobIsReadyForFinancialClosureWithoutModifySecurityErrorMessage);
						}
					}
				}

				using (Factory.SetTempContext(BusinessContext.JobCreatedFromImporter))
				{
					GatewaySellToCostSynchroniser.Synchronise(job);
				}
			}

			ThrowIfLoggerHasErrors(logger);

			this.Company = null;
			this.Factory = null;
		}

		void AddJobCannotBeUpdatedLog(IXmlImportLogger logger, string changeProperty, ZString jobNum)
		{
			logger.Log(Enterprise.Integration.LogType.Warning, Res.GetString("A1D249DF-6CB0-4635-8843-46535FD14D27", "The Job {1} cannot be updated as the Job {0} is closed.", jobNum, changeProperty));
		}

		void ThrowIfLoggerHasErrors(IXmlImportLogger logger)
		{
			if (logger.HasErrors())
			{
				var errorMessage = new ZStringBuilder();
				foreach (var log in logger.Logs)
				{
					if (log.Type == Enterprise.Integration.LogType.Error)
					{
						errorMessage.AppendLine(log.Message);
					}
				}
				throw new DataObjectReadFailureException(errorMessage.ToString());
			}
		}

		void ProcessCharges(IJobCostingData shipment, Job job, IXmlImportLogger logger, JobChargePostingQueueCreator postingQueueCreator)
		{
			foreach (ChargeLine chargeLine in shipment.JobCosting.ChargeLineCollection)
			{
				if (chargeLine.ImportMetaData != null)
				{
					Charge charge = null;
					try
					{
						switch (chargeLine.ImportMetaData.Instruction)
						{
							case InstructionType.Insert:
								charge = insertChargeLine(job, chargeLine, logger);
								break;

							case InstructionType.Update:
								charge = updateChargeLine(job, chargeLine, logger, insertIfNotFound: false);
								break;

							case InstructionType.UpdateAndInsertIfNotFound:
								charge = updateChargeLine(job, chargeLine, logger, insertIfNotFound: true);
								break;

							case InstructionType.Delete:
								deleteChargeLine(job, chargeLine, logger);
								break;

							default:
								break;
						}

						if (chargeLine.ImportMetaData.Instruction != InstructionType.Delete && charge != null)
						{
							using (TemporarilyResumeValidation(charge))
							{
								charge.RunPreSaveValidation();
								var notificationCollector = new ZNotificationCollector(charge, true, true, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);

								if (charge.HasErrors)
								{
									var errorMessage = new ZStringBuilder();
									errorMessage.AppendLine(getMessage(job, chargeLine));
									notificationCollector.GetUniqueMessageList().ForEach(x => errorMessage.AppendLine(x));
									logger.Log(Enterprise.Integration.LogType.Error, errorMessage.ToString());
								}
								else if (charge.HasWarnings)
								{
									var warningMessage = new ZStringBuilder();
									warningMessage.AppendLine(getMessage(job, chargeLine));
									notificationCollector.GetUniqueMessageList().ForEach(x => warningMessage.AppendLine(x));
									logger.Log(Enterprise.Integration.LogType.Warning, warningMessage.ToString());
								}
							}
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						var builder = new ZStringBuilder();
						builder.AppendLine(getMessage(job, chargeLine));
						builder.AppendLine(ex.Message);

						logger.Log(Enterprise.Integration.LogType.Error, builder.ToString());
					}

					if (!logger.HasErrors() && charge != null && !charge.IsDeleted)
					{
						postingQueueCreator.AddChargePostingInstruction(chargeLine.ImportMetaData.PostingInstruction, charge);
					}
				}
			}
		}

		IDisposable TemporarilyResumeValidation(Charge charge)
		{
#if DEBUG
			if (Enterprise.ZArchitecture.Environment.Globals.IsTest && UniversalXmlWorkflowProcessor.PreventTemporarilyResumeValidation_ForTestOnly.Value)
			{
				return null;
			}
#endif

			return ((IBusinessObjectInternals)charge).ResumeValidationForAllDescendantsTemporarily();
		}

		string getMessage(Job job, ChargeLine chargeLine)
		{
			return Res.GetString("c5d61fd3-8792-4599-b12e-9624f2913eff", "Whilst importing Charge Line: Job Number={0} Charge Code={1} Creditor={2}, Debtor={3} Cost OS Amount={4} Sell OS Amount={5}", job.JH_JobNum, chargeLine.ChargeCode != null ? chargeLine.ChargeCode.Code : ZString.Empty, chargeLine.Creditor != null ? chargeLine.Creditor.Key : ZString.Empty, chargeLine.Debtor != null ? chargeLine.Debtor.Key : ZString.Empty, chargeLine.CostOSAmount, chargeLine.SellOSAmount);
		}

		void setValues(Job job, Charge charge, ChargeLine chargeLine, IXmlImportLogger logger)
		{
			if (chargeLine.Department != null)
			{
				setValue(job, chargeLine, charge, ChargeLineElementType.Department, chargeLine.Department.Code, logger);
			}

			if (chargeLine.ChargeCode != null)
			{
				setValue(job, chargeLine, charge, ChargeLineElementType.ChargeCode, (ZString?)chargeLine.ChargeCode.Code, logger);
			}

			setValue(job, chargeLine, charge, ChargeLineElementType.Description, chargeLine.Description, logger);

			if (chargeLine.Creditor != null)
			{
				setValue(job, chargeLine, charge, ChargeLineElementType.Creditor, (ZString?)chargeLine.Creditor.Key, logger);
			}

			setValue(job, chargeLine, charge, ChargeLineElementType.CostAPInvoiceNumber, chargeLine.CostAPInvoiceNumber, logger);
			setValue(job, chargeLine, charge, ChargeLineElementType.CostInvoiceDate, chargeLine.CostInvoiceDate, logger);
			setValue(job, chargeLine, charge, ChargeLineElementType.CostDueDate, chargeLine.CostDueDate, logger);

			if (chargeLine.Debtor != null)
			{
				setValue(job, chargeLine, charge, ChargeLineElementType.Debtor, (ZString?)chargeLine.Debtor.Key, logger);
			}

			if (chargeLine.Branch != null)
			{
				setValue(job, chargeLine, charge, ChargeLineElementType.Branch, chargeLine.Branch.Code, logger);
			}

			if (charge.Branch == null)
			{
				throw new DataObjectReadFailureException(Res.GetString("E34878C1-EE66-4292-899F-229F43295969", "charge branch is invalid."));
			}

			if (chargeLine.CostOSCurrency != null)
			{
				setValue(job, chargeLine, charge, ChargeLineElementType.CostOSCurrency, chargeLine.CostOSCurrency.Code, logger);
			}

			setValue(job, chargeLine, charge, ChargeLineElementType.CostOSAmount, chargeLine.CostOSAmount, logger);
			setValue(job, chargeLine, charge, ChargeLineElementType.CostLocalAmount, chargeLine.CostLocalAmount, logger);

			TryToSetSupplyType();

			if (chargeLine.CostGSTVATID != null)
			{
				setValue(job, chargeLine, charge, ChargeLineElementType.CostGSTVATID, chargeLine.CostGSTVATID.TaxCode, logger);
			}

			TryToSetCostGSTValue();

			if (chargeLine.SellOSCurrency != null)
			{
				setValue(job, chargeLine, charge, ChargeLineElementType.SellOSCurrency, chargeLine.SellOSCurrency.Code, logger);
			}

			setValue(job, chargeLine, charge, ChargeLineElementType.SellOSAmount, chargeLine.SellOSAmount, logger);
			setValue(job, chargeLine, charge, ChargeLineElementType.SellLocalAmount, chargeLine.SellLocalAmount, logger);

			if (chargeLine.SellGSTVATID != null)
			{
				setValue(job, chargeLine, charge, ChargeLineElementType.SellGSTVATID, chargeLine.SellGSTVATID.TaxCode, logger);
			}

			setValue(job, chargeLine, charge, ChargeLineElementType.SellInvoiceType, chargeLine.SellInvoiceType, logger);
			setValue(job, chargeLine, charge, ChargeLineElementType.DisplaySequence, chargeLine.DisplaySequence, logger);
			setValue(job, chargeLine, charge, ChargeLineElementType.SupplierReference, chargeLine.SupplierReference, logger);

			var checker = ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>();
			if (checker.IsReceivablesCashAdvanceFunctionalityEnabled)
			{
				setValue(job, chargeLine, charge, ChargeLineElementType.ARCashAdvanceRequired, chargeLine.ARCashAdvanceRequired, logger);
			}
			if (checker.IsPayablesCashAdvanceFunctionalityEnabled)
			{
				setValue(job, chargeLine, charge, ChargeLineElementType.APCashAdvanceRequired, chargeLine.APCashAdvanceRequired, logger);
			}

			if (chargeLine.SellReference.HasValue)
			{
				setValue(job, chargeLine, charge, ChargeLineElementType.SellReference, chargeLine.SellReference, logger);
			}

			// Setting JR_OSCost/SellAmt would set JR_Calc_Cost/SellRatingBehavior = NEW
			// To persist JR_Calc_Cost/SellRatingBehavior, set it after setting JR_OSCost/SellAmt
			if (chargeLine.CostRatingBehaviour != null && chargeLine.CostRatingBehaviour.Code.HasValue && !charge.JR_Calc_CostRatingBehaviorInfo.ReadOnly)
			{
				charge.JR_Calc_CostRatingBehavior = chargeLine.CostRatingBehaviour.Code.Value;
			}

			if (chargeLine.SellRatingBehaviour != null && chargeLine.SellRatingBehaviour.Code.HasValue && !charge.JR_Calc_SellRatingBehaviorInfo.ReadOnly)
			{
				charge.JR_Calc_SellRatingBehavior = chargeLine.SellRatingBehaviour.Code.Value;
			}

			TryToSetGovernmentReportingChargeCodes();

			TryToSetPlaceOfSupply();

			if (chargeLine.SellIsPosted.HasValue || chargeLine.CostIsPosted.HasValue || chargeLine.SellPostedTransactionNumber.HasValue)
			{
				logger.Log(Enterprise.Integration.LogType.Warning, Res.GetString("b6531a4c-d7e9-41ab-82aa-2f275ce089ec", "'{0}', '{1}' and '{2}' should not be populated for Import. {3}", "CostIsPosted", "RevenueIsPosted", "ARInvoiceNumber", getMessage(job, chargeLine)));
			}

			void TryToSetGovernmentReportingChargeCodes()
			{
				if (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.Value)
				{
					if (chargeLine.GovernmentReportingSellChargeCode.HasValue)
					{
						setValue(job, chargeLine, charge, ChargeLineElementType.GovernmentReportingSellChargeCode, chargeLine.GovernmentReportingSellChargeCode, logger);
					}

					if (chargeLine.GovernmentReportingCostChargeCode.HasValue)
					{
						setValue(job, chargeLine, charge, ChargeLineElementType.GovernmentReportingCostChargeCode, chargeLine.GovernmentReportingCostChargeCode, logger);
					}
				}
			}

			void TryToSetSupplyType()
			{
				if (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value)
				{
					if (chargeLine.CostSupplyType != null)
					{
						setValue(job, chargeLine, charge, ChargeLineElementType.CostSupplyType, chargeLine.CostSupplyType.Code, logger);
					}

					if (chargeLine.SellSupplyType != null)
					{
						setValue(job, chargeLine, charge, ChargeLineElementType.SellSupplyType, chargeLine.SellSupplyType.Code, logger);
					}
				}
			}

			void TryToSetCostGSTValue()
			{
				if (!charge.JR_IsCostTaxAmountOverridden)
				{
					return; //setting JR_IsCostTaxAmountOverridden to true on shipment level charge is not supported currently as tax field is readonly on billing tab now
				}

				var isValueSet = false;
				var oldFlagValue = charge.JR_IsCostTaxAmountOverridden;
				var oldGSTAmt = charge.JR_OSCostGSTAmt_Calc;
				try
				{
					charge.JR_IsCostTaxAmountOverridden = true;
					isValueSet = setValue(job, chargeLine, charge, ChargeLineElementType.CostOSGSTVATAmount, chargeLine.CostOSGSTVATAmount, logger);
				}
				finally
				{
					if (!isValueSet)
					{
						charge.JR_IsCostTaxAmountOverridden = oldFlagValue;
						if (charge.JR_IsCostTaxAmountOverridden)
						{
							charge.JR_OSCostGSTAmt_Calc = oldGSTAmt;
						}
					}
				}
			}

			void TryToSetPlaceOfSupply()
			{
				if (PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(charge.Company))
				{
					if (chargeLine.CostPlaceOfSupply != null)
					{
						setValue(job, chargeLine, charge, CostPlaceOfSupplyElement.Location, chargeLine.CostPlaceOfSupply.Location.Code, logger);
						setValue(job, chargeLine, charge, CostPlaceOfSupplyElement.LocationType, chargeLine.CostPlaceOfSupply.LocationType.Code, logger);
					}
					if (chargeLine.SellPlaceOfSupply != null)
					{
						setValue(job, chargeLine, charge, SellPlaceOfSupplyElement.Location, chargeLine.SellPlaceOfSupply.Location.Code, logger);
						setValue(job, chargeLine, charge, SellPlaceOfSupplyElement.LocationType, chargeLine.SellPlaceOfSupply.LocationType.Code, logger);
					}
				}
			}
		}

		Charge insertChargeLine(Job job, ChargeLine chargeLine, IXmlImportLogger logger)
		{
			Charge charge = job.Charges.AddNew();
			setValues(job, charge, chargeLine, logger);
			return charge;
		}

		Charge updateChargeLine(Job job, ChargeLine chargeLine, IXmlImportLogger logger, bool insertIfNotFound)
		{
			Charge charge = null;
			string error = string.Empty;
			Charge[] charges = GetMatchingCharges(job, chargeLine, logger);

			if (charges == null)
			{
				error = Res.GetString("75a21189-b232-4b43-88aa-68e85321dcae", "No matching criteria specified.");
			}
			else if (charges.Length > 1)
			{
				error = Res.GetString("96c73a47-52b5-4ca3-9117-63b5c69dc96f", "Multiple charges found when updating Charge Line.");
			}
			else if (charges.Length == 0 && insertIfNotFound)
			{
				charge = insertChargeLine(job, chargeLine, logger);
			}
			else if (charges.Length == 0 && !insertIfNotFound)
			{
				error = Res.GetString("fb2cb63c-ca44-4635-8444-1118f53b10fc", "Charge not found when updating Charge Line.");
			}
			else
			{
				charge = charges[0];
				setValues(job, charge, chargeLine, logger);
			}

			if (!error.IsNullOrEmpty())
			{
				throw new DataObjectReadFailureException(error);
			}

			return charge;
		}

		void deleteChargeLine(Job job, ChargeLine chargeLine, IXmlImportLogger logger)
		{
			string error = string.Empty;
			Charge[] charges = GetMatchingCharges(job, chargeLine, logger);

			if (charges == null)
			{
				error = Res.GetString("75a21189-b232-4b43-88aa-68e85321dcae", "No matching criteria specified.");
			}
			else if (charges.Length == 0)
			{
				error = Res.GetString("c3bbe44c-1ceb-4f67-93f6-21925adf9bbd", "No charges found when deleting Charge Line.");
			}
			else if (charges.Length > 0)
			{
				if (charges.All(charge => charge.CanDelete))
				{
					foreach (Charge charge in charges)
					{
						charge.Delete();
					}
				}
				else
				{
					foreach (Charge charge in charges)
					{
						if (!charge.CanDelete)
						{
							error += charge.ReasonForNotAbleToDelete + "\r\n";
						}
					}
				}
			}

			if (!error.IsNullOrEmpty())
			{
				throw new DataObjectReadFailureException(error);
			}
		}

		object getValue(SchemaColumn column, ZString value)
		{
			return Helpers.GetValue(column, value, Company.PK, Factory);
		}

		bool setValue<T>(Job job, ChargeLine chargeLine, Charge businessObj, Enum xmlFieldName, T? value, IXmlImportLogger logger)
			where T : struct, IZType
		{
			Action<ZString> onPropertyReadOnlyAction = propertyName =>
				{
					string postedMessage = string.Empty;

					if (businessObj.IsCostPosted)
					{
						postedMessage += "  " + Res.GetString("a0ac97ef-7528-4a3a-b68d-3777e793a22a", "The charge's cost is posted.");
					}

					if (businessObj.IsRevenuePosted)
					{
						postedMessage += "  " + Res.GetString("f7498422-741b-45d1-a6bd-37c1f808a7f8", "The charge's revenue is posted.");
					}

					if (businessObj.JR_IsApportioned)
					{
						postedMessage += "  " + Res.GetString("c7e1812d-796c-4540-a2ab-c1e128b88ccc", "The charge is apportioned.");
					}

					logger.Log(Enterprise.Integration.LogType.Warning, Res.GetString("8bb87546-c25a-4c49-bc74-6e4c761f2b99", "{0}\r\n{1} is read-only and was not updated.{2}", getMessage(job, chargeLine), propertyName, postedMessage));
				};

			if (mapping[xmlFieldName].Name == JobChargeSchema.Constants.JR_OSCostGSTAmt)
			{
				return Helpers.SetValue(businessObj, xmlFieldName, value, mapping, onPropertyReadOnlyAction: onPropertyReadOnlyAction, customValueSetter: (val) => businessObj.JR_OSCostGSTAmt_Calc = new ZDecimal(val));
			}
			else
			{
				return Helpers.SetValue(businessObj, xmlFieldName, value, mapping, onPropertyReadOnlyAction: onPropertyReadOnlyAction);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Matching criteria fixed label.")]
		ZQuery getQueryFromMatchingCriteriaCollection(Job job, List<MatchingCriteria> matchingCriteriaCollection, IXmlImportLogger logger)
		{
			ZQuery query;
			if (job.IsInDatabase)
			{
				var dbQuery = new ZDBOnlyQuery(typeof(JobCharge));
				dbQuery.AddSubQuery(JobChargeSchema.PK, GetExcludedFromMatchingSubQuery(job.PK), JoinCondition.And);
				query = dbQuery;
			}
			else
			{
				query = new ZQuery();
			}
			query.AddToFilter(JobChargeSchema.JR_JH, job.PK);

			CodeMapper codeMapper = new CodeMapper(DataProvider, logger, Factory);

			foreach (MatchingCriteria matchingCriteria in matchingCriteriaCollection)
			{
				string code = null;

				switch (matchingCriteria.FieldName)
				{
					case "ChargeCode":
						code = Constants.OrgPatternMatchOverrideRelationships.ChargeCodes;
						break;

					case "SellOSCurrency":
					case "CostOSCurrency":
						code = Constants.OrgPatternMatchOverrideRelationships.Currency;
						break;

					case "Creditor":
						code = Constants.OrgPatternMatchOverrideRelationships.Organisation;
						break;
				}

				if (!string.IsNullOrEmpty(code))
				{
					matchingCriteria.Value = codeMapper.GetMappedOrInput(matchingCriteria.Value.Value, code, 0);
				}

				ChargeLineElementType key = GetFieldKey(matchingCriteria);
				SchemaColumn column;

				if (mapping.TryGetValue(key, out column) && matchingCriteria.Value.HasValue)
				{
					object value = getValue(column, matchingCriteria.Value.Value);
					query.AddToFilter(column, value);
				}
				else if (!filterMapping.ContainsKey(key))
				{
					throw new MatchingCriteriaException(string.Format(CultureInfo.CurrentCulture, "Unable to find SchemaColumn for MatchingCriteria: {0}", matchingCriteria.FieldName.ToString()));
				}
			}
			return query;
		}

		ZDBOnlySubQuery GetExcludedFromMatchingSubQuery(ZGuid jobPK)
		{
			ZDBOnlySubQuery result = new ZDBOnlySubQuery(typeof(JobCharge), JobChargeSchema.PK, true);
			ZDBOnlySubQuery journalLines = new ZDBOnlySubQuery(typeof(AccTransactionLines), JobChargeSchema.JR_AL_ARLine);
			journalLines.AddToFilter(AccTransactionLinesSchema.AL_JH, jobPK);
			ZDBOnlySubQuery journal = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionLinesSchema.AL_AH);
			journal.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.JobCosting);
			journal.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.JobRevenueJournal);

			journalLines.AddSubQuery(journal, JoinCondition.And);
			result.AddSubQuery(journalLines, JoinCondition.And);

			return result;
		}

		bool MatchCriterias(Charge charge, List<MatchingCriteria> matchingCriteriaCollection)
		{
			bool result = true;

			foreach (MatchingCriteria matchingCriteria in matchingCriteriaCollection)
			{
				var key = GetFieldKey(matchingCriteria);
				MatchingCriteriaDelegate criteriaDelegate;
				if (filterMapping.TryGetValue(key, out criteriaDelegate))
				{
					result &= criteriaDelegate(charge, matchingCriteria);
					if (!result)
					{
						break;
					}
				}
			}

			return result;
		}

		ChargeLineElementType GetFieldKey(MatchingCriteria matchingCriteria)
		{
			ChargeLineElementType key;

			if (matchingCriteria.FieldName.HasValue)
			{
				if (!Enum.TryParse(matchingCriteria.FieldName, true, out key))
				{
					throw new Exception(string.Format("Invalid MatchingCriteria FieldName: {0}", matchingCriteria.FieldName.Value));
				}
			}
			else
			{
				throw new Exception("MatchingCriteria FieldName not specified.");
			}

			return key;
		}

		Charge[] GetMatchingCharges(Job job, ChargeLine chargeLine, IXmlImportLogger logger)
		{
			Charge[] charges = null;

			if (chargeLine?.ImportMetaData?.MatchingCriteriaCollection?.Any() ?? false)
			{
				var primaryKeyMatchingCriteria = chargeLine.ImportMetaData.MatchingCriteriaCollection.FirstOrDefault(x => x.FieldName.HasValue && x.FieldName.Value.EqualsIgnoringCase(nameof(ZDataTable.PrimaryKey)));

				if (primaryKeyMatchingCriteria != null)
				{
					var primaryKeyQuery = primaryKeyMatchingCriteria.Value.HasValue && ZGuid.TryParse(primaryKeyMatchingCriteria.Value.Value, out var jobChargePK)
						? new ZQuery(JobChargeSchema.PK, jobChargePK)
						: ZQuery.NoResultQuery;

					charges = Factory.Load<Charge>(primaryKeyQuery);
				}
				else
				{
					charges = Factory.Load<Charge>(getQueryFromMatchingCriteriaCollection(job, chargeLine.ImportMetaData.MatchingCriteriaCollection, logger)).
						Where(x => MatchCriterias(x, chargeLine.ImportMetaData.MatchingCriteriaCollection)).ToArray();
				}
			}

			return charges;
		}

		#endregion
	}
}
