using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Business
{
	public class CommissionFlattenedDataTransferProcessor : DataTransferProcessor
	{
		public CommissionFlattenedDataTransferProcessor(AccCommissionLineCollection commissionLineCollection, CommissionImportCollectionInfo flattenedCollectionInfo)
		{
			this.commissionLineCollection = commissionLineCollection;
			this.flattenedCollectionInfo = flattenedCollectionInfo;
			this.flattenedCollection = (CommissionFlattenedCollection)((IImportCollectionInfo)flattenedCollectionInfo).Collection;
			this.factory = flattenedCollection.Factory;
		}

		readonly AccCommissionLineCollection commissionLineCollection;
		readonly CommissionImportCollectionInfo flattenedCollectionInfo;
		readonly CommissionFlattenedCollection flattenedCollection;
		readonly BusinessObjectFactory factory;

		#region Import

		public override void Import()
		{
			NewCount = 0;
			OverriddenCount = 0;
			ErrorCount = 0;
			IgnoredCount = 0;
			LogList = new List<string>();
			newCommissionHeadersBeingImported.Clear();

			var lineNumber = 0;
			foreach (CommissionFlattened flattenedRecord in flattenedCollection)
			{
				lineNumber++;

				if (IsCanceled)
				{
					break;
				}

				if (!OnProgressChanged(lineNumber * 100 / flattenedCollection.Count, Res.GetString("5cc5f220-5d91-4fad-868e-340a41a7f547", "Importing entity commissions ({0} of {1})", lineNumber, flattenedCollection.Count)))
				{
					break;
				}

				BusinessObject groupingSource;
				GlbCompany company;
				if (flattenedCollectionInfo.CurrentCompanyOnly)
				{
					company = GlbCompany.CurrentCompany;
					groupingSource = GetGroupingSource(factory, GlbCompany.CurrentCompany, flattenedRecord);
					if (groupingSource == null)
					{
						NotifyError(lineNumber, Res.GetString("05530fdf-c292-4e61-8116-25b67597e540", "Invalid job / transaction (Transaction # = {0}, Transaction Type = {1})", flattenedRecord.GroupingSourceCode, flattenedRecord.InvoiceType));
						continue;
					}
				}
				else
				{
					company = factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, flattenedRecord.CompanyCode);
					if (company == null)
					{
						NotifyError(lineNumber, Res.GetString("301e244f-a8b0-4e81-b47e-4f3ce78e386e", "Invalid company code '{0}'", flattenedRecord.CompanyCode));
						continue;
					}
					else
					{
						groupingSource = GetGroupingSource(factory, company, flattenedRecord);
						if (groupingSource == null)
						{
							NotifyError(lineNumber, Res.GetString("4ce92729-238f-4bfd-acf5-654124c18c23", "Invalid job / transaction (Company = {0}, Transaction # = {1}, Transaction Type = {2})", flattenedRecord.CompanyCode, flattenedRecord.GroupingSourceCode, flattenedRecord.InvoiceType));
							continue;
						}
					}
				}

				OrgHeader party = null;
				if (!flattenedRecord.PartyCode.IsEmpty)
				{
					party = factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, flattenedRecord.PartyCode);
					if (party == null)
					{
						NotifyError(lineNumber, Res.GetString("baf50f69-5044-40e6-825b-e91ba1176c13", "Invalid organization code '{0}'", flattenedRecord.PartyCode));
						continue;
					}
				}

				if (!flattenedRecord.StaffCode.IsEmpty)
				{
					var staff = factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, flattenedRecord.StaffCode);
					if (staff == null)
					{
						NotifyError(lineNumber, Res.GetString("879b780d-d613-481c-896d-00d60e9eae95", "Invalid staff code '{0}'", flattenedRecord.StaffCode));
						continue;
					}
				}

				if (flattenedRecord.PartyCode.IsEmpty && flattenedRecord.StaffCode.IsEmpty)
				{
					NotifyError(lineNumber, Res.GetString("0d4e6605-8e03-4c13-98f0-0ba88028f901", "No entity entered"));
					continue;
				}

				if (flattenedRecord.CH0_CommissionDate.IsEmpty)
				{
					NotifyError(lineNumber, Res.GetString("ce34568b-ac95-4767-93c0-6546b843a7f8", "No recognition date entered"));
					continue;
				}

				if (!CommissionTypesIncludingCustom.ContainsCode(flattenedRecord.CL0_CommissionType))
				{
					NotifyError(lineNumber, Res.GetString("08ecfc5b-37a3-4d58-8ee5-b1ee4a24c004", "Invalid commission type '{0}'", flattenedRecord.CL0_CommissionType));
					continue;
				}

				var commissionCurrency = factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, flattenedRecord.CommissionCurrencyCode);
				if (commissionCurrency == null)
				{
					NotifyError(lineNumber, Res.GetString("35bbeccc-ab17-4c3f-add8-9843c1c241b8", "Invalid commission currency code '{0}'", flattenedRecord.CommissionCurrencyCode));
					continue;
				}

				bool hasOverriden = false;
				if (flattenedCollectionInfo.ImportType == CommissionImportTypeList.Codes.OverrideIfChangedOtherwiseSkip)
				{
					var commissionHeaderQuery = new ZQuery();
					commissionHeaderQuery.AddToFilter(AccCommissionHeaderSchema.CH0_AH_Source, null);
					commissionHeaderQuery.AddToFilter(AccCommissionHeaderSchema.CH0_CA0, null);
					commissionHeaderQuery.AddToFilter(AccCommissionHeaderSchema.CH0_CommissionDate, flattenedRecord.CH0_CommissionDate.Date);
					commissionHeaderQuery.AddToFilter(AccCommissionHeaderSchema.CH0_GC, company.PK);
					commissionHeaderQuery.AddToFilter(AccCommissionHeaderSchema.CH0_GroupingSourceID, groupingSource.PK);
					commissionHeaderQuery.AddToFilter(AccCommissionHeaderSchema.CH0_GroupingSourceTableCode, groupingSource.TablePrefix);
					commissionHeaderQuery.AddToFilter(AccCommissionHeaderSchema.CH0_OH_Customer, null);
					commissionHeaderQuery.AddToFilter(AccCommissionHeaderSchema.CH0_OH_Debtor, null);
					commissionHeaderQuery.AddToFilter(AccCommissionHeaderSchema.CH0_OverridenDateTimeUtc, null);
					commissionHeaderQuery.AddToFilter(AccCommissionHeaderSchema.CH0_Product, ZString.Empty);
					commissionHeaderQuery.AddToFilter(AccCommissionHeaderSchema.CH0_Service, ZString.Empty);
					commissionHeaderQuery.AddToFilter(AccCommissionHeaderSchema.CH0_SubModule, ZString.Empty);
					commissionHeaderQuery.AddToFilter(AccCommissionHeaderSchema.CH0_Mode, ZString.Empty);
					commissionHeaderQuery.AddToFilter(AccCommissionHeaderSchema.CH0_NKOrigin, ZString.Empty);
					commissionHeaderQuery.AddToFilter(AccCommissionHeaderSchema.CH0_NKDestination, ZString.Empty);
					commissionHeaderQuery.AddToFilter(AccCommissionHeaderSchema.CH0_SnapshotEventCode, AccCommissionHeaderSnapshotEventList.Codes.Imported);
					var existingCommissionHeaders = factory.Load<AccCommissionHeader>(commissionHeaderQuery).Where(x => x.IsInDatabase);
					var existingCommissionLines = existingCommissionHeaders
						.SelectMany(x => x.Lines)
						.Where(line => IsImportLine(line) &&
								line.CL0_CommissionType == flattenedRecord.CL0_CommissionType &&
								(flattenedRecord.StaffCode.IsEmpty ?
									line.CL0_OH_Party == party.PK :
									line.CL0_GS_NKStaff == flattenedRecord.StaffCode) &&
								line.CL0_OverridenDateTimeUtcInfo.OriginalValue.IsEmpty)
						.ToArray();

					var hasCommissionLineThatWasntOverriden = false;
					foreach (var existingCommissionLine in existingCommissionLines)
					{
						var shouldOverride =
							existingCommissionLine.CL0_EntityCommissionAmount != flattenedRecord.EntityCommissionAmount ||
							existingCommissionLine.CL0_RX_NKCommissionCurrency != flattenedRecord.CommissionCurrencyCode;

						if (shouldOverride)
						{
							existingCommissionLine.MarkAsOverriden();
							hasOverriden = true;
						}
						else
						{
							hasCommissionLineThatWasntOverriden = true;
						}
					}

					if (hasCommissionLineThatWasntOverriden)
					{
						IgnoredCount++;
						continue;
					}
				}

				AccCommissionHeader commissionHeaderBeingImported;
				var key = Tuple.Create(groupingSource, flattenedRecord.CH0_CommissionDate);
				if (!newCommissionHeadersBeingImported.TryGetValue(key, out commissionHeaderBeingImported))
				{
					commissionHeaderBeingImported = factory.New<AccCommissionHeader>();
					commissionHeaderBeingImported.CH0_GC = company.PK;
					commissionHeaderBeingImported.CH0_GroupingSourceID = groupingSource.PK;
					commissionHeaderBeingImported.CH0_GroupingSourceTableCode = groupingSource.TablePrefix;
					commissionHeaderBeingImported.CH0_CommissionDate = flattenedRecord.CH0_CommissionDate.Date;
					if (groupingSource.TablePrefix == JobHeaderSchema.Constants.Prefix)
					{
						commissionHeaderBeingImported.CH0_JobNumber = (ZString)groupingSource[JobHeaderSchema.JH_JobNum];
					}

					commissionHeaderBeingImported.CH0_SnapshotDateTime = ZDateTime.Now;
					commissionHeaderBeingImported.CH0_SnapshotEventCode = AccCommissionHeaderSnapshotEventList.Codes.Imported;

					newCommissionHeadersBeingImported[key] = commissionHeaderBeingImported;
				}

				var commissionLine = commissionHeaderBeingImported.Lines.AddNew();
				commissionLine.CL0_CommissionType = flattenedRecord.CL0_CommissionType;
				commissionLine.CL0_GS_NKStaff = flattenedRecord.StaffCode;
				commissionLine.CL0_OH_Party = party != null ? party.PK : ZGuid.Empty;
				commissionLine.CL0_RX_NKCommissionCurrency = flattenedRecord.CommissionCurrencyCode;
				commissionLine.CL0_EntityPercentage = 100;
				commissionLine.CL0_EntityCommissionAmount = flattenedRecord.EntityCommissionAmount;

				if (hasOverriden)
				{
					OverriddenCount++;
				}
				else
				{
					NewCount++;
				}
			}
		}

		readonly Dictionary<Tuple<BusinessObject, ZDateTime>, AccCommissionHeader> newCommissionHeadersBeingImported = new Dictionary<Tuple<BusinessObject, ZDateTime>, AccCommissionHeader>();

		static bool IsImportLine(AccCommissionLine line)
		{
			return
				line.CL0_CAT.IsEmpty &&
				line.CL0_TransactionAmount.IsEmpty &&
				line.CL0_RX_NKTransactionCurrency.IsEmpty &&
				line.CL0_SharePortion.IsEmpty &&
				line.CL0_ShareTotal.IsEmpty &&
				line.CL0_ShareCommissionAmount.IsEmpty;
		}

		static BusinessObject GetGroupingSource(BusinessObjectFactory factory, GlbCompany company, CommissionFlattened flattenedRecord)
		{
			if (flattenedRecord.InvoiceType.IsEmpty)
			{
				var jobQuery = new ZQuery();
				jobQuery.AddToFilter(JobHeaderSchema.JH_GC, company.PK);
				jobQuery.AddToFilter(JobHeaderSchema.JH_JobNum, flattenedRecord.GroupingSourceCode);
				var job = factory.LoadTop1<JobHeader>(jobQuery);
				if (job != null)
				{
					return job;
				}
			}
			else
			{
				var invoiceQuery = new ZQuery();
				invoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, company.PK);
				invoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, flattenedRecord.GroupingSourceCode);
				invoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, flattenedRecord.InvoiceType);
				invoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
				var invoice = factory.LoadTop1<TransactionHeader>(invoiceQuery);
				if (invoice != null)
				{
					return invoice;
				}
			}

			return null;
		}

		#endregion

		#region Rollback

		public override void Rollback()
		{
			commissionLineCollection.DeleteAll();
		}

		#endregion

		#region Logging

		void NotifyError(int lineNumber, string message)
		{
			ErrorCount++;

			var log = Res.GetString("746f09b9-8a1e-40c3-a682-e7e19bbb6057", "Line {0}: {1}", lineNumber, message);
			LogList.Add(log);
		}

		public int CommissionsToImport { get { return flattenedCollection.Count; } }

		public int NewCount { get; protected set; }
		public int OverriddenCount { get; protected set; }
		public int IgnoredCount { get; protected set; }
		public int ErrorCount { get; protected set; }

		public IEnumerable<string> Logs
		{
			get { return LogList; }
		}

		protected List<string> LogList = new List<string>();

		#endregion

		#region Lists

		CommissionTypesIncludingCustom CommissionTypesIncludingCustom
		{
			get { return commissionTypesIncludingCustom ?? (commissionTypesIncludingCustom = new CommissionTypesIncludingCustom()); }
		}
		CommissionTypesIncludingCustom commissionTypesIncludingCustom;

		#endregion
	}
}
