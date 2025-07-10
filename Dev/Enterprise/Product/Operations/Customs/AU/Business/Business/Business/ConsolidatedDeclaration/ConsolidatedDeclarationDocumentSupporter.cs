using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ConsolidatedDeclarationDocumentSupporter : Customs.Business.BaseConsolidatedDeclarationDocumentSupporter
	{
		public ConsolidatedDeclarationDocumentSupporter(ConsolidatedDeclaration consolidatedDeclaration)
			: base(consolidatedDeclaration)
		{
		}

		protected new JobDeclaration Declaration => ConsolidatedDeclaration.BuildAggregateJobDeclaration() as JobDeclaration;

		protected CusEntryHeader EntryHeader => Declaration?.EntryHeader;

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			if (dataContextValue.DataContext == DataContext.Declaration)
			{
				return Res.GetString("Enterprise.Customs.AU.Declaration.Business.ConsolidatedDeclarationDocumentSupporter|NotFoundDeclarationProviders", "Consolidated Declaration cannot be found.");
			}
			return base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			var result = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, BusinessObject);

			if (result == null)
			{
				switch (dataContext)
				{
					case DataContext.CusEntryHeader:
						var list = new System.Collections.ArrayList();
						var entryHeaderWrappers = EntryHeader?.DocumentSupporter.GetDocumentWrappers(dataContext, commandBeingRun);
						if (entryHeaderWrappers != null)
						{
							foreach (var wrapper in entryHeaderWrappers)
							{
								list.Add(wrapper);
							}
						}
						result = new DocumentWrapper[list.Count];
						for (int i = 0; i < list.Count; i++)
						{
							result[i] = (DocumentWrapper)list[i];
						}
						break;
				}
			}
			return result;
		}

		public override string GetFilterValue(DocumentFilters filterName)
		{
			string result;
			if (filterName == DocumentFilters.MSGBKRCTY && Declaration is { } declaration)
			{
				result = declaration.MessageTypeForDocumentFilter + Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(declaration.CountryCode);
			}
			else
			{
				result = base.GetFilterValue(filterName);
			}
			return result;
		}
	}
}
