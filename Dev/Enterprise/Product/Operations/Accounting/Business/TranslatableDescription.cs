using CargoWise.ComponentModel;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business
{
	/// <summary>
	/// Provide translatable property name for ListValidation in NonPersistentBusinessObject properties 
	/// </summary>
	public class TranslatableDescription : IDescription
	{
		public TranslatableDescription(MultilingualString description)
		{
			this.description = description;
		}

		readonly MultilingualString description;

		#region IDescription Members

		public int Count
		{
			get { return 0; }
		}

		public string GetDescription(int index, System.Globalization.CultureInfo culture)
		{
			return description.ToString(Culture.GetLanguageForCulture(culture));
		}

		public string GetDescription(int index)
		{
			return description;
		}

		#endregion
	}
}
