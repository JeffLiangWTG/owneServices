using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EvvRequestSendingObjectParentValidation))]
sealed class EvvRequestSendingObjectParentValidationTest : TestCaseWithFactory
{
	public void TestCheckMrnVersion()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var sendingObjectParent = new EvvRequestSendingObjectParent(entryHeader);

		ValidationTestHelper.AssertValueCannotBeNegativeMessageError(sendingObjectParent.MrnVersionInfo);
	}
}
