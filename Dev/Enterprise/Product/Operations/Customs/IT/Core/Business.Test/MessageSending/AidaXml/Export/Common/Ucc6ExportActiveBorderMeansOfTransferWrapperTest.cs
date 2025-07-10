using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export.Testing;

sealed class Ucc6ExportActiveBorderMeansOfTransferWrapperTest : TestCaseWithFactory
{
	public void TestGetNewOrNull_Arguments()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		AssertExceptionThrown<ArgumentNullException>(() => Ucc6ExportActiveBorderMeansOfTransferWrapper.GetNewOrNull(declaration: null, entryInstruction: null));
		AssertExceptionThrown<ArgumentNullException>(() => Ucc6ExportActiveBorderMeansOfTransferWrapper.GetNewOrNull(declaration, entryInstruction: null));
		AssertExceptionThrown<ArgumentNullException>(() => Ucc6ExportActiveBorderMeansOfTransferWrapper.GetNewOrNull(declaration: null, entryInstruction: entryInstruction));
		AssertNoExceptionThrown(() => Ucc6ExportActiveBorderMeansOfTransferWrapper.GetNewOrNull(declaration, entryInstruction));
	}
}
