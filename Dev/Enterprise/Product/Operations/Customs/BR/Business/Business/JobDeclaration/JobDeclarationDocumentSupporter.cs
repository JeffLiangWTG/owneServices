using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Customs.BR.Business
{
	public class JobDeclarationDocumentSupporter : BaseJobDeclarationDocumentSupporter
	{
		public JobDeclarationDocumentSupporter(JobDeclaration declaration)
			: base(declaration)
		{
			Declaration = declaration;
		}

		protected readonly JobDeclaration Declaration;

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			switch (dataContext)
			{
				case DataContext.CusEntryHeader:
					var result = new List<DocumentWrapper>();
					foreach (var entryHeader in Declaration.ActiveEntryHeaders.FormalEntries)
					{
						result.Add(DocumentWrapperFactory.CreateCustomsWrapper(DataContext.CusEntryHeader, entryHeader, Core.Constants.CountryCodes.Brazil));
					}
					return result.ToArray();
			}
			return base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
		}

		protected override DataContext[] GetSupportedDataContexts()
		{
			return base.GetSupportedDataContexts().Append(DataContext.CusEntryHeader).ToArray();
		}

		public override BusinessContext BusinessContext
		{
			get
			{
				if (Declaration.IsImportLicense)
				{
					return BusinessContext.BRImportLicense;
				}
				else if (Declaration.IsLPCO)
				{
					return BusinessContext.INVALID;
				}
				else
				{
					return base.BusinessContext;
				}
			}
		}
	}
}
