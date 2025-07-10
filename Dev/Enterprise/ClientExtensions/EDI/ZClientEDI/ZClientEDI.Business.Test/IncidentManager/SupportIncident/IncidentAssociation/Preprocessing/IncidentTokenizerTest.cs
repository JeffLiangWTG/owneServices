using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation.Test
{
	public partial class IncidentTokenizerTest : TestCaseWithFactory
	{
		public void TestProtectTokens()
		{
			var strings = new List<string>
			{
				$"< !-- tag space -- > this text is not a  xml tag <!--TagNoSpace -->",
				$"<a> this is not a tag <a>",
				$"<a> this is </a>",
				$"<><><><<<<><><>> <more than one word/>",
				$"<tag value=str> str </tag>",
				$"<nest> 1 <tags> 2 <tags> 3 </nest>",
				$"http:www.google.com/hi",
				$"ftp:www.uwa.edu.au.com/page",
				$"www.nohttps.com",
				$"https:nowww.com",
				$"https://www.slash.co.za",
				$"https://slashNoWWW.co.za",
				$"			www.a.com www.b.com www.c.com",
				$"www.noDotCom",
				$"10000000-1000-1000-1000-100000000000",
				$"(10000000-1000-1000-1000-100000000000)",
				"{10000000-1000-1000-1000-100000000000}",
				$"1bcdef0a-10a0-10b0-100c-10abcdef0000",
				$"10000000-100010001000100000000000",
				$"(10000000100010001000100000000000)",
				"{1000000010001000-1000100000000000}",
				$"some text 1bcdef0a10a010b0100c-10abcdef0000",
				$"1bcdefga10a010b0100c-10abcdef0000",
				$"1bcdef0a-10a-010b0100c-10abcdef0000",
				$"http://*.cargowise.com",
				$"http://cargowise.*",
				$"http://*:*@cargowise.com",
			};
			var expectedStrings = new List<string>
			{
				" protectedtokenn1  this text is not a  xml tag  protectedtokenn2 ",
				$"<a> this is not a tag <a>",
				$" protectedtokenn1 ",
				$"<><><><<<<><><>>  protectedtokenn1 ",
				$" protectedtokenn1 ",
				$" protectedtokenn1 ",
				$" protectedtokenn1 ",
				$" protectedtokenn1 ",
				$" protectedtokenn1 ",
				$" protectedtokenn1 ",
				$" protectedtokenn1 ",
				$" protectedtokenn1 ",
				$"			 protectedtokenn1   protectedtokenn2   protectedtokenn3 ",
				$"www.noDotCom",
				$" protectedtokenn1 ",
				$" protectedtokenn1 ",
				$" protectedtokenn1 ",
				$" protectedtokenn1 ",
				$" protectedtokenn1 ",
				$" protectedtokenn1 ",
				$" protectedtokenn1 ",
				$"some text  protectedtokenn1 ",
				$"1bcdefga10a010b0100c-10abcdef0000",
				$"1bcdef0a-10a-010b0100c-10abcdef0000",
				$" protectedtokenn1 ",
				$"http://cargowise.*",
				$" protectedtokenn1 ",
			};
			// Arrange

			for (var index = 0; index < strings.Count; index++)
			{
				// Act
				var result = new IncidentTokenizer(new Dictionary<string, string>(), null).ProtectTokens(strings[index], new Dictionary<string, string>());
				// Assert
				AssertEquals($"Current test case {strings[index]}.", expectedStrings[index], result);
			}
		}

		public void TestStripTrainingLinks()
		{
			// Arrange
			var strings = new List<string>
			{
				$"https://myaccount-portal.cargowise.com/my-account/documents/userguides/workbooks/1cor123.pdf",
				$"http://www.myaccount-portal.cargowise.com/my-account/documents/userguides/workbooks/cor123.pdf",
				$"(http://www.myaccount-portal.cargowise.com/my-account/documents/userguides/workbooks/1cor123)",
				$"foo https://myaccount-portal.cargowise.com/my-account/documents/userguides/workbooks/cor123.pdf bar",
				$"foohttps://myaccount-portal.cargowise.com/my-account/documents/userguides/workbooks/1cor112.pdfbar",
				$"(1cor123.pdf)",
				$"( 1cor123.pdf )",
				$" all your base are belong to us "
			};
			var expectedStrings = new List<string>
			{
				"1cor123",
				"cor123",
				"(1cor123)",
				"foo cor123 bar",
				"foo1cor112bar",
				"(1cor123)",
				"( 1cor123 )",
				$" all your base are belong to us "
			};

			for (var index = 0; index < strings.Count; index++)
			{
				// Act
				var result = new IncidentTokenizer(new Dictionary<string, string>(), null).StripTrainingLinks(strings[index]);
				// Assert
				AssertEquals($"Current test case {strings[index]}.", expectedStrings[index], result);
			}
		}

		public void TestStripUpdateNotes()
		{
			// Arrange
			var strings = new List<string>
			{
				$"https://myaccount-portal.cargowise.com/my-account/documents/updatenotes/unittestingupdatenote20190101c.pdf",
				$"http://myaccount-portal.cargowise.com/my-account/documents/updatenotes/cargowiseoneupdatenote20160505.pdf",
				$"https://www.cargowise.com/my-account/documents/updatenotes/unittestingupdatenote20190101p.pdf",
				$"(https://myaccount-portal.cargowise.com/my-account/documents/updatenotes/unittestingupdatenote20190101c)",
				$"(unittestingupdatenote20190101c.pdf)",
				$"(https://myaccount-portal.cargowise.com/my-account/documents/updatenotes/unittestingupdatenote20191101q)",
				$"(https://www.cargowise.com/my-account/documents/updatenotes/unittsdaingupdatenote20190101c.pdf)",
				$"(https://myaccount-portal.cargowise.com/my-account/documents/updatenotes/thisnoteistoolongforthistocountupdatenote20190101c)",
				$"(https://www.myaccount-portal.cargowise.com/documents/updatenotes/unittestingupdatenote20190101c.pdf)",
				$" thisnoteistoolongforthistocountupdatenote20190101n",
				$"unittsdaingupdatenote20190101c.pdf",
				$"updatenote20190101a",
			};
			var expectedStrings = new List<string>
			{
				" unittestingupdatenotetokenn ",
				" cargowiseoneupdatenotetokenn ",
				" unittestingupdatenotetokenn ",
				"( unittestingupdatenotetokenn )",
				"(unittestingupdatenote20190101c.pdf)",
				"( unittestingupdatenotetokenn )",
				"( unittsdaingupdatenotetokenn )",
				"(https://myaccount-portal.cargowise.com/my-account/documents/updatenotes/thisnoteistoolongforthistocountupdatenote20190101c)",
				"( unittestingupdatenotetokenn )",
				" thisnoteistoolongforthistocountupdatenote20190101n",
				"unittsdaingupdatenote20190101c.pdf",
				"updatenote20190101a",
			};

			for (var index = 0; index < strings.Count; index++)
			{
				// Act
				var result = new IncidentTokenizer(new Dictionary<string, string>(), null).StripUpdateNotes(strings[index]);
				// Assert
				AssertEquals($"Current test case {strings[index]}.", expectedStrings[index], result);
			}
		}

		public void TestTransformTrainingTokens()
		{
			// Arrange
			var strings = new List<string>
			{
				$" 1cor123\t",
				$"\ncor123\r",
			};
			var expectedStrings = new List<string>
			{
				" cortrainingtokenn ",
				" cortrainingtokenn ",
			};

			for (var index = 0; index < strings.Count; index++)
			{
				// Act
				var result = new IncidentTokenizer(new Dictionary<string, string>(), null).TransformTrainingTokens(strings[index]);
				// Assert
				AssertEquals($"Current test case {strings[index]}.", expectedStrings[index], result);
			}
		}

		public void TestTransformCountries()
		{
			// Arrange
			var strings = new List<string>
			{
				" australia ",
				" straya ",
				" australia is a country ",
				" united arab emirates is different from the united states of america ",
			};
			var expectedStrings = new List<string>
			{
				" countrytokenn ",
				" straya ",
				" countrytokenn is a country ",
				" countrytokenn is different from the countrytokenn ",
			};

			for (var index = 0; index < strings.Count; index++)
			{
				// Act
				var result = new IncidentTokenizer(new Dictionary<string, string>(), null).TransformCountries(strings[index]);
				// Assert
				AssertEquals($"Current test case {strings[index]}.", expectedStrings[index], result);
			}
		}

		public void TestRemoveHyphens()
		{
			// Arrange
			var strings = new List<string>
			{
				$"wise-tech",
				$"This is wise-tech-global foo-bar",
				$"foo-bar ",
				$" ThisIsASingleLongWord-ASecondLongWord",
				$" A-B-C-D-E-213-W"
			};
			var expectedStrings = new List<string>
			{
				"wisetech",
				$"This is wisetechglobal foobar",
				"foobar ",
				$" ThisIsASingleLongWordASecondLongWord",
				$" A-B-C-D-E-213-W"
			};

			for (var index = 0; index < strings.Count; index++)
			{
				// Act
				var result = new IncidentTokenizer(new Dictionary<string, string>(), null).RemoveHyphens(strings[index]);
				// Assert
				AssertEquals($"Current test case {strings[index]}.", expectedStrings[index], result);
			}
		}
	}
}