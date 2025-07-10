using System.Collections.Generic;
using Enterprise.DataTransfer.Native.DB.Validators;

namespace Enterprise.DataTransfer.Native.Common
{
	/// <summary>
	/// Property for an entity, each property corresponds to a column in database
	/// </summary>
	public class Property
	{
		public Property(IPropertyDef definition)
		{
			Definition = definition;
		}

		public IPropertyDef Definition { get; private set; }

		public object Value { get; set; }

		Dictionary<string, object> Attributes => attributes ?? (attributes = new Dictionary<string, object>());
		Dictionary<string, object> attributes;

		public string GetAttributeValue(string key)
		{
			if (attributes == null || !Attributes.TryGetValue(key, out var result))
			{
				return string.Empty;
			}
			else
			{
				return (string)result;
			}
		}

		public void AddAttributeValue(string key, object value)
		{
			if (value != null)
			{
				Attributes.Add(key, value);
			}
		}

		public string Name => Definition.PropertyName;

		public ValidationResult Validate()
			=> Definition.ColumnDef.Validator != null ? Definition.ColumnDef.Validator.Validate(Value) : new ValidationResult(true);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		public override string ToString() => string.Format("Property [{0}, {1}]", Name, Value);
	}
}
