using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing.USSalesTax;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.MasterFiles.Progress;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class MonthlyUsageBilling : UsageBilling
	{
		public MonthlyUsageBilling(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Billing Systems

		[BusinessObjectMaxLengthTestExclude()]
		[MaxLength(200)]
		public ZString BillingSystemsText
		{
			get
			{
				ZString result = "";
				var enabledCodeList = EnabledBillingSystems.Select(s => s.SystemCode).ToArray();
				if (enabledCodeList.Length == BillingSystems.Count)
				{
					result = "ALL";
				}
				else if (enabledCodeList.Length > 0)
				{
					result = string.Join(", ", enabledCodeList);
				}

				return result;
			}
			set
			{
				foreach (var item in BillingSystems)
				{
					item.IsEnabled = string.Compare(value, "ALL", StringComparison.OrdinalIgnoreCase) == 0 || value.Contains(item.SystemCode, StringComparison.OrdinalIgnoreCase);
				}

				BillingSystemsTextInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo BillingSystemsTextInfo
		{
			get { return GetZPropertyInfo(nameof(BillingSystemsText)); }
		}

		public BillingSystemList BillingSystems
		{
			get { return billingSystems ?? (billingSystems = new BillingSystemList()); }
		}
		BillingSystemList billingSystems;

		BillingSystem[] EnabledBillingSystems
		{
			get { return BillingSystems.Where(s => s.IsEnabled).ToArray(); }
		}

		#endregion

		#region Organisation Bills

		public OrganisationBillCollection OrganisationBills
		{
			get { return organisationBills ?? (organisationBills = new OrganisationBillCollection(Factory)); }
		}
		OrganisationBillCollection organisationBills;

		#endregion

		#region All Usages

		public OrgSystemUsageCollection AllUsages
		{
			get { return allUsages ?? (allUsages = OrgSystemUsageCollection.CreateAndBuild(OrganisationBills)); }
		}
		OrgSystemUsageCollection allUsages;

		#endregion

		#region Generate Report

		public void GenerateReport(IEdiProgress progress)
		{
			if (!Globals.IsTest)
			{
				FactoryForGenerate = null;
			}

			OrganisationBills.RemoveAll();

			ZGuid organisationPK = OrganisationPK.IsValid ? OrganisationPK : ZGuid.Empty;
			BillingRunContext context = new BillingRunContext(FactoryForGenerate, ZDateTime.Today, DateTo, organisationPK, progress);
			context.IncludeStl = LicenceMode != LicenceModeConstants.Codes.Odpl;
			context.IncludeOdpl = LicenceMode != LicenceModeConstants.Codes.STL;

			var enabledBillingSystems = EnabledBillingSystems;
			SystemBill[] systemBills = LoadSystemBills(progress, enabledBillingSystems, context);
			if (progress.SafeIsCancelled())
			{
				return;
			}

			OrganisationBill[] bills = CreateOrganisationBills(systemBills, progress, enabledBillingSystems, context);
			if (progress.SafeIsCancelled())
			{
				return;
			}

			OrganisationBills.AddRange(bills);
			if (allUsages != null)
			{
				allUsages.Rebuild();
			}

			EnterpriseUsage.ClearCache();
		}

		public static SystemBill[] LoadSystemBills(IEdiProgress progress, IEnumerable<BillingSystem> enabledBillingSystems, BillingRunContext context)
		{
			List<SystemBill> result = new List<SystemBill>();

			// Do OnDemand first so transactional systems can query the total licence units for volume discounts
			var odplBilling = enabledBillingSystems.FirstOrDefault(x => x.SystemCode == BillingConstants.BillingSystem.ODM);
			if (odplBilling != null)
			{
				var systemBills = odplBilling.LoadSystemBills(context);
				result.AddRange(systemBills);
				context.ModuleUsersService = new ModuleUsers(systemBills.Cast<ODPL.OdplSystemBill>().ToArray(), context.PeriodStart, context.GenerateDate);
			}
			else
			{
				context.ModuleUsersService = new ModuleUsers(context.GenerateDate);
			}

			var borderWiseBilling = (BorderWise.BorderWiseBillingSystem)enabledBillingSystems.FirstOrDefault(x => x.SystemCode == BillingConstants.BillingSystem.BorderWise);
			if (borderWiseBilling != null)
			{
				var systemBills = borderWiseBilling.LoadSystemBills(context);
				borderWiseBilling.CombineWithOdpl(systemBills, result.Cast<ODPL.OdplSystemBill>());
				result.AddRange(systemBills);
			}

			foreach (BillingSystem billingSystem in enabledBillingSystems
				.Where(x => x.SystemCode != BillingConstants.BillingSystem.ODM
						&& x.SystemCode != BillingConstants.BillingSystem.BorderWise))
			{
				if (progress.SafeIsCancelled())
				{
					break;
				}
				var systemBills = billingSystem.LoadSystemBills(context);
				result.AddRange(systemBills);
			}

			return result.ToArray();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public OrganisationBill[] CreateOrganisationBills(SystemBill[] systemBills, IEdiProgress progress, IEnumerable<BillingSystem> enabledBillingSystems, BillingRunContext context)
		{
			var branchQuery = new ZQuery(GlbBranchSchema.PK, systemBills.Where(x => !x.InvoiceGroupKey.BranchPK.IsEmpty).Select(x => x.InvoiceGroupKey.BranchPK.ToGuid()).Distinct());
			var branches = Factory.Load<GlbBranch>(branchQuery);
			UpdateIsBackPostAvailable(branches);
			context.IsBackPost = IsBackPostAvailable && IsBackPostAllowed;
			var dateForExchangeRate = context.DateForExchangeRate;

			progress.SafeSetExpectedCount(systemBills.Select(s => s.InvoiceGroupKey).Distinct().Count());

			IEnumerable<string> enabledSystemCodes = enabledBillingSystems.Select(s => s.SystemCode);
			List<OrganisationBill> allBills = new List<OrganisationBill>();
			context.SystemMinimumFeesService = new SystemMinimumFees(systemBills);

			AssignMinimumFeeOwnerPerDatabase(systemBills, context);

			foreach (var branchBills in systemBills.GroupBy(s => s.InvoiceGroupKey.BranchPK))
			{
				ZGuid branchPK = branchBills.Key;
				using (BillingInvoicingHelper.BranchContext(branchPK))
				{
					// NOTE: Every branch should have its own factory due to currency conversion issues
					BusinessObjectFactory branchFactory = new BusinessObjectFactory();

					foreach (var invoiceGroup in branchBills.GroupBy(s => s.InvoiceGroupKey))
					{
						if (progress.SafeIsCancelled())
						{
							return null;
						}
						progress.SafeUpdateCurrentCount("Calculating amounts...");

						// NOTE: OrganisationBill MUST be created with related branch factory
						OrganisationBill organisationBill = new OrganisationBill(branchFactory, branchPK, invoiceGroup.Key.InvoicedOrganisationPK, invoiceGroup.Key.Currency, dateForExchangeRate, context);
						allBills.Add(organisationBill);

						// Sort On Demand first, then alphabetically
						foreach (var systemBill in invoiceGroup
							.OrderBy(s => s.PeriodStart)
							.ThenBy(s => s.SystemCode == BillingConstants.BillingSystem.ODM ? 0 : 1)
							.ThenBy(s => s.SystemDescription))
						{
							organisationBill.AddSystemBill(systemBill);
						}
						organisationBill.DateTo = context.DateToInclusive;
						organisationBill.SetEnabledSystemCodes(enabledSystemCodes);
						organisationBill.CalculateAll(SystemMinimumAmountToBill);
					}
				}
			}

			OrganisationBill[] result = allBills.ToArray();

			if (!context.IsPreviewOnly)
			{
				ValidateMultipleBills(result, enabledBillingSystems);
			}
			return result;
		}

		/// <summary>
		/// Pick a SystemUsage per database to be the owner of the minimum fee.
		/// It must be for the billing period.
		/// Prefer usage with an owning licence that is the database owner licence, since database owner pays for per-database charges.
		/// Prefer ODPL SystemUsage, then any usage for an ODM pricelist.
		/// </summary>
		void AssignMinimumFeeOwnerPerDatabase(SystemBill[] systemBills, BillingRunContext context)
		{
			foreach (var usagesForDatabase in systemBills
				.SelectMany(x => x.SystemUsages)
				.Where(x => x.PeriodStart == context.PeriodStart
					&& !x.User.DatabasePK.IsEmpty
					&& x.User.UsageOwnerLicence != null
					&& x.PriceHeader != null)
				.GroupBy(x => x.User.DatabasePK))
			{
				var db = Factory.Load<LicenceDatabase>(usagesForDatabase.Key);
				if (db != null && db.LD_LicenceType == DatabaseTypes.Codes.Production)
				{
					var dbOwner = db.UsageOwnerOrFirstLicence;
					var minFeeUsage = usagesForDatabase
						.OrderBy(x => x.User.UsageOwnerLicence == dbOwner ? 0 : 1)
						.ThenBy(x => x.SystemCode == BillingConstants.BillingSystem.ODM ? 0 : 1)
						.ThenBy(x => x.PriceHeader.L6_SystemCode.ToString() == BillingConstants.PriceHeaderType.ODM ? 0 : 1)
						.ThenBy(x => x.User.UsageOwnerLicence.LA_AgreedLiveDate.IsEmpty ? 1 : 0) // prefer has live date
						.ThenBy(x => x.User.UsageOwnerLicence.LA_AgreedLiveDate)
						.ThenBy(x => x.User.CompanyCode)
						.First();

					minFeeUsage.IsMinimumFeeOwner = true;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		static void ValidateMultipleBills(OrganisationBill[] bills, IEnumerable<BillingSystem> billSystems)
		{
			// Check one bill per system and enterprise per paying org
			// since deposits and discounts assume only one bill per Org.
			// We could relax this restriction if there are no deposits/discounts, but safer to always enforce it for now.
			foreach (BillingSystem billingSystem in billSystems)
			{
				foreach (var billsPerPayingOrg in bills
					.Where(bill => bill.IsBilled && bill.IsBillable && bill.SystemBills.Cast<SystemBill>().Any(s => s.SystemCode == billingSystem.SystemCode))
					.GroupBy(bill => bill.OrganisationPK))
				{
					if (billsPerPayingOrg.Count() > 1)
					{
						foreach (var billsPerUsingEnterprise in billsPerPayingOrg
							.Where(x => x.SystemBills
										.Cast<SystemBill>()
										.FirstOrDefault(s => s.SystemCode == billingSystem.SystemCode).SystemUsages.Any())
							.GroupBy(x => x.SystemBills
										.Cast<SystemBill>()
										.FirstOrDefault(s => s.SystemCode == billingSystem.SystemCode)
										.SystemUsages
										.First()
										.EnterpriseCode))
						{
							if (billsPerUsingEnterprise.Count() > 1)
							{
								foreach (var bill in billsPerUsingEnterprise)
								{
									string msg = "Multiple " + billingSystem.SystemDescription + " bills to the same Org for the same using enterprise are not supported - enterprise code: " + billsPerUsingEnterprise.Key;
									bill.AddRowError(msg);
								}
							}
						}
					}
				}
			}
		}

		#endregion

		#region Create Invoices

		ZDateTime PostAndInvoiceDateOverride
		{
			get
			{
				ZDateTime postAndInvoiceDateOverride = ZDateTime.Empty;

				var backPost = IsBackPostAvailable && IsBackPostAllowed;

				if (backPost)
				{
					var today = ZDateTime.Today;
					postAndInvoiceDateOverride = today.AddDays(-today.Day);
				}

				return postAndInvoiceDateOverride;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.GC.Collect", Scope = "member", Target = "Enterprise.Client.EDI.Billing.Business.#CreateInvoices(OrganisationBill[], IEdiProgress)")]
		public void CreateInvoices(OrganisationBill[] billsToCreate, IEdiProgress progress)
		{
			List<ZipStream> partnerClientSummaries = new List<ZipStream>();
			var postAndInvoiceDateOverride = PostAndInvoiceDateOverride;
			var invoicesCount = 0;

			progress.SafeSetExpectedCount(billsToCreate.Length);
			using (var usSalesTaxCalculator = ObjectFactory.Get<IUSSalesTaxCalculator>())
			{
				foreach (var branchBills in billsToCreate.GroupBy(s => s.BranchPK))
				{
					using (BillingInvoicingHelper.BranchContext(branchBills.Key.ToGuid()))
					{
						foreach (var partnerBills in branchBills.GroupBy(s => s.PartnerOrgPK))
						{
							foreach (OrganisationBill organisationBill in partnerBills)
							{
								if (progress.SafeIsCancelled())
								{
									return;
								}
								progress.SafeUpdateCurrentCount("Processing " + organisationBill.OrganisationCode);

								if (partnerBills.Key.IsEmpty)
								{
									organisationBill.CreateInvoice(postAndInvoiceDateOverride, usSalesTaxCalculator: usSalesTaxCalculator);

									if (++invoicesCount % MaxInvoicesCountPerGC == 0)
									{
										GC.Collect();
									}
								}
								else
								{
									partnerClientSummaries.Add(organisationBill.CreateSummaryInExcel());
								}
							}

							if (partnerClientSummaries.Count > 0)
							{
								SendPartnerEmail(partnerBills.First().Partner, partnerClientSummaries);
								partnerClientSummaries.Clear();
							}
						}
					}
				}
			}
		}

		const int MaxInvoicesCountPerGC = 20;

		void SendPartnerEmail(EDIOrgHeader partner, List<ZipStream> partnerClientSummaries)
		{
			ZString recipients = partner.LicCompany.SelfBilling.PartnerEmail;

			EmailDef mail = new EmailDef();
			mail.Subject = partner.OH_Code + " client billing summaries " + DateTo.ToString("MMM yyyy", CultureInfo.InvariantCulture);
			foreach (string recipient in recipients.ToString().Split(new char[] { ';', ',', ' ' }))
			{
				string trimmed = recipient.Trim();
				if (!string.IsNullOrEmpty(trimmed))
				{
					mail.AddRecipientForUserCommunication(trimmed);
				}
			}

			using (MemoryStream outputStream = new MemoryStream())
			{
				ZipCreator creator = new ZipCreator();
				creator.ZipStream(partnerClientSummaries, outputStream);
				mail.Attachments.Add(new AttachmentDef("Billing Summaries.zip", outputStream.ToArray()));
			}

			Env.OutgoingMailManager.Create(partner.Factory, mail);
			partner.Factory.Save();
		}

		#endregion
	}
}

