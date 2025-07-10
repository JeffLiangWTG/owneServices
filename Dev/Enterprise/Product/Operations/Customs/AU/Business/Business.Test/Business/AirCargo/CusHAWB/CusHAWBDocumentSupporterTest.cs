using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusHAWBDocumentSupporter))]
	sealed class CusHAWBDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestGetBODocDataProvidersNotFoundMessage()
		{
			AssertEquals("This shipment is not associated with a HAWB data.", DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.CusHAWB), null));
		}

		public void TestShowReasonForNotPrinting()
		{
			AssertEquals(true, DocumentSupporter.ShowReasonForNotPrinting(Enterprise.Core.Constants.DataContext.CusHAWB, null));
		}

		public void TestSupportedDataContexts()
		{
			AssertEquals("Core.Constants.DataContext.CusHAWB is Supported", true, DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.CusHAWB)));
		}

		public void TestBusinessContext()
		{
			AssertEquals("BusinessContext=CusHAWB", BusinessContext.CusHAWB, DocumentSupporter.BusinessContext);
		}

		public void TestGetDocBusinessObjects()
		{
			DocumentWrapper[] wrappers = DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CusHAWB, null);
			AssertEquals("1 document wrapper should be returned", 1, wrappers.Length);
			AssertEquals("Correct document wrapper should be returned", "DocCusHAWB", wrappers[0].GetType().Name);
		}

		public void TestGetContactOrganisation()
		{
			CusHAWBBO.CS_OA_ConsigneeAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			CusHAWBBO.CS_OA_ConsignorAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;

			var consigneeContact = DocumentSupporter.GetContactOrganisation("xxx", ContactType.Consignee, DocumentDirection.ANY);
			AssertEquals("Consignee contact should be CusHAWB.Consignee", CusHAWBBO.Consignee.PK, consigneeContact.OrgHeader.PK);
			var consignorContact = DocumentSupporter.GetContactOrganisation("xxx", ContactType.Consignor, DocumentDirection.ANY);
			AssertEquals("Consignor contact should be CusHAWB.Consignor", CusHAWBBO.Consignor.PK, consignorContact.OrgHeader.PK);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject() => Factory.New<CusHAWB>();

		CusHAWBDocumentSupporter DocumentSupporter
		{
			get
			{
				if (fDocumentSupporter == null)
				{
					IDocumentSupportable docSupportable = CusHAWBBO;
					fDocumentSupporter = (CusHAWBDocumentSupporter)docSupportable.DocumentSupporter;
				}
				return fDocumentSupporter;
			}
		}
		CusHAWBDocumentSupporter fDocumentSupporter;

		CusHAWB CusHAWBBO
		{
			get
			{
				if (fCusHAWBBO == null)
				{
					fCusHAWBBO = Factory.New<CusHAWB>();
				}
				return fCusHAWBBO;
			}
		}
		CusHAWB fCusHAWBBO;
	}
}
