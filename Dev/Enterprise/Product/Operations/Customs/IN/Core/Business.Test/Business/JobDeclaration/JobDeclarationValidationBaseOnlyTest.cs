using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(JobDeclarationValidation))]
sealed class JobDeclarationValidationBaseOnlyTest : JobDeclarationValidationAbstractTest
{
	public void TestCheckJE_ContainerMode_Mandatory()
	{
		const string message = "You have not entered a Container Mode / Nature Of Cargo.";
		Declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
		Declaration.JE_ContainerMode = ZString.Empty;
		CombineAssertions(() =>
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Declaration.JE_ContainerModeInfo, message, "JE_TransportMode is Sea, JE_ContainerMode is required");

			Declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			Declaration.JE_ContainerMode = ZString.Empty;
			AssertNoMessageError("JE_TransportMode is not Sea, JE_ContainerMode is not required", Declaration.JE_ContainerModeInfo, message);
		});
	}

	public void TestJE_CustomsOffice()
	{
		var declaration = Factory.New<JobDeclaration>();
		RefDataSetupTestHelper.SetupCustomsOfficeData(Factory);
		var customsOfficeList = (ZZRefCusCodeListCombinedCollection)declaration.Lookups.CustomsOfficeList;
		customsOfficeList.Load();
		AssertGreaterThan("count", declaration.Lookups.CustomsOfficeList.Count, 0);
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.JE_CustomsOfficeInfo, new ZString[] { "X", "XX" }, declaration.Lookups.CustomsOfficeList.OfType<ICodeDescription>().Select(x => new ZString(x.Code)).ToArray());
	}

	protected override string MessageType => JobMessageTypeList.Codes.MiscellaneousCustoms;
}
