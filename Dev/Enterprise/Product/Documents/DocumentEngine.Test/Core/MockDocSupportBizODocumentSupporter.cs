using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using BusinessObject = CargoWise.EntityFramework.BusinessObject;

namespace Enterprise.DocumentEngine.Testing
{
	class MockDocSupportBizODocumentSupporter : DocumentSupporter
	{
		public MockDocSupportBizODocumentSupporter(BusinessObject businessObject)
			: base(businessObject)
		{
		}

		public MockDocSupportBizODocumentSupporter(MockDocSupportBizO bizO, DocumentWrapper[] wrappers, short copyCount)
			: base(bizO)
		{
			this.BizO = bizO;
			this.Wrappers = wrappers;
			this.CopyCount = copyCount;
		}

		readonly MockDocSupportBizO BizO;
		readonly DocumentWrapper[] Wrappers;
		readonly short CopyCount;

		protected override bool GetIsNonPersistent()
		{
			return false;
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Test; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Enterprise.Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (Wrappers != null)
			{
				return Wrappers;
			}
			else
			{
				return new DocumentWrapper[] { null };
			}
		}

		protected override Enterprise.Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return null;
		}

		public override TitleCopyCountPair GetDocumentTitlesForPivot(ZString documentMenuName, IDocumentSupportable parentBusinessObject, IStmMenuTemplatePivot pivot)
		{
			if (pivot.SI_DocumentTitle == "TestDoc")
			{
				return new TitleCopyCountPair("TestDoc1", CopyCount);
			}
			else
			{
				return null;
			}
		}

		public override string GetMenuTemplateFilterValue(MenuTemplateFilterType filterType, IBODocDataProvider dataProvider)
		{
			if (UseSeaForMenuTemplateFilterValue)
			{
				return "SEA";
			}
			else if (UseWrappedObjectFilterValue)
			{
				DocumentWrapper docWrapper = (DocumentWrapper)dataProvider;
				return ((MockDocSupportBizO)docWrapper.WrappedObject).Description;
			}
			else
			{
				return null;
			}
		}

		public bool UseSeaForMenuTemplateFilterValue;
		public bool UseWrappedObjectFilterValue;

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint { get { return Env.Security.None; } }
		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			return BizO == null ? null : new OrgHeaderContact(BizO.Organisation, BizO.RelatedParty, null);
		}
		public ZBool ShowReasonForNotPrintingForTesting { get; set; }
		public override ZBool ShowReasonForNotPrinting(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return ShowReasonForNotPrintingForTesting;
		}
	}
}
