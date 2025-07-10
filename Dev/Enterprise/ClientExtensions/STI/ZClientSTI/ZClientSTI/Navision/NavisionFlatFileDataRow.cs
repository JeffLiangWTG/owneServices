using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.STI.Navision
{
	public class NavisionFlatFileDataRow : FlatFileDataRow
	{
		public NavisionFlatFileDataRow(int fieldCount) : base(fieldCount)
		{
		}

		public void SetField(FlatFileFieldProperty fieldProperty, ZString value)
		{
			SetField(fieldProperty, value, false);
		}

		public void SetField(FlatFileFieldProperty fieldProperty, ZString value, bool isPrimaryKeyField)
		{
			ZString pattern = string.Format(isPrimaryKeyField ? "[{0}]" : "[{1}]|[^\x20-\x7E]", CharactersToRemoveFromPrimaryKeyFields, CharactersToRemoveFromAllFields);
			value = Regex.Replace(value, pattern, "");
			base.SetField(fieldProperty.Name, value.Left(fieldProperty.Length));
		}

		public void SetField(FlatFileFieldProperty fieldProperty, ZDecimal value)
		{
			base.SetField(fieldProperty.Name, value, 2);
		}

		public void SetField(FlatFileFieldProperty fieldProperty, ZDateTime value)
		{
			base.SetField(fieldProperty.Name, value, Constants.DateTimeFormat);
		}

		public void SetField(FlatFileFieldProperty fieldProperty, ZInt value)
		{
			base.SetField(fieldProperty.Name, value);
		}

		const string CharactersToRemoveFromPrimaryKeyFields = "^a-zA-Z0-9";
		const string CharactersToRemoveFromAllFields = "\"',";
	}
}
