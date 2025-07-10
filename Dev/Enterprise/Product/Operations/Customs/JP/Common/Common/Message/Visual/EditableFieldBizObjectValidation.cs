using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.JP.Common
{
	public sealed class EditableFieldBizObjectValidation : ZValidation
	{
		public EditableFieldBizObjectValidation(EditableFieldBizObject parent)
			: base(parent)
		{
			Parent = parent;
		}

		EditableFieldBizObject Parent { get; }

		public void ValidateOverrideValue()
		{
			((IValidationInternals)this).Validate(Parent.OverrideValueInfo, CheckOverrideValue);
		}

		void CheckOverrideValue()
		{
			var parent = Parent;
			var overrideValue = parent.OverrideValue;
			var info = parent.OverrideValueInfo;

			if (!overrideValue.IsEmpty)
			{
				var definition = parent.SourceField.FieldDefinition;
				var definitionType = definition.Type;
				switch (definitionType)
				{
					case "n":
						if (!ZDecimal.CanParse(overrideValue))
						{
							info.AddError(Res.GetString("48ab2a0b-294e-4698-8153-7556ddb93918", "The override value is not valid."));
						}
						break;
					default:
						Func<char, bool> charChecker = definitionType switch
						{
							"an" => (char a) => a.IsAn(parent.ProcedureCode == JPProcedureCodeList.Codes.IVA),
							"sn" => (char a) => a.IsSn(),
							"j" => (char a) => a.IsJ(),
							_ => (char a) => false,
						};
						var shouldUnderlineBeIgnored = ShouldUnderlineBeIgnored();
						foreach (var a in overrideValue)
						{
							if (!charChecker.Invoke(a) && !(a == '_' && shouldUnderlineBeIgnored))
							{
								info.AddMessageError(Res.GetString("4F9004E8-F03E-4981-A7B7-73593B5C4A62", "The character '{0}' is not supported by NACCS.", a));
							}
						}
						break;
				}
			}
		}

		bool ShouldUnderlineBeIgnored()
		{
			var previousItem = Parent.ParentCollections?
					.FirstOrDefault()?
					.ElementInFrontOf(Parent) as EditableFieldBizObject;
			if (previousItem != null && previousItem.No == 62 && ApprovalCertificateInfoCodes.GENS.Equals(previousItem.OverrideValue))
			{
				return true;
			}

			return false;
		}

		public override void ValidateAll()
		{
			using ((Parent as ISingleElementListInternal).SuspendListChanged())
			{
				ValidateOverrideValue();
			}
		}

		public override Type AutoValidationType => typeof(EditableFieldBizObjectValidation);
	}
}
