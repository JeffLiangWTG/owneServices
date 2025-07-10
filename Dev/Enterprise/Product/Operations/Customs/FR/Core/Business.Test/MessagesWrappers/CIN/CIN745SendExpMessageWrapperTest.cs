using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MessagesWrappers.Testing;
using Enterprise.Customs.FR.Messaging.Interfaces.CIN;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.CIN.Testing
{
	public class CIN745SendExpMessageWrapperTest : TestCaseWithFactory
	{
		[TestDate(2020, 12, 31, 12, 00, 00)]
		public void TestCIN745MessageEnvelopProperties()
		{
			var wrapper = new CIN745NestedEnvelopeWrapper(entryHeaderItem);

			AssertEquals(CIN745NestedEnvelopeWrapper.oaciCode, wrapper.OACI);
			ICINMessage745 iCusEntryHeader = entryHeaderItem;
			AssertEquals(iCusEntryHeader.BGMReference, wrapper.REFERENCE);

			var testMR = (CusEntryNumber.LoadOrCreate(entryHeaderItem, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.France)).CE_EntryNum;
			AssertEquals("", wrapper.MRN_ECS);

			AssertEquals(WrapperTestHelper.OfficeOfExit, wrapper.BUR_DOUANE);
			AssertEquals(CIN745NestedEnvelopeWrapper.oaciDest, wrapper.DEST_OACI);

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
