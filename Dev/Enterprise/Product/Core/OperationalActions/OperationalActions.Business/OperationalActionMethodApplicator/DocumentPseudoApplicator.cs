using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Business
{
	sealed class DocumentPseudoApplicator : PseudoApplicator, IOperationalActionMethodApplicatorDelayingBizoLoading
	{
		public DocumentPseudoApplicator(OperationalActionRunner runner)
				: base(runner, Res.GetString("DocumentPseudoApplicator|Name", "Documents")) { }

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			throw new NotImplementedException();
		}

		BaseDocumentProcessor documentProcessor;
		internal BaseDocumentProcessor DocumentProcessor => documentProcessor ?? (documentProcessor = new DocumentProcessorFactory().GetProcessor(Runner));

		void IOperationalActionMethodApplicatorDelayingBizoLoading.Apply(IOperationalActionSectionLog log, ZGuid[] targetsPKs, Type bizObjType)
		{
			DocumentProcessor.InitializeProcessor(log, targetsPKs, bizObjType);
			DocumentProcessor.ProcessDocument();
		}
	}
}
