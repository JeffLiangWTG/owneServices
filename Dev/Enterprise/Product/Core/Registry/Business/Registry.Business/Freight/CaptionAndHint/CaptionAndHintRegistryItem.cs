using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class CaptionAndHintRegistryItem : StronglyTypedRegistryItem<ICaptionAndHint, CaptionAndHint>
	{
		public CaptionAndHintRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: this(name, category, caption, hint, storage, new CaptionAndHint())
		{
		}

		public CaptionAndHintRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, CaptionAndHint defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new CaptionAndHintRegistryDataType(), storage, defaultValue))
		{
		}

		[RegistryEditor("Enterprise.Registry.GUI.CaptionAndHintRegistryItemEditor, Enterprise.Registry.GUI")]
		internal class CaptionAndHintRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CaptionAndHint>
		{
			public CaptionAndHintRegistryDataType()
			{
			}
			#region Validation
			protected override void ValidateCore(IRegistryItem registryItem, CaptionAndHint proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				if (proposedValue.Caption.Length > proposedValue.CaptionMaxLength)
				{
					throw new RegistryValidationException(Res.GetString("dbd21402-3d5b-4974-9c70-3132d6429c27", "Caption must be less than or equal to {0} characters", proposedValue.CaptionMaxLength));
				}
			}
			#endregion
		}
	}
}
