using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(HVLVPreScreeningSpecialCharacterValue))]
	sealed class HVLVPreScreeningSpecialCharacterValueTest : RegistryBusinessObjectTemplateTestCase<HVLVPreScreeningSpecialCharacterValue>
	{
		public void TestSpecialCharacter_Validation()
		{
			var validationRule = new HVLVPreScreeningRule();
			var validationField = new HVLVPreScreeningField(validationRule);
			validationField.FieldDescription = "Special Characters";
			var characterValue1 = validationField.SpecialCharacters.AddNew();
			characterValue1.CharacterValue = "";
			AssertHasErrors("Special character can not be null", characterValue1.CharacterValueInfo);

			characterValue1.CharacterValue = "XXX";
			AssertHasErrors("Special character can not be invaild", characterValue1.CharacterValueInfo);

			characterValue1.CharacterValue = "Carriage Return";
			AssertNoErrors("Precondition : no errors", characterValue1.CharacterValueInfo);

			var characterValue2 = validationField.SpecialCharacters.AddNew();
			characterValue2.CharacterValue = "Carriage Return";
			characterValue1.RunPreSaveValidation();
			characterValue2.RunPreSaveValidation();

			AssertHasError(characterValue1.CharacterValueInfo, "The Character Value has been duplicated and must be unique.");
			AssertHasError(characterValue2.CharacterValueInfo, "The Character Value has been duplicated and must be unique.");
		}

		#region Implementation

		protected override HVLVPreScreeningSpecialCharacterValue GetBusinessObjectToClone()
		{
			return (HVLVPreScreeningSpecialCharacterValue)GetNewBusinessObject();
		}

		protected override HVLVPreScreeningSpecialCharacterValue GetBusinessObjectToSerialise()
		{
			return (HVLVPreScreeningSpecialCharacterValue)GetNewBusinessObject();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
