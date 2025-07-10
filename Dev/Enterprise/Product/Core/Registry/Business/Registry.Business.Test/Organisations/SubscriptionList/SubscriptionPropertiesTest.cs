using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Registry.Business.Testing
{
	sealed class SubscriptionPropertiesTest : SubscriptionProperties
	{
		[List("Lookups.MediaTypeWithAllList")]
		[MaxLength(5)]
		public override ZString MediaTypeWithAll
		{
			get => base.mediaType;
			set
			{
				base.MediaTypeWithAll = value;
			}
		}

		[List("Lookups.MediaCategoryWithAllList")]
		[MaxLength(5)]
		public override ZString MediaCategoryWithAll
		{
			get => base.mediaCategory;
			set
			{
				base.MediaCategoryWithAll = value;
			}
		}
	}
}
