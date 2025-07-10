using System.Collections.Generic;
using Enterprise.BusinessObjectGenerator.ModelView;

namespace Enterprise.BusinessObjectGenerator
{
	public class ModelViewCodeCollection
	{
		public ModelViewCodeCollection(ModelViewContext context, List<ModelViewContext> allContexts)
		{
			this.context = context;
			this.allContexts = allContexts;
		}

		public ModelViews ModelViews => modelViews ??= new ModelViews(allContexts);
		ModelViews modelViews;

		public ModelViewDefinition ModelViewDefinition => modelViewDefinition ??= new ModelViewDefinition(context);
		ModelViewDefinition modelViewDefinition;

		public SqlView SqlView => sqlView ??= new SqlView(context);
		SqlView sqlView;

		public SqlIndexedView SqlIndexedView => sqlIndexedView ??= new SqlIndexedView(context);
		SqlIndexedView sqlIndexedView;

		readonly ModelViewContext context;
		readonly List<ModelViewContext> allContexts;
	}
}
