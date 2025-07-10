using System.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public abstract class ModuleFilterSubGroup : BlueprintModuleFilterSubGroup
	{
		#region Default

		public static ModuleFilterSubGroup Default
		{
			get { return DefaultImplementation.instance; }
		}

		[ImmutableObject(true)]
		class DefaultImplementation : ModuleFilterSubGroup
		{
			DefaultImplementation()
				: base(null, true) { }

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "It is readonly")]
			public static readonly DefaultImplementation instance = new DefaultImplementation();

			public override ZQuery GetSubQuery(ZQuery query)
			{
				return query;
			}
		}

		#endregion

		protected ModuleFilterSubGroup()
			: base(ModuleFilterSubGroup.Default, false) { }

		protected ModuleFilterSubGroup(ModuleFilterSubGroup parent)
			: base(parent) { }

		protected ModuleFilterSubGroup(ModuleFilterSubGroup parent, bool allowNullParent)
			: base(parent, allowNullParent) { }

		public sealed override QueryBlueprintPart GetSubQuery(QueryBlueprintPart query)
		{
			query.Query = GetSubQuery(query.Construct());
			query.Children.Clear();
			return query;
		}

		public abstract ZQuery GetSubQuery(ZQuery filter);
	}
}
