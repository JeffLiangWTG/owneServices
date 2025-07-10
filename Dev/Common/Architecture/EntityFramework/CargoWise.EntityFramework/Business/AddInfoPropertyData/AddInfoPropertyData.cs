using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public class AddInfoPropertyData<TZType> : IAddInfoPropertyData
		where TZType : IZType
	{
		public AddInfoPropertyData(string propertyName)
		{
			PropertyName = propertyName;
		}

		internal AddInfoPropertyData(string propertyName, TZType value)
		{
			PropertyName = propertyName;
			Value = value;
		}

		public string PropertyName { get; }
		public TZType OriginalValue;
		public TZType Value;

		IZType IAddInfoPropertyData.OriginalValue
		{
			get => OriginalValue;
			set => OriginalValue = (TZType)value;
		}

		IZType IAddInfoPropertyData.Value
		{
			get => Value;
			set => Value = (TZType)value;
		}
	}
}
