using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.CIN.Testing
{
	sealed public class CINHeaderEnvelopeWrapperTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			AssertEquals("SchemaID is 750", "750", cINHeaderEnvelopWrapper.SchemaID);
			AssertEquals("SchemaVersion is xml", "XML", cINHeaderEnvelopWrapper.SchemaVersion);
			AssertEquals("PartnerId is empty", ZString.Empty, cINHeaderEnvelopWrapper.PartnerId);
			AssertEquals("TransactionId is populated by a numberFountain from 0000000001", "0000000001", cINHeaderEnvelopWrapper.TransactionId);
			AssertEquals("NumSeq is 0", (ZShort)0, cINHeaderEnvelopWrapper.NumSeq);
		}

		protected override void SetUp()
		{
			base.SetUp();
			jobHeader = CusTempStorageJobHeader.New(Factory);
			jobHeader.FillWithValidTestData();
			cINHeaderEnvelopWrapper = new CINHeaderEnvelopeWrapper(jobHeader);
			Factory.Save();
		}
		CusTempStorageJobHeader jobHeader;
		CINHeaderEnvelopeWrapper cINHeaderEnvelopWrapper;
	}
}
