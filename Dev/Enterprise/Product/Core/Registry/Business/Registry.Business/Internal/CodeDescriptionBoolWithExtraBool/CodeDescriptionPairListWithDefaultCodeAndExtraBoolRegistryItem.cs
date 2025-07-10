using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public sealed class CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryItem : StronglyTypedRegistryItem<ICodeDescriptionPairListWithDefaultCodeAndExtraBool, SystemDefinableCodeDescriptionBoolWithExtraBoolCollection>, ICodeDescriptionPairListProvider
	{
		public CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions registryOptions, CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryDataType dataType, bool allowNew = true)
			: base(new RegistryItemImpl(name, category, caption, hint, dataType ?? throw new ArgumentNullException(nameof(dataType)), storage, registryOptions, GetDefaultValue(dataType, allowNew)))
		{
		}

		static SystemDefinableCodeDescriptionBoolWithExtraBoolCollection GetDefaultValue(CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryDataType dataType, bool allowNew)
		{
			SystemDefinableCodeDescriptionBoolWithExtraBoolCollection result;
			if (dataType.SystemDefinedList != null)
			{
				result = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection(dataType.CodeMaxLength, dataType.SystemDefinedList, false, allowNew);
				result.SetDefaultCode(dataType.SystemDefinedList.DefaultCode, false);
				foreach (var extraBoolCheckedCode in dataType.ExtraBoolCheckedCodes)
				{
					result.FindElementByCode(extraBoolCheckedCode).Bool2 = true;
				}
			}
			else
			{
				result = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection(dataType.CodeMaxLength);
			}

			return result;
		}

		#region ICodeDescriptionPairListProvider

		CodeDescriptionPairList ICodeDescriptionPairListProvider.CodeDescriptionPairList => Value.GetCodeDescriptionPairList();

		#endregion
	}

	[RegistryEditor("Enterprise.Registry.GUI.CodeDescriptionBoolWithExtraBoolRegistryItemEditor, Enterprise.Registry.GUI")]
	public class CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryDataType : NonPersistentBusinessObjectRegistryDataType<SystemDefinableCodeDescriptionBoolWithExtraBoolCollection>
	{
		public CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryDataType(ReadOnlyCodeDescriptionPairList systemDefinedList, bool allowNew, params string[] extraBoolCheckCodes)
		{
			SystemDefinedList = systemDefinedList;
			AllowNew = allowNew;
			CodeMaxLength = systemDefinedList?.MaxCodeLength ?? 0;
			ExtraBoolCheckedCodes = extraBoolCheckCodes
				.Intersect(systemDefinedList?.GetAllCodes() ?? Enumerable.Empty<string>())
				.ToArray();
		}

		protected override SystemDefinableCodeDescriptionBoolWithExtraBoolCollection DeserialiseCore(byte[] value)
		{
			var result = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection(CodeMaxLength, SystemDefinedList, false, AllowNew);
			result.Populate(value);
			return result;
		}

		protected override void ValidateCore(IRegistryItem registryItem, SystemDefinableCodeDescriptionBoolWithExtraBoolCollection proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
			if (proposedValue.Count == 0)
			{
				throw new RegistryValidationException((NoResString)"Please enter something in the list.");
			}
			else if (proposedValue.DefaultElement == null)
			{
				throw new RegistryValidationException((NoResString)"Please select a default code.");
			}
		}

		public bool AllowNew { get; }
		public ReadOnlyCodeDescriptionPairList SystemDefinedList { get; }
		public int CodeMaxLength { get; }
		public IEnumerable<string> ExtraBoolCheckedCodes { get; }
	}
}
