using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.DialogDefault;

namespace Enterprise.ZArchitecture.GUI
{
	sealed class StmDialogDefaultValidationTest : BusinessObjectValidationTestCase
	{
		public void TestEmptyXmlString()
		{
			var defaults = Factory.NewWithValidTestData<StmDialogDefault>();
			defaults.SDD_SerializedDefaults = "<good>stuff</good>";

			Factory.Save();

			defaults.SDD_SerializedDefaults = string.Empty;
			AssertHasError(defaults.SDD_SerializedDefaultsInfo, "XML is invalid");
		}

		public void TestXmlValidation()
		{
			var defaults = Factory.NewWithValidTestData<StmDialogDefault>();

			Factory.Save(); //Ensures ZPropertyInfo.HasChanges will be true

			defaults.SDD_SerializedDefaults = "<xml><very>valid</very></xml>";
			AssertNoError(defaults.SDD_SerializedDefaultsInfo, "XML is invalid");
			AssertHasWarning(defaults.SDD_SerializedDefaultsInfo, "Modifying the XML may result in unexpected behavior; If the software cannot determine then the desired response from the XML the default will not be used.");

			defaults.SDD_SerializedDefaults = "<<<xml>>>";
			AssertHasError(defaults.SDD_SerializedDefaultsInfo, "XML is invalid");
		}

		const string duplicateDefaultErrorMessage = "There is another default with the same identifier, owner and context. Please change the owner of either default, or delete the other default";

		public void TestDuplicateDefaultErrorForOwner()
		{
			var firstDefault = Factory.NewWithValidTestData<StmDialogDefault>();
			firstDefault.SDD_DialogIdentifier = ZGuid.NewZGuid();
			firstDefault.SDD_Level = DialogDefaultLevel.Codes.User;
			firstDefault.SDD_Owner = ZGuid.NewZGuid();

			Factory.Save();

			var secondDefault = Factory.NewWithValidTestData<StmDialogDefault>();
			secondDefault.SDD_DialogIdentifier = firstDefault.SDD_DialogIdentifier;
			secondDefault.SDD_Level = DialogDefaultLevel.Codes.User;

			AssertNoError(secondDefault.SDD_OwnerInfo, duplicateDefaultErrorMessage);
			secondDefault.SDD_Owner = firstDefault.SDD_Owner;
			AssertHasError(secondDefault.SDD_OwnerInfo, duplicateDefaultErrorMessage);
		}

		public void TestDuplicateDefaultErrorForLevel()
		{
			var firstDefault = Factory.NewWithValidTestData<StmDialogDefault>();
			firstDefault.SDD_DialogIdentifier = ZGuid.NewZGuid();
			firstDefault.SDD_Level = DialogDefaultLevel.Codes.Global;

			Factory.Save();

			var secondDefault = Factory.NewWithValidTestData<StmDialogDefault>();
			secondDefault.SDD_DialogIdentifier = firstDefault.SDD_DialogIdentifier;

			AssertNoError(secondDefault.SDD_LevelInfo, duplicateDefaultErrorMessage);
			secondDefault.SDD_Level = DialogDefaultLevel.Codes.Global;
			AssertHasError(secondDefault.SDD_LevelInfo, duplicateDefaultErrorMessage);
		}
	}
}
