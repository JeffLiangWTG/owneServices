using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Customs.GB.Business
{
	public class EdiMessageDocumentSupporter : Customs.Business.EDIMessageDocumentSupporter
	{
		public EdiMessageDocumentSupporter(EDIMessage ediMessage)
			: base(ediMessage)
		{
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			ZString classAndNamespaceName = null;
			switch (dataContext)
			{
				case DataContext.GbCcsuk:
					classAndNamespaceName = "Enterprise.Customs.GB.DocumentWrappers.Ccsuk.CcsukWrapper, Enterprise.Customs.GB.DocumentWrappers";
					break;

				case DataContext.GbMcpPhs11:
					classAndNamespaceName = "Enterprise.Customs.GB.DocumentWrappers.MCP.PHS11.DocMcpPhs11Wrapper, Enterprise.Customs.GB.DocumentWrappers";
					break;
			}

			if (!classAndNamespaceName.IsEmpty)
			{
				var result = new List<DocumentWrapper>();
				var wrapper = DocumentWrapperFactory.CreateWrapperWithoutException(classAndNamespaceName, EdiMessage);
				if (wrapper != null)
				{
					result.Add(wrapper);
				}
				return result.ToArray();
			}

			return base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
		}

		protected override DataContext[] GetSupportedDataContexts()
		{
			var result = new List<DataContext>(base.GetSupportedDataContexts())
			{
				DataContext.GbCcsuk,
				DataContext.GbMcpPhs11
			};
			return result.ToArray();
		}

		public override string GetMenuTemplateFilterValue(MenuTemplateFilterType filterType, IBODocDataProvider docWrapperForCurrentPivot)
		{
			if (filterType == MenuTemplateFilterType.GbCcsuk)
			{
				if (docWrapperForCurrentPivot is Integration.Customs.GB.ICcsukCusunderbondDocumentProvider ccsukReport)
				{
					return ccsukReport.ReportTypeCode;
				}
			}
			return base.GetMenuTemplateFilterValue(filterType, docWrapperForCurrentPivot);
		}

		public override TitleCopyCountPair GetDocumentTitlesForPivot(ZString parentDocumentMenuName, IDocumentSupportable parentBusinessObject, IStmMenuTemplatePivot pivot)
		{
			return new TitleCopyCountPair(((StmMenuTemplatePivot)pivot).Template.SO_Name, 1);
		}
	}
}
