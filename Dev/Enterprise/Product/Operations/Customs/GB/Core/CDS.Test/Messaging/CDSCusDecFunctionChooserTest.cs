using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Messaging;

namespace Enterprise.Customs.GB.CDS.Testing
{
	class CDSCusDecFunctionChooserTest : TestCaseWithFactory
	{
		public void TestFunctionTypeChooser()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var chooser = new CDSCusDecFunctionChooser(entry, new Customs.Business.CusdecMessageFunction.Deleted());
			AssertEquals("DELETE with no comments --> Delete", CusDecMessageTypeFunction.Delete, chooser.Function);
			AssertEquals("Message type for deleting ediMessage --> Empty for now", "", chooser.MessageTypeForEdiMessage);
			chooser = new CDSCusDecFunctionChooser(entry, new Customs.Business.CusdecMessageFunction.New());
			AssertEquals("NEW with no comments --> Original", CusDecMessageTypeFunction.Original, chooser.Function);
			entry.CH_CustomsMessageRemarks = "I messed up, here is the amended version. I will give a really long waffly reason for the amendment, hopefully on that spans more than five hundred and twelve characters so that the comments wrap over two elements.  I confess that I messed up, here is the amended version. I will give a really long waffly reason for the amendment, hopefully on that spans more than five hundred and twelve characters so that the comments wrap over two elements. Still not long enough?  What about if I say that 512 is 2 to the power of 9. Does that help?";
			chooser = new CDSCusDecFunctionChooser(entry, new Customs.Business.CusdecMessageFunction.New());
			entry.CH_EntryStatus = "ABC";
			AssertEquals("NEW with comments --> Replacement", CusDecMessageTypeFunction.Replacement, chooser.Function);
			chooser = new CDSCusDecFunctionChooser(entry, new Customs.Business.CusdecMessageFunction.Deleted());
			AssertEquals("DELETE with comments --> Delete", CusDecMessageTypeFunction.Delete, chooser.Function);
		}
	}
}
