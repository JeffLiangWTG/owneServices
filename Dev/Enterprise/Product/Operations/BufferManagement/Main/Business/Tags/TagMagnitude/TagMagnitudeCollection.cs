using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	[ModuleID(ModuleId.BMTagMagnitude)]
	public class TagMagnitudeCollection : ActiveBusinessObjectCollection<TagMagnitude>, ITagMagnitudeCollection
	{
		public TagMagnitudeCollection(TagDefinition definition, ZQuery filter)
			: base(definition.Factory, definition, filter, TagMagnitudeSchema.TGM_TGD_Tag)
		{
			this.definition = definition;
		}

		readonly TagDefinition definition;

		public TagMagnitudeCollection(TagDefinition definition)
			: this(definition, new ZQuery())
		{
		}

		public TagMagnitudeCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public TagMagnitudeCollection(BusinessObjectFactory factory)
			: this(factory, new ZQuery())
		{
		}

		protected override bool AllowNew
		{
			get { return base.AllowNew && (definition == null || !definition.TGD_IsSystem); }
		}

		protected override void SetDefaultsForNewElementCore(TagMagnitude newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			if (definition != null && definition.TGD_IsExclusive && Count > 0)
			{
				newElement.TGM_RuleRunSequence = this.Max(m => m.TGM_RuleRunSequence) + 1;
			}
		}
	}
}
