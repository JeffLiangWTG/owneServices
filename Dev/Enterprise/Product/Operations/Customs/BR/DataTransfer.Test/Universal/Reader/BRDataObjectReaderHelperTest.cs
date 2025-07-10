using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Customs.BR.DataTransfer.Universal.Testing
{
	public class BRDataObjectReaderHelperTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestLoadOrCreateEntryNumForEntryInstruction()
		{
			var helper = new BRDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Brazil);
			var testDeclaration = Factory.New<JobDeclaration>();
			var instruction1 = testDeclaration.CustomsEntryInstructions.AddNew();
			var instruction2 = testDeclaration.CustomsEntryInstructions.AddNew();
			instruction2.UCRNumber = "UCRNumber";
			instruction2.IsUCROverridden = false;
			Factory.SaveForTesting();

			var entryNumberEmpty = helper.LoadOrCreateEntryNumForEntryInstruction(instruction1.PK, CusEntryNumberTypes.Standard.UniqueConsignementReference, instruction1.IsInDatabase);
			AssertEquals(ZString.Empty, entryNumberEmpty.CE_EntryNum);
			AssertEquals(ZBool.True, entryNumberEmpty.CE_EntryIsSystemGenerated);

			var entryNumber = helper.LoadOrCreateEntryNumForEntryInstruction(instruction2.PK, CusEntryNumberTypes.Standard.UniqueConsignementReference, instruction2.IsInDatabase);
			AssertEquals("UCRNumber", entryNumber.CE_EntryNum);
			AssertEquals(ZBool.True, entryNumber.CE_EntryIsSystemGenerated);
			instruction2.IsUCROverridden = true;
			AssertEquals(ZString.Empty, entryNumber.CE_EntryNum);

			var anotherFactory = new UniversalObjectFactory();
			var anotherHelper = new BRDataObjectReaderHelper(anotherFactory, Core.Constants.CountryCodes.Brazil);

			var blnumInAnother = anotherHelper.LoadOrCreateEntryNumForEntryInstruction(instruction2.PK, CusEntryNumberTypes.Standard.UniqueConsignementReference, instruction2.IsInDatabase);
			AssertEquals("The existing note should be load", entryNumber.PK, blnumInAnother.PK);
		}
	}
}
