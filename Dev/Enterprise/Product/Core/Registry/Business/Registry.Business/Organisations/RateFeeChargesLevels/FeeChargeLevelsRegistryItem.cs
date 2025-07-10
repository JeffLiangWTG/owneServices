using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class FeeChargeLevelsRegistryItem : TranslatableRegistryItem<FeeChargeLevelsSection, FeeChargeLevelsSection>
	{
		const int MaxLengthDescription = 256;

		public FeeChargeLevelsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, FeeChargeLevelsSection feeChargeLevels)
			: base(new RegistryItemImpl(name, category, caption, hint, new FeeChargeLevelsRegistryDataType(), storage, feeChargeLevels))
		{
			defaultValue = feeChargeLevels;
		}

		[RegistryEditor("Enterprise.Registry.GUI.FeeChargeLevelsRegistryItemEditor, Enterprise.Registry.GUI")]
		public class FeeChargeLevelsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<FeeChargeLevelsSection>
		{
		}

		#region TranslatableRegistryItem

		protected override FeeChargeLevelsSection Convert(FeeChargeLevelsSection value)
		{
			foreach (FeeChargeType type in value.FeeChargeTypes)
			{
				type.Description = GetMultilingualString(type.EnglishDescription);

				foreach (FeeChargeLevel level in type.FeeChargeLevels)
				{
					level.Description = GetMultilingualString(level.EnglishDescription);
				}
			}

			return value;
		}

		public override IEnumerable<ResourceString> DefaultStrings
		{
			get
			{
				foreach (FeeChargeType type in defaultValue.FeeChargeTypes)
				{
					yield return (ResourceString)type.Description;

					foreach (FeeChargeLevel level in type.FeeChargeLevels)
					{
						yield return (ResourceString)level.Description;
					}
				}
			}
		}

		public override IEnumerable<string> GetCaptions(FeeChargeLevelsSection value)
		{
			var results = new List<string>();

			var types = value.FeeChargeTypes.Cast<FeeChargeType>()
				  .Select(i => i.EnglishDescription.Trim().ToString())
				  .Where(s => !string.IsNullOrWhiteSpace(s));

			results.AddRange(types);

			foreach (FeeChargeType type in value.FeeChargeTypes)
			{
				var levels = type.FeeChargeLevels.Cast<FeeChargeLevel>()
					.Select(i => i.EnglishDescription.Trim().ToString())
					.Where(s => !string.IsNullOrWhiteSpace(s))
					.Cast<string>();

				results.AddRange(levels);
			}

			return results.Distinct();
		}

		public override bool IsTranslatable
		{
			get { return true; }
		}

		public override int MaxLength
		{
			get { return MaxLengthDescription; }
		}

		#endregion

		readonly FeeChargeLevelsSection defaultValue;
	}
}
