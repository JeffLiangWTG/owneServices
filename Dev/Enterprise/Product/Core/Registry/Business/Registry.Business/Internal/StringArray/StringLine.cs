using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Internal
{
	public class StringLine : NonPersistentBusinessObject, IObsoleteValidation
	{
		internal StringLine(StringArrayRegistryDataType dataType)
		{
			this.dataType = dataType;
		}

		public StringArrayRegistryDataType DataType
		{
			get { return dataType; }
		}
		readonly StringArrayRegistryDataType dataType;

		#region Value

		public ZString Value
		{
			get { return fValue; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(ValueInfo, ref fValue, value);
				if (!IsValidationSuspended)
				{
					ValidateValue();
				}
			}
		}
		ZString fValue;

		public ZPropertyInfo ValueInfo
		{
			get { return GetZPropertyInfo(nameof(Value)); }
		}

		public int Value_MaxLength
		{
			get { return dataType == null ? -1 : dataType.MaximumLength; }
		}

		public void ValidateValue()
		{
			ValueInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ValueInfo);
		}

		#endregion
	}
}
