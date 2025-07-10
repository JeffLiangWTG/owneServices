using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.GUI
{
	class ReportMenuCustomisation : MenuCustomisation
	{
		readonly string businessContext;

		public ReportMenuCustomisation(BusinessObjectFactory factory, string businessContext)
			: base(factory)
		{
			this.businessContext = businessContext;
		}

		protected override DocumentSupporter DocumentSupporter
		{
			get
			{
				return new ReportMenuCustomisationDocumentSupporter(this);
			}
		}

		protected override string Description => (NoResString)"Report"; // This value is used for SI_DocumentTitle, which can only accept Western European characters

		protected override PrintTask GetPrintTaskCore(StmMenuItemBase menu)
		{
			return new ReportPrintSet((ReportCommand)menu);
		}

		protected override StmMenuItemBaseCollection InitialiseMenus()
		{
			var result = new ReportCommandCollection(Factory, businessContext);
			result.SetAllowNew(true);

			return result;
		}

		protected override StmTemplateBaseCollection InitialiseAvailableTemplates()
		{
			var filter = new ZQuery(StmTemplateSchema.SO_DataContext, nameof(Core.Constants.DataContext.None));

			return new StmTemplateBaseCollection(Factory, filter);
		}

		protected override string ValidateNewTemplate(DataContextValue dataContext)
		{
			return dataContext.Equals(DataContextValue.None) ? null : Res.GetString("b2b8599a-e0bf-491f-80c0-c2749122248c", "This template cannot be added because it requires a specific data context but reports cannot have a data context.");
		}

		protected override void SetAdditionalProperties(StmMenuItemBase menu, StmMenuTemplatePivotBase pivot)
		{
			base.SetAdditionalProperties(menu, pivot);

			if (!menu.SU_IsSystemDefined && ScheduledReportDocType != null)
			{
				pivot.SI_RT_DocType = ScheduledReportDocType.PK;
			}
		}

		RefDocType scheduledReportDocType;
		internal RefDocType ScheduledReportDocType
		{
			get
			{
				if (scheduledReportDocType == null)
				{
					var filter = new ZQuery(RefDocTypeSchema.RT_DocType, Core.Constants.RefDocTypes.ScheduledReport);
					scheduledReportDocType = Factory.LoadTop1<RefDocType>(filter);
				}
				return scheduledReportDocType;
			}
		}
	}
}
