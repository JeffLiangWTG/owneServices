using System;
using System.Text;
using NUnit.Framework;

namespace CargoWise.Types.Tests
{
	public class ZStringTest : IZTypeTest
	{
		public void TestRemoveNonASCIICharacters()
		{
			var content = new ZString("Hello 你好啊World世界!");
			AssertEquals("Hello World!", content.RemoveNonASCIICharacters());
			content = "Embarque S00003580 (Guía Hija='4742027')";
			AssertEquals("Embarque S00003580 (Gua Hija='4742027')", content.RemoveNonASCIICharacters());
		}

		public void TestRemoveNonPrintableASCIICharacters()
		{
			var content = new ZString("Hello 你好啊World世界!");
			AssertEquals("Hello World!", content.StripNonPrintableASCIICharacters());
			content = "Embarque S00003580 (Guía Hija='4742027')";
			AssertEquals("Embarque S00003580 (Gua Hija='4742027')", content.StripNonPrintableASCIICharacters());
			content = "\u0000 testing123 \u007f";
			AssertEquals(" testing123 ", content.StripNonPrintableASCIICharacters());
		}

		public void TestReplaceNonBreakingSpaceWithNormalSpace()
		{
			var content = new ZString("Consol C00022170 ");
			AssertEquals(160, content[6]);
			AssertEquals(160, content[16]);

			var newContent = content.ReplaceNonBreakingSpaceWithNormalSpace();
			AssertEquals(32, newContent[6]);
			AssertEquals(32, newContent[16]);
		}

		public void TestNullConstructor()
		{
			AssertEquals(ZString.Empty, new ZString(null));
		}

		public void TestToUpperReturnsSameObjectIfNoDifference()
		{
			var a = new ZString("HELLO");
			Assert(ReferenceEquals(a.ToUpper().ToString(), a.ToString()));
			var b = new ZString("Goodbye");
			Assert(!ReferenceEquals(b.ToUpper().ToString(), b.ToString()));
		}

		public void TestFindLineNumbersUponWhichStringAppears()
		{
			ZString edifact = @"UNH+1+CUSDEC:D:04A:UN:109700+<<SYSCAR>>'BGM+IFD::109++9'CST++IMD+2'LOC+14+GBFXT::109'LOC+35+WW'LOC+11+GBFXT'GEI+5+ACC:PI:109'RFF+ABO:8-B00001000'RFF+UCN'TDT+13++1'NAD+CZ+++CHERRY BODY FASHION+UNIT B 4/F WINNER BUILDING+KOWLOON, HONG KONG++NA+HK'NAD+CN+GB543938615000++THE BODY SHOP+THE REGIONAL DISTRIBUTION CENTRE+LITTLEHAMPTON++BN17 6LS+GB'NAD+DT+GB945390992000++DANIEL MAIN ORG+314 MIDSUMMER BLVD+MLITON KEYNES++MK9 2UB+GB'UNS+D'DMS+B00001205'MOA+39:10000.00:GBP'CST++4000000+6403911898+A999++100'FTX+ACB++LIC99'LOC+27+CN'MEA+AAR++KGM:50'MEA+AAS++ZZZ:1'PAC+1++PK'PCI++ASDADS'MOA+38:10000'RFF+ZZZ'IMD+++:::COVERING THE ANKLE BUT NO PART OF THE CALF, WITH INSOLES OF A LENGTH  OF 24 CM OR MORE  OTHER THAN FOOTWEAR WHICH CANNOT BE IDENTIFIED AS MEN S OR WOMEN S FOOTWEAR FOR WOMEN; EXCLUDING THOSE SHOES WHICH HAVE A CIF PRICE PER PAIR OF NOT LESS THAN EURO 9 FO: USE IN SPORTING ACTIVIT'DOC+998:::380+ASDADASDA:::::Z'DOC+916:::N934+:JE'GEI+2+1:VM:109'GEI+2+B:VA:109'PCD+9:0'TAX+1+B00+D++S::::::109'MOA+161'TAX+1+A00+D++F::::::109'MOA+161'TAX+1+A30+D++::::::109'MOA+161'UNS+S'CNT+11:1'UNT+40+1'";
			int[] linesOnWhichCstIsFound = edifact.FindLineNumbersUponWhichStringAppears("CST", "'");
			AssertArrayEqualsByElements(new int[] { 3, 17 }, linesOnWhichCstIsFound);

			ZString danMessage = @"Dan 
Says
Hello
Dan
I
Like
		Dan   (!)
Cos
Dan
Is
Cool
Bye
Dan";
			int[] linesOnWhichDanIsFound = danMessage.FindLineNumbersUponWhichStringAppears("Dan", Environment.NewLine);
			AssertArrayEqualsByElements(new int[] { 1, 4, 7, 9, 13 }, linesOnWhichDanIsFound);
		}

		public void TestFindLineNumbersWhichStartWithString()
		{
			ZString edifact = @"UNH+1+CUSDEC:D:04A:UN:109700+<<SYSCAR>>'BGM+IFD::109++9'CST++IMD+2'LOC+14+GBFXT::109'LOC+35+WW'LOC+11+GBFXT'GEI+5+ACC:PI:109'RFF+ABO:8-B00001000'RFF+UCN'TDT+13++1'NAD+CZ+++CHERRY BODY FASHION+UNIT B 4/F WINNER BUILDING+KOWLOON, HONG KONG++NA+HK'NAD+CN+GB543938615000++THE BODY SHOP+THE REGIONAL DISTRIBUTION CENTRE+LITTLEHAMPTON++BN17 6LS+GB'NAD+DT+GB945390992000++DANIEL MAIN ORG+314 MIDSUMMER BLVD+MLITON KEYNES++MK9 2UB+GB'UNS+D'DMS+B00001205'MOA+39:10000.00:GBP'CST++4000000+6403911898+A999++100'FTX+ACB++LIC99'LOC+27+CN'MEA+AAR++KGM:50'MEA+AAS++ZZZ:1'PAC+1++PK'PCI++ASDADS'MOA+38:10000'RFF+ZZZ'IMD+++:::COVERING THE ANKLE BUT NO PART OF THE CALF, WITH INSOLES OF A LENGTH  OF 24 CM OR MORE  OTHER THAN FOOTWEAR WHICH CANNOT BE IDENTIFIED AS MEN S OR WOMEN S FOOTWEAR FOR WOMEN; EXCLUDING THOSE SHOES WHICH HAVE A CIF PRICE PER PAIR OF NOT LESS THAN EURO 9 FO: USE IN SPORTING ACTIVIT'DOC+998:::380+ASDADASDA:::::Z'DOC+916:::N934+:JE'GEI+2+1:VM:109'GEI+2+B:VA:109'PCD+9:0'TAX+1+B00+D++S::::::109'MOA+161'TAX+1+A00+D++F::::::109'MOA+161'TAX+1+A30+D++::::::109'MOA+161'UNS+S'CNT+11:1'UNT+40+1'";
			int[] linesWhichStartWithCst = edifact.FindLineNumbersWhichStartWithString("CST", "'");
			AssertArrayEqualsByElements(new int[] { 3, 17 }, linesWhichStartWithCst);

			ZString danMessage = @"Dan 
Says
Hello
Dan
I
Like
		Dan   (!)
Cos
Dan
Is
Cool
Bye
Dan";
			int[] linesWhichStartWithDan = danMessage.FindLineNumbersWhichStartWithString("Dan", Environment.NewLine);
			AssertArrayEqualsByElements(new int[] { 1, 4, 9, 13 }, linesWhichStartWithDan);

			ZString cusdec = "UNH+25639+CUSDEC:D:04A:UN:109700+<<SYSCAR>>'BGM+IFD::109++9'CST++IMA+2'LOC+14+GBSTN::109'LOC+35+CN'GEI+5+ACC:PI:109'RFF+ABO:9GB800547754000-BALD008986:'RFF+UCN:PGM1B0KHZ00300'RFF+ABI:8585351:A'TDT+13++1+++++:::MOL TRIBUTE:CN'TDT+1'DOC+916:::Y024+GBGB AEOF 000-19/18:JP'NAD+CZ+++NINGBO CSTAR IMPORT & EXPORT CO LTD+NO. 568 TIANTONG SOUTH RD+NINGBO++CHINA+CN'NAD+CN+GB941877292000++SNAP PRODUCTS LTD+7 REDAN HILL ESTATE+ALDERSHOT++GU12 4SJ+GB'NAD+DT+GB800547754000++KINGSCOTE ROJAY LTD+UNIT 3,  EASTERN ROAD+ALDERSHOT++GU12 4TD+GB'UNS+D'DMS+SALD37109'MOA+64:163.04:USD'MOA+39:27300.00:USD'MOA+103:100.00:USD'MOA+105:200.78:GBP'CST++4000000+3926909790+++100'LOC+27+CN'MEA+AAR++KGM:200'PAC+20++CT'PCI++AS ADDR'MOA+38:4200'RFF+AAQ:BEAU5297774'RFF+ZZZ'IMD+++:::PHONE WALLET'DOC+998:::380+19B239A:::::Z'DOC+916:::N934+:JE'GEI+2+1:VM:109'GEI+2+A:VA:109'PCD+9:0'TAX+1+B00+F++S::::::109'TAX+1+A00+F++F::::::109'CST++4000000+8518299590+++100'LOC+27+CN'MEA+AAR++KGM:270.477'MEA+AAS++ZZZ:2000'PAC+40++CT'PCI++AS ADDR'MOA+38:5680'RFF+AAQ:BEAU5297774'RFF+ZZZ'IMD+++:::SPEAKERS'DOC+998:::380+19B239A:::::Z'DOC+916:::N934+:JE'GEI+2+1:VM:109'GEI+2+A:VA:109'PCD+9:0'TAX+1+B00+F++S::::::109'TAX+1+A00+F++F::::::109'CST++4000000+8470100000+++100'LOC+27+CN'MEA+AAR++KGM:48.572'PAC+5++CT'PCI++AS ADDR'MOA+38:1020'RFF+AAQ:BEAU5297774'RFF+ZZZ'IMD+++:::CALCULATORS'DOC+998:::380+19B239A:::::Z'DOC+916:::N934+:JE'GEI+2+1:VM:109'GEI+2+A:VA:109'PCD+9:0'TAX+1+B00+F++S::::::109'CST++4000000+8507800090+++100'LOC+27+CN'MEA+AAR++KGM:285.714'PAC+75++CT'PCI++AS ADDR'MOA+38:6000'RFF+AAQ:BEAU5297774'RFF+ZZZ'IMD+++:::CHARGER'DOC+998:::380+19B239A:::::Z'DOC+916:::N934+:JE'GEI+2+1:VM:109'GEI+2+A:VA:109'PCD+9:0'TAX+1+B00+F++S::::::109'TAX+1+A00+F++F::::::109'CST++4000000+8544429090+++100'LOC+27+CN'MEA+AAR++KGM:345.238'PAC+17++CT'PCI++AS ADDR'MOA+38:7250'RFF+AAQ:BEAU5297774'RFF+ZZZ'IMD+++:::CABLE'DOC+998:::380+19B239A:::::Z'DOC+916:::N934+:JE'GEI+2+1:VM:109'GEI+2+A:VA:109'PCD+9:0'TAX+1+B00+F++S::::::109'TAX+1+A00+F++F::::::109'CST++4000000+9029100090+++100'LOC+27+CN'MEA+AAR++KGM:150'PAC+10++CT'PCI++AS ADDR'MOA+38:3150'RFF+AAQ:BEAU5297774'RFF+ZZZ'IMD+++:::PEDOMETER'DOC+998:::380+19B239A:::::Z'DOC+916:::N934+:JE'GEI+2+1:VM:109'GEI+2+A:VA:109'PCD+9:0'TAX+1+B00+F++S::::::109'TAX+1+A00+F++F::::::109'UNS+S'CNT+11:167'UNT+120+25639'";
			linesWhichStartWithCst = cusdec.FindLineNumbersWhichStartWithString("CST", "'");
			AssertArrayEqualsByElements(new int[] { 3, 22, 38, 55, 70, 86, 102 }, linesWhichStartWithCst);

			ZString testData = @"CST
is what we are
looking for here  
  CST  CST  CST
:)
CST  CST  ----   
  //  
  CST";
			linesWhichStartWithCst = testData.FindLineNumbersWhichStartWithString("CST");
			AssertArrayEqualsByElements(new int[] { 1, 6 }, linesWhichStartWithCst);
		}

		public void TestEmpty()
		{
			AssertEquals("IsEmpty", true, ZString.Empty.IsEmpty);
			AssertEquals("IsValid", true, ZString.Empty.IsValid);
			Assert("Equals", ZString.Empty.Equals(ZString.Empty));
		}

		public void TestCharConstructor()
		{
			AssertEquals((ZString)"A", new ZString('A'));
		}

		public void TestReplicateCharacter()
		{
			AssertEquals(new ZString("AAAAA"), ZString.Replicate('A', 5));
		}

		public void TestEmptyStringPreservesBlankCharacters()
		{
			string actual = new ZString(" ");
			AssertEquals(" ", actual);
		}

		public void TestControlCharactesConsiderAsEmpty()
		{
			ZString controlCharacter = "\u009d";
			AssertEquals("IsEmpty", controlCharacter.IsEmpty, true);
		}

		public void TestContains()
		{
			ZString testString = new ZString("ABCD TEST EFGH");
			ZString testSubstring = new ZString("TEST");
			ZString testString2 = new ZString("ABCDE");

			AssertEquals("TestString Contains TEST", true, testString.Contains(testSubstring));
			AssertEquals("TestString Contains ABCDE", false, testString.Contains(testString2));
		}

		public void TestInsertSafe()
		{
			ZString testString = "ABCD";
			AssertEquals("InsertSafe", "ABCD", testString.InsertSafe(100, "E"));

			AssertEquals("InsertSafe", "ABCDE", testString.InsertSafe(4, "E"));
			AssertEquals("InsertSafe", "AEBCD", testString.InsertSafe(1, "E"));
		}

		public void TestSplitByLength()
		{
			ZString testString = "ABCD";
			AssertEquals(2, testString.Split(2).Length);
			AssertEquals("AB", testString.Split(2)[0]);
			AssertEquals("CD", testString.Split(2)[1]);

			AssertEquals(2, testString.Split(3).Length);
			AssertEquals("ABC", testString.Split(3)[0]);
			AssertEquals("D", testString.Split(3)[1]);

			AssertEquals(1, testString.Split(5).Length);
			AssertEquals("ABCD", testString.Split(5)[0]);

			AssertExceptionThrown<ArgumentException>("When Split lengthToSplit = -1", () => testString.Split(-1));
			AssertExceptionThrown<ArgumentException>("When Split lengthToSplit = 0", () => testString.Split(0));
		}

		public void TestEqualOperator()
		{
			ZString lhs = "Good";
			ZString rhs = "Good";

			AssertEquals(true, lhs == rhs);
			AssertEquals(true, lhs == "Good");
			AssertEquals(true, "Good" == rhs);

			lhs = "Bad";

			AssertEquals(false, lhs == rhs);
			AssertEquals(false, lhs == "Good");
			AssertEquals(false, "Bad" == rhs);
		}

		public void TestNotEqualOperator()
		{
			ZString lhs = "Good";
			ZString rhs = "Good";

			AssertEquals(false, lhs != rhs);
			AssertEquals(false, lhs != "Good");
			AssertEquals(false, "Good" != rhs);

			lhs = "Bad";

			AssertEquals(true, lhs != rhs);
			AssertEquals(true, lhs != "Good");
			AssertEquals(true, "Bad" != rhs);
		}

		public void TestPlusOperator()
		{
			string s1 = "a";
			string s2 = "b";
			string result = s1 + s2;

			ZString z1 = "a";
			ZString z2 = "b";
			ZString zResult = z1 + z2;

			Assert("ZString + operator overload did not match string + operator.", result == zResult);
		}

		public void TestPlusEqualsOperator()
		{
			// This test would normally throw an exception due to a compiler bug.
			// The fix is to manually overload the + operator in ZString.

			ZString z = "";
			object aLMOST_ANYTHING = " - ";

			z += "xyz" + aLMOST_ANYTHING + "xyz";

			AssertEquals("ZString += operator overload did not match string += operator", "xyz - xyz", z);
		}

		public void TestPlusOperatorWithVariousStringCombinations()
		{
			ZString z = "a";
			AssertEquals("ab", z + "b");

			z += "b";
			AssertEquals("ab", z);

			z += new ZString("c") + "d";
			AssertEquals("abcd", z);

			object o = "";
			z += new ZString("") + "e" + o + "f";
			AssertEquals("abcdef", z);
		}

		public void TestStripNonWesternEuropeanCharacters()
		{
			ZString myString = "abc";
			AssertEquals("string with valid western european charaters.", "abc", myString.StripNonWesternEuropeanCharacters());

			myString = ZString.Empty;
			AssertEquals("empty string", string.Empty, myString.StripNonWesternEuropeanCharacters());

			myString = "\0";
			AssertEquals("string with null character", string.Empty, myString.StripNonWesternEuropeanCharacters());

			myString = "\0\abc?";
			AssertEquals("string with non western european charaters", "bc?", myString.StripNonWesternEuropeanCharacters());

			myString = "こ%ん1にbbちはaa";
			AssertEquals("string with non western european charaters", "%1bbaa", myString.StripNonWesternEuropeanCharacters());
		}

		public void TestSplit()
		{
			ZString[] expected = new ZString[] { ZString.Empty };
			AssertSplit("Empty split by null", expected, ZString.Empty.Split(Array.Empty<char>()));
			AssertSplit("Empty split by comma", expected, ZString.Empty.Split(','));
			AssertSplit("Empty split by various characters", expected, ZString.Empty.Split('.', '-', '\r', '\n'));

			expected = new ZString[] { "1", "2", "3" };

			AssertSplit("\"1 2 3\" split by space", expected, new ZString("1 2 3").Split(' '));

			ZString[] result = new ZString("More words then the maximum value the split can hold").Split(new char[] { ' ' }, 3);
			AssertEquals("Split maximum returns array of that size", 3, result.Length);
			ZString[] expectedMax = new ZString[] { ZString.Empty };

			expectedMax = new ZString[]
			{
				new ZString("More"),
				new ZString("words"),
				new ZString("then the maximum value the split can hold")
			};

			AssertSplit("The elements contain the correct information", expectedMax, result);

			AssertSplit("string split by string", new ZString[] { "1", "2", "3" }, new ZString("1, 2, 3").Split(", "));
		}

		public void TestToTitleCase()
		{
			AssertEquals("The Dog Is Brown", new ZString("the dog is brown").ToTitleCase());
			AssertEquals("The Dog Is Brown", new ZString("THE DOG IS BROWN").ToTitleCase());
			AssertEquals("The Dog Is Brown", new ZString("tHe dOg is brOwn").ToTitleCase());
		}

		public void TestIEnumerable()
		{
			foreach (object value in ValidValues)
			{
				ZString zValue = new ZString(value);
				string testString = "";

				foreach (char character in zValue)
				{
					testString += character;
				}

				AssertEquals(testString, zValue);
			}
		}

		public void TestIndexer()
		{
			ZString zValue = "hello";
			string value = "hello";

			for (int i = 0; i < zValue.Length; i++)
			{
				AssertEquals("String[" + i + "] differs from ZString[" + i + "]", value[i], zValue[i]);
			}
		}

		public void TestEndsWith()
		{
			AssertEquals(true, new ZString("").EndsWith(""));
			AssertEquals(true, new ZString("123").EndsWith(""));
			AssertEquals(true, new ZString("123").EndsWith("3"));
			AssertEquals(true, new ZString("123").EndsWith("123"));
			AssertEquals(false, new ZString("").EndsWith("123"));
		}

		public void TestEndsWithIgnoringCase()
		{
			AssertEquals(true, new ZString("").EndsWith("", StringComparison.OrdinalIgnoreCase));
			AssertEquals(true, new ZString("aBc").EndsWith("", StringComparison.OrdinalIgnoreCase));
			AssertEquals(true, new ZString("aBc").EndsWith("C", StringComparison.OrdinalIgnoreCase));
			AssertEquals(true, new ZString("aBc").EndsWith("AbC", StringComparison.OrdinalIgnoreCase));
			AssertEquals(false, new ZString("").EndsWith("aBc", StringComparison.OrdinalIgnoreCase));
		}

		public void TestIndexOfString()
		{
			ZString testString = new ZString("ABCD TEST EFGH");
			ZString testSubstring = new ZString("TEST");
			ZString testString2 = new ZString("ABCDE");
			ZString testString3 = new ZString("TesT");

			AssertEquals("Index of TEST", 5, testString.IndexOf(testSubstring));
			AssertEquals("Index of ABCDE", -1, testString.IndexOf(testString2));
			AssertEquals("Index of empty string", 0, testString.IndexOf(""));
			AssertEquals("Index of null", 0, testString.IndexOf(null));

			AssertEquals("Index of TesT (mixed case)", -1, testString.IndexOf(testString3));
			AssertEquals("Index of TesT (mixed case ignoring case)", 5, testString.IndexOf(testString3, StringComparison.OrdinalIgnoreCase));
			AssertEquals("Index of TesT (mixed case with start index)", 5, testString.IndexOf(testString3, 4, StringComparison.OrdinalIgnoreCase));
			AssertEquals("Index of TesT (mixed case with bad start index)", -1, testString.IndexOf(testString3, 8, StringComparison.OrdinalIgnoreCase));
		}

		public void TestIndexOfChar()
		{
			ZString zValue = new ZString("ABCD TEST EFGH");

			AssertEquals("Index of D", 3, zValue.IndexOf('D'));
			AssertEquals("Index of X", -1, zValue.IndexOf('X'));
		}

		public void TestIndexOfAny()
		{
			ZString zValue = "hello";
			string value = "hello";

			AssertEquals("Char not in string", value.IndexOfAny(new char[] { '0' }), zValue.IndexOfAny(new char[] { '0' }));
			AssertEquals("Char that is in string", value.IndexOfAny(new char[] { 'h' }), zValue.IndexOfAny(new char[] { 'h' }));
			AssertEquals("2 chars in string", value.IndexOfAny(new char[] { 'h', 'e' }), zValue.IndexOfAny(new char[] { 'h', 'e' }));
			AssertEquals("No chars to search for", -1, zValue.IndexOfAny(Array.Empty<char>()));

			AssertEquals("2 chars in string", 1, zValue.IndexOfAny(new char[] { 'e', 'l' }, 1));
			AssertEquals("2 chars in string", 2, zValue.IndexOfAny(new char[] { 'e', 'l' }, 2));
			AssertEquals("2 chars in string", -1, zValue.IndexOfAny(new char[] { 'e', 'l' }, 4));
		}

		public void TestInsert()
		{
			ZString zValue = "teststring";
			zValue = zValue.Insert(4, "-");
			AssertEquals("Did not insert '-'.", "test-string", zValue);
		}

		public void TestKeepChars()
		{
			ZString zValue = "625 sRV123";
			AssertEquals("KeepChars", "625123", zValue.KeepChars("1234567890"));
			zValue = "534 sPO3432";
			AssertEquals("KeepChars replace Invalid with space", "534    343 ", zValue.KeepChars("543", " "));
		}

		public void TestKeepCharsXMLFormat()
		{
			ZString zValue = "625Margin_% .&+\\@#$%^&*() _+=!~ sRV123";
			AssertEquals("KeepChars", "Margin_Percent.Percent_sRV123", zValue.KeepCharsXMLFormatting());
			zValue = "625Margin_%& sRV123";
			AssertEquals("KeepChars", "Margin_PercentsRV123", zValue.KeepCharsXMLFormatting());
			zValue = "猫";
			AssertEquals("KeepChars", "猫", zValue.KeepCharsXMLFormatting());
			zValue = "41猫_ 475三乡写了这个代码";
			AssertEquals("KeepChars", "猫_475三乡写了这个代码", zValue.KeepCharsXMLFormatting());
			zValue = "41猫_ 475三乡&写了这个代码";
			AssertEquals("KeepChars", "猫_475三乡写了这个代码", zValue.KeepCharsXMLFormatting());
			zValue = "-.123ASDF1234";
			AssertEquals("First numbers, '.' or '-' should be removed", "ASDF1234", zValue.KeepCharsXMLFormatting());
		}

		public void TestKeepCharsUntil()
		{
			ZString value = "1050C/2U";
			AssertEquals("KeepCharsUntil", "1050", value.KeepCharsUntil("0123456789", new char[] { '/' }));
			AssertEquals("KeepCharsUntil", "1050", value.KeepCharsUntil("0123456789", new char[] { '/', 'U' }));
			AssertEquals("KeepCharsUntil", "10502", value.KeepCharsUntil("0123456789", new char[] { '%' }));
		}

		public void TestExcludeChars()
		{
			ZString zValue = "^$%__123";
			AssertEquals("ExcludeChars", "$__23", zValue.ExcludeChars("^%1"));
		}

		public void TestExcludeNonValidXMLCharacters()
		{
			ZString zValue = "\x00 @ $& # ABC \x08 \x09 \x0A \x0B \x0C \x0D A B C \x1E TEST \u000C \u000D \u000E \U00021d8c 𡶌𡵖𡴓马";
			AssertEquals("ExcludeChars", " @ $& # ABC  \x09 \x0A   \x0D A B C  TEST  \u000D  𡶌 𡶌𡵖𡴓马", zValue.ExcludeNonValidXMLCharacters());
		}

		public void TestLength()
		{
			AssertEquals(0, ZString.Empty.Length);
			AssertEquals(1, new ZString(" ").Length);
		}

		public void TestPadLeft()
		{
			ZString zValue = "ab";
			AssertEquals(" ab", zValue.PadLeft(3));
			AssertEquals(" ab", zValue.PadLeft(3, ' '));
			AssertEquals("ab", zValue.PadLeft(2));
			AssertEquals("ab", zValue.PadLeft(2, ' '));

			zValue = ZString.Empty;
			AssertEquals("   ", zValue.PadLeft(3));
			AssertEquals("   ", zValue.PadLeft(3, ' '));
			AssertEquals("  ", zValue.PadLeft(2));
			AssertEquals("  ", zValue.PadLeft(2, ' '));
		}

		public void TestPadRight()
		{
			ZString zValue = "ab";
			AssertEquals("ab ", zValue.PadRight(3));
			AssertEquals("ab ", zValue.PadRight(3, ' '));
			AssertEquals("ab", zValue.PadRight(2));
			AssertEquals("ab", zValue.PadRight(2, ' '));

			zValue = ZString.Empty;
			AssertEquals("   ", zValue.PadRight(3));
			AssertEquals("   ", zValue.PadRight(3, ' '));
			AssertEquals("  ", zValue.PadRight(2));
			AssertEquals("  ", zValue.PadRight(2, ' '));
		}

		public void TestReplaceChar()
		{
			ZString zValue = "Old value";
			AssertEquals("OlD value", zValue.Replace('d', 'D'));
		}

		public void TestReplaceString()
		{
			ZString zValue = "Old Value";
			AssertEquals("OlD value", zValue.Replace("d V", "D v"));
		}

		public void TestReplaceIgnoringCase()
		{
			ZString value = "(*JohnAthOn*)";
			AssertEquals("(*Johnny*)", value.ReplaceIgnoringCase("athon*)", "ny*)"));
		}

		public void TestTruncate()
		{
			ZString zValue = " value ";
			AssertEquals("When string length < truncate maxLength", " value ", zValue.Truncate(20));
			AssertEquals("When string length = truncate maxLength", " value ", zValue.Truncate(7));
			AssertEquals("When string length > truncate maxLength", " va...", zValue.Truncate(6));
			AssertExceptionThrown("When truncate maxLength = 3", typeof(ArgumentException), () => zValue.Truncate(3));
			AssertExceptionThrown("When truncate maxLength = 2 ", typeof(ArgumentException), () => zValue.Truncate(2));
			AssertExceptionThrown("When truncate maxLength = 1", typeof(ArgumentException), () => zValue.Truncate(1));
			AssertExceptionThrown("When truncate maxLength = 0", typeof(ArgumentException), () => zValue.Truncate(0));
			AssertExceptionThrown("When truncate maxLength = -1", typeof(ArgumentException), () => zValue.Truncate(-1));
		}

		public void TestTrim()
		{
			ZString zValue = " value ";
			AssertEquals("value", zValue.Trim());
		}

		public void TestTrimChars()
		{
			ZString zValue = " value ";
			AssertEquals("When char[] is null", "value", zValue.Trim(null));
			AssertEquals("When char[] is empty", "value", zValue.Trim(Array.Empty<char>()));
			AssertEquals("When char[] is space", "value", zValue.Trim(new char[] { ' ' }));
			AssertEquals("When char[] is non-space", " value ", zValue.Trim(new char[] { 'x' }));
		}

		public void TestTrimStart()
		{
			ZString zValue = " value ";
			AssertEquals("value ", zValue.TrimStart());
		}

		public void TestTrimStartChars()
		{
			ZString zValue = " value ";
			AssertEquals("When char[] is null", "value ", zValue.TrimStart(null));
			AssertEquals("When char[] is empty", "value ", zValue.TrimStart(Array.Empty<char>()));
			AssertEquals("When char[] is space", "value ", zValue.TrimStart(new char[] { ' ' }));
			AssertEquals("When char[] is non-space", " value ", zValue.TrimStart(new char[] { 'x' }));
		}

		public void TestTrimEnd()
		{
			ZString zValue = " value ";
			AssertEquals(" value", zValue.TrimEnd());
		}

		public void TestTrimEndChars()
		{
			ZString zValue = " value ";
			AssertEquals("When char[] is null", " value", zValue.TrimEnd(null));
			AssertEquals("When char[] is empty", " value", zValue.TrimEnd(Array.Empty<char>()));
			AssertEquals("When char[] is space", " value", zValue.TrimEnd(new char[] { ' ' }));
			AssertEquals("When char[] is non-space", " value ", zValue.TrimEnd(new char[] { 'x' }));
		}

		public void TestIndexerThrowsExceptionOnEmptyString()
		{
			foreach (object value in EmptyValues)
			{
				bool threwException = false;
				ZString zValue = new ZString(value);

				try
				{
					char c = zValue[0];
				}
				catch
				{
					threwException = true;
				}

				Assert("Accessing the indexer of an empty ZString should have thrown an exception.", threwException);
			}
		}

		public void TestToStringArray()
		{
			ZString[] nullZStringArray = null;
			ZString[] zValues = new ZString[] { "Fred", "Barney", "Wilma", "Betty", "Dino" };

			string[] nullStringArray = ZString.ToStringArray(nullZStringArray);
			string[] values = ZString.ToStringArray(zValues);

			AssertEquals("The string array should have been null.", null, nullStringArray);
			AssertEquals("The number of elements in the string[] does not match those in the ZString[].", zValues.Length, values.Length);

			for (int i = 0; i < values.Length; i++)
			{
				AssertEquals("The ZString value at index [" + i + "] does not match the string value.", values[i], zValues[i]);
			}
		}

		public void TestJoin()
		{
			string[] values1 = new string[] { "one", "two", "three" };
			ZString[] zValues1 = new ZString[] { "one", "two", "three" };
			AssertEquals("ZString join should match String join", string.Join(",", values1), ZString.Join(",", zValues1));

			string[] values2 = new string[] { "one", null, "three" };
			ZString[] zValues2 = new ZString[] { "one", null, "three" };
			AssertEquals("ZString join should match String join", string.Join(",", values2), ZString.Join(",", zValues2));

			AssertEquals("Join with one word", string.Join(",", values2, 0, 1), ZString.Join(",", zValues2, 0, 1));
			AssertEquals("Join with two words", string.Join(",", values2, 0, 2), ZString.Join(",", zValues2, 0, 2));
			AssertEquals("Join with three words", string.Join(",", values2, 0, 3), ZString.Join(",", zValues2, 0, 3));

			AssertEquals("", "onethree", ZString.Join(zValues2));
			AssertEquals("", ZString.Join("", zValues2), ZString.Join(zValues2));
		}

		public void TestLeft()
		{
			ZString value = "ABCDEF";
			AssertEquals("", value.Left(0));
			AssertEquals("AB", value.Left(2));
			AssertEquals("ABCDEF", value.Left(6));
			AssertEquals("ABCDEF", value.Left(7));
		}

		public void TestLeftWhenEmpty()
		{
			AssertEquals("", ZString.Empty.Left(0));
			AssertEquals("", ZString.Empty.Left(1));
		}

		public void TestLeftDoesNotAllocateDuplicateString()
		{
			ZString value = "TESTVALUE";
			ZString leftValue = value.Left(value.Length);
			string valueInternalString = (string)((IZTypeInternals)value).GetValueForLogicalDataLayer(false);
			string leftValueInternalString = (string)((IZTypeInternals)leftValue).GetValueForLogicalDataLayer(false);
			Assert("Original value and value.Left(value.Length) should be the same object in memory", ReferenceEquals(valueInternalString, leftValueInternalString));
		}

		public void TestRight()
		{
			ZString value = "ABCDEF";
			AssertEquals("", value.Right(0));
			AssertEquals("EF", value.Right(2));
			AssertEquals("ABCDEF", value.Right(6));
			AssertEquals("ABCDEF", value.Right(7));
		}

		public void TestRightWhenEmpty()
		{
			AssertEquals("", ZString.Empty.Right(0));
			AssertEquals("", ZString.Empty.Right(1));
		}

		public void TestRightDoesNotAllocateDuplicateString()
		{
			ZString value = "TESTVALUE";
			ZString rightValue = value.Right(value.Length);
			string valueInternalString = (string)((IZTypeInternals)value).GetValueForLogicalDataLayer(false);
			string rightValueInternalString = (string)((IZTypeInternals)rightValue).GetValueForLogicalDataLayer(false);
			Assert("Original value and value.Right(value.Length) should be the same object in memory", ReferenceEquals(valueInternalString, rightValueInternalString));
		}

		public void TestSubstringSafeWithValidData()
		{
			ZString value = "ABCDEF";
			AssertEquals("A", value.SubstringSafe(0, 1));
			AssertEquals("B", value.SubstringSafe(1, 1));
			AssertEquals("ABCDEF", value.SubstringSafe(0, 6));
			AssertEquals("", value.SubstringSafe(0, 0));
			AssertEquals("", value.SubstringSafe(1, 0));

			AssertEquals("BCDEF", value.SubstringSafe(1));
			AssertEquals("ABCDEF", value.SubstringSafe(0));
		}

		public void TestSubstringSafeDoesNotAllocateDuplicateString()
		{
			ZString value = "TESTVALUE";
			ZString substringSafeValue = value.SubstringSafe(0);
			string valueInternalString = (string)((IZTypeInternals)value).GetValueForLogicalDataLayer(false);
			string substringSafeValueInternalString = (string)((IZTypeInternals)substringSafeValue).GetValueForLogicalDataLayer(false);
			Assert("Original value and value.SubstringSafe(0) should be the same object in memory", ReferenceEquals(valueInternalString, substringSafeValueInternalString));

			substringSafeValue = value.SubstringSafe(0, value.Length);
			substringSafeValueInternalString = (string)((IZTypeInternals)substringSafeValue).GetValueForLogicalDataLayer(false);
			Assert("Original value and value.SubstringSafe(0) should be the same object in memory", ReferenceEquals(valueInternalString, substringSafeValueInternalString));

			substringSafeValue = value.SubstringSafe(0, value.Length + 1);
			substringSafeValueInternalString = (string)((IZTypeInternals)substringSafeValue).GetValueForLogicalDataLayer(false);
			Assert("Original value and value.SubstringSafe(0) should be the same object in memory", ReferenceEquals(valueInternalString, substringSafeValueInternalString));
		}

		public void TestSubstringDoesNotAllocateDuplicateString()
		{
			ZString value = "TESTVALUE";
			ZString substringValue = value.Substring(0);
			string valueInternalString = (string)((IZTypeInternals)value).GetValueForLogicalDataLayer(false);
			string substringValueInternalString = (string)((IZTypeInternals)substringValue).GetValueForLogicalDataLayer(false);
			Assert("Original value and value.Substring(0) should be the same object in memory", ReferenceEquals(valueInternalString, substringValueInternalString));

			substringValue = value.Substring(0, value.Length);
			substringValueInternalString = (string)((IZTypeInternals)substringValue).GetValueForLogicalDataLayer(false);
			Assert("Original value and value.Substring(0, value.Length) should be the same object in memory", ReferenceEquals(valueInternalString, substringValueInternalString));
		}

		public void TestSubstringSafeWithInvalidStartIndex()
		{
			ZString value = "ABCDEF";
			AssertEquals("", value.SubstringSafe(7));
			AssertEquals("", value.SubstringSafe(7, 1));

			AssertEquals("ABCDEF", value.SubstringSafe(-1));
			AssertEquals("A", value.SubstringSafe(-1, 1));
		}

		public void TestSubstringSafeWithInvalidLength()
		{
			ZString value = "ABCDEF";
			AssertEquals("ABCDEF", value.SubstringSafe(0, 50));
			AssertEquals("BCDEF", value.SubstringSafe(1, 50));
			AssertEquals("ABCDEF", value.SubstringSafe(0, -1));
			AssertEquals("BCDEF", value.SubstringSafe(1, -1));
		}

		public void TestIsLettersAndNumbersAndPunctuationOnly()
		{
			AssertEquals("Letters and numbers only", true, new ZString("xxx \r\n123").IsLettersAndNumbersAndPunctuationOnlyOrEmpty);
			AssertEquals("Punctuation", true, new ZString("!@$#^$%*&(&*)_)+{}\":|><?/';/,[.]\\=-0").IsLettersAndNumbersAndPunctuationOnlyOrEmpty);
			AssertEquals("Crap", false, new ZString("123abc" + new string((char)1, 1)).IsLettersAndNumbersAndPunctuationOnlyOrEmpty);
			AssertEquals("Should return True if empty", true, new ZString("").IsLettersAndNumbersAndPunctuationOnlyOrEmpty);
		}

		public void TestConvertToOnlyLettersAndNumbersAndPunctuation()
		{
			AssertEquals("Letters and numbers only", "xxx \r\n123", new ZString("xxx \r\n123").ConvertToOnlyLettersAndNumbersAndPunctuation());
			AssertEquals("Punctuation", "!@$#^$%*&(&*)_)+{}\":|><?/';/,[.]\\=-0", new ZString("!@$#^$%*&(&*)_)+{}\":|><?/';/,[.]\\=-0").ConvertToOnlyLettersAndNumbersAndPunctuation());
			AssertEquals("Crap", "123abc", new ZString("123abc" + new string((char)1, 1)).ConvertToOnlyLettersAndNumbersAndPunctuation());
		}

		public void TestIsLettersAndNumbersOnly()
		{
			AssertEquals("Letters and numbers only", true, new ZString("xxx123k").IsLettersAndNumbersOnlyOrEmpty);
			AssertEquals("Punctuation", false, new ZString("!@$#^$%*&(&*)_)+{}\":|><?/';/,[.]\\=-0").IsLettersAndNumbersOnlyOrEmpty);
			AssertEquals("Crap", false, new ZString("123abc" + new string((char)1, 1)).IsLettersAndNumbersOnlyOrEmpty);
			AssertEquals("Should return True if empty", true, new ZString("").IsLettersAndNumbersOnlyOrEmpty);
		}

		public void TestIsNumbersOnlyOrEmpty()
		{
			AssertEquals("Contains letters, MyZString.IsNumbersOnlyOrEmpty should return false.", false, new ZString("ABC").IsNumbersOnlyOrEmpty);
			AssertEquals("Contains numbers and letters, MyZString.IsNumbersOnlyOrEmpty should return false.", false, new ZString("ABC123").IsNumbersOnlyOrEmpty);
			AssertEquals("Contains numbers with a decimal point, MyZString.IsNumbersOnlyOrEmpty should return false.", false, new ZString("12.3").IsNumbersOnlyOrEmpty);
			AssertEquals("Contains numbers with whitespace, MyZString.IsNumbersOnlyOrEmpty should return false.", false, new ZString("12 3").IsNumbersOnlyOrEmpty);
			AssertEquals("Contains numbers only, MyZString.IsNumbersOnlyOrEmpty should return true.", true, new ZString("1234567890").IsNumbersOnlyOrEmpty);
			AssertEquals("Sholud return true for empty string", true, new ZString("").IsNumbersOnlyOrEmpty);
		}

		public void TestIsLettersOnlyOrEmpty()
		{
			AssertEquals("Contains numbers, MyZString.IsLettersOnlyOrEmpty should return false.", false, new ZString("123").IsLettersOnlyOrEmpty);
			AssertEquals("Contains letters and numbers, MyZString.IsLettersOnlyOrEmpty should return false.", false, new ZString("ABC123").IsLettersOnlyOrEmpty);
			AssertEquals("Contains letters and punctuation, MyZString.IsLettersOnlyOrEmpty should return false.", false, new ZString("AB.C").IsLettersOnlyOrEmpty);
			AssertEquals("Contains letters with whitespace, MyZString.IsLettersOnlyOrEmpty should return false.", false, new ZString("AB C").IsLettersOnlyOrEmpty);
			AssertEquals("Contains letters only, MyZString.IsLettersOnlyOrEmpty should return true.", true, new ZString("ABCDEFG").IsLettersOnlyOrEmpty);
			AssertEquals("Sholud return true for empty string", true, new ZString("").IsLettersOnlyOrEmpty);
		}

		public void TestLastIndexOf()
		{
			ZString value = "helloByehelloBye";
			AssertEquals("Found last hello index", 8, value.LastIndexOf("hello"));
			AssertEquals("Found nothing", -1, value.LastIndexOf("coffee"));
			// Different behavior of LastIndexOf between .NET Framework and .NET Core
#if NETFRAMEWORK
			AssertEquals("Empty search string", value.Length - 1, value.LastIndexOf(""));
#else
			AssertEquals("Empty search string", value.Length, value.LastIndexOf(""));
#endif
			AssertEquals("Found nothing", -1, new ZString().LastIndexOf("coffee"));
		}

		public void TestLastIndexOfForChar()
		{
			ZString value = "helloByehelloBye";
			AssertEquals("Found nothing", -1, value.LastIndexOf(' '));
			AssertEquals("Found last y", 14, value.LastIndexOf('y'));
		}

		public void TestIndexOfOccurrence()
		{
			ZString value = "F|4|DX 0103|BPM|BMO|BAH||2005-03-01|11:30:00|";
			AssertEquals("Occurrence greater then number in string", -1, value.IndexOf('|', 10));
			AssertEquals("Position of second Occurrence", 3, value.IndexOf('|', 2));
			AssertEquals("Position of the seventh occurrence", 24, value.IndexOf('|', 7));
		}

		[ExpectException(typeof(ArgumentOutOfRangeException))]
		public void TestIndexOfOccurrenceException()
		{
			ZString value = "F|4|DX 0103|BPM|BMO|BAH||2005-03-01|11:30:00|";
			value.IndexOf('|', -1);
		}

		public void TestZStringFormat()
		{
			ZString formatString = "Name: {0}";
			ZString argument = "Fred Flintstone";

			AssertEquals(string.Format(formatString, argument), ZString.Format(formatString, argument));
			AssertEquals("Name: Fred Flintstone", ZString.Format(formatString, argument));
		}

		public void TestOccurences()
		{
			ZString value1 = "Hark the herald angels sing glory to the new-born King.";
			AssertEquals(2, value1.Occurrences("the"));
			AssertEquals(1, value1.Occurrences("angels"));
			AssertEquals(0, value1.Occurrences("Angels"));
			AssertEquals(0, value1.Occurrences("song"));

			ZString value2 = "aaa";
			AssertEquals(3, value2.Occurrences("a"));
			AssertEquals(1, value2.Occurrences("aa"));
			AssertEquals(1, value2.Occurrences("aaa"));
			AssertEquals(0, value2.Occurrences("aAa"));
			AssertEquals(0, value2.Occurrences("aaaa"));
		}

		public void TestOccurencesIgnoringCase()
		{
			ZString value1 = "Hark the herald angels sing glory to the new-born King.";
			AssertEquals(2, value1.OccurrencesIgnoringCase("the"));
			AssertEquals(1, value1.OccurrencesIgnoringCase("angels"));
			AssertEquals(0, value1.OccurrencesIgnoringCase("song"));

			ZString value2 = "aaa";
			AssertEquals(3, value2.OccurrencesIgnoringCase("a"));
			AssertEquals(1, value2.OccurrencesIgnoringCase("aa"));
			AssertEquals(1, value2.OccurrencesIgnoringCase("aaa"));
			AssertEquals(0, value2.OccurrencesIgnoringCase("aaaa"));

			// testing ignore case
			AssertEquals(1, value1.OccurrencesIgnoringCase("Angels"));
			AssertEquals(1, value2.OccurrencesIgnoringCase("aAa"));
		}

		public void TestTrimEndIncludingWhiteSpace()
		{
			var whiteSpaceString = new string(WhiteSpace);
			ZString example = "XYZ , \r,	\n , \r\n\n\n,	\r\r" + whiteSpaceString;
			AssertEquals("XYZ", example.TrimEndIncludingWhiteSpace(','));
		}

		public void TestTrimIncludingWhiteSpace()
		{
			var whiteSpaceString = new string(WhiteSpace);
			ZString example = whiteSpaceString + " , \r,	\n , \r\n\n\n,	\r\r,XYZ , \r,	\n , \r\n\n\n,	\r\r" + whiteSpaceString;
			AssertEquals("XYZ", example.TrimIncludingWhiteSpace(','));
		}

		readonly char[] WhiteSpace = new char[]
		{
			'\t', '\n', '\v', '\f', '\r', ' ', '\x00a0', '\u2000', '\u2001', '\u2002', '\u2003', '\u2004',
			'\u2005', '\u2006', '\u2007', '\u2008', '\u2009', '\u200a', '\u200b', '\u3000', '\ufeff'
		};

		public void TestXmlSerializable()
		{
			AssertZTypeSerializesToXml("<a>v&lt;&gt;\\\"alue</a>", new ZString("v<>\\\"alue"));
		}

		public void TestEqualsIgnoringCase()
		{
			string foo = "foo";
			ZString zfoo = "foo";
			string fOO = "fOO";
			ZString zfOO = "fOO";
			string fOO1 = "FOO";
			ZString zFOO = "FOO";
			string bar = "bar";
			ZString zbar = "bar";

			AssertEquals(true, zfoo.EqualsIgnoringCase(zfoo));
			AssertEquals(true, zfoo.EqualsIgnoringCase(zfOO));
			AssertEquals(true, zfoo.EqualsIgnoringCase(zFOO));
			AssertEquals(false, zfoo.EqualsIgnoringCase(zbar));

			AssertEquals(true, zfoo.EqualsIgnoringCase(foo));
			AssertEquals(true, zfoo.EqualsIgnoringCase(fOO));
			AssertEquals(true, zfoo.EqualsIgnoringCase(fOO1));
			AssertEquals(false, zfoo.EqualsIgnoringCase(bar));
			AssertEquals(true, ZString.Empty.EqualsIgnoringCase(""));

			string nullString = null;
			AssertEquals(false, ZString.Empty.EqualsIgnoringCase(nullString));
			AssertEquals(false, ZString.Empty.EqualsIgnoringCase((string)null));
			AssertEquals(true, ZString.Empty.EqualsIgnoringCase((ZString)null));
			AssertEquals(true, ZString.Empty.EqualsIgnoringCase(""));
		}

		public void TestRemove()
		{
			ZString smurf = "smurf";

			AssertEquals("murf", smurf.Remove(0, 1));
			AssertEquals("smrf", smurf.Remove(2, 1));
			AssertEquals("smur", smurf.Remove(4, 1));

			AssertEquals("sf", smurf.Remove(1, 3));
			AssertEquals("", smurf.Remove(0, 5));
		}

		public void TestRemoveSafe()
		{
			ZString smurf = "smurf";

			// valid
			AssertEquals("murf", smurf.RemoveSafe(0, 1));
			AssertEquals("smrf", smurf.RemoveSafe(2, 1));
			AssertEquals("smur", smurf.RemoveSafe(4, 1));

			AssertEquals("sf", smurf.RemoveSafe(1, 3));
			AssertEquals("", smurf.RemoveSafe(0, 5));

			// invalid
			AssertEquals("smurf", smurf.RemoveSafe(-1, 1));
			AssertEquals("smurf", smurf.RemoveSafe(5, 1));
			AssertEquals("", smurf.RemoveSafe(0, 6));
			AssertEquals("sm", smurf.RemoveSafe(2, 20));
		}

		public void TestSplitIgnoringEscapedDelimiterLeavingDelimiter()
		{
			ZString data = "FOO:BAR";
			ZString[] result = data.SplitIgnoringEscapedDelimiter(':', 'X', true);
			AssertEquals(2, result.Length);
			AssertEquals("FOO:", result[0]);
			AssertEquals("BAR", result[1]);
		}

		public void TestSplitIgnoringEscapedDelimiterWithDelimiterAsLastCharacter()
		{
			ZString data = "FOO:BAR:";
			ZString[] result = data.SplitIgnoringEscapedDelimiter(':', 'X', true);
			AssertEquals(2, result.Length);
			AssertEquals("FOO:", result[0]);
			AssertEquals("BAR:", result[1]);
		}

		public void TestSplitIgnoringEscapedDelimiterWithDelimiterAsLastCharacterWithEscapedCharacters()
		{
			ZString data = "FOO?::BAR?::";
			ZString[] result = data.SplitIgnoringEscapedDelimiter(':', '?', true);
			AssertEquals(2, result.Length);
			AssertEquals("FOO?::", result[0]);
			AssertEquals("BAR?::", result[1]);
		}

		public void TestSplitIgnoringEscapedDelimiterEndEmpty()
		{
			ZString data = "FOO|BAR|";
			ZString[] result = data.SplitIgnoringEscapedDelimiter('|', '\\', false, true);
			AssertEquals(3, result.Length);
			AssertEquals("FOO", result[0]);
			AssertEquals("BAR", result[1]);
			AssertEquals("", result[2]);
		}

		public void TestSplitIgnoringEscapedDelimiter()
		{
			ZString inputRawRow = "foo:bar::spam?:and??:eggs:?::h?a";
			ZString[] outputRawFields = { "foo", "bar", "", "spam?:and??", "eggs", "?:", "h?a" };

			ZString[] output = inputRawRow.SplitIgnoringEscapedDelimiter(':', '?');

			for (int i = 0; i < output.Length; i++)
			{
				AssertEquals("Field " + (i + 1).ToString() + ":", outputRawFields[i], output[i]);
			}

			AssertEquals("number of fields match", outputRawFields.Length, output.Length);
		}

		public void TestKeepAlphanumericChars()
		{
			ZString testString = "A~a@b#B%Cc^d&D*eE(f)F-g=G+H_h/I'i;J:j<K>k?L.l,m[M]nN{O}o|pP~q!Q r R S s t T u U V v w W xX y Y Z z 0 5 1 2 3 476 8 9\t";
			AssertEquals("Should not include non-alphanumeric characters", "AabBCcdDeEfFgGHhIiJjKkLlmMnNOopPqQrRSstTuUVvwWxXyYZz0512347689", testString.KeepAlphanumericCharacters());
		}

		public void TestKeepAlphabeticChars()
		{
			ZString testString = "A~a@b#B%Cc^d&D*eE(f)F-g=G+H_h/I'i;J:j<K>k?L.l,m[M]nN{O}o|pP~q!Q r R S s t T u U V v w W xX y Y Z z 0 5 1 2 3 476 8 9\t";
			AssertEquals("Should not include non-alphabetic characters", "AabBCcdDeEfFgGHhIiJjKkLlmMnNOopPqQrRSstTuUVvwWxXyYZz", testString.KeepAlphabeticCharacters());
		}

		public void TestKeepNumericChars()
		{
			ZString testString = "A~a@b#B%Cc^d&D*eE(f)F-g=G+H_h/I'i;J:j<K>k?L.l,m[M]nN{O}o|pP~q!Q r R S s t T u U V v w W xX y Y Z z 0 5 1 2 3 476 8 9\t";
			AssertEquals("Should not include non-numeric characters", "0512347689", testString.KeepNumericCharacters());
		}

		public void TestRemoveDiacritics()
		{
			ZString testString = "ÀÇÈÎÔÙ áéïôü";
			AssertEquals("Diacritics should be removed from characters", "ACEIOU aeiou", testString.RemoveDiacritics());
		}

		public void TestContainsDiacritics()
		{
			ZString testString1 = "À";
			ZString testString2 = "áéïôü";
			ZString testString3 = "Aáa EÈe";

			ZString testString4 = "ABCD";
			ZString testString5 = "";
			ZString testString6 = "白理查";

			CombineAssertions("Should indicate if string contains diacritics:", () =>
			{
				AssertEquals("Single diacritic (À)", true, testString1.ContainsAnyDiacritics);
				AssertEquals("Multiple diacritics (áéïôü)", true, testString2.ContainsAnyDiacritics);
				AssertEquals("Mix of diacritics (Aáa EÈe)", true, testString3.ContainsAnyDiacritics);

				AssertEquals("No diacritics (ABCD)", false, testString4.ContainsAnyDiacritics);
				AssertEquals("Empty string", false, testString5.ContainsAnyDiacritics);
				AssertEquals("Chinese characters - not diacritics (白理查)", false, testString6.ContainsAnyDiacritics);
			});
		}

		#region Test .IsWesternEuropeanOrEmpty & ContainsAnyChar()

		public void TestIsWesternEuropean()
		{
			ZString s1 = "some plain text";
			AssertEquals(true, s1.IsWesternEuropeanOrEmpty);

			ZString s2 = "“text with m–dash and curly quotes”";
			AssertEquals(true, s2.IsWesternEuropeanOrEmpty);

			AssertEquals(true, AllWesternEuropeanCharacters.IsWesternEuropeanOrEmpty);

			foreach (char c in AllNonWesternEuropeanCharacters)
			{
				AssertEquals("The unicode character " + c + " is WesternEuropean and should not be.", false, ((ZString)c.ToString()).IsWesternEuropeanOrEmpty);
			}

			for (char c = '\x0'; c <= '\x8'; ++c)
			{
				AssertEquals("control character IsWesternEuropeanOrEmpty: " + (int)c, false, new ZString(c).IsWesternEuropeanOrEmpty);
			}

			for (char c = '\x9'; c <= '\x0d'; ++c)
			{
				AssertEquals("control character IsWesternEuropeanOrEmpty: " + (int)c, true, new ZString(c).IsWesternEuropeanOrEmpty);
			}

			for (char c = '\x0e'; c <= '\x1f'; ++c)
			{
				AssertEquals("control character IsWesternEuropeanOrEmpty: " + (int)c, false, new ZString(c).IsWesternEuropeanOrEmpty);
			}

			AssertEquals("should return true if empty", true, new ZString("").IsWesternEuropeanOrEmpty);
		}

		public void TestIsWindows1252()
		{
			ZString s1 = "some plain text";
			AssertEquals(true, s1.IsWindows1252OrEmpty);

			AssertEquals("should return true if emtpy", true, new ZString("").IsWindows1252OrEmpty);

			s1 += '\u03C0';
			AssertEquals(false, s1.IsWindows1252OrEmpty);

			var allChars = new char[char.MaxValue + 1];
			for (int i = 0; i < char.MaxValue; ++i)
			{
				allChars[i] = (char)i;
			}

			var encoding = Encoding.GetEncoding(1252);
			var bytes = new byte[1];
			for (int i = 0; i <= 255; ++i)
			{
				bytes[0] = (byte)i;
				char good = encoding.GetChars(bytes)[0];
				AssertEquals("IsWindows1252OrEmpty " + i, true, new ZString(good).IsWindows1252OrEmpty);
				allChars[good] = '\0';
			}

			for (int i = 0; i < char.MaxValue; ++i)
			{
				if (allChars[i] != '\0')
				{
					AssertEquals("IsWindows1252OrEmpty " + i, false, new ZString((char)i).IsWindows1252OrEmpty);
				}
			}
		}

		public void TestConvertToWesternEuropeanCharacters()
		{
			ZString testString = "some western euROPEAN text";
			ZString result = testString.ConvertToWesternEuropeanCharacters();
			AssertEquals(testString, result);

			testString = "\u0444";
			result = testString.ConvertToWesternEuropeanCharacters();
			AssertEquals("?", result);

			testString = "\u0444 \u0445 \u0446";
			result = testString.ConvertToWesternEuropeanCharacters();
			AssertEquals("? ? ?", result);
		}

		public void TestIsEnglishOnlyOrEmpty()
		{
			AssertEquals(true, new ZString("some plain text").IsEnglishOnlyOrEmpty);
			AssertEquals(false, new ZString("é").IsEnglishOnlyOrEmpty);
			AssertEquals(false, new ZString("Ä").IsEnglishOnlyOrEmpty);
			AssertEquals(false, new ZString("ö").IsEnglishOnlyOrEmpty);
			AssertEquals(false, new ZString("ß").IsEnglishOnlyOrEmpty);
			AssertEquals(true, new ZString("").IsEnglishOnlyOrEmpty);

			foreach (char c in AllWesternEuropeanCharacters)
			{
				if (c <= 127)
				{
					AssertEquals("The unicode character " + c + " is not IsEnglishOnlyOrEmpty but should be.", true, ((ZString)c.ToString()).IsEnglishOnlyOrEmpty);
				}
				else
				{
					AssertEquals("The unicode character " + c + " is IsEnglishOnlyOrEmpty and should not be.", false, ((ZString)c.ToString()).IsEnglishOnlyOrEmpty);
				}
			}

			foreach (char c in AllNonWesternEuropeanCharacters)
			{
				AssertEquals("The unicode character " + c + " is IsEnglishOnlyOrEmpty and should not be.", false, ((ZString)c.ToString()).IsEnglishOnlyOrEmpty);
			}

			for (char c = '\x0'; c <= '\x8'; ++c)
			{
				AssertEquals("control character should not be IsEnglishOnlyOrEmpty: " + (int)c, false, new ZString(c).IsEnglishOnlyOrEmpty);
			}

			for (char c = '\x9'; c <= '\x0d'; ++c)
			{
				AssertEquals("control character IsEnglishOnlyOrEmpty: " + (int)c, true, new ZString(c).IsEnglishOnlyOrEmpty);
			}

			for (char c = '\x0e'; c <= '\x1f'; ++c)
			{
				AssertEquals("control character should not be IsEnglishOnlyOrEmpty: " + (int)c, false, new ZString(c).IsEnglishOnlyOrEmpty);
			}
		}

		public void TestContainsAnyChar()
		{
			ZString testString = "hello";
			string allWesternEuroCharsExcludingHELO = AllWesternEuropeanCharacters.Replace("h", "").Replace("e", "").Replace("l", "").Replace("o", "");

			AssertEquals(true, testString.ContainsAnyChar("ole"));
			AssertEquals(true, testString.ContainsAnyChar("oxle1"));
			AssertEquals(false, testString.ContainsAnyChar("xyz|"));

			AssertEquals(false, testString.ContainsAnyChar(allWesternEuroCharsExcludingHELO));
			AssertEquals(false, testString.ContainsAnyChar(AllNonWesternEuropeanCharacters));
		}

		public void TestContainsAnyLetters()
		{
			ZString testString1 = "Hello";
			ZString testString2 = "111h111#$%#$%";
			ZString testString3 = "!(#*%!(#*!%234234(#%";

			AssertEquals(true, testString1.ContainsAnyLetters);
			AssertEquals(true, testString2.ContainsAnyLetters);
			AssertEquals(false, testString3.ContainsAnyLetters);
		}

		public void TestTypedConstructorWithNull()
		{
			string x = null;
			ZString zString = new ZString(x);
			AssertEquals(string.Empty, (string)zString);
		}

		public void TestTypedConstructorWithNull2()
		{
			ZString zString = new ZString(null);
			AssertEquals(string.Empty, (string)zString);
		}

		ZString AllWesternEuropeanCharacters
		{
			get
			{
				if (fAllWesternEuropeanCharacters.IsEmpty)
				{
					StringBuilder result = new StringBuilder(127);
					for (int i = 9; i <= 13; i++)
					{
						result.Append((char)i);
					}

					for (int i = 32; i <= 127; i++)
					{
						result.Append((char)i);
					}

					byte[] westernEuroBytes = new byte[128];
					for (byte b = 0; b < westernEuroBytes.Length; b++)
					{
						westernEuroBytes[b] = (byte)(b + 128);
					}

					char[] westernEuroChars = Encoding.GetEncoding(1252).GetChars(westernEuroBytes);
					result.Append(westernEuroChars);

					fAllWesternEuropeanCharacters = result.ToString();
				}

				return fAllWesternEuropeanCharacters;
			}
		}

		ZString AllNonWesternEuropeanCharacters
		{
			get
			{
				if (fAllNonAsciiCharacters.IsEmpty)
				{
					int unicodeMax = char.MaxValue;
					StringBuilder result = new StringBuilder(unicodeMax - 256);

					for (int i = 128; i <= unicodeMax; i++)
					{
						char c = (char)i;
						if (!AllWesternEuropeanCharacters.Contains(c))
						{
							result.Append(c);
						}
					}

					fAllNonAsciiCharacters = result.ToString();
				}

				return fAllNonAsciiCharacters;
			}
		}

		ZString fAllWesternEuropeanCharacters;
		ZString fAllNonAsciiCharacters;

		#endregion

		#region Implementation

		protected void AssertSplit(string message, ZString[] expected, ZString[] actual)
		{
			AssertEquals(message + ": length", expected.Length, actual.Length);

			for (int i = 0; i < expected.Length; i++)
			{
				AssertEquals(message + ": index " + i, expected[i], actual[i]);
			}
		}

		#endregion

		#region IZTypeTest Overrides

		protected override IZType NewZ(object value)
		{
			return new ZString(value);
		}

		protected override object[] ValidValues
		{
			get { return new object[] { "_", "''", "0", "\"", "A string with some interesting text", new ZString("_") }; }
		}

		protected override object[] EmptyValues
		{
			get { return new object[] { null, DBNull.Value, ZString.Empty, new ZString(), "" }; }
		}

		protected override object[] UnsupportedValues
		{
			get { return new object[] { new object(), 3, "x".ToCharArray() }; }
		}

		#endregion
	}
}
