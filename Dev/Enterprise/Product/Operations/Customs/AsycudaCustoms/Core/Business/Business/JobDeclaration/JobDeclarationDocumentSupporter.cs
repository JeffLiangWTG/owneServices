using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class JobDeclarationDocumentSupporter : BaseJobDeclarationDocumentSupporter
	{
		public JobDeclarationDocumentSupporter(JobDeclaration jobDeclaration)
			: base(jobDeclaration)
		{
		}

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			var result = new ZStringBuilder();
			if (dataContextValue.FullDataContext == CusEntryHeaderDocumentSupporter.SummaryCusDecDocument)
			{
				var entryHeaders = JobDeclaration.ActiveEntryHeaders;
				if (entryHeaders.Count == 0)
				{
					result.AppendLine(Res.GetString("BEEEF265-B981-454E-A633-6A90AE0307CD", "No Entry Header has been found for {0} document.", commandBeingRun?.SU_MenuName ?? ZString.Empty));
				}
				else
				{
					foreach (CusEntryHeader header in entryHeaders)
					{
						result.AppendLine(header.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun));
					}
				}
			}
			else
			{
				result.AppendLine(base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun));
			}
			return result.ToString().Trim();
		}

		protected JobDeclaration JobDeclaration => (JobDeclaration)BusinessObject;

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			var result = GetSupportedBODataSourcesFor(BusinessObject.GetType());
			result.Add(new DataContextValue(CusEntryHeaderDocumentSupporter.SummaryCusDecDocument));
			return result;
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result;
			if (dataContext == Core.Constants.DataContext.CusEntryHeader)
			{
				var countryCode = JobDeclaration.Country.Code;
				result = JobDeclaration.CustomsEntryHeaders
					.Select(x => DocCusEntryHeader.New(x, x.Factory))
					.ToArray();
			}
			else
			{
				result = base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
			}
			return result;
		}

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			return dataContextValue.FullDataContext == CusEntryHeaderDocumentSupporter.SummaryCusDecDocument
				? JobDeclaration.ActiveEntryHeaders.Cast<CusEntryHeader>().SelectMany(x => x.DocumentSupporter.GetBODocDataProviders(dataContextValue, commandBeingRun)).ToArray()
				: base.GetBODocDataProvidersInternal(dataContextValue, commandBeingRun);
		}
	}
}
