using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class TranslatableRegistryItemForTest<TGet, TSet> : TranslatableRegistryItem<TGet, TSet> where TGet : BusinessObjectCollection
	{
		public TranslatableRegistryItemForTest(IRegistryItem inner)
			: base(inner)
		{ }

		public override IEnumerable<ResourceString> DefaultStrings
		{
			get
			{
				yield break;
			}
		}

		public override bool IsTranslatable
		{
			get
			{
				return true;
			}
		}

		public override int MaxLength
		{
			get
			{
				return 256;
			}
		}

		public override IEnumerable<string> GetCaptions(TGet value)
		{
			foreach (var item in value)
			{
				yield return item.HumanReadableName;
			}
		}
	}
}
