using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MessagesWrappers.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.CIN.Testing
{
	class CIN755MessageEnvelopWrapperTest : TestCaseWithFactory
	{
		public void TestCIN755MessageEnvelopWrapperConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CIN755NestedEnvelopeWrapper(null));
		}

		[TestDate(2020, 12, 31, 12, 00, 00)]
		public void TestCIN755MessageEnvelopProperties()
		{
			var wrapper = new CIN755NestedEnvelopeWrapper(entryHeaderItem);

			AssertEquals(CIN755NestedEnvelopeWrapper.OACICode, wrapper.OACI);
			AssertEquals("201231#ID_MESSAGE#", wrapper.REFERENCE);

			AssertEquals("", wrapper.MRN_ECS);
			AssertEquals(WrapperTestHelper.OfficeOfExit, wrapper.BUR_DOUANE);
			AssertEquals(CIN755NestedEnvelopeWrapper.OACIDestination, wrapper.DEST_OACI);
			AssertEquals(WrapperTestHelper.CINNumLta, wrapper.NUM_LTA);
			AssertEquals(@"UNB+UNOC:3+EZ1+DGV2+201231:1200+201231#ID_MESSAGE#'UNH+201231#ID_MESSAGE#+755:2'BGM+35+366#ID_MESSAGE#+01'DTM+137:20201231120000:204'RFF+AWB:05783013825'RFF+ACD:05783013825'NAD+GY+PUT-CIN-ID-HERE'NAD+ST+AFR'LIN+1'RFF+REF:0-B00001000'QTY+156:0:COL'GIS+N:117:106'NAD+AM+++++A123456'UNT+13+201231#ID_MESSAGE#'UNZ+1+201231#ID_MESSAGE#'", wrapper.CIN);

			entryHeaderItem.Declaration.UnlockDoMergeMutex();
		}

		protected override void SetUp()
		{
			base.SetUp();
			wrapperHelperItem = new WrapperTestHelper();
			entryHeaderItem = wrapperHelperItem.CreateTestCusEntryHeader(false, true);
		}
		WrapperTestHelper wrapperHelperItem;
		CusEntryHeader entryHeaderItem;
	}
}
