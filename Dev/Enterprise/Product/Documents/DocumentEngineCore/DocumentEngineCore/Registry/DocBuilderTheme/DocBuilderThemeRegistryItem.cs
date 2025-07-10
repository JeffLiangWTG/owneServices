using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	public sealed class DocBuilderThemeRegistryItem : TranslatableRegistryItem<DocBuilderThemeRegistry, DocBuilderThemeRegistry>
	{
		public DocBuilderThemeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, DocBuilderThemeRegistry defaultThemeSelector)
			: base(new RegistryItemImpl(name, category, caption, hint, new DocBuilderThemeRegistryDataType(), RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, defaultThemeSelector))
		{
			this.defaultValue = defaultThemeSelector;
			Convert(defaultThemeSelector);
		}

		[RegistryEditor("Enterprise.Registry.GUI.DocBuilderThemeRegistryItemEditor, Enterprise.Registry.GUI")]
#if DEBUG
		public
#endif
		class DocBuilderThemeRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DocBuilderThemeRegistry>
		{
		}

		public event EventHandler<EventArgs> ThemeChanged;

		void OnThemeChanged()
		{
			ThemeChanged?.Invoke(this, EventArgs.Empty);
		}

		protected override void SetValueCore(Guid companyOrOwnerPk, Guid branchPk, Guid departmentPk, object newValue)
		{
			base.SetValueCore(companyOrOwnerPk, branchPk, departmentPk, newValue);
			OnThemeChanged();
		}

		protected override void DeleteValueCore(Guid companyPk, Guid branchPk, Guid departmentPk)
		{
			base.DeleteValueCore(companyPk, branchPk, departmentPk);
			OnThemeChanged();
		}

		public override bool IsTranslatable => true;

		public override int MaxLength => 255;

		protected override DocBuilderThemeRegistry Convert(DocBuilderThemeRegistry value)
		{
			foreach (DocBuilderTheme item in value.Themes)
			{
				item.NameMultilingual = GetMultilingualString(item.Name);
			}
			return value;
		}

		public override IEnumerable<ResourceString> DefaultStrings
		{
			get
			{
				return defaultValue.Themes.Where(p => !p.NameMultilingual.IsEmpty).Select(q => (ResourceString)q.NameMultilingual);
			}
		}

		public override IEnumerable<string> GetCaptions(DocBuilderThemeRegistry value)
		{
			return value.Themes.Select(p => p.Name.ToString());
		}

		readonly DocBuilderThemeRegistry defaultValue;
	}
}
