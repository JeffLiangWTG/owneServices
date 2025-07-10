using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Modules;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsEdiMessageDocumentSupporter : EDIMessageDocumentSupporter
	{
		public NctsEdiMessageDocumentSupporter(EDIMessage ediMessage)
			: base(ediMessage)
		{
			this.ediMessage = ediMessage;
		}

		public override ZBool ShowReasonForNotPrinting(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			var result = new List<DocumentWrapper>();
			ZString classAndNamespaceName = null;
			switch (dataContext)
			{
				case DataContext.EuNcts:
					classAndNamespaceName = "Enterprise.DocumentWrappers.Customs.EU.NCTS.NctsIE29DocumentWrapper";
					break;
				case DataContext.EuNcts5TAD:
					classAndNamespaceName = "Enterprise.DocumentWrappers.Customs.EU.NCTS.Phase5NctsHeaderTADDocumentWrapper";
					break;
					// Other cases go here as needed...
			}

			if (!classAndNamespaceName.IsEmpty)
			{
				var wrapper = DocumentWrapperFactory.CreateWrapperWithoutException(classAndNamespaceName, ediMessage);
				if (wrapper != null)
				{
					result.Add(wrapper);
				}
			}
			return result.ToArray();
		}

		protected override DataContext[] GetSupportedDataContexts()
		{
			var result = new List<DataContext>();
			result.Add(DataContext.EuNcts);
			return result.ToArray();
		}

		public override string GetFilterValue(DocumentFilters filterName)
		{
			switch (filterName)
			{
				case DocumentFilters.NCTS:
					return "Y";
			}
			return base.GetFilterValue(filterName);
		}

		public override string GetMenuTemplateFilterValue(MenuTemplateFilterType filterType, IBODocDataProvider dataProvider)
		{
			switch (filterType)
			{
				case MenuTemplateFilterType.Security:
					var nctsHeader = ediMessage.EM_LinkedObject as NctsHeader;
					if (nctsHeader != null)
					{
						return nctsHeader.IsSecurityDeclaration ? "Y" : "N";
					}
					break;
			}
			return base.GetMenuTemplateFilterValue(filterType, dataProvider);
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CusInBondHeader; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Environment.Env.Security.None; }
		}

		readonly EDIMessage ediMessage;
	}
}
