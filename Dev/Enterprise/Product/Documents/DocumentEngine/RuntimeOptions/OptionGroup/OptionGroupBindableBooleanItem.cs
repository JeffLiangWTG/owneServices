using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class OptionGroupBindableBooleanItem : NonPersistentBusinessObject, IBindableBooleanItem, IObsoleteValidation
	{
		public OptionGroupBindableBooleanItem(ZBoolDescriptionPair pair)
		{
			this.pair = pair;
		}

		readonly ZBoolDescriptionPair pair;

		#region IBindableBooleanItem Members

		public ZBool BoolValue
		{
			get
			{
				return pair.Value;
			}
			set
			{
				pair.Value = value;

				BoolValueInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo BoolValueInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(BoolValue));
			}
		}

		public ZString Text
		{
			get
			{
				return pair.Description;
			}
		}

		#endregion

		public ZGuid Identifier
		{
			get
			{
				return pair.PK;
			}
		}
	}
}
