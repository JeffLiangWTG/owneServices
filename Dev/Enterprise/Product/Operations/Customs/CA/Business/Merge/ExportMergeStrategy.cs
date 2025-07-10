namespace Enterprise.Customs.CA.Business
{
	public class ExportMergeStrategy : Customs.Business.EntryCreationStrategy
	{
		public ExportMergeStrategy(JobDeclaration declaration, string entryType)
			: base(declaration, entryType)
		{
		}

		public override Customs.Business.MergeKey GetKeyForLine(Customs.Business.BaseJobComInvoiceLine baseInvoiceLine)
		{
			Customs.Business.MergeKey result = base.GetKeyForLine(baseInvoiceLine);
			JobComInvoiceLine line = baseInvoiceLine as JobComInvoiceLine;
			if (line != null)
			{
				result.Add(line.EffectiveCountryOfOrigin);
				result.Add(line.EffectiveProvinceOfOrigin);
				result.Add(line.JI_CustomsUnitQty);
				result.Add(line.CA_ConveyanceIdentificationNumber);
			}
			return result;
		}

		protected override Customs.Business.MergeKey GetKeyForHeaderCore(Customs.Business.BaseJobComInvoiceLine invoiceLine)
		{
			return new Customs.Business.MergeKey();
		}

		protected override void AfterCreateOrGetEntryHeader(Customs.Business.CusEntryHeader entryHeader, Customs.Business.BaseJobComInvoiceLine baseInvoiceLine)
		{
			base.AfterCreateOrGetEntryHeader(entryHeader, baseInvoiceLine);
			entryHeader.CH_BGMReference = Declaration.JE_DeclarationReference;
			if (entryHeader.CH_MessageType == MessageTypeList.Codes.DataLoadingModule)
			{
				entryHeader.EntryNumber = Declaration.JE_DeclarationReference;
			}
		}
	}
}
