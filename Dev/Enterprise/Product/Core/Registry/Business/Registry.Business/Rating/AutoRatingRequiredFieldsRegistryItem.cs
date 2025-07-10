using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class AutoRatingRequiredFieldsRegistryItem : StronglyTypedRegistryItem<IAutoRatingRequiredFields, AutoRatingRequiredFields>
	{
		public AutoRatingRequiredFieldsRegistryItem(string ratingHeaderType, string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, bool showIncoterm)
			: this(ratingHeaderType, name, category, caption, hint, storage, RegistryOptions.Default, showIncoterm)
		{
		}

		public AutoRatingRequiredFieldsRegistryItem(string ratingHeaderType, string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, bool showIncoterm)
			: base(new RegistryItemImpl(name, category, caption, hint, new AutoRatingRequiredFieldsRegistryDataType(), storage, options))
		{
			this.EditorInfo = new AutoRatingRequiredFieldsRegistryEditorInfo(showIncoterm);
			this.RatingHeaderType = ratingHeaderType;
		}

		public IAutoRatingRequiredFields TypedValue
		{
			get
			{
				AutoRatingRequiredFields result = (AutoRatingRequiredFields)base.Value;
				result.SetRatingHeaderType(RatingHeaderType);
				return result;
			}
		}

		public readonly string RatingHeaderType;
	}

	class AutoRatingRequiredFieldsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<AutoRatingRequiredFields>
	{
		public AutoRatingRequiredFieldsRegistryDataType()
		{
		}
	}
}
