using System;
using System.Collections.Generic;
using Enterprise.Metadata.Integration;
using NUnit.Framework;

namespace Enterprise.Metadata.Business.Tests
{
	public class MetadataFactoryTest : TestCase
	{
		public void TestMetadatas()
		{
			var expected = new Dictionary<MetadataContext, Type>();
			expected.Add(MetadataContext.CommonShipment, typeof(CommonShipment));
			expected.Add(MetadataContext.CommonConsol, typeof(CommonConsol));
			expected.Add(MetadataContext.CommonCartage, typeof(CommonCartage));
			expected.Add(MetadataContext.BaseJobDeclaration, typeof(BaseJobDeclaration));
			expected.Add(MetadataContext.AUJobDeclaration, typeof(AUJobDeclaration));
			expected.Add(MetadataContext.BaseJobComInvoiceHeader, typeof(BaseJobComInvoiceHeader));
			expected.Add(MetadataContext.Order, typeof(Order));
			expected.Add(MetadataContext.ForwardingConsol, typeof(ForwardingConsol));
			expected.Add(MetadataContext.ForwardingShipment, typeof(ForwardingShipment));
			expected.Add(MetadataContext.EnterpriseBusinessObject, typeof(EnterpriseBusinessObject));
			expected.Add(MetadataContext.DummyEnterpriseBusinessObject, typeof(DummyEnterpriseBusinessObject));
			expected.Add(MetadataContext.DummyBO, typeof(DummyBO));
			expected.Add(MetadataContext.DummyBOChild, typeof(DummyBOChild));
			expected.Add(MetadataContext.DummyLinkedBO, typeof(DummyLinkedBO));
			expected.Add(MetadataContext.CAJobDeclaration, typeof(CAJobDeclaration));
			expected.Add(MetadataContext.USJobDeclaration, typeof(USJobDeclaration));
			expected.Add(MetadataContext.USCusInBondHeader, typeof(USCusInBondHeader));
			expected.Add(MetadataContext.AgencyShipment, typeof(AgencyShipment));
			expected.Add(MetadataContext.BillOfLading, typeof(BillOfLading));
			expected.Add(MetadataContext.GatePassShipment, typeof(GatePassShipment));
			expected.Add(MetadataContext.WhsDocket, typeof(WhsDocket));
			expected.Add(MetadataContext.WhsTransfer, typeof(WhsTransfer));
			expected.Add(MetadataContext.WhsWorkOrder, typeof(WhsWorkOrder));
			expected.Add(MetadataContext.WhsReceive, typeof(WhsReceive));
			expected.Add(MetadataContext.WhsOrder, typeof(WhsOrder));
			expected.Add(MetadataContext.NZCusMAWB, typeof(NZCusMAWB));
			expected.Add(MetadataContext.DtbTransport, typeof(DtbTransport));
			expected.Add(MetadataContext.LandTransportConsignment, typeof(DtbConsignment));
			expected.Add(MetadataContext.EMCSCusContainer, typeof(EMCSCusContainer));
			expected.Add(MetadataContext.DtbBookingConsolidation, typeof(DtbBookingConsolidation));
			expected.Add(MetadataContext.USCusISFHeader, typeof(USCusISFHeader));
			expected.Add(MetadataContext.AUCusHAWB, typeof(AUCusHAWB));
			expected.Add(MetadataContext.CAJobComInvoiceLine, typeof(CAJobComInvoiceLine));
			expected.Add(MetadataContext.ZACusEntryHeader, typeof(ZACusEntryHeader));
			expected.Add(MetadataContext.Project, typeof(Project));
			expected.Add(MetadataContext.NZCusSCAOceanBill, typeof(NZCusSCAOceanBill));
			expected.Add(MetadataContext.QuotedBooking, typeof(QuotedBooking));
			expected.Add(MetadataContext.Quote, typeof(Quote));
			expected.Add(MetadataContext.DENctsHeader, typeof(DENctsHeader));
			expected.Add(MetadataContext.JobSupplierBooking, typeof(JobSupplierBooking));
			expected.Add(MetadataContext.CommonContainerLoadList, typeof(CommonContainerLoadList));
			var actual = MetadataFactory.Metadatas;
			new TestHelper().AssertDictionaryEqualsByElements(expected, actual);
		}
	}
}
