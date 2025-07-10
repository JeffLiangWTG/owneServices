using System;
using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class CommunicationStatusRegistryItem : TranslatableRegistryBusinessItemCollectionRegistryItem<CommunicationStatusCollection, CommunicationStatusCollection>
	{
		public CommunicationStatusRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, CommunicationStatusRegistryEditorInfo editorInfo, CommunicationStatusCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new CommunicationStatusRegistryDataType(defaultValue), storage, defaultValue))
		{
			this.EditorInfo = editorInfo;
		}

		public override int MaxLength
		{
			get { return 256; }
		}
	}

	public class CommunicationStatusRegistryDataType : NonPersistentBusinessObjectRegistryDataTypeWithEnabledItem<CommunicationStatusCollection>
	{
		public CommunicationStatusRegistryDataType(CommunicationStatusCollection defaultValue)
		{
			this.defaultValue = defaultValue;
		}

		readonly CommunicationStatusCollection defaultValue;

		protected override bool HasEnabledItem(CommunicationStatusCollection proposedValue)
		{
			return proposedValue.Any(x => ((CommunicationStatus)x).Bool);
		}

		protected override CommunicationStatusCollection DeserialiseCore(byte[] value)
		{
			var result = new CommunicationStatusCollection(defaultValue.DefaultClosedForNewChild, defaultValue.DefaultBoolForNewChild);

			result.AddRange(base.DeserialiseCore(value));

			foreach (CommunicationStatus defaultStatus in defaultValue)
			{
				if (defaultStatus.SystemDefined)
				{
					var deserialisedStatusWithSameCodeAsDefault = ((CommunicationStatus)result.FindByCode(defaultStatus.Code));
					if (deserialisedStatusWithSameCodeAsDefault == null)
					{
						result.Add(defaultStatus);
					}
					else if (deserialisedStatusWithSameCodeAsDefault.Description.GetUnresolvedString().Equals(defaultStatus.Description, StringComparison.OrdinalIgnoreCase))
					{
						deserialisedStatusWithSameCodeAsDefault.SystemDefined = true;
						deserialisedStatusWithSameCodeAsDefault.Description = defaultStatus.Description;
					}
				}
			}

			return result;
		}
	}
}
