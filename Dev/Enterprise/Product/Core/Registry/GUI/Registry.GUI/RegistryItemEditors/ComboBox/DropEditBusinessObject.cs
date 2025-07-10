using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.GUI
{
	public class DropEditBusinessObject : NonPersistentBusinessObject, IObsoleteValidation
	{
		public DropEditBusinessObject(CodeDescriptionPairList list)
		{
			fList = list;
		}

		[MaxLength("List.MaxCodeLength")]
		public virtual ZString Value
		{
			get { return fValue; }
			set
			{
				value = value.Substring(0, ValueInfo.MaxLength);
				CheckMaximumLength(ValueInfo, value);
				SetNonPersistentPropertyValue<ZString>(ValueInfo, ref fValue, value);
			}
		}
		ZString fValue;

		public ZPropertyInfo ValueInfo
		{
			get { return GetZPropertyInfo(nameof(Value)); }
		}

		public CodeDescriptionPairList List
		{
			get { return fList; }
		}
		readonly CodeDescriptionPairList fList;
	}
}
