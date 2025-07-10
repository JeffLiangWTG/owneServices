using CargoWise.Common;
using CargoWise.Customs.JP.MessageDefinitions;

namespace Enterprise.Customs.JP.Common;

sealed class UserDefinedMessageField : IEditableMessageField
{
	public UserDefinedMessageField(IEditableMessageField sourceField)
	{
		this.sourceField = Argument.NotNull(sourceField, nameof(sourceField));

		OriginalValue = null;
		PrefixForDisplay = sourceField.PrefixForDisplay;
		IsReadOnly = sourceField.IsReadOnly;
	}

	readonly IEditableMessageField sourceField;

	public int Sequence { get; set; }

	public int RepeatIndex { get; set; }

	public string OriginalValue { get; }

	public string PrefixForDisplay { get; }

	public bool IsReadOnly { get; }

	public string OverrideValue { get; set; }

	PropertyAccessor IMessageField.PropertyAccessor => null;

	FieldDefinition IMessageField.FieldDefinition => sourceField.FieldDefinition;
}
