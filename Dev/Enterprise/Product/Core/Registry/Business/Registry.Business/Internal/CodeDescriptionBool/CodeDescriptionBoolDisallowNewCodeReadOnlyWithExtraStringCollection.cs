using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class CodeDescriptionBoolDisallowNewCodeReadOnlyWithExtraStringCollection : CodeDescriptionBoolDisallowNewCollection
	{
		public CodeDescriptionBoolDisallowNewCodeReadOnlyWithExtraStringCollection()
			: base()
		{
		}

		public CodeDescriptionBoolDisallowNewCodeReadOnlyWithExtraStringCollection(int codeMaxLength = default)
			: base()
		{
			if (codeMaxLength > 0)
			{
				CodeMaxLength = codeMaxLength;
			}
		}

		public CodeDescriptionBoolDisallowNewCodeReadOnlyWithExtraStringCollection(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public CodeDescriptionBoolDisallowNewCodeReadOnlyWithExtraStringCollection(ReadOnlyCodeDescriptionPairList list)
			: base(list)
		{
		}

		protected override bool AllowNewCore => false;

		public new CodeDescriptionBoolDisallowNewCodeReadOnlyWithExtraString AddNew() => (CodeDescriptionBoolDisallowNewCodeReadOnlyWithExtraString)base.AddNew();

		protected override BusinessObject CreateNonPersistentBusinessObject() => new CodeDescriptionBoolDisallowNewCodeReadOnlyWithExtraString();

		public new CodeDescriptionBoolDisallowNewCodeReadOnlyWithExtraString this[int i] => (CodeDescriptionBoolDisallowNewCodeReadOnlyWithExtraString)base[i];

		protected override CodeDescriptionBoolCollection GetNewCollection() => new CodeDescriptionBoolDisallowNewCodeReadOnlyWithExtraStringCollection(CurrentFallbackLevel);
	}
}
