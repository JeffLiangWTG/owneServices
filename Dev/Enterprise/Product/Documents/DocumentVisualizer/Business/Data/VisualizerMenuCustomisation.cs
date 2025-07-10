using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentVisualizer.Business
{
	public sealed class VisualizerMenuCustomisation : MenuCustomisation
	{
		public VisualizerMenuCustomisation(BusinessObject parent, BusinessObjectFactory factory, string businessContext)
			: base(factory)
		{
			Argument.NotNull(parent, nameof(parent));
			Argument.NotNullOrEmpty(businessContext, nameof(businessContext));

			this.parent = parent;
			this.businessContext = businessContext;
		}

		readonly BusinessObject parent;
		readonly string businessContext;

		protected override DocumentSupporter DocumentSupporter => new DocumentMenuCustomisationDocumentSupporter(this);

		public BusinessObject Parent => parent;

		protected override string Description => (NoResString)"Form";

		protected override PrintTask GetPrintTaskCore(StmMenuItemBase menuItem)
		{
			return new PrintTask(menuItem);
		}

		protected override StmMenuItemBaseCollection InitialiseMenus()
		{
			return new VisualizerMenuItemCollection(Factory, businessContext, ExcludedTemplatePKs);
		}

		protected override StmTemplateBaseCollection InitialiseAvailableTemplates()
		{
			return new VisualizerTemplateCollection(Factory, ExcludedTemplatePKs);
		}

		ZGuid[] ExcludedTemplatePKs => excludedTemplatePKs ?? (excludedTemplatePKs = ClientMenuCustomisationHelper.GetExcludedTemplatePKs());
		ZGuid[] excludedTemplatePKs;

		protected override string ValidateNewTemplate(DataContextValue dataContext)
		{
			return dataContext.Equals(DataContextValue.None)
				? null
				: Res.GetString("283421e6-4902-4989-8cba-8e89f4153a4c", "This template cannot be added because it requires a specific data context but visualizer templates cannot have a data context.");
		}

		protected override bool SetDataContextFromTemplate => false;
	}
}
