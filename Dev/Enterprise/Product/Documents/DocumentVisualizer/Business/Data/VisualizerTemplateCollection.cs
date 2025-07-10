using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentVisualizer.Business
{
	sealed class VisualizerTemplateCollection : StmTemplateBaseCollection
	{
		public VisualizerTemplateCollection(BusinessObjectFactory factory, ZGuid[] excludedTemplatePKs)
			: this(factory, new ZQuery(), excludedTemplatePKs)
		{
		}

		public VisualizerTemplateCollection(BusinessObjectFactory factory, ZQuery filter, ZGuid[] excludedTemplatePKs)
			: base(factory, filter)
		{
			this.excludedTemplatePKs = excludedTemplatePKs;
		}

		readonly ZGuid[] excludedTemplatePKs;

		#region Overrides

		protected override bool AllowNewCore => false;

		public new VisualizerTemplate this[int index] => (VisualizerTemplate)Elements[index];

		public new VisualizerTemplate AddNew() => (VisualizerTemplate)base.AddNew();

		protected override ZQuery CreateRelationshipFilter() => new ZQuery(StmTemplateSchema.SO_TemplateType, StmTemplateTypes.Codes.Form);

		protected override ZQuery CreateAdditionalFilter() => new ZQuery(StmTemplateSchema.PK, SQLComparisonOperator.NotEqual, excludedTemplatePKs);

		#endregion
	}
}
