using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class LineMerger : EU.Business.Declaration.LineMerger
	{
		public const string SimplifiedAuthorizationCode = "00100";

		public LineMerger(JobDeclaration declaration) : base(declaration)
		{
		}

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected override Customs.Business.IDutyCalculatorStrategy GetNewDutyCalculatorStrategy() => new DutyCalculatorStrategy(Declaration);

		protected override void PerformCountrySpecificOperationAfterMergeBeforeCalculateDuty()
		{
			base.PerformCountrySpecificOperationAfterMergeBeforeCalculateDuty();
			CalculateInvoiceAmount();
		}

		protected override void PerformCountrySpecificOperationAfterMergeAfterCalculateDuty()
		{
			base.PerformCountrySpecificOperationAfterMergeAfterCalculateDuty();
			UpdateVATDeferType();
		}

		void UpdateVATDeferType()
		{
			var declaration = Declaration;
			if (declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().All(x => x.IsAllEntryLinesVatSuspended))
			{
				declaration.ZG_VATDeferType = ZString.Empty;
			}
		}

		protected override void OnMerging()
		{
			base.OnMerging();
			Declaration.PopulateInvoiceCharges();
			Declaration.JobComInvoiceGroupHeaders.Cast<JobComInvoiceGroupHeader>().ForEach(x => x.PopulateCharges());
			UpdateSimplifiedAuthorizationSpecialMentions();
		}

		void UpdateSimplifiedAuthorizationSpecialMentions()
		{
			var invoiceLines = Declaration.InvoiceLines;
			foreach (JobComInvoiceLine line in invoiceLines)
			{
				if (line.EntryInstruction?.SpecificRegimeAuthorisationUsage?.RelatedAuthorisationHeader?.CPH_IsSingleUse ?? false)
				{
					var additionalInfos = line.AdditionalInfos;
					if (!additionalInfos.Find(_ => _.CSI_Code == SimplifiedAuthorizationCode).Any())
					{
						var additionalInfo = additionalInfos.AddNew();
						additionalInfo.CSI_Code = SimplifiedAuthorizationCode;
					}
				}
				else
				{
					var existingadditionalInfo = line.AdditionalInfos.Find(p => p.CSI_Code == SimplifiedAuthorizationCode).ToList();
					foreach (AdditionalInfo info in existingadditionalInfo)
					{
						info.Delete();
					}
				}
			}
		}

		protected override Customs.Business.ILineNumberAssigner GetLineNumberAssigner(Customs.Business.CusEntryHeader entryHeader) => new LineNumberAssigner(entryHeader);
	}
}
