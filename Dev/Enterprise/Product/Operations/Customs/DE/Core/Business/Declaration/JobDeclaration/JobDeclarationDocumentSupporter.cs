using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Documents;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class JobDeclarationDocumentSupporter : EU.Business.Declaration.JobDeclarationDocumentSupporter
	{
		public JobDeclarationDocumentSupporter(JobDeclaration declaration)
			: base(declaration)
		{
		}

		public override ZBool ShowReasonForNotPrinting(DataContext dataContext, IStmMenuItem commandBeingRun) => false;

		protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)	=>
			dataContext switch
			{
				DataContext.SADH => DocumentSupporterHelper.GetSADHWrappers(applicableEntryHeaders),
				DataContext.CusEntryHeader => DocumentSupporterHelper.GetCusEntryHeaderWrappers(applicableEntryHeaders),
				_ => base.GetDocumentWrappersInternal(dataContext, commandBeingRun),
			};

		IEnumerable<CusEntryHeader> applicableEntryHeaders => Declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().Where(ceh => ceh.EntryInstruction != null && ceh.CH_MessageType == DeclarationApplicationCodeList.Codes.Builtin);
	}
}
