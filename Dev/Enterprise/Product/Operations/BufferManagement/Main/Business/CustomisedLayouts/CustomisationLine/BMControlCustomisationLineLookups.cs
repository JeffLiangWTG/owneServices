using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public class BMControlCustomisationLineLookups : ControlCustomisationBaseLookups
	{
		public BMControlCustomisationLineLookups(BMControlCustomisationLine parent)
			: base(parent)
		{
		}

		new BMControlCustomisationLine Parent
		{
			get { return (BMControlCustomisationLine)base.Parent; }
		}

		public CodeDescriptionPairList PropertySources
		{
			get { return Factory.GetCachedValue<PropertySourceList>(); }
		}

		public CodeDescriptionPairList PropertySourceDescriptions
		{
			get
			{
				return Factory.GetCachedValue("BMControlCustomisationLineLookups.PropertySourceDescriptions", () =>
				{
					var list = new CodeDescriptionPairList();

					foreach (CodeDescriptionPair pair in PropertySources)
					{
						list.AddPair(pair.Description);
					}

					return list;
				});
			}
		}

		public CodeDescriptionPairList PropertyNames
		{
			get { return Factory.GetCachedValue("BMControlCustomisationLineLookups.PropertyNames." + Parent.PropertySource, () => new CustomisationLinePropertyList(Parent.PropertySource)); }
		}

		public CodeDescriptionPairList PropertyNameDescriptions
		{
			get
			{
				return Factory.GetCachedValue("BMControlCustomisationLineLookups.PropertyNameDescriptions." + Parent.PropertySource, () =>
				{
					var list = new CodeDescriptionPairList();

					foreach (CodeDescriptionPair pair in PropertyNames)
					{
						list.AddPair(pair.Description);
					}

					return list;
				});
			}
		}
	}
}
