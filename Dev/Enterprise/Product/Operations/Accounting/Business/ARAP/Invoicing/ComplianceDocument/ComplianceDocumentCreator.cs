using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class ComplianceDocumentCreator
	{
		public ComplianceDocumentCreator(InvoicingBase[] invoices, string createOption)
		{
			Argument.NotNull(invoices, nameof(invoices));

			if (!invoices.Any())
			{
				throw new ArgumentException("We need at least one invoice");
			}

			Factory = invoices.First().Factory;

			if (!invoices.All(x => x.Factory == Factory))
			{
				throw new ArgumentException("All invoices must be in the same factory");
			}

			Invoices = invoices;
			CreateOption = createOption;
		}

		readonly InvoicingBase[] Invoices;
		public BusinessObjectFactory Factory { get; }
		string CreateOption { get; }

		List<(ZGuid linePK, AccComplianceDocumentLine complianceDocumentLine)> LinePivots => linePivots ?? (linePivots = new List<(ZGuid linePK, AccComplianceDocumentLine complianceDocumentLine)>());
		List<(ZGuid linePK, AccComplianceDocumentLine complianceDocumentLine)> linePivots;

		public AccComplianceDocumentHeader[] CreateComplianceDocumentRecords()
		{
			var allComplianceDocuments = CreateComplianceDocumentRecordsCore();
			DeleteNegativeCompliances(allComplianceDocuments);
			SetComplianceDocumentNumber(allComplianceDocuments);

			return allComplianceDocuments.Where(x => !x.IsDeleted).ToArray();
		}

		void SetComplianceDocumentNumber(AccComplianceDocumentHeader[] compliances)
		{
			compliances.Where(x => !x.IsDeleted).ForEach((y) =>
			{
				y.SetComplianceSubType();
				y.SetComplianceSequenceBook();
				y.SetComplianceDocumentNumber();
			});
		}

		void DeleteNegativeCompliances(AccComplianceDocumentHeader[] compliances)
		{
			var allowNegativeLineAmount = AccountingMasterFilesRegistry.Instance.AllowNegativeComplianceDocumentLines.Value;

			var pendingDeleteCompliances = new HashSet<AccComplianceDocumentHeader>();
			var positiveCompliances = new HashSet<AccComplianceDocumentHeader>();
			var transactionPKsOfPendingDeleteCompliances = new List<ZGuid>();

			foreach (AccComplianceDocumentHeader header in compliances)
			{
				var isNegativeCompliance = allowNegativeLineAmount ? header.Amount < 0 : header.ComplianceDocumentLines.Cast<AccComplianceDocumentLine>().Any(x => x.Amount < 0);
				if (isNegativeCompliance)
				{
					pendingDeleteCompliances.Add(header);
					transactionPKsOfPendingDeleteCompliances.AddRange(header.TransactionHeaders.Select(x => x.PK));
				}
				else
				{
					positiveCompliances.Add(header);
				}
			}

			if (pendingDeleteCompliances.Count > 0)
			{
				foreach (AccComplianceDocumentHeader header in positiveCompliances)
				{
					var headerPKs = header.TransactionHeaders.Select(x => x.PK);
					if (headerPKs.Any(x => transactionPKsOfPendingDeleteCompliances.Contains(x)))
					{
						pendingDeleteCompliances.Add(header);
					}
				}
			}

			pendingDeleteCompliances.DeleteAll();
		}

		bool IsRollUpOrNot
		{
			get
			{
				switch (CreateOption)
				{
					case Core.Constants.OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge:
						return true;
					case Core.Constants.OrganisationCreateComplianceDocumentOnPostingTypes.NotRollup:
					case Core.Constants.OrganisationCreateComplianceDocumentOnPostingTypes.PerComplianceDocumentNumber:
						return false;
					default:
						return false;
				}
			}
		}

		AccComplianceDocumentHeader[] CreateComplianceDocumentRecordsCore()
		{
			var allHeaders = new List<AccComplianceDocumentHeader>();
			var complianceDocumentGroups = GetComplianceDocumentGroup();

			if (complianceDocumentGroups.Any())
			{
				foreach (var groupByHeader in complianceDocumentGroups.GroupBy(c => c.GroupKey))
				{
					int sequence = 1;
					var complianceDocuments = groupByHeader.ToList();

					var complianceDocumentHeader = CreateComplianceHeader(complianceDocuments.FirstOrDefault());
					allHeaders.Add(complianceDocumentHeader);
					if (IsRollUpOrNot)
					{
						foreach (var groupByCharge in complianceDocuments.GroupBy(x => new { x.ChargeCodePK, x.GlAccountPK }))
						{
							CreateComplianceDocumentLineAndPivot(groupByCharge.ToList(), complianceDocumentHeader.PK, ref sequence);
						}
					}
					else
					{
						CreateComplianceDocumentLineAndPivot(complianceDocuments, complianceDocumentHeader.PK, ref sequence);
					}

					var isOverrideOrganization = !complianceDocuments.FirstOrDefault().ComplianceDocumentHeaderDetail?.ComplianceDocumentOrganization.IsEmpty ?? false;
					complianceDocumentHeader.SetComplianceDocumentHeaderAddress(isOverrideOrganization);
				}
			}

			return allHeaders.ToArray();
		}

		ComplianceDocumentGroup[] GetComplianceDocumentGroup()
		{
			var complianceDocumentGroups = new List<ComplianceDocumentGroup>();

			Invoices.ForEach(x => x.Lines.Cast<InvoicingLineBase>().Where(y => (CreateOption != Core.Constants.OrganisationCreateComplianceDocumentOnPostingTypes.PerComplianceDocumentNumber ||
				y.CreateComplianceDocumentRecordOnPosting) && y.AL_AT.IsValid).ForEach(z => complianceDocumentGroups.Add(new ComplianceDocumentGroup(z))));

			return complianceDocumentGroups.ToArray();
		}

		AccComplianceDocumentHeader CreateComplianceHeader(ComplianceDocumentGroup complianceDocumentGroup)
		{
			var complianceDocumentHeader = complianceDocumentGroup.Ledger == LedgerTypes.AccountsReceivable ?
				Factory.New<ARComplianceDocumentHeader>() : (AccComplianceDocumentHeader)Factory.New<APComplianceDocumentHeader>();

			complianceDocumentHeader.ADH_TransactionType = complianceDocumentGroup.TransactionType;
			complianceDocumentHeader.ADH_Description = complianceDocumentGroup.HeaderDescription;
			complianceDocumentHeader.ADH_GC_Company = complianceDocumentGroup.CompanyPK;

			var complianceDocumentHeaderDetail = complianceDocumentGroup.ComplianceDocumentHeaderDetail;
			if (complianceDocumentHeaderDetail != null)
			{
				complianceDocumentHeader.ADH_OH_Organisation = complianceDocumentHeaderDetail.ComplianceDocumentOrganization;
				complianceDocumentHeader.ADH_ComplianceSubType = complianceDocumentHeaderDetail.ComplianceSubType;
				complianceDocumentHeader.ADH_DocumentNumber = complianceDocumentHeaderDetail.ComplianceDocumentNumber;
				complianceDocumentHeader.ADH_VATRegistrationNumberOverride = complianceDocumentHeaderDetail.ComplianceDocumentVATRegistrationNum;
				complianceDocumentHeader.ADH_DocumentDate = complianceDocumentHeaderDetail.ComplianceDocumentDate;
				complianceDocumentHeader.ADH_ReportingPeriod = complianceDocumentHeaderDetail.ComplianceDocumentReportingPeriod;
				complianceDocumentHeader.ADH_SupportingReason = complianceDocumentHeaderDetail.ComplianceDocumentSupportingReason;
				complianceDocumentHeader.ADH_SupportingDocumentType = complianceDocumentHeaderDetail.ComplianceSupportingDocumentType;
				complianceDocumentHeader.ADH_SupportingDocumentNumber = complianceDocumentHeaderDetail.ComplianceSupportingDocumentNumber;
			}
			else
			{
				complianceDocumentHeader.ADH_OH_Organisation = complianceDocumentGroup.GroupKey.OrgHeaderPK;
			}

			return complianceDocumentHeader;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		AccComplianceDocumentLine CreateComplianceDocumentLine(ZGuid documentHeaderPK, ComplianceDocumentGroup complianceDocumentGroup, int sequence)
		{
			var complianceDocumentLine = Factory.New<AccComplianceDocumentLine>();
			complianceDocumentLine.ADL_ADH = documentHeaderPK;
			complianceDocumentLine.ADL_Sequence = sequence;

			var transactionLine = Factory.Load<AccTransactionLines>(complianceDocumentGroup.LinePK);
			var chargeCode = transactionLine.ChargeCode;

			complianceDocumentLine.ADL_Description = IsRollUpOrNot ?
				chargeCode != null ?
					!string.IsNullOrEmpty(chargeCode.AC_LocalLanguageDescription) ?
						chargeCode.AC_LocalLanguageDescription
					: chargeCode.AC_Desc
				: transactionLine.GLHeader.AG_Description
			: transactionLine.AL_Desc;

			return complianceDocumentLine;
		}

		void CreateComplianceDocumentLineAndPivot(IEnumerable<ComplianceDocumentGroup> groupedDocuments, ZGuid documentHeaderPK, ref int sequence)
		{
			AccComplianceDocumentLine complianceDocumentLine = null;
			var rollUp = IsRollUpOrNot;

			if (rollUp)
			{
				complianceDocumentLine = CreateComplianceDocumentLine(documentHeaderPK, groupedDocuments.First(), sequence++);
			}

			foreach (var group in groupedDocuments)
			{
				if (!rollUp)
				{
					complianceDocumentLine = CreateComplianceDocumentLine(documentHeaderPK, group, sequence++);
				}

				var pivot = Factory.New<AccComplianceDocumentPivot>();
				pivot.ADP_ADL = complianceDocumentLine.PK;
				pivot.ADP_AL = group.LinePK;

				LinePivots.Add((group.LinePK, complianceDocumentLine));
			}
		}
	}

	class ComplianceDocumentGroup
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007:Customizable Data Translation Rule", Justification = "Baseline")]
		public ComplianceDocumentGroup(InvoicingLineBase invoicingLineBase)
		{
			if (invoicingLineBase.CreateComplianceDocumentRecordOnPosting && !invoicingLineBase.ComplianceDocumentNumber.IsEmpty)
			{
				GroupKey = new ComplianceDocumentGroupKey(invoicingLineBase.ComplianceDocumentNumber);
				ComplianceDocumentHeaderDetail = new ComplianceDocumentHeaderDetail(invoicingLineBase);
			}
			else
			{
				var invoicingBase = invoicingLineBase.InvoiceBase;
				GroupKey = new ComplianceDocumentGroupKey(invoicingBase.AH_OH, invoicingBase.AH_TransactionType, invoicingLineBase.TaxRate?.AT_PostingGroupId ?? 0);
			}

			Ledger = invoicingLineBase.InvoiceBase.AH_Ledger;
			TransactionType = invoicingLineBase.InvoiceBase.AH_TransactionType;
			HeaderDescription = invoicingLineBase.InvoiceBase.AH_Desc;
			CompanyPK = invoicingLineBase.InvoiceBase.AH_GC;
			LinePK = invoicingLineBase.PK;
			ChargeCodePK = invoicingLineBase.AL_AC;
			GlAccountPK = invoicingLineBase.AL_AG;
			ChargeDescription = invoicingLineBase.ChargeCode?.AC_Desc;
			GlAccountDescription = invoicingLineBase.GLHeader?.AG_Description;
			OrganisationPK = invoicingLineBase.InvoiceBase.AH_OH;
		}

		public ComplianceDocumentGroupKey GroupKey { get; private set; }
		public ZString Ledger { get; private set; }
		public ZString TransactionType { get; private set; }
		public ZString HeaderDescription { get; private set; }
		public ZGuid CompanyPK { get; private set; }
		public ZGuid LinePK { get; private set; }
		public ZGuid ChargeCodePK { get; private set; }
		public ZString? ChargeDescription { get; private set; }
		public ZGuid GlAccountPK { get; private set; }
		public ZString? GlAccountDescription { get; private set; }
		public ZGuid OrganisationPK { get; private set; }
		public ComplianceDocumentHeaderDetail ComplianceDocumentHeaderDetail { get; private set; }
	}

	class ComplianceDocumentGroupKey
	{
		public ComplianceDocumentGroupKey(ZGuid orgHeaderPK, ZString transactionType, ZShort postingGroupID)
		{
			OrgHeaderPK = orgHeaderPK;
			TransactionType = transactionType;
			PostingGroupID = postingGroupID;
		}

		public ComplianceDocumentGroupKey(ZString documentNumber)
		{
			DocumentNumber = documentNumber;
		}

		public readonly ZGuid OrgHeaderPK;
		public readonly ZString TransactionType;
		public readonly ZShort PostingGroupID;
		public readonly ZString DocumentNumber;

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}

			var key = (ComplianceDocumentGroupKey)obj;
			return key.OrgHeaderPK == OrgHeaderPK && key.TransactionType == TransactionType && key.PostingGroupID == PostingGroupID && key.DocumentNumber == DocumentNumber;
		}

		public static bool operator ==(ComplianceDocumentGroupKey x1, ComplianceDocumentGroupKey x2)
		{
			return Equals(x1, x2);
		}

		public static bool operator !=(ComplianceDocumentGroupKey x1, ComplianceDocumentGroupKey x2)
		{
			return !Equals(x1, x2);
		}

		public override int GetHashCode()
		{
			return OrgHeaderPK.GetHashCode() ^ TransactionType.GetHashCode() ^ PostingGroupID.GetHashCode() ^ DocumentNumber.GetHashCode();
		}
	}

	class ComplianceDocumentHeaderDetail
	{
		public ComplianceDocumentHeaderDetail(InvoicingLineBase invoicingLineBase)
		{
			ComplianceSubType = invoicingLineBase.ComplianceSubType;
			ComplianceDocumentNumber = invoicingLineBase.ComplianceDocumentNumber;
			ComplianceDocumentVATRegistrationNum = invoicingLineBase.ComplianceDocumentVATRegistrationNum;
			ComplianceDocumentDate = invoicingLineBase.ComplianceDocumentDate;
			ComplianceDocumentReportingPeriod = invoicingLineBase.ComplianceDocumentReportingPeriod;
			ComplianceDocumentSupportingReason = invoicingLineBase.ComplianceDocumentSupportingReason;
			ComplianceSupportingDocumentType = invoicingLineBase.ComplianceSupportingDocumentType;
			ComplianceSupportingDocumentNumber = invoicingLineBase.ComplianceSupportingDocumentNumber;
			var complianceDocumentOrganization = invoicingLineBase.ComplianceDocumentOrganization;
			ComplianceDocumentOrganization = complianceDocumentOrganization.IsEmpty ? invoicingLineBase.InvoiceBase.AH_OH : complianceDocumentOrganization;
		}

		public ZString ComplianceSubType { get; private set; }
		public ZString ComplianceDocumentNumber { get; private set; }
		public ZString ComplianceDocumentVATRegistrationNum { get; private set; }
		public ZDateTime ComplianceDocumentDate { get; private set; }
		public ZInt ComplianceDocumentReportingPeriod { get; private set; }
		public ZString ComplianceDocumentSupportingReason { get; private set; }
		public ZString ComplianceSupportingDocumentType { get; private set; }
		public ZString ComplianceSupportingDocumentNumber { get; private set; }
		public ZGuid ComplianceDocumentOrganization { get; private set; }
	}
}
