using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing.USSalesTax;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.MasterFiles.Progress;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.RemoteDesktopServices;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business.Maintenance
{
	public interface IPerDatabaseChargeDecider
	{
		bool IsOwner(LicenceHeader licHeader, string moduleCode);
	}

	public class MaintenanceBilling : NonPersistentBusinessObject, IObsoleteValidation
	{
		public MaintenanceBilling(BusinessObjectFactory factory)
			: base(factory)
		{
			attachmentFolder = theAttachmentFolder;
		}

		public BusinessObjectFactory BillsFactory
		{
			get { return billsFactory ?? (billsFactory = new BusinessObjectFactory()); }
		}

		BusinessObjectFactory billsFactory;

		ZDateTime GenerateDate
		{
			get
			{
				if (generateDate.IsEmpty)
				{
					generateDate = ZDateTime.Today;
				}
				return generateDate;
			}
			set { generateDate = value; }
		}
		ZDateTime generateDate;

		#region Business Object Overrides

		public override bool HasChanges
		{
			get { return HasBillChanges; }
			set { }
		}

		#endregion

		#region Report Parameters

		public MaintenanceFilter Filter
		{
			get { return filter ?? (filter = new MaintenanceFilter(Factory)); }
		}
		MaintenanceFilter filter;

		#endregion

		public ZBool HasBillChanges
		{
			get { return Bills.Cast<MaintenanceBill>().Any(s => s.HasChanges); }
		}

		public ZBool HasSaveErrors
		{
			get { return Bills.Cast<MaintenanceBill>().Any(s => s.HasSaveError); }
		}

		#region AttachmentFolder

		[CargoWise.ComponentModel.MaxLength(260)]
		public ZString AttachmentFolder
		{
			get { return attachmentFolder; }
			set
			{
				SetNonPersistentPropertyValue(AttachmentFolderInfo, ref attachmentFolder, value);
				theAttachmentFolder = attachmentFolder;
				if (!IsValidationSuspended)
				{
					ValidateAttachmentFolder();
				}
			}
		}
		ZString attachmentFolder;
		public ZPropertyInfo AttachmentFolderInfo { get { return GetZPropertyInfo(nameof(AttachmentFolder)); } }

		string GetMappedAttachmentFolder()
		{
			return ObjectFactory.Get<IFileMapper>().IsRemote
				? ObjectFactory.Get<IMappedClientPath>().GetMappedPath(AttachmentFolder)
				: (string)AttachmentFolder;
		}

		public void ValidateAttachmentFolder()
		{
			AttachmentFolderInfo.ClearAllNotifications();
			if (!AttachmentFolder.IsEmpty)
			{
				var mappedPath = GetMappedAttachmentFolder();
				if (mappedPath == null)
				{
					AttachmentFolderInfo.AddError("Folder cannot be accessed from the terminal server. Please use a network shared folder instead.");
				}
				else if (!Directory.Exists(mappedPath))
				{
					AttachmentFolderInfo.AddError("Folder not found.");
				}
				else if (string.IsNullOrEmpty(EDIDataRegistry.Instance.InvoiceAttachmentDocType.Value)
					|| Factory.Load<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, EDIDataRegistry.Instance.InvoiceAttachmentDocType.Value)).Length == 0)
				{
					AttachmentFolderInfo.AddError("Doc Type not found in registry " + ((IRegistryItemInternals)EDIDataRegistry.Instance.InvoiceAttachmentDocType).Location);
				}
			}
		}

		[ThreadStatic]
		static ZString theAttachmentFolder;

		#endregion

		#region Bills

		public bool? ShowPerModuleAmounts
		{
			get
			{
				var items = Recipients;
				if (items.Count > 0)
				{
					showPerModuleAmounts = items[0].ShowPerModuleAmounts;
					if (items.Cast<MaintenanceBillRecipient>().Skip(1).Any(x => x.ShowPerModuleAmounts != showPerModuleAmounts))
					{
						showPerModuleAmounts = null;
					}
				}
				return showPerModuleAmounts;
			}
			set
			{
				showPerModuleAmounts = value;
				if (value.HasValue)
				{
					foreach (MaintenanceBillRecipient item in Recipients)
					{
						item.ShowPerModuleAmounts = showPerModuleAmounts.Value;
					}
				}
			}
		}

		bool? showPerModuleAmounts = false;

		public MaintenanceBillCollection Bills
		{
			get
			{
				if (bills == null)
				{
					bills = new MaintenanceBillCollection(Factory);
					RegisterEditableChildObject(bills);

					if (!EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed)
					{
						bills.SetReadOnlyIncludingChildren(true);
					}
				}
				return bills;
			}
		}
		MaintenanceBillCollection bills;

		public MaintenanceBillRecipientCollection Recipients
		{
			get
			{
				if (recipients == null)
				{
					recipients = new MaintenanceBillRecipientCollection(Factory);
					RegisterEditableChildObject(recipients);

					if (!EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed)
					{
						recipients.SetReadOnlyIncludingChildren(true);
					}
				}
				return recipients;
			}
		}
		MaintenanceBillRecipientCollection recipients;

		#endregion

		#region Generate

		public void Generate(IEdiProgress progress)
		{
			GenerateDate = ZDateTime.Today;

			progress.SafeSetStatusAndPercentComplete("Generating...", 0);

			Bills.RemoveAll();
			Recipients.RemoveAll();

			billsFactory = null;

			LicenceHeader[] licHeaders = LoadLicenceHeaders();
			if (progress.SafeIsCancelled())
			{
				return;
			}
			progress.SafeSetStatusAndPercentComplete("Generating...", 10);

			var billInfos = CreateBillInfo(licHeaders, progress);
			if (progress.SafeIsCancelled())
			{
				return;
			}
			progress.SafeSetStatusAndPercentComplete("Generating...", 25);

			CalculateBilling(billInfos, progress);
			if (progress.SafeIsCancelled())
			{
				return;
			}
			progress.SafeSetStatusAndPercentComplete("Generating...", 50);

			if (!filter.IncludeNonBilled)
			{
				billInfos = new List<BillInfo>(billInfos.Where(s => s.delivery == null || s.delivery.L9_IsBilled));
			}

			CalculatePayingOrg(billInfos, progress);
			if (progress.SafeIsCancelled())
			{
				return;
			}
			progress.SafeSetStatusAndPercentComplete("Generating...", 70);

			List<MaintenanceBillRecipient> billRecipients = ApplyIncludedFilterAndCreateBills(billInfos, progress);
			if (progress.SafeIsCancelled())
			{
				return;
			}
			progress.SafeSetStatusAndPercentComplete("Generating...", 90);

			using (Bills.SuspendListChanged())
			{
				PopulateCollection(billInfos);

				using (Recipients.SuspendListChanged())
				{
					foreach (var billRecipient in billRecipients)
					{
						bool add = false;
						foreach (MaintenanceBill bill in billRecipient.Bills)
						{
							if (bill.StatusText != MaintenanceBill.StatusMessages.NotBilled)
							{
								add = true;
								break;
							}
						}

						if (billRecipient.Fees.Any())
						{
							add = true;
						}

						if (add)
						{
							Recipients.Add(billRecipient);
						}
					}
				}
			}

			progress.SafeSetStatusAndPercentComplete("Generating...", 99);
		}

		LicenceHeader[] LoadLicenceHeaders()
		{
			return Filter.LoadLicenceHeaders(BillsFactory);
		}

		List<BillInfo> CreateBillInfo(LicenceHeader[] licHeaders, IEdiProgress progress)
		{
			var billInfos = new List<BillInfo>(licHeaders.Length);

			foreach (var licHeader in licHeaders)
			{
				BillInfo billInfo = new BillInfo() { licHeader = licHeader };
				billInfos.Add(billInfo);
			}

			AddFees(billInfos);

			// bulk fetch LicenceCompany
			var companyPks = billInfos.Select(x => x.LicCompanyPk).Distinct();
			var licenceCompanies = BillsFactory.Load<LicenceCompany>(new ZQuery(LicenceCompanySchema.PK, companyPks))
				.ToDictionary(x => x.PK);
			foreach (var billInfo in billInfos)
			{
				billInfo.LicCompany = licenceCompanies[billInfo.LicCompanyPk];
			}
			if (progress.SafeIsCancelled())
			{
				return null;
			}

			// bulk fetch LicenceDatabase
			var databasePks = licHeaders.Select(x => x.LA_LD).Distinct();
			BillsFactory.Load<LicenceDatabase>(new ZQuery(LicenceDatabaseSchema.PK, databasePks));
			if (progress.SafeIsCancelled())
			{
				return null;
			}

			// bulk fetch EDIOrgHeader
			var orgPks = licenceCompanies.Values.Select(x => x.LC_OH);
			BillsFactory.Load<EDIOrgHeader>(new ZQuery(OrgHeaderSchema.PK, orgPks));

			return billInfos;
		}

		void CalculateBilling(List<BillInfo> billInfos, IEdiProgress progress)
		{
			// bulk fetch InvoiceDeliveries
			var licenceCompanyPks = billInfos.Select(x => (x.licHeader?.Company ?? x.LicCompany).PK).Distinct();
			var companyPkToDeliveries = BillsFactory.Load<ClientInvoiceDelivery>(new ZQuery(ClientInvoiceDeliverySchema.L9_LC, licenceCompanyPks))
				.GroupBy(x => x.L9_LC)
				.ToDictionary(x => x.Key);

			foreach (var billInfo in billInfos)
			{
				if (progress.SafeIsCancelled())
				{
					return;
				}

				ClientInvoiceDelivery invoiceDelivery = null;
				if (billInfo.licHeader != null)
				{
					var licHeader = billInfo.licHeader;
					if (companyPkToDeliveries.TryGetValue(licHeader.LA_LC, out var deliveries))
					{
						invoiceDelivery = ClientInvoiceDeliveryCollection.FindByServerAndSystem(deliveries, licHeader.Database.LD_ServerCode, BillingConstants.BillingSystem.Maintenance);
					}
				}
				else
				{
					if (companyPkToDeliveries.TryGetValue(billInfo.LicCompany.PK, out var deliveries))
					{
						var serverCode = billInfo.fee?.Database?.LD_ServerCode ?? ZString.Empty;
						invoiceDelivery = ClientInvoiceDeliveryCollection.FindByServerAndSystem(deliveries, serverCode, BillingConstants.BillingSystem.Fee)
							?? ClientInvoiceDeliveryCollection.FindByServerAndSystem(deliveries, serverCode, BillingConstants.BillingSystem.Maintenance);
					}
				}
				billInfo.delivery = invoiceDelivery;
				if (invoiceDelivery != null && invoiceDelivery.L9_IsBilled && !invoiceDelivery.L9_OH_InvoiceTo.IsEmpty)
				{
					BillsFactory.AddFetchHint(typeof(EDIOrgHeader), invoiceDelivery.L9_OH_InvoiceTo);
				}
			}
		}

		void CalculatePayingOrg(List<BillInfo> billInfos, IEdiProgress progress)
		{
			foreach (var billInfo in billInfos)
			{
				if (progress.SafeIsCancelled())
				{
					return;
				}

				var licCompany = billInfo.LicCompany;
				var delivery = billInfo.delivery;
				ZGuid branchPk = ClientInvoiceDelivery.GetInvoicingBranchPk(delivery);

				ZGuid payingOrgPk = ClientInvoiceDelivery.CalcInvoicedOrganisationPK(delivery, licCompany.LC_OH);
				BillsFactory.AddFetchHint(typeof(EDIOrgHeader), payingOrgPk);
				billInfo.branchPk = branchPk;
				billInfo.payingOrgPk = payingOrgPk;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		List<MaintenanceBillRecipient> ApplyIncludedFilterAndCreateBills(List<BillInfo> billInfos, IEdiProgress progress)
		{
			PerDatabaseChargeDecider decider = new PerDatabaseChargeDecider();

			List<MaintenanceBillRecipient> billRecipients = new List<MaintenanceBillRecipient>();

			var branchPks = billInfos.Select(x => x.branchPk).Where(x => !x.IsEmpty).Distinct();
			var branchPkToBranch = BillsFactory.Load<GlbBranch>(new ZQuery(GlbBranchSchema.PK, branchPks))
				.ToDictionary(x => x.PK);
			var allGlbCompanyPks = branchPkToBranch.Values.Select(x => x.GB_GC).Distinct();
			var glbCompanyPkToCompany = BillsFactory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.PK, allGlbCompanyPks))
				.ToDictionary(x => x.PK);
			var loginCompany = GlbCompany.CurrentCompany;

			var payingOrgPks = billInfos.Select(x => x.payingOrgPk).Where(x => !x.IsEmpty).Distinct().ToList();
			var payingOrgPkMap = BillsFactory.Load<EDIOrgHeader>(new ZQuery(OrgHeaderSchema.PK, payingOrgPks))
				.ToDictionary(x => x.PK);
			var payingOrgPkToLicCompany = BillsFactory.Load<LicenceCompany>(new ZQuery(LicenceCompanySchema.LC_OH, payingOrgPks))
				.ToDictionary(x => x.LC_OH);
			var payingLicCompanyPks = payingOrgPkToLicCompany.Values.Select(x => x.PK);
			var payingLicCompanyPkToLicBilling = BillsFactory.Load<ClientLicenceBilling>(new ZQuery(ClientLicenceBillingSchema.L4_LC, payingLicCompanyPks))
				.ToDictionary(x => x.L4_LC);

			var payingLicCompanyPkToPartner = BuildPayCoToPartner(payingLicCompanyPks);
			var dueDateToFeeToUsage = BuildFeeToUsage(billInfos);

			if (filter.IncludeLoginCompanyInvoicesOnly)
			{
				ZGuid currentCompanyPk = Env.CurrentCompanyPK;
				billInfos.RemoveAll(billInfo =>
					branchPkToBranch.TryGetValue(billInfo.branchPk, out var branch)
					&& branch.GB_GC != currentCompanyPk);
			}

			foreach (var glbCompanyBills in billInfos.GroupBy(s => branchPkToBranch.ContainsKey(s.branchPk) ? branchPkToBranch[s.branchPk].GB_GC : ZGuid.Empty))
			{
				var glbCompanyPk = glbCompanyBills.Key;
				var glbCompany = !glbCompanyPk.IsEmpty ? glbCompanyPkToCompany[glbCompanyPk] : null ?? loginCompany;

				foreach (var branchBills in glbCompanyBills.GroupBy(s => s.branchPk))
				{
					using (BillingInvoicingHelper.BranchContext(branchBills.Key))
					{
						var factory = BillsFactory;
						var orgGroupBills = branchBills.GroupBy(s => s.CreateGroup());

						foreach (var orgBills in orgGroupBills)
						{
							dueDateToFeeToUsage.TryGetValue(orgBills.Key.DueDate, out var feeToUsage);
							var payingOrg = !orgBills.Key.InvoicedOrganisationPK.IsEmpty ? payingOrgPkMap[orgBills.Key.InvoicedOrganisationPK] : null;
							LicenceCompany payingLicCompany = null;
							if (payingOrg != null)
							{
								payingOrgPkToLicCompany.TryGetValue(payingOrg.PK, out payingLicCompany);
							}
							ClientLicenceBilling payingLicBilling = null;
							if (payingLicCompany != null)
							{
								payingLicCompanyPkToLicBilling.TryGetValue(payingLicCompany.PK, out payingLicBilling);
							}
							var invoiceCurrency = BillsFactory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, orgBills.Key.Currency);
							var isLocalCurrency = invoiceCurrency == glbCompany.GC_RX_NKLocalCurrency;
							var dateForExchangeRate = BillingInvoicingHelper.GetDateForExchangeRate(GenerateDate, isLocalCurrency, glbCompany.PK);
							var currencyConverter = CurrencyConverter.New(glbCompany, BillsFactory, dateForExchangeRate, ZArchitecture.Core.ExchangeRateType.Sell, 0);
							var localExchangeRate = invoiceCurrency != null ? currencyConverter.GetExchangeRate(invoiceCurrency) : new ZDecimal(1m);
							EDIOrgHeader partner = null;
							if (payingLicCompany != null)
							{
								payingLicCompanyPkToPartner.TryGetValue(payingLicCompany.PK, out partner);
							}
							var billRecipient = new MaintenanceBillRecipient(factory, branchBills.Key,
								payingOrg,
								payingLicCompany,
								payingLicBilling,
								orgBills.Key.DueDate,
								orgBills.Key.Currency,
								localExchangeRate,
								dateForExchangeRate,
								partner);
							billRecipients.Add(billRecipient);

							foreach (var billInfo in orgBills.Where(s => s.licHeader != null))
							{
								if (progress.SafeIsCancelled())
								{
									return null;
								}

								billInfo.bill = billRecipient.AddNewBill(billInfo.licHeader, billInfo.delivery, decider);
							}

							foreach (var billInfo in orgBills.Where(s => s.fee != null).OrderBy(s => s.fee.Company.Header.OH_Code).ThenBy(s => s.fee.L8_Order))
							{
								if (progress.SafeIsCancelled())
								{
									return null;
								}

								feeToUsage.TryGetValue(billInfo.fee.PK, out var usages);

								billRecipient.AddNewFee(billInfo.fee, billInfo.feeDate, billInfo.delivery, usages ?? Enumerable.Empty<ClientChargeableUsage>());
							}
						}
					}
				}
			}

			return billRecipients;
		}

		Dictionary<ZDateTime, Dictionary<ZGuid, IEnumerable<ClientChargeableUsage>>> BuildFeeToUsage(List<BillInfo> billInfos)
		{
			// Bulk load chargeableUsage for each fee - grouped by fee due date
			var dueDateToFeeToUsage = new Dictionary<ZDateTime, Dictionary<ZGuid, IEnumerable<ClientChargeableUsage>>>();
			foreach (var feeDueDateGroup in billInfos.Where(x => x.fee != null)
				.GroupBy(x => x.feeDate))
			{
				var query = new ZQuery();
				query.AddToFilter(JoinCondition.And, ClientChargeableUsageSchema.U1_PeriodStart, feeDueDateGroup.Key);
				query.AddToFilter(ClientChargeableUsageSchema.U1_Code, BillingConstants.BillingSystem.Fee);
				query.AddToFilter(JoinCondition.And, ClientChargeableUsageSchema.U1_Parent, feeDueDateGroup.Select(x => x.fee.PK));

				var feeToUsage = BillsFactory.Load<ClientChargeableUsage>(query)
					.GroupBy(x => x.U1_Parent)
					.ToDictionary(x => x.Key, y => (IEnumerable<ClientChargeableUsage>)y);
				dueDateToFeeToUsage.Add(feeDueDateGroup.Key, feeToUsage);
				foreach (var invoicePk in feeToUsage.Values.Select(x => x.First().U1_AH_Invoice).Where(x => !x.IsEmpty).Distinct())
				{
					BillsFactory.AddFetchHint(AccTransactionHeaderSchema.Constants.TableName, invoicePk);
				}
			}

			return dueDateToFeeToUsage;
		}

		Dictionary<ZGuid, EDIOrgHeader> BuildPayCoToPartner(IEnumerable<ZGuid> payingLicCompanyPks)
		{
			// if there is single billed delivery on the paying co
			// then if it is invoice-to an org, and that org is partner, then that org is the partner org
			var payCoDeliveryFilter = new ZQuery(ClientInvoiceDeliverySchema.L9_LC, payingLicCompanyPks);
			payCoDeliveryFilter.AddToFilter(ClientInvoiceDeliverySchema.L9_IsBilled, ZBool.True);
			var payCompanyPkToInvoiceOrgPk = BillsFactory.Load<ClientInvoiceDelivery>(payCoDeliveryFilter)
				.GroupBy(x => x.L9_LC)
				.Where(x => x.Count() == 1 && !x.First().L9_OH_InvoiceTo.IsEmpty)
				.ToDictionary(x => x.Key, y => y.First().L9_OH_InvoiceTo);
			var partnerLicCompanies = BillsFactory.Load<LicenceCompany>(new ZQuery(LicenceCompanySchema.LC_OH, payCompanyPkToInvoiceOrgPk.Values));
			var partnerLicBillingQuery = new ZQuery(ClientLicenceBillingSchema.L4_LC, partnerLicCompanies.Select(x => x.PK));
			partnerLicBillingQuery.AddToFilter(ClientLicenceBillingSchema.L4_IsPartner, ZBool.True);
			var partnerCompanyPkToPartnerLicBilling = BillsFactory.Load<ClientLicenceBilling>(partnerLicBillingQuery)
				.ToDictionary(x => x.L4_LC);
			var partnerOrgPks = partnerLicCompanies.Where(x => partnerCompanyPkToPartnerLicBilling.ContainsKey(x.PK))
				.Select(x => x.LC_OH);
			var partnerOrgPkMap = BillsFactory.Load<EDIOrgHeader>(new ZQuery(OrgHeaderSchema.PK, partnerOrgPks))
				.ToDictionary(x => x.PK);
			// if payCompanyPK is in dictionary payCompanyPkToInvoiceOrgPk
			// and InvoiceOrgPk is in partnerOrgPkMap
			// then the partner is InvoiceOrgPk
			var payingLicCompanyPkToPartner = payCompanyPkToInvoiceOrgPk.Where(x => partnerOrgPkMap.ContainsKey(x.Value))
				.ToDictionary(x => x.Key, y => partnerOrgPkMap[y.Value]);
			return payingLicCompanyPkToPartner;
		}

		void PopulateCollection(List<BillInfo> billInfos)
		{
			foreach (var billInfo in billInfos)
			{
				if (billInfo.bill != null)
				{
					Bills.Add(billInfo.bill);
				}
			}
		}

		class BillInfo
		{
			// Used when this represents maintenance
			internal LicenceHeader licHeader;
			internal MaintenanceBill bill;

			// Used when this represents a fee
			internal ClientLicenceFee fee;
			internal ZDateTime feeDate;

			// Commmon
			internal ClientInvoiceDelivery delivery;
			internal ZGuid payingOrgPk;
			internal ZGuid branchPk;

			internal ZGuid LicCompanyPk => licHeader?.LA_LC ?? fee.L8_LC;

			internal LicenceCompany LicCompany
			{
				get => licCompany ?? (licCompany = licHeader?.Company ?? fee?.Company);
				set => licCompany = value;
			}
			LicenceCompany licCompany;

			internal InvoiceGroup CreateGroup()
			{
				if (licHeader != null)
				{
					return ClientInvoiceDelivery.CreateGroup(
						delivery,
						payingOrgPk,
						licHeader,
						MaintenanceBill.CalculateDueDate(licHeader),
						licHeader != null && licHeader.ReadonlyBilling != null ? (int)licHeader.ReadonlyBilling.L0_RenewalMonths : 0);
				}
				else
				{
					return ClientInvoiceDelivery.CreateGroup(
						delivery,
						payingOrgPk,
						LicCompany,
						feeDate,
						fee.L8_RenewalMonths);
				}
			}
		}

		/// <summary>
		/// Decides which licence out of all the licences on a database pays per-database charges.
		/// </summary>
		class PerDatabaseChargeDecider : IPerDatabaseChargeDecider
		{
			public bool IsOwner(LicenceHeader licToCheck, string moduleCode)
			{
				LicenceDatabase db = licToCheck.Database;

				ZDateTime minExpiry = ZDateTime.Today.AddMonths(-4);
				if (minExpiry > licToCheck.LA_ContractExpiryDate)
				{
					minExpiry = licToCheck.LA_ContractExpiryDate;
				}

				List<LicenceHeader> candidateLicences = new List<LicenceHeader>();
				candidateLicences.Add(licToCheck);

				foreach (LicenceHeader licHeader in db.LicHeadersForAllCompanies)
				{
					if (licHeader.PK != licToCheck.PK &&
						licHeader.LA_IsActive &&
						!licHeader.LA_ContractExpiryDate.IsEmpty &&
						licHeader.LA_ContractExpiryDate >= minExpiry &&
						licHeader.Company.Header.OH_IsActive)
					{
						var invoiceDelivery = licHeader.Company.InvoiceDeliveries.FindByServerAndSystem(db.LD_ServerCode, BillingConstants.BillingSystem.Maintenance);
						if (invoiceDelivery == null || invoiceDelivery.L9_IsBilled)
						{
							LicenceModules module = licHeader.Modules.FindByCode(moduleCode);
							if (module != null && MaintenancePrices.IsMaintenanceFeeType(module))
							{
								candidateLicences.Add(licHeader);
							}
						}
					}
				}

				LicenceHeader owner = candidateLicences
					.OrderBy(s => (s.LA_SupportStartDate.IsEmpty ? DateTime.MaxValue : s.LA_SupportStartDate.ToDateTime()))
					.ThenBy(s => s.Company.LC_CompanyCode)
					.First();

				return owner.PK == licToCheck.PK;
			}
		}

		#endregion

		#region Create Invoices

		public void CreateInvoices(MaintenanceBillRecipient[] invRecipients, IEdiProgress progress)
		{
			progress.SafeSetExpectedCount(invRecipients.Length);
			using (var usSalesTaxCalculator = ObjectFactory.Get<IUSSalesTaxCalculator>())
			{
				foreach (var branchBills in invRecipients.GroupBy(s => s.BranchPK))
				{
					ZGuid branchPk = branchBills.Key;
					using (BillingInvoicingHelper.BranchContext(branchPk.ToGuid()))
					{
						var attachments = GetAttachments();

						foreach (var partnerBills in branchBills.GroupBy(s => s.PartnerOrgPK))
						{
							if (partnerBills.Key.IsEmpty)
							{
								foreach (var recipient in partnerBills)
								{
									if (progress.SafeIsCancelled())
									{
										return;
									}
									progress.SafeUpdateCurrentCount("Processing " + recipient.OrganisationCode);
									var clientAttachments = GetClientAttachments(recipient.OrganisationCode);
									var allAttachments = attachments;
									if (clientAttachments.Count > 0)
									{
										clientAttachments.AddRange(attachments);
										allAttachments = clientAttachments.ToArray();
									}
									recipient.CreateInvoices(allAttachments, usSalesTaxCalculator: usSalesTaxCalculator);
								}
							}
							else
							{
								foreach (var recipient in partnerBills)
								{
									if (progress.SafeIsCancelled())
									{
										return;
									}
									progress.SafeUpdateCurrentCount("Processing " + recipient.OrganisationCode);
								}
							}
						}
					}
				}
			}
		}

		KeyValuePair<string, byte[]>[] GetAttachments()
		{
			List<KeyValuePair<string, byte[]>> result = new List<KeyValuePair<string, byte[]>>();

			if (!AttachmentFolder.IsEmpty)
			{
				var mappedPath = GetMappedAttachmentFolder();

				string folder = Path.Combine(mappedPath, Env.CurrentCompany.Country.Code);
				if (!Directory.Exists(folder))
				{
					folder = mappedPath;
				}

				string[] paths = Directory.GetFiles(folder);
				foreach (string path in paths)
				{
					byte[] contents = File.ReadAllBytes(path);
					result.Add(new KeyValuePair<string, byte[]>(Path.GetFileName(path), contents));
				}
			}

			return result.ToArray();
		}

		public const string ClientAttachmentFolderName = "OrgCodes";

		List<KeyValuePair<string, byte[]>> GetClientAttachments(string orgCode)
		{
			List<KeyValuePair<string, byte[]>> result = new List<KeyValuePair<string, byte[]>>();

			if (!AttachmentFolder.IsEmpty)
			{
				var mappedPath = GetMappedAttachmentFolder();
				result.AddRange(GetClientAttachments(Path.Combine(mappedPath, Env.CurrentCompany.Country.Code), orgCode));
				result.AddRange(GetClientAttachments(mappedPath, orgCode));
			}

			return result;
		}

		List<KeyValuePair<string, byte[]>> GetClientAttachments(string rootFolder, string orgCode)
		{
			List<KeyValuePair<string, byte[]>> result = new List<KeyValuePair<string, byte[]>>();
			string folder = Path.Combine(rootFolder, ClientAttachmentFolderName);
			if (Directory.Exists(folder))
			{
				string[] paths = Directory.GetFiles(folder, orgCode + "*");
				foreach (string path in paths)
				{
					byte[] contents = File.ReadAllBytes(path);
					result.Add(new KeyValuePair<string, byte[]>(Path.GetFileName(path), contents));
				}
			}
			return result;
		}

		#endregion

		#region Fees

		void AddFees(List<BillInfo> billInfos)
		{
			var feeQuery = new ZDBOnlyQuery(typeof(ClientLicenceFee));
			BuildFeeQuery(feeQuery);
			var fees = BillsFactory.Load<ClientLicenceFee>(feeQuery);

			if (fees.Length == 0)
			{
				return;
			}

			foreach (var fee in fees)
			{
				ZDateTime startDate = filter.DueDateFrom;
				if (startDate.IsEmpty || startDate < fee.L8_StartDate)
				{
					startDate = fee.L8_StartDate;
				}

				const int MonthScale = 100;
				int feeStart = (fee.L8_StartDate.Year * 12 + fee.L8_StartDate.Month - 1) * MonthScale + fee.L8_StartDate.Day;
				int dueStart = (startDate.Year * 12 + startDate.Month - 1) * MonthScale + startDate.Day;
				int recurrence = (dueStart - feeStart + fee.L8_RenewalMonths * MonthScale - 1) / (fee.L8_RenewalMonths * MonthScale);
				ZDateTime feeDueDate = fee.L8_StartDate.AddMonths(recurrence * fee.L8_RenewalMonths);

				if (filter.IsDateInRange(feeDueDate)
					&& (fee.L8_EndDate.IsEmpty || fee.L8_EndDate > feeDueDate))
				{
					var billInfo = new BillInfo();
					billInfo.fee = fee;
					billInfo.feeDate = feeDueDate;
					billInfos.Add(billInfo);
				}
			}
		}

		void BuildFeeQuery(ZDBOnlyQuery feeQuery)
		{
			feeQuery.AddToFilter(ClientLicenceFeeSchema.L8_SystemCode, BillingConstants.BillingSystem.Maintenance);

			if (filter.DueDateTo.IsValidSqlDateTime)
			{
				feeQuery.AddToFilter(ClientLicenceFeeSchema.L8_StartDate, SQLComparisonOperator.LessThan, filter.DueDateTo);
			}

			if (filter.DueDateFrom.IsValidSqlDateTime)
			{
				ZQuery endDateQuery = new ZQuery(ClientLicenceFeeSchema.L8_EndDate, ZDateTime.Empty);
				endDateQuery.AddToFilter(JoinCondition.Or, ClientLicenceFeeSchema.L8_EndDate, SQLComparisonOperator.GreaterThan, filter.DueDateFrom);
				feeQuery.AddToFilter(endDateQuery);
			}

			if (!filter.OrganisationPK.IsEmpty)
			{
				ZDBOnlySubQuery licenceCompanySubQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), ClientLicenceFeeSchema.L8_LC);
				licenceCompanySubQuery.AddToFilter(LicenceCompanySchema.LC_OH, filter.OrganisationPK);
				feeQuery.AddSubQuery(licenceCompanySubQuery, JoinCondition.And);
			}
			else if (!filter.EnterpriseCode.IsEmpty)
			{
				ZDBOnlySubQuery licenceCompanySubQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), ClientLicenceFeeSchema.L8_LC);
				ZDBOnlySubQuery enterpriseQuery = new ZDBOnlySubQuery(typeof(LicenceEnterprise), LicenceCompanySchema.LC_LE);
				enterpriseQuery.AddToFilter(LicenceEnterpriseSchema.LE_EnterpriseCode, filter.EnterpriseCode);
				licenceCompanySubQuery.AddSubQuery(enterpriseQuery, JoinCondition.And);
				feeQuery.AddSubQuery(licenceCompanySubQuery, JoinCondition.And);
			}
		}

		#endregion
	}
}

