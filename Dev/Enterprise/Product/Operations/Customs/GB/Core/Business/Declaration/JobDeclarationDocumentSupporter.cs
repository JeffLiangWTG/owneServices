using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class JobDeclarationDocumentSupporter : EU.Business.Declaration.JobDeclarationDocumentSupporter
	{
		public JobDeclarationDocumentSupporter(JobDeclaration declaration)
			: base(declaration)
		{
			gbDeclaration = declaration;
		}

		protected JobDeclaration gbDeclaration;

		public override ZBool ShowReasonForNotPrinting(DataContext dataContext, IStmMenuItem commandBeingRun) => false;

		protected override DataContext[] GetSupportedDataContexts()
		{
			var result = new List<DataContext>(base.GetSupportedDataContexts())
			{
				DataContext.GbTaxEstimator
			};
			return result.ToArray();
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			IEnumerable<DocumentWrapper> wrappers = null;

			switch (dataContext)
			{
				case DataContext.GbTaxEstimator:
					wrappers = CreateEntryHeaderWrappers("Enterprise.Customs.GB.DocumentWrappers.DocTaxEstimatorWrapper, Enterprise.Customs.GB.DocumentWrappers");
					break;

				case DataContext.SADH:
					wrappers = CreateEntryHeaderWrappers("Enterprise.Customs.GB.DocumentWrappers.DocSADH, Enterprise.Customs.GB.DocumentWrappers");
					break;
			}

			return wrappers?.ToArray() ?? base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
		}

		IEnumerable<DocumentWrapper> CreateEntryHeaderWrappers(string wrapperFullName)
		{
			return Declaration.CustomsEntryHeaders.Select(e => DocumentWrapperFactory.CreateWrapperWithoutException(wrapperFullName, e))
												  .Where(w => w != null);
		}
	}
}
