using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.JP.Common
{
	public sealed class EditableFieldBizObject : NonPersistentBusinessObject, IObsoleteValidation
	{
		public EditableFieldBizObject(ZString procedureCode, IEditableMessageField sourceField)
		{
			ProcedureCode = procedureCode;
			SourceField = Argument.NotNull(sourceField, nameof(sourceField));
			Length = SourceField.FieldDefinition.Size;

			var prefix = string.IsNullOrWhiteSpace(SourceField.PrefixForDisplay) ? string.Empty : $"{SourceField.PrefixForDisplay} ";
			Name = prefix + SourceField.FieldDefinition.Name;
			JPName = prefix + SourceField.FieldDefinition.JPName;
			Validation.ValidateOverrideValue();
		}

		public ZString ProcedureCode { get; }

		public IEditableMessageField SourceField { get; }

		[ResourceStringData("Enterprise.Customs.JP.Common.EditableFieldBizObject|Name", Caption = "English")]
		public ZString Name { get; }

		public ZPropertyInfo NameInfo => GetZPropertyInfo(nameof(Name));

		[ResourceStringData("Enterprise.Customs.JP.Common.EditableFieldBizObject|JPName", Caption = "Japanese")]
		public ZString JPName { get; }

		public ZPropertyInfo JPNameInfo => GetZPropertyInfo(nameof(JPName));

		[ResourceStringData("Enterprise.Customs.JP.Common.EditableFieldBizObject|ID", Caption = "ID")]
		public ZString ID
		{
			get
			{
				var id = SourceField.FieldDefinition.ID;
				if (id != null && CultureInfo.CurrentCulture.CompareInfo.IsSuffix(id, "_", CompareOptions.None))
				{
					var suffixLength = id.Count(c => c == '_');
					if (suffixLength == 1 || suffixLength == 2)
					{
						var prefix = id.Substring(0, id.IndexOf('_'));
						if (ZInt.TryParse(SourceField.PrefixForDisplay, out var suffix))
						{
							id = (suffixLength == 2) ? $"{prefix}{suffix:D2}" : $"{prefix}{suffix}";
						}
					}
				}

				return id;
			}
		}

		public ZPropertyInfo IDInfo => GetZPropertyInfo(nameof(ID));

		[ResourceStringData("Enterprise.Customs.JP.Common.EditableFieldBizObject|Type", Caption = "Type")]
		public ZString Type => SourceField.FieldDefinition.Type;

		public ZPropertyInfo TypeInfo => GetZPropertyInfo(nameof(Type));

		[ResourceStringData("Enterprise.Customs.JP.Common.EditableFieldBizObject|Instruction", Caption = "Instruction")]
		public ZString Instruction => SourceField.FieldDefinition.Instruction;

		public ZPropertyInfo InstructionInfo => GetZPropertyInfo(nameof(Instruction));

		[ResourceStringData("Enterprise.Customs.JP.Common.EditableFieldBizObject|No", Caption = "No")]
		public ZInt No => SourceField.FieldDefinition.No;

		public ZPropertyInfo NoInfo => GetZPropertyInfo(nameof(No));

		[ResourceStringData("Enterprise.Customs.JP.Common.EditableFieldBizObject|Length", Caption = "Length")]
		public ZInt Length { get; }

		public ZPropertyInfo LengthInfo => GetZPropertyInfo(nameof(Length));

		[ResourceStringData("Enterprise.Customs.JP.Common.EditableFieldBizObject|OriginalValue", Caption = "Value")]
		public ZString OriginalValue => SourceField.OriginalValue;

		public ZPropertyInfo ValueInfo => GetZPropertyInfo(nameof(OriginalValue));

		[ResourceStringData("Enterprise.Customs.JP.Common.EditableFieldBizObject|OverrideValue", Caption = "Override")]
		public ZString OverrideValue
		{
			get => SourceField.OverrideValue;
			set
			{
				if (OverrideValue != value)
				{
					CheckMaximumLength(OverrideValueInfo, value);
					SourceField.OverrideValue = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateOverrideValue();
					}

						OverrideValueInfo.RefreshBinding();
				}
			}
		}

		public int OverrideValue_MaxLength => Length;

		public bool OverrideValue_ReadOnly => SourceField.IsReadOnly;

		public ZPropertyInfo OverrideValueInfo => GetZPropertyInfo(nameof(OverrideValue));

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		public EditableFieldBizObjectValidation Validation => validation ??= new EditableFieldBizObjectValidation(this);
		EditableFieldBizObjectValidation validation;
	}
}
