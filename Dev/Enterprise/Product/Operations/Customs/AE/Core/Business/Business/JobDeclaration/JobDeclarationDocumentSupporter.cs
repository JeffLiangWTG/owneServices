
using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.AE.Business;

public class JobDeclarationDocumentSupporter : BaseJobDeclarationDocumentSupporter
{
	public JobDeclarationDocumentSupporter(JobDeclaration jobDeclaration)
		: base(jobDeclaration)
	{
	}

	protected JobDeclaration JobDeclaration
	{
		get { return (JobDeclaration)BusinessObject; }
	}

	#region Overrides

	protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
	{
		List<DocumentWrapper> list = [];
		switch (dataContext)
		{
			case Core.Constants.DataContext.CusEntryHeader:
				for (int i = 0; i < JobDeclaration.CustomsEntryHeaders.Count; i++)
				{
					list.Add(DocCusEntryHeader.New(JobDeclaration.CustomsEntryHeaders[i], JobDeclaration.CustomsEntryHeaders[i].Factory));
				}
				break;
			case Core.Constants.DataContext.ComInvoiceHeader:
				for (int i = 0; i < JobDeclaration.Invoices.Count; i++)
				{
					list.Add(DocJobComInvoiceHeader.New(JobDeclaration.Invoices[i], JobDeclaration.Invoices[i].Factory));
				}
				break;
			case Core.Constants.DataContext.Declaration:
				list.Add(DocDeclaration.New(JobDeclaration, JobDeclaration.Factory));
				break;
			default:
				return base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
		}

		return [.. list];
	}
	#endregion
}
