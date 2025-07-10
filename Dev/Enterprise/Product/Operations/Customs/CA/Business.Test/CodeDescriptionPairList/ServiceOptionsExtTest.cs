using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ServiceOptionsExtTest : TestCase
	{
		public void TestGetShortDescription()
		{
			AssertEquals("CLASSFILE", "G7 Export", ServiceOptions.GetShortDescription(ServiceOptions.Codes.G7EDIExport));
			AssertEquals("CLASSFILE", "Syntax", ServiceOptions.GetShortDescription(ServiceOptions.Codes.GenericSyntax));
			AssertEquals("CLASSFILE", "Cash, Paper", ServiceOptions.GetShortDescription(ServiceOptions.Codes.PaperCash));
			AssertEquals("CLASSFILE", "ETA, Paper", ServiceOptions.GetShortDescription(ServiceOptions.Codes.PaperEnterToArrive));
			AssertEquals("CLASSFILE", "FIRST, Paper", ServiceOptions.GetShortDescription(ServiceOptions.Codes.PaperFIRST));
			AssertEquals("CLASSFILE", "PARS, Paper", ServiceOptions.GetShortDescription(ServiceOptions.Codes.PaperPARS));
			AssertEquals("CLASSFILE", "IID", ServiceOptions.GetShortDescription(ServiceOptions.Codes.IID));
			AssertEquals("CLASSFILE", "RMD, Paper", ServiceOptions.GetShortDescription(ServiceOptions.Codes.PaperRMD));
			AssertEquals("CLASSFILE", "VI, Paper", ServiceOptions.GetShortDescription(ServiceOptions.Codes.PaperValueIncluded));
			AssertEquals("CLASSFILE", "PARS, EDI", ServiceOptions.GetShortDescription(ServiceOptions.Codes.PARS));
			AssertEquals("CLASSFILE", "PARS, OGD, EDI", ServiceOptions.GetShortDescription(ServiceOptions.Codes.PARSOGD));
			AssertEquals("CLASSFILE", "AQ, EDI", ServiceOptions.GetShortDescription(ServiceOptions.Codes.ReplaceRMDwithAQ));
			AssertEquals("CLASSFILE", "RMD, OGD, EDI", ServiceOptions.GetShortDescription(ServiceOptions.Codes.RMDOGD));
			AssertEquals("CLASSFILE", "Special Rel", ServiceOptions.GetShortDescription(ServiceOptions.Codes.SpecialRelease));
			AssertEquals("CLASSFILE", "Supp ACI", ServiceOptions.GetShortDescription(ServiceOptions.Codes.SupplementaryCargoReport));
			AssertEquals("CLASSFILE", "Temp. Rel", ServiceOptions.GetShortDescription(ServiceOptions.Codes.TemporaryRelease));
		}
	}
}
