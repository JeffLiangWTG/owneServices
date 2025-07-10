using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ResourceStrings.Business
{
	public class CustomizableDataTranslationEntry : AutoCustomizableDataTranslationEntry
	{
		public CustomizableDataTranslationEntry(CustomizableDataTranslationPage page, ZString language, ResourceString caption)
		{
			this.page = page;
			this.Language = language;
			this.caption = caption;
			this.English = caption.ToStringWithParameters(Res.DefaultLanguage);
			this.Translation = caption.ToStringWithParameters(language);
		}

		public ResourceString Caption
		{
			get { return caption; }
		}
		readonly ResourceString caption;

		[ReadOnly(true)]
		public override ZString Language
		{
			get { return base.Language; }
			set { base.Language = value; }
		}

		public ZString LanguageDescription
		{
			get { return languageDescription ?? (languageDescription = new CodeDescriptionPairList(OLookUpEditType.Language).GetDescriptionFromCode(Language)).Value; }
		}
		ZString? languageDescription;

		[ReadOnly(true)]
		public override ZString English
		{
			get { return base.English; }
			set { base.English = value; }
		}

		[BusinessObjectTestExclude]
		public override ZString Translation
		{
			get { return base.Translation; }
			set
			{
				ZString newValue;
				if (value.IsEmpty)
				{
					newValue = Caption.ToString(Language);
				}
				else
				{
					newValue = value.Trim();
				}

				var translation_MaxLength = Translation_MaxLength;
				if (translation_MaxLength >= 0 && newValue.Length > translation_MaxLength)
				{
					OriginalTranslation = newValue;
					newValue = newValue.SubstringSafe(0, translation_MaxLength);
				}
				else
				{
					OriginalTranslation = null;
				}

				base.Translation = newValue;
			}
		}

		internal ZString OriginalTranslation { get; private set; }

		protected override int Translation_MaxLength
		{
			get
			{
				int result;
				if (page != null)
				{
					var translatableRegistryItem = page.Customizable.Source as TranslatableRegistryItemValueCaptionSource;
					if (translatableRegistryItem != null)
					{
						result = translatableRegistryItem.GetMaxLength(Caption);
					}
					else
					{
						var multiCaptionSource = page.Customizable.Source as MultipleDataCaptionSource;
						if (multiCaptionSource != null)
						{
							result = multiCaptionSource.GetMaxLength(Caption);
						}
						else
						{
							result = page.Customizable.Source.MaxLength;
						}
					}
				}
				else
				{
					result = base.Translation_MaxLength;
				}
				return result;
			}
		}

		readonly CustomizableDataTranslationPage page;
	}
}
