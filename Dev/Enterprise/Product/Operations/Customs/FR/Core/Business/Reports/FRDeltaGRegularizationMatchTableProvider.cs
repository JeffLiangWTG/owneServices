namespace Enterprise.Customs.FR.Business.Reports
{
	using System;
	using System.Data;
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.Business;
	using Enterprise.Customs.FR.Business.Declaration;
	using Enterprise.DocumentEngine.DataProviders;
	using Enterprise.DocumentEngine.Exceptions;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Schema;

	public class FRDeltaGRegularizationMatchTableProvider : ParameterisedTableProvider
	{
		public override bool HandlesSortInternally => true;

		internal int BatchSize { get; set; } = 100;
		internal int MaxDeclarationCount { get; set; } = 100_000;

		internal static string TooManyDeclarationsMessage(int estimatedDeclarationCount, int maxDeclarationCount)
		{
			return Enterprise.Customs.FR.Business.Res.GetString(
				"FRDeltaGRegularizationMatchTableProvider.Messages.TooManyDeclarations",
				"Estimated number of declarations is {0:N0}. That is greater than allowed maximum of {1:N0}.",
				estimatedDeclarationCount, maxDeclarationCount);
		}

		protected override DataTable GetDataTable()
		{
			var table = new FRDeltaGRegularizationMatchDataSet.FRDeltaGRegularizationMatchDataSetDataTable();

			var reader = new FilteredBusinessObjectReader(CreateDeclarationQuery(), typeof(JobDeclaration));
			reader.BatchSize = BatchSize;
			reader.FactoryProvider.Current.RefreshEnabled = false;

			var estimatedDeclarationCount = reader.ApproximateCount;
			if (estimatedDeclarationCount > MaxDeclarationCount)
			{
				throw new DataProviderException(TooManyDeclarationsMessage(estimatedDeclarationCount, MaxDeclarationCount));
			}

			var batch = reader.LoadNextBatchInANewFactory(null).Cast<JobDeclaration>().ToList();
			while (batch.Count > 0)
			{
				foreach (var declaration in batch)
				{
					AddReportLine(table, declaration);
				}
				batch = reader.LoadNextBatchInANewFactory(batch.Last()).Cast<JobDeclaration>().ToList();
			}

			return table;
		}

		void AddReportLine(FRDeltaGRegularizationMatchDataSet.FRDeltaGRegularizationMatchDataSetDataTable table, JobDeclaration declaration)
		{
			var row = table.NewFRDeltaGRegularizationMatchDataSetRow();
			table.AddFRDeltaGRegularizationMatchDataSetRow(row);

			row.JE_DeclarationReference = declaration.JE_DeclarationReference;
			row.JE_CustomsProfile = declaration.JE_CustomsProfile;
			row.FallbackEntryDate = declaration.FallbackEntryDate;
			row.FallbackEntryNumber = declaration.FallbackEntryNumber;
			row.FallbackEntryStatus = declaration.FallbackEntryStatus;
		}

		#region Filter
		string[] DeclarationBranches { get; set; }
		string[] Declarations { get; set; }
		ZDateTime DeclarationDateFrom { get; set; }
		ZDateTime DeclarationDateTo { get; set; }
		ZDateTime ImportDateFrom { get; set; }
		ZDateTime ImportDateTo { get; set; }
		ZGuid ImporterPK { get; set; }
		ZGuid SupplierPK { get; set; }

		protected override void SetupParameterValues(object[] values)
		{
			int idx = 0;
			DeclarationBranches = Split((string)values[idx++]);
			Declarations = Split((string)values[idx++]);
			DeclarationDateFrom = (ZDateTime)values[idx++];
			DeclarationDateTo = (ZDateTime)values[idx++];
			ImportDateFrom = (ZDateTime)values[idx++];
			ImportDateTo = (ZDateTime)values[idx++];
			ImporterPK = (Guid)values[idx++];
			SupplierPK = (Guid)values[idx++];
		}

		static string[] Split(string listAsString)
		{
			if (string.IsNullOrWhiteSpace(listAsString))
			{
				return Array.Empty<string>();
			}
			return listAsString.Split(',').Select(i => i.Trim()).ToArray();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Parameter strings")]
		protected override Parameter[] ExpectedParameters()
		{
			return new Parameter[]
			{
				new Parameter("Declaration Branch", typeof(string)),
				new Parameter("Declarations", typeof(string)),
				new Parameter("Job Registered On->DateFrom", typeof(ZDateTime)),
				new Parameter("Job Registered On->DateTo", typeof(ZDateTime)),
				new Parameter("Import Date->DateFrom", typeof(ZDateTime)),
				new Parameter("Import Date->DateTo", typeof(ZDateTime)),
				new Parameter("Importer", typeof(Guid)),
				new Parameter("Supplier", typeof(Guid))
			};
		}

		ZQuery CreateDeclarationQuery()
		{
			var companyQuery = new ZDBOnlySubQuery(typeof(GlbCompany), GlbBranchSchema.GB_GC);
			companyQuery.AddToFilter(GlbCompanySchema.GC_RN_NKCountryCode, Core.Constants.CountryCodes.France);

			var branchQuery = new ZDBOnlySubQuery(typeof(GlbBranch), JobDeclarationSchema.JE_GB);
			branchQuery.AddSubQuery(companyQuery, JoinCondition.And);
			if (DeclarationBranches.Any())
			{
				branchQuery.AddToFilter(GlbBranchSchema.GB_Code, DeclarationBranches);
			}

			var declarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
			declarationQuery.AddToFilter(JobDeclarationSchema.JE_MessageType, new ZString[] { JobMessageTypeList.Codes.Import, JobMessageTypeList.Codes.Export });
			declarationQuery.AddSubQuery(branchQuery, JoinCondition.And);

			if (Declarations.Any())
			{
				declarationQuery.AddToFilter(JobDeclarationSchema.JE_DeclarationReference, Declarations);
			}
			if (DeclarationDateFrom.IsValid)
			{
				declarationQuery.AddToFilter(JobDeclarationSchema.JE_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, DeclarationDateFrom);
			}
			if (DeclarationDateTo.IsValid)
			{
				declarationQuery.AddToFilter(JobDeclarationSchema.JE_SystemCreateTimeUtc, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, DeclarationDateTo);
			}
			if (ImportDateFrom.IsValid)
			{
				declarationQuery.AddToFilter(JobDeclarationSchema.JE_DateOfArrival, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ImportDateFrom);
			}
			if (ImportDateTo.IsValid)
			{
				declarationQuery.AddToFilter(JobDeclarationSchema.JE_DateOfArrival, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, ImportDateTo);
			}
			if (ImporterPK.IsValid)
			{
				declarationQuery.AddToFilter(JobDeclarationSchema.JE_OH_Importer, ImporterPK);
			}
			if (SupplierPK.IsValid)
			{
				declarationQuery.AddToFilter(JobDeclarationSchema.JE_OH_Supplier, SupplierPK);
			}
			return declarationQuery;
		}
		#endregion
	}
}
