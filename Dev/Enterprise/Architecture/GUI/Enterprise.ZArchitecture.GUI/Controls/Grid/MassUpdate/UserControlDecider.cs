using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	public static class UserControlDecider
	{
		public static Control CreateControl(FieldType type, ModuleIdentifier moduleID, string bindTo, string bindToList, string mappingName = "")
		{
			Control result = null;

			switch (type)
			{
				case FieldType.Boolean:
					result = CreateCheckBox(bindTo);
					break;
				case FieldType.Date:
					result = CreateDateEdit(ZDateTimePickerFormat.Short, bindTo);
					break;
				case FieldType.DateTime:
					result = CreateDateEdit(ZDateTimePickerFormat.Long, bindTo);
					break;
				case FieldType.DateTimeOffset:
					result = CreateDateTimeOffsetEdit(bindTo);
					break;
				case FieldType.Time:
					result = CreateTimeEdit(bindTo);
					break;
				case FieldType.Geography:
					result = CreateGeographyEdit(bindTo);
					break;
				case FieldType.Byte:
					result = CreateCalcEdit(typeof(ZByte), 0, bindTo);
					break;
				case FieldType.Decimal:
					result = CreateCalcEdit(typeof(ZDecimal), 6, bindTo);
					break;
				case FieldType.Integer:
					result = CreateCalcEdit(typeof(ZInt), 0, bindTo);
					break;
				case FieldType.Guid:
				case FieldType.OrganisationGuid:
					result = CreateGuidFindBox(bindTo, bindToList, moduleID);
					break;
				case FieldType.GuidDropEdit:
					result = CreateGuidDropEdit(bindTo, bindToList);
					break;
				case FieldType.Text:
					result = CreateTextBox(bindTo);
					break;
				case FieldType.TextCodeFindBox:
					result = CreateCodeFindBox(bindTo, bindToList, moduleID);
					break;
				case FieldType.TextDropEdit:
					result = CreateDropEdit(bindTo, bindToList, moduleID, mappingName);
					break;
				case FieldType.TextMultiLine:
					result = CreateMultiLineTextBox(bindTo);
					break;
				default:
					throw new ArgumentException(type.ToString(), nameof(type));
			}
			return result;
		}

		static Control CreateCheckBox(string bindTo)
		{
			var result = new ZCheckBox();
			result.BindTo = bindTo;
			return result;
		}

		static ZCalcEdit CreateCalcEdit(Type bindToType, int decimals, string bindTo)
		{
			ZCalcEdit result = new ZCalcEdit.Bare();
			result.BindTo = bindTo;
			result.Core.SetBindToType(bindToType);
			result.Decimals = decimals;
			return result;
		}

		static ZDateEdit CreateDateEdit(ZDateTimePickerFormat dateTimeFormat, string bindTo)
		{
			ZDateEdit result = new ZDateEdit.Bare();
			result.DateTimeFormat = dateTimeFormat;
			result.BindTo = bindTo;
			return result;
		}

		static ZTimeEdit CreateTimeEdit(string bindTo)
		{
			var result = new ZTimeEdit();
			result.BindTo = bindTo;
			return result;
		}

		static ZDateTimeOffsetEdit CreateDateTimeOffsetEdit(string bindTo)
		{
			ZDateTimeOffsetEdit result = new ZDateTimeOffsetEdit.Bare();
			result.DateTimeFormat = ZDateTimePickerFormat.Long;
			result.BindTo = bindTo;
			return result;
		}

		static ZGeographyEdit CreateGeographyEdit(string bindTo)
		{
			ZGeographyEdit result = new ZGeographyEdit.Bare();
			result.BindTo = bindTo;
			return result;
		}

		static ZCodeFindBox CreateCodeFindBox(string bindTo, string bindToList, ModuleIdentifier moduleID)
		{
			ZCodeFindBox result = new ZCodeFindBox.Bare();
			result.BindTo = bindTo;
			result.BindToList = bindToList;
			result.ModuleID = moduleID;
			return result;
		}

		static ZGuidFindBox CreateGuidFindBox(string bindTo, string bindToList, ModuleIdentifier moduleID)
		{
			ZGuidFindBox result = new ZGuidFindBox.Bare();
			result.BindTo = bindTo;
			result.BindToList = bindToList;
			result.ModuleID = moduleID;
			return result;
		}

		static ZGuidDropEdit CreateGuidDropEdit(string bindTo, string bindToList)
		{
			var result = new ZGuidDropEdit();
			result.BindTo = bindTo;
			result.BindToList = bindToList;
			result.CharacterCasing = CharacterCasing.Normal;
			return result;
		}

		static ZTextBox CreateTextBox(string bindTo)
		{
			ZTextBox result = new ZTextBox.Bare();
			result.BindTo = bindTo;
			result.CharacterCasing = CharacterCasing.Normal;
			return result;
		}

		static ZDropEdit CreateDropEdit(string bindTo, string bindToList, ModuleIdentifier moduleID, string mappingName)
		{
			ZDropEdit result = new ZDropEdit.Bare();
			result.BindTo = bindTo;
			result.BindToList = bindToList;
			result.CharacterCasing = CharacterCasing.Normal;
			result.CodeBox.ModuleID = moduleID;
			result.CodeBox.MappingName = mappingName;
			return result;
		}

		static ZTextBox CreateMultiLineTextBox(string bindTo)
		{
			ZTextBox textBox = new ZTextBox.Bare();
			textBox.BindTo = bindTo;
			textBox.CharacterCasing = CharacterCasing.Normal;
			MultiLineTextBoxGridHelper.SetupTextBoxAsMultiline(textBox);
			return textBox;
		}
	}
}
