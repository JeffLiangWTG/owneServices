using System;
using System.Linq;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class UnapprovedTransactionImporter : IProcessor
	{
		public UnapprovedTransactionImporter(ZGuid companyPK, IJobInvoicingPlugIn plugIn)
		{
			this.companyPK = companyPK;
			this.plugIn = plugIn;
		}

		readonly ZGuid companyPK;
		readonly IJobInvoicingPlugIn plugIn;

		public void Process(INotifications notifications, CancellationToken token
#if DEBUG
			= new CancellationToken()
#endif
		)
		{
			var filter = new ZQuery();
			var consol = plugIn as IJobCostingPlugIn;

			if (consol != null)
			{
				var jobConsolFilter = new ZQuery(AccTransactionHeaderSchema.AH_JobNumber, consol.JK_UniqueConsignRef);
				jobConsolFilter.AddToFilter(AccTransactionHeaderSchema.AH_JH, null);
				filter.AddToFilter(jobConsolFilter);

				var shipmentJobNumbers = consol.CostSupporter.ShipmentsList.Select(x => x.JobNumber).ToArray();
				var jobShipmentFilter = new ZQuery(AccTransactionHeaderSchema.AH_JobNumber, shipmentJobNumbers);
				jobShipmentFilter.AddToFilter(AccTransactionHeaderSchema.AH_JH, SQLComparisonOperator.NotEqual, null);
				filter.AddToFilter(jobShipmentFilter, JoinCondition.Or);
			}
			else
			{
				var jobPluginFilter = new ZQuery(AccTransactionHeaderSchema.AH_JobNumber, plugIn.JobNumber);
				jobPluginFilter.AddToFilter(AccTransactionHeaderSchema.AH_JH, SQLComparisonOperator.NotEqual, null);
				filter.AddToFilter(jobPluginFilter);
			}

			var originalDepartment = GlbDepartment.CurrentDepartment;

			var converter = new UnapprovedTransactionConverter(Factory, filter);
#if DEBUG
			UnapprovedTransactionConverterForTest = converter;
#endif
			using (converter.GetUnapprovedTransactionCandidateCollectionValidationSuspender())
			{
				if (converter.Candidates.Any())
				{
					var company = Factory.Load<GlbCompany>(companyPK);
					if (company != null)
					{
						var firstBranch = company.FirstActiveBranch;
						if (firstBranch != null)
						{
							using (new TemporaryUserContext { BranchPK = firstBranch.PK.ToGuid(), DepartmentPK = originalDepartment.PK.ToGuid() }.Set())
							{
								if (!AccountingConfigurationRegistry.Instance.AutoImportIntercompanyInvoices.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
								{
									foreach (InvoicingBase header in converter.Candidates)
									{
										if (header.AH_Ledger == LedgerTypes.AccountsReceivable &&
											(header.AH_TransactionType == TransactionTypes.Invoice || header.AH_TransactionType == TransactionTypes.CreditNote))
										{
											var postFromCompany = header.Branch.Company;

											var branchForLogin = IntercompanyTransactionImportHelper.GetAPBranchForAutoImport(header, x => true).BranchForIntercompanyImport;
											if (branchForLogin != null && branchForLogin.Company.PK == company.PK)
											{
												var departmentForLogin = IntercompanyTransactionImportHelper.GetDepartmentToLogIntoAPCompany(branchForLogin, originalDepartment);
												using (new TemporaryUserContext { BranchPK = branchForLogin.PK.ToGuid(), DepartmentPK = departmentForLogin.PK.ToGuid() }.Set())
												{
													var convertedHeader = converter.ConvertToAP(header, false, true, isAutoImport: true);

													var success = false;

													if (!convertedHeader.HasErrors)
													{
														if (GetOrdinal(convertedHeader.AuthorisationLevel) <= GetOrdinal(GetMaxAuthorisationLevel(postFromCompany)))
														{
															convertedHeader.Factory.Save();
															success = true;
														}
														else
														{
															convertedHeader.AddRowError(Res.GetString("01b653ef-a52e-4868-a655-c92e5cb2bbad",
																"The Authorization Level '{0}' required to post this transaction exceeds Maximum Authorization Level specified in the 'Accounting > Intercompany Posting Configuration' registry item",
																convertedHeader.AuthorisationLevel));
														}
													}
													if (!success)
													{
														convertedHeader.ReleaseAllMutexOnInvoice();
														var email = new IntercompanyTransactionImportEmail(convertedHeader);
														email.Send();
													}
													else if (convertedHeader.HasNegativeComplianceLinesWhenCreating)
													{
														notifications.AddWarning(AccountingConstants.GetComplianceDocumentNegativeMessageWithInfo(convertedHeader.AH_Ledger, convertedHeader.AH_TransactionType, convertedHeader.AH_TransactionNum));
													}
												}
											}
											else
											{
												var errorMessage = Res.GetString("0111e7f2-09e5-4cc8-ba71-fd9aa69b8ce8", "Intercompany Invoice cannot be auto-imported as Transaction Branch or Company cannot be set with reference to the invoice debtor organization proxy. Please try to use manual import.");
												var email = new IntercompanyTransactionImportEmail(header, errorMessage);
												email.Send();
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		static ZString GetMaxAuthorisationLevel(GlbCompany company)
		{
			var result = ZString.Empty;
			if (company != null)
			{
				result = TransactionHeader.GetMaxCompanyAuthorizationLevel(company.GC_Code);
			}
			return result;
		}

		int GetOrdinal(ZString authorisationLevel)
		{
			return AuthorisationRequirementForLevelComparison.GetAuthorisationRequirementWeight(authorisationLevel);
		}

		[NonSerialized]
		CostVarianceApprovalAuthorisationRequirement fAuthorisationRequirementForLevelComparison;
		CostVarianceApprovalAuthorisationRequirement AuthorisationRequirementForLevelComparison
		{
			get
			{
				return fAuthorisationRequirementForLevelComparison ??
					(fAuthorisationRequirementForLevelComparison = new CostVarianceApprovalAuthorisationRequirement());
			}
		}

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		[NonSerialized]
		BusinessObjectFactory factory;

#if DEBUG
		public UnapprovedTransactionConverter UnapprovedTransactionConverterForTest { get; set; }
#endif
	}
}
