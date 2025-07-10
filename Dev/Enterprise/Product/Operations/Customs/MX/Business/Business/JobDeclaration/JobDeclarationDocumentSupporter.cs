using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.MX.Business
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
					foreach (CusEntryHeader entryHeader in Declaration.CustomsEntryHeaders)
					{
						result.Add(DocumentWrapperFactory.CreateCustomsWrapper(Core.Constants.DataContext.CusEntryHeader, entryHeader, Enterprise.Core.Constants.CountryCodes.Mexico));
					}
					return result.ToArray();
			}
			return base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
		}

		protected override DataContext[] GetSupportedDataContexts()
		{
			var result = new List<DataContext>(base.GetSupportedDataContexts());
			result.Add(DataContext.CusEntryHeader);
			return result.ToArray();
		}
	}
}
