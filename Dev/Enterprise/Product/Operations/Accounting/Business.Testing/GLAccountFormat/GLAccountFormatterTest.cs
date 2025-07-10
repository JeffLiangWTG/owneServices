using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GLAccountFormat.Testing
{
	[TestedType(typeof(GLAccountFormatter))]
	public class GLAccountFormatterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestUpdate()
		{
			AccGLHeader header = Factory.NewWithValidTestData<AccGLHeader>();
			header.AG_AccountNum = "ABCDEFGH";
			Factory.Save();
			formatter.Update("XX.XXXX.XX");
			AssertEquals("AccGLHeader Value", "AB.CDEF.GH", header.AG_AccountNum);
			AssertEquals("Registry Value", "XX.XXXX.XX", AccountingConfigurationRegistry.Instance.GLAccountFormat.Value);
		}

		public void TestValidateGLAccountNumLength()
		{
			AccGLHeader header = Factory.NewWithValidTestData<AccGLHeader>();

			formatter.NewFormat = "XX.XXXX.XX";

			header.AG_AccountNum = "A.BCDE.F.G";
			Factory.Save();
			formatter.NewFormatInfo.AddError("XXX");
			Assert("Precondition", formatter.HasErrors());
			Assert("Precondition", header.AG_AccountNum.ToString().Count(c => c != '.') < formatter.NewFormat.ToString().Count(c => c == 'X'));
			formatter.ValidateGLAccountNumLength();
			AssertNoErrors(formatter.CurrentFormatInfo);

			formatter.NewFormatInfo.ClearAllNotifications();
			AssertNoErrors("Precondition", formatter);
			Assert("Precondition", header.AG_AccountNum.ToString().Count(c => c != '.') < formatter.NewFormat.ToString().Count(c => c == 'X'));
			formatter.ValidateGLAccountNumLength();
			AssertHasError(formatter.CurrentFormatInfo, $@"The current GL Account format is {formatter.CurrentFormat}.
The following GL accounts do not comply with this format:
A.BCDE.F.G
… …
Please review and ensure that all GL Accounts under Maintain > Account > GL Accounts comply to the current GL Account Format before changing to the new GL Account Format.");

			header.AG_AccountNum = "ABCD.EF.GH";
			Factory.Save();
			Assert("Precondition", header.AG_AccountNum.ToString().Count(c => c != '.') == formatter.NewFormat.ToString().Count(c => c == 'X'));
			formatter.ValidateGLAccountNumLength();
			AssertNoErrors(formatter.CurrentFormatInfo);

			header.AG_AccountNum = "ABCDEFGHIJ";
			Factory.Save();
			Assert("Precondition", header.AG_AccountNum.ToString().Count(c => c != '.') > formatter.NewFormat.ToString().Count(c => c == 'X'));
			formatter.ValidateGLAccountNumLength();
			AssertNoErrors(formatter.CurrentFormatInfo);

			AccGLHeader header1 = Factory.NewWithValidTestData<AccGLHeader>();
			header1.AG_AccountNum = "A.BCDE.F.G";
			AccGLHeader header2 = Factory.NewWithValidTestData<AccGLHeader>();
			header2.AG_AccountNum = "A.BCDE.F.H";
			AccGLHeader header3 = Factory.NewWithValidTestData<AccGLHeader>();
			header3.AG_AccountNum = "A.BCDE.F.I";
			AccGLHeader header4 = Factory.NewWithValidTestData<AccGLHeader>();
			header4.AG_AccountNum = "ABCD.EF.GI";
			AccGLHeader header5 = Factory.NewWithValidTestData<AccGLHeader>();
			header5.AG_AccountNum = "ABCD.EF.GJ";
			AccGLHeader header6 = Factory.NewWithValidTestData<AccGLHeader>();
			header6.AG_AccountNum = "ABCD.EF.GK";
			Factory.Save();
			Assert("Precondition", header1.AG_AccountNum.ToString().Count(c => c != '.') < formatter.NewFormat.ToString().Count(c => c == 'X'));
			Assert("Precondition", header2.AG_AccountNum.ToString().Count(c => c != '.') < formatter.NewFormat.ToString().Count(c => c == 'X'));
			Assert("Precondition", header3.AG_AccountNum.ToString().Count(c => c != '.') < formatter.NewFormat.ToString().Count(c => c == 'X'));
			Assert("Precondition", header4.AG_AccountNum.ToString().Count(c => c != '.') == formatter.NewFormat.ToString().Count(c => c == 'X'));
			Assert("Precondition", header5.AG_AccountNum.ToString().Count(c => c != '.') == formatter.NewFormat.ToString().Count(c => c == 'X'));
			Assert("Precondition", header6.AG_AccountNum.ToString().Count(c => c != '.') == formatter.NewFormat.ToString().Count(c => c == 'X'));
			formatter.ValidateGLAccountNumLength();
			AssertHasError(formatter.CurrentFormatInfo, $@"The current GL Account format is {formatter.CurrentFormat}.
The following GL accounts do not comply with this format:
A.BCDE.F.G
A.BCDE.F.H
A.BCDE.F.I
… …
Please review and ensure that all GL Accounts under Maintain > Account > GL Accounts comply to the current GL Account Format before changing to the new GL Account Format.");

			header4.AG_AccountNum = "ABCD.EF.I";
			header5.AG_AccountNum = "ABCD.EF.J";
			header6.AG_AccountNum = "ABCD.EF.K";
			Factory.Save();
			Assert("Precondition", header1.AG_AccountNum.ToString().Count(c => c != '.') < formatter.NewFormat.ToString().Count(c => c == 'X'));
			Assert("Precondition", header2.AG_AccountNum.ToString().Count(c => c != '.') < formatter.NewFormat.ToString().Count(c => c == 'X'));
			Assert("Precondition", header3.AG_AccountNum.ToString().Count(c => c != '.') < formatter.NewFormat.ToString().Count(c => c == 'X'));
			Assert("Precondition", header4.AG_AccountNum.ToString().Count(c => c != '.') < formatter.NewFormat.ToString().Count(c => c == 'X'));
			Assert("Precondition", header5.AG_AccountNum.ToString().Count(c => c != '.') < formatter.NewFormat.ToString().Count(c => c == 'X'));
			Assert("Precondition", header6.AG_AccountNum.ToString().Count(c => c != '.') < formatter.NewFormat.ToString().Count(c => c == 'X'));
			formatter.ValidateGLAccountNumLength();

			AssertNotEquals(6, formatter.NotMatchNumberList_ForTestOnly.Count());
			AssertEquals(3, formatter.NotMatchNumberList_ForTestOnly.Count());
		}

		public void TestValidateNewFormat_WithNoCurrentValue()
		{
			formatter.CurrentFormat = "";
			AssertNoErrors("Precondition: There should not be any errors.", formatter.NewFormatInfo);
			formatter.NewFormat = "X";
			AssertEquals("NewFormatInfo.GetErrors().Count()", 1, formatter.NewFormatInfo.GetErrors().Count());
			AssertHasError(formatter.NewFormatInfo, "The GL format string must have 1 or 2 dots in the string.");
			TestValidateNewFormat();
		}

		public void TestValidateNewFormat_WithCurrentValue()
		{
			formatter.CurrentFormat = "XX.X";
			AssertNoErrors("Precondition: There should not be any errors.", formatter.NewFormatInfo);
			formatter.NewFormat = "X";
			AssertEquals("NewFormatInfo.GetErrors().Count()", 2, formatter.NewFormatInfo.GetErrors().Count());
			AssertHasError(formatter.NewFormatInfo, "Cannot decrease the amount of X in the string.");
			AssertHasError(formatter.NewFormatInfo, "The GL format string must have 1 or 2 dots in the string.");
			TestValidateNewFormat();
			formatter.CurrentFormat = "XX.XX.X";
			formatter.NewFormat = "XX.XXX";
			AssertEquals("NewFormatInfo.GetErrors().Count()", 1, formatter.NewFormatInfo.GetErrors().Count());
			AssertHasError(formatter.NewFormatInfo, "The GL format string must have 2 dots in the string.");
			formatter.NewFormat = "XX.X.X.X";
			AssertEquals("NewFormatInfo.GetErrors().Count()", 1, formatter.NewFormatInfo.GetErrors().Count());
			AssertHasError(formatter.NewFormatInfo, "The GL format string must have 2 dots in the string.");
			formatter.NewFormat = "XX.X.XX";
			AssertNoErrors(formatter.NewFormatInfo);
		}

		public void TestCurrentFormat()
		{
			AssertEquals("CurrentFormat", AccGLHeader.CurrentGLAccountFormat, new GLAccountFormatter().CurrentFormat);
		}

		public void TestNewFormat()
		{
			formatter.NewFormat = "abcdXXX.X";
			AssertEquals("NewFormat", "abcdXXX.X", formatter.NewFormat);
		}

		public void TestGLAccountFormatterHaasFactory()
		{
			AssertNotNull("Pre-condition: Formatter", formatter);
			AssertNotNull("Formatter Factory", formatter.Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			formatter = new MockGLAccountFormatter();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new GLAccountFormatter();
		}

		void TestValidateNewFormat()
		{
			formatter.NewFormat = "1234567890";
			AssertEquals("NewFormatInfo.GetErrors().Count()", 3, formatter.NewFormatInfo.GetErrors().Count());
			AssertHasError(formatter.NewFormatInfo, "Only X and . are valid symbols.");
			AssertHasError(formatter.NewFormatInfo, "The GL format string should start and end with X.");
			AssertHasError(formatter.NewFormatInfo, "The GL format string must have 1 or 2 dots in the string.");
			formatter.NewFormat = "X.X.X.X";
			AssertEquals("NewFormatInfo.GetErrors().Count()", 1, formatter.NewFormatInfo.GetErrors().Count());
			AssertHasError(formatter.NewFormatInfo, "The GL format string must have 1 or 2 dots in the string.");
			formatter.NewFormat = ".X.XX";
			AssertEquals("NewFormatInfo.GetErrors().Count()", 1, formatter.NewFormatInfo.GetErrors().Count());
			AssertHasError(formatter.NewFormatInfo, "The GL format string should start and end with X.");
			formatter.NewFormat = "X.X.X.";
			AssertEquals("NewFormatInfo.GetErrors().Count()", 2, formatter.NewFormatInfo.GetErrors().Count());
			AssertHasError(formatter.NewFormatInfo, "The GL format string should start and end with X.");
			AssertHasError(formatter.NewFormatInfo, "The GL format string must have 1 or 2 dots in the string.");
			formatter.NewFormat = "XXX..XXX";
			AssertEquals("NewFormatInfo.GetErrors().Count()", 1, formatter.NewFormatInfo.GetErrors().Count());
			AssertHasError(formatter.NewFormatInfo, "Two dots next to each other are not allowed.");
			formatter.NewFormat = "X.XX";
			AssertNoErrors(formatter.NewFormatInfo);
			formatter.NewFormat = "X.X.X";
			AssertNoErrors(formatter.NewFormatInfo);
		}

		MockGLAccountFormatter formatter;
		class MockGLAccountFormatter : GLAccountFormatter
		{
			public new ZString CurrentFormat
			{
				get
				{
					return base.CurrentFormat;
				}

				set
				{
					currentFormat = value;
				}
			}

			protected override ZString CurrentFormatCore
			{
				get
				{
					return currentFormat;
				}
			}

			string currentFormat;
		}
	}
}
