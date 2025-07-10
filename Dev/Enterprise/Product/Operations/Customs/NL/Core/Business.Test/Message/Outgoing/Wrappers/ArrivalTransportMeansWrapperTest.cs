using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.NL.Business.Declaration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class ArrivalTransportMeansWrapperTest : DataProviderTestCase<ArrivalTransportMeansWrapper>
{
	public void TestId()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_TransportIDInland = "12AB34";
		declaration.JE_TransportMeans = "10";
		declaration.JE_TransportModeInland = TransportModes.Road;
		var entry = declaration.CustomsEntryInstructions.AddNew();
		entry.CEI_Style = DeclarationTypeList.Codes.H1;

		var wrapperTest = new ArrivalTransportMeansWrapper(declaration);
		AssertEquals("Entry Instruction among acceptable codes and transport mode among acceptable modes", "12AB34", wrapperTest.Id);

		entry.CEI_Style = DeclarationTypeList.Codes.H2;
		wrapperTest = new ArrivalTransportMeansWrapper(declaration);
		AssertEquals("Entry Instruction not among acceptable codes but transport mode among acceptable modes", string.Empty, wrapperTest.Id);
		entry.CEI_Style = DeclarationTypeList.Codes.H1;

		declaration.JE_TransportModeInland = TransportModes.Mail;
		wrapperTest = new ArrivalTransportMeansWrapper(declaration);
		AssertEquals("Entry Instruction among acceptable codes but transport mode not among acceptable modes", string.Empty, wrapperTest.Id);
		declaration.JE_TransportModeInland = TransportModes.Road;

		declaration.JE_TransportModeInland = TransportModes.Mail;
		entry.CEI_Style = DeclarationTypeList.Codes.H2;
		wrapperTest = new ArrivalTransportMeansWrapper(declaration);
		AssertEquals("Entry Instruction not among acceptable codes and transport mode not among acceptable modes", string.Empty, wrapperTest.Id);
	}

	public void TestIdentificationTypeCode()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_TransportIDInland = "12AB34";
		declaration.JE_TransportMeans = "10";
		declaration.JE_TransportModeInland = TransportModes.Road;
		var entry = declaration.CustomsEntryInstructions.AddNew();
		entry.CEI_Style = DeclarationTypeList.Codes.H1;

		var wrapperTest = new ArrivalTransportMeansWrapper(declaration);
		AssertEquals("Entry Instruction among acceptable codes and transport mode among acceptable modes", "10", wrapperTest.IdentificationTypeCode);

		entry.CEI_Style = DeclarationTypeList.Codes.H2;
		wrapperTest = new ArrivalTransportMeansWrapper(declaration);
		AssertEquals("Entry Instruction not among acceptable codes but transport mode among acceptable modes", string.Empty, wrapperTest.IdentificationTypeCode);
		entry.CEI_Style = DeclarationTypeList.Codes.H1;

		declaration.JE_TransportModeInland = TransportModes.Mail;
		wrapperTest = new ArrivalTransportMeansWrapper(declaration);
		AssertEquals("Entry Instruction among acceptable codes but transport mode not among acceptable modes", string.Empty, wrapperTest.IdentificationTypeCode);
		declaration.JE_TransportModeInland = TransportModes.Road;

		declaration.JE_TransportModeInland = TransportModes.Mail;
		entry.CEI_Style = DeclarationTypeList.Codes.H2;
		wrapperTest = new ArrivalTransportMeansWrapper(declaration);
		AssertEquals("Entry Instruction not among acceptable codes and transport mode not among acceptable modes", string.Empty, wrapperTest.IdentificationTypeCode);
	}

	public void TestModeCode()
	{
		AssertEquals("5", wrapper.ModeCode);
	}

	protected override ArrivalTransportMeansWrapper GetProvider() => wrapper;

	protected override void SetUp()
	{
		base.SetUp();
		var decl = Factory.New<JobDeclaration>();
		decl.JE_TransportIDInland = "12AB34";
		decl.JE_TransportMeans = "10";
		decl.JE_TransportModeInland = TransportModes.Mail;
		wrapper = new ArrivalTransportMeansWrapper(decl);
	}
	ArrivalTransportMeansWrapper wrapper;
}
