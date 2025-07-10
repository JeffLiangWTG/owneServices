using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.NL.Business.Declaration;

public class JobDeclarationDocumentSupporter : EU.Business.Declaration.JobDeclarationDocumentSupporter
{
	public JobDeclarationDocumentSupporter(JobDeclaration jobDeclaration)
		: base(jobDeclaration)
	{
	}

	public override ZBool ShowReasonForNotPrinting(DataContext dataContext, IStmMenuItem commandBeingRun) => false;

	protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
	{
		DocumentWrapper[] result;
		if (dataContext == DataContext.CusEntryHeader)
		{
			var list = new List<DocumentWrapper>();
			foreach (var cusEntryHeader in Declaration.CustomsEntryHeaders)
			{
				if (cusEntryHeader.MergedLines.Count > 0)
				{
					list.Add(DocumentWrapperFactory.CreateCustomsWrapper(DataContext.CusEntryHeader, cusEntryHeader, CountryCodes.Netherlands));
				}
			}
			result = list.ToArray();
		}
		else
		{
			result = base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
		}

		return result;
	}

	protected override List<DataContextValue> GetSupportedBODataSources() => new()
	{
		new(CusEntryHeaderDocumentSupporter.UTBDocument),
		new(CusEntryHeaderDocumentSupporter.ReleaseDocument)
	};

	protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
	{
		var fullDataContext = dataContextValue.FullDataContext;
		if (fullDataContext == CusEntryHeaderDocumentSupporter.UTBDocument || fullDataContext == CusEntryHeaderDocumentSupporter.ReleaseDocument)
		{
			var result = new List<IBODocDataProvider>();
			foreach (var entryHeader in Declaration.ActiveEntryHeaders.Cast<CusEntryHeader>())
			{
				result.AddRange(entryHeader.DocumentSupporter.GetBODocDataProviders(dataContextValue, commandBeingRun));
			}
			return result.ToArray();
		}
		else
		{
			return base.GetBODocDataProvidersInternal(dataContextValue, commandBeingRun);
		}
	}
}
