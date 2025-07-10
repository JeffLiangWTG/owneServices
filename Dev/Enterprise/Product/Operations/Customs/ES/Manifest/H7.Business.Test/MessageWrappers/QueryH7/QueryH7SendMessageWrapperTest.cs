using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	[TestedType(typeof(QueryH7SendMessageWrapper))]
	public class QueryH7SendMessageWrapperTest : H7CommonSendMessageWrapperBaseTest<QueryH7SendMessageWrapper>
	{ 
		public void TestDeclarationMRN()
		{
			AssertEquals("Expected filled DeclarationMRN", bill.H7MovementReferenceNumber, Provider.DeclarationMRN);
		}

		public void TestG3DeclarationMRN()
		{
			AssertEquals("Expected empty G3DeclarationMRN", ZString.Empty, Provider.G3DeclarationMRN);
		}

		public void TestNextH7DeclarationMRN()
		{
			AssertEquals("Expected empty NextH7DeclarationMRN", ZString.Empty, Provider.NextH7DeclarationMRN);
		}

		protected override void SetUp()
		{
			base.SetUp();

			bill.H7MovementReferenceNumber = "test";
		}

		protected override QueryH7SendMessageWrapper GetWrapperCore(AsycudaBill bill, ICertificateProvider certificate)
		{
			return new QueryH7SendMessageWrapper(bill, certificate);
		}
	}
}
