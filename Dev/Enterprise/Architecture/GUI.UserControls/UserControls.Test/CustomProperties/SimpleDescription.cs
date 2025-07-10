using CargoWise.ComponentModel;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class SimpleDescription : IDescription
	{
		public SimpleDescription(string description)
		{
			this.description = description;
		}

		public int Count
		{
			get { return 1; }
		}

		public string GetDescription(int index, System.Globalization.CultureInfo culture)
		{
			return description;
		}

		public string GetDescription(int index)
		{
			return description;
		}

		readonly string description;
	}
}
