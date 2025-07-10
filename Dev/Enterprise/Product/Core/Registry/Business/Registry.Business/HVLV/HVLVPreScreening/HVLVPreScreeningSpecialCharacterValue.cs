using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class HVLVPreScreeningSpecialCharacterValue : RegistryBusinessObjectTemplate, ICanDelete
	{
		internal HVLVPreScreeningField HVLVPreScreeningField;

		#region Schema

		protected abstract class Schema : RegistryBusinessObject.Schema
		{
			public const string characterValue = "CharacterValue";
		}

		#endregion

		public HVLVPreScreeningSpecialCharacterValue(HVLVPreScreeningField screeningField)
		{
			base.SetCustomDefaultValuesCore();

			HVLVPreScreeningField = screeningField;
		}

		public HVLVPreScreeningSpecialCharacterValue()
		{
			base.SetCustomDefaultValuesCore();
		}

		#region Special Characters

		[List("SpecialCharactersList")]
		public ZString CharacterValue
		{
			get
			{
				return characterValue;
			}
			set
			{
				SetNonPersistentPropertyValue(CharacterValueInfo, ref characterValue, value);

				if (!IsValidationSuspended)
				{
					ValidateSpecialCharacter();
				}
			}
		}

		public ZPropertyInfo CharacterValueInfo => GetZPropertyInfo(Schema.characterValue);

		ZString characterValue;

		void ValidateSpecialCharacter()
		{
			CharacterValueInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CharacterValueInfo);
			ListValidation.ErrorIfInvalidCode(CharacterValueInfo, SpecialCharactersList);

			if (!CharacterValueInfo.HasErrors())
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(CharacterValueInfo);
			}
		}

		public CodeDescriptionPairList SpecialCharactersList
		{
			get
			{
				return CurrentFactory.GetCachedValue<CodeDescriptionPairList>("HVLVPreScreening.SpecialCharactersList", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(Res.GetString("41cd1327-c92d-4d3e-851d-aa97612cb76a", "Carriage Return"), "\r\n");
					return result;
				});
			}
		}

		#endregion

		public bool IsMatched(ZString fieldValue)
		{
			return fieldValue.Contains(SpecialCharactersList.GetDescriptionFromCode(CharacterValue), StringComparison.OrdinalIgnoreCase);
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new HVLVPreScreeningSpecialCharacterValue(HVLVPreScreeningField);
		}

		#region XML Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.characterValue, CharacterValue);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			CharacterValue = reader.ReadElementString(Schema.characterValue);
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateSpecialCharacter();
		}

		#endregion
	}
}
