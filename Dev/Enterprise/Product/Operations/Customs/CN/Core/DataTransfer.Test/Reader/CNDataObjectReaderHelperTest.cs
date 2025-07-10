using CargoWise.Types;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.Common;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.Customs.CN.DataTransfer.Universal.Testing
{
	class CNDataObjectReaderHelperTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestLoadOrCreateEntryNumForEntryInstruction()
		{
			var testDeclaration = Factory.New<JobDeclaration>();
			var instruction1 = testDeclaration.CustomsEntryInstructions.AddNew();
			var instruction2 = testDeclaration.CustomsEntryInstructions.AddNew();
			instruction2.BillOfLading = "BONUM2";
			instruction2.BillOfLadingDate = new ZDateTime(2019, 1, 1);
			Factory.SaveForTesting();

			var helper = new CNDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.China);

			var blnum1 = helper.LoadOrCreateEntryNumForEntryInstruction(instruction1.PK, CusEntryNumberTypes.China.BillOfLading, instruction1.IsInDatabase);
			AssertEquals(ZString.Empty, blnum1.CE_EntryNum);
			AssertEquals(ZDateTime.Empty, blnum1.CE_IssueDate);

			var blnum2 = helper.LoadOrCreateEntryNumForEntryInstruction(instruction2.PK, CusEntryNumberTypes.China.BillOfLading, instruction2.IsInDatabase);
			AssertEquals("BONUM2", blnum2.CE_EntryNum);
			AssertEquals(new ZDateTime(2019, 1, 1), blnum2.CE_IssueDate);

			var newInstruction = testDeclaration.CustomsEntryInstructions.AddNew();
			var blnum3 = helper.LoadOrCreateEntryNumForEntryInstruction(newInstruction.PK, CusEntryNumberTypes.China.BillOfLading, newInstruction.IsInDatabase);
			AssertEquals(ZString.Empty, blnum3.CE_EntryNum);
			AssertEquals(ZDateTime.Empty, blnum3.CE_IssueDate);

			var anotherFactory = new UniversalObjectFactory();
			var anotherHelper = new CNDataObjectReaderHelper(anotherFactory, Core.Constants.CountryCodes.China);

			var blnumInAnother = anotherHelper.LoadOrCreateEntryNumForEntryInstruction(instruction2.PK, CusEntryNumberTypes.China.BillOfLading, instruction2.IsInDatabase);
			AssertEquals("The existing note should be load", blnum2.PK, blnumInAnother.PK);
		}
	}
}
