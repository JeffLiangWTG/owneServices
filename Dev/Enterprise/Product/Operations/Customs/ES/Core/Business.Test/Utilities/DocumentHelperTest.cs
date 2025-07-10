using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class DocumentHelperTest : TestCaseWithFactory
	{
		public void TestIsSameDocumentReference()
		{
			var supDoc = Factory.New<CusSupportingInfo>();
			supDoc.CSI_ReferenceNumber = "Invoice";
			CombineAssertions(() =>
			{
				AssertEquals("False if it is not the same", false, DocumentHelper.IsSameDocumentReference(supDoc, "Transport"));
				AssertEquals("True if it is the same", true, DocumentHelper.IsSameDocumentReference(supDoc, "Invoice"));
			});
		}

		public void TestGetDocumentByTransportType()
		{
			CombineAssertions(() =>
			{
				AssertEquals("When transport type is Empty", ZString.Empty, DocumentHelper.GetDocumentByTransportType(ZString.Empty));
				AssertEquals("When transport type is OWN", ZString.Empty, DocumentHelper.GetDocumentByTransportType(TransportModes.OwnPropulsion));
				AssertEquals("When transport type is SEA", "N705", DocumentHelper.GetDocumentByTransportType(TransportModes.Sea));
				AssertEquals("When transport type is RAI", "N720", DocumentHelper.GetDocumentByTransportType(TransportModes.Rail));
				AssertEquals("When transport type is ROA", "N730", DocumentHelper.GetDocumentByTransportType(TransportModes.Road));
				AssertEquals("When transport type is AIR", "N740", DocumentHelper.GetDocumentByTransportType(TransportModes.Air));
			});
		}

		public void TestGetDsdtMRNNumberFormat()
		{
			AssertEquals("GetDsdtMRNNumberFormat return formatted dsdtMRN (with 18 chars)", "99984876543", DocumentHelper.GetDsdtMRNNumberFormat("24ES00999898765432"));
			AssertEquals("GetDsdtMRNNumberFormat return formatted dsdtMRN (with more than 18 chars)", "95983812543", DocumentHelper.GetDsdtMRNNumberFormat("23ES00959898125432123"));
		}
	}
}
