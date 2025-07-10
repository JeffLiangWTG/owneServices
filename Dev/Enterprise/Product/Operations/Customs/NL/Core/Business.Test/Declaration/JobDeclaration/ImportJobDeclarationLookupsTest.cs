using CargoWise.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(ImportJobDeclarationLookups))]
class ImportJobDeclarationLookupsTest : JobDeclarationLookupsAbstractTest<ImportJobDeclarationLookups>
{
	protected override string MessageType => MessageTypeList.Codes.Import;

	protected override ImportJobDeclarationLookups GetLookups() => new ImportJobDeclarationLookups(jobDeclaration);

	public void TestIntracomReceiverLookupType()
	{
		AssertType<OrganisationsFindBoxCollection>(lookups.IntracomReceiverList);
	}

	public void TestCustomsOfficesLookupValues()
	{
		jobDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		AssertContainsExactElementsInAnyOrder(new ICodeDescription[]
		{
			new CodeDescriptionPair(CustomsOfficesList.Codes.DouaneSchipholAirport, CustomsOfficesList.Descriptions.DouaneSchipholAirport),
			new CodeDescriptionPair(CustomsOfficesList.Codes.EindhovenKoeriersNormalProcedure, CustomsOfficesList.Descriptions.EindhovenKoeriersNormalProcedure),
			new CodeDescriptionPair(CustomsOfficesList.Codes.EindhovenKoeriersSimplProcedure, CustomsOfficesList.Descriptions.EindhovenKoeriersSimplProcedure),
			new CodeDescriptionPair(CustomsOfficesList.Codes.RotterdamHavenKantoorMaasvlakte, CustomsOfficesList.Descriptions.RotterdamHavenKantoorMaasvlakte),
		}, jobDeclaration.Lookups.CustomsOfficesList);
	}

	public void TestCustomsOfficesLookupValues_Interface()
	{
		jobDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
		AssertContainsExactElementsInAnyOrder(new ICodeDescription[]
		{
			new CodeDescriptionPair(CustomsOfficesImportInterfaceList.Codes.Amsterdam, CustomsOfficesImportInterfaceList.Descriptions.Amsterdam),
			new CodeDescriptionPair(CustomsOfficesImportInterfaceList.Codes.DeLutte, CustomsOfficesImportInterfaceList.Descriptions.DeLutte),
			new CodeDescriptionPair(CustomsOfficesImportInterfaceList.Codes.Duiven, CustomsOfficesImportInterfaceList.Descriptions.Duiven),
			new CodeDescriptionPair(CustomsOfficesImportInterfaceList.Codes.Eemshaven, CustomsOfficesImportInterfaceList.Descriptions.Eemshaven),
			new CodeDescriptionPair(CustomsOfficesImportInterfaceList.Codes.EindhovenAirport, CustomsOfficesImportInterfaceList.Descriptions.EindhovenAirport),
			new CodeDescriptionPair(CustomsOfficesImportInterfaceList.Codes.EindhovenKntrVenlo, CustomsOfficesImportInterfaceList.Descriptions.EindhovenKntrVenlo),
			new CodeDescriptionPair(CustomsOfficesImportInterfaceList.Codes.Groningen, CustomsOfficesImportInterfaceList.Descriptions.Groningen),
			new CodeDescriptionPair(CustomsOfficesImportInterfaceList.Codes.GroningenAirport, CustomsOfficesImportInterfaceList.Descriptions.GroningenAirport),
			new CodeDescriptionPair(CustomsOfficesImportInterfaceList.Codes.JfcHqBrunssum, CustomsOfficesImportInterfaceList.Descriptions.JfcHqBrunssum),
			new CodeDescriptionPair(CustomsOfficesImportInterfaceList.Codes.KntrGroningenTir, CustomsOfficesImportInterfaceList.Descriptions.KntrGroningenTir),
			new CodeDescriptionPair(CustomsOfficesImportInterfaceList.Codes.KoeriersNormalProc, CustomsOfficesImportInterfaceList.Descriptions.KoeriersNormalProc),
			new CodeDescriptionPair(CustomsOfficesImportInterfaceList.Codes.KoeriersSimplProc, CustomsOfficesImportInterfaceList.Descriptions.KoeriersSimplProc),
			new CodeDescriptionPair(CustomsOfficesImportInterfaceList.Codes.MaastrichtAirport, CustomsOfficesImportInterfaceList.Descriptions.MaastrichtAirport),
			new CodeDescriptionPair(CustomsOfficesImportInterfaceList.Codes.Moerdijk, CustomsOfficesImportInterfaceList.Descriptions.Moerdijk),
			new CodeDescriptionPair(CustomsOfficesImportInterfaceList.Codes.RDamTheHagueAirp, CustomsOfficesImportInterfaceList.Descriptions.RDamTheHagueAirp),
			new CodeDescriptionPair(CustomsOfficesImportInterfaceList.Codes.RotterdamMaasvlakte, CustomsOfficesImportInterfaceList.Descriptions.RotterdamMaasvlakte),
			new CodeDescriptionPair(CustomsOfficesImportInterfaceList.Codes.RtmReewegNormalPr, CustomsOfficesImportInterfaceList.Descriptions.RtmReewegNormalPr),
			new CodeDescriptionPair(CustomsOfficesImportInterfaceList.Codes.RtmReewegSimplif, CustomsOfficesImportInterfaceList.Descriptions.RtmReewegSimplif),
			new CodeDescriptionPair(CustomsOfficesImportInterfaceList.Codes.SchipholAirport, CustomsOfficesImportInterfaceList.Descriptions.SchipholAirport),
			new CodeDescriptionPair(CustomsOfficesImportInterfaceList.Codes.Vlissingen, CustomsOfficesImportInterfaceList.Descriptions.Vlissingen),
		}, jobDeclaration.Lookups.CustomsOfficesList);
	}

	public void TestCustomsOfficesLookupType()
	{
		jobDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		AssertType<CustomsOfficesList>(lookups.CustomsOfficesList);
	}

	public void TestCustomsOfficesLookupType_Interface()
	{
		jobDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
		AssertType<CustomsOfficesImportInterfaceList>(lookups.CustomsOfficesList);
	}
}
