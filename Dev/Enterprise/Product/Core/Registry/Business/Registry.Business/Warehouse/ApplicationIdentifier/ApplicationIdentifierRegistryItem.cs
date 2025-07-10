using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Warehouse
{
	public class ApplicationIdentifierRegistryItem : TranslatableRegistryItem<ApplicationIdentifierCollection, ApplicationIdentifierCollection>
	{
		public ApplicationIdentifierRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, ApplicationIdentifierCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ApplicationIdentifierRegistryDataType(), storage, defaultValue))
		{
		}

		#region Convert

		protected override ApplicationIdentifierCollection Convert(ApplicationIdentifierCollection value)
		{
			foreach (ApplicationIdentifier item in value)
			{
				item.FullTitle = GetMultilingualString(item.EnglishFullTitle);
				item.DataTitle = GetMultilingualString(item.EnglishDataTitle);
			}
			return value;
		}

		#endregion

		#region TranslatableRegistryItem Members

		public override IEnumerable<ResourceString> DefaultStrings
		{
			get { return System.Array.Empty<ResourceString>(); }
		}

		public override IEnumerable<string> GetCaptions(ApplicationIdentifierCollection value)
		{
			var fullTitles = value.Cast<ApplicationIdentifier>()
							  .Select(i => i.FullTitle.ToString().Trim()).Distinct()
							  .Where(s => !string.IsNullOrWhiteSpace(s)).Distinct();

			var dataTitles = value.Cast<ApplicationIdentifier>()
						  .Select(i => i.DataTitle.ToString().Trim()).Distinct()
						  .Where(s => !string.IsNullOrWhiteSpace(s)).Distinct();

			return fullTitles.Concat(dataTitles);
		}

		public override bool IsTranslatable
		{
			get { return true; }
		}

		public override int MaxLength
		{
			get { return ApplicationIdentifier.MaxFullTitleLength; }
		}

		#endregion
	}

	[RegistryEditor("Enterprise.Registry.GUI.ApplicationIdentifierRegistryItemEditor, Enterprise.Registry.GUI")]
	class ApplicationIdentifierRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ApplicationIdentifierCollection>
	{
		public ApplicationIdentifierRegistryDataType()
		{
		}
	}
}
