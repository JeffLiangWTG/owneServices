using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.JP.Common;

public sealed class EditableFieldBizObjectCollection : NonPersistentBusinessObjectCollection<EditableFieldBizObject>
{
	public EditableFieldBizObjectCollection(BusinessObjectFactory factory)
		: base(factory)
	{
	}

	protected override bool AllowSort => false;

	protected override bool AllowNewCore => false;

	protected override BusinessObject CreateNonPersistentBusinessObject() => new EditableFieldBizObject(ZString.Empty, new EmptyEditableMessageField());

	sealed class EmptyEditableMessageField : IEditableMessageField
	{
		int IEditableMessageField.Sequence => 0;

		int IEditableMessageField.RepeatIndex => 0;

		string IEditableMessageField.OriginalValue => string.Empty;

		string IEditableMessageField.OverrideValue { get; set; }

		string IEditableMessageField.PrefixForDisplay => string.Empty;

		bool IEditableMessageField.IsReadOnly => false;

		FieldDefinition IMessageField.FieldDefinition => new();

		PropertyAccessor IMessageField.PropertyAccessor => null;
	}
}
