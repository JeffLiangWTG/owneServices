using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Presentation;
using Moq;

namespace Enterprise.Customs.FR.Business.Documents.Testing
{
	class CusEntryHeaderFRVisualizableDocumentSupporterTest : TestCaseWithFactory
	{
		public void TestGetDocProviderKey()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var supporter = new CusEntryHeaderFRVisualizableDocumentSupporterForTest(entryHeader);
			var docProviderKey = supporter.GetDocProviderKey();
			AssertEquals("FR", docProviderKey);
		}

		public void TestGetCustomCommands()
		{
			var header = Factory.New<CusEntryHeader>();
			var supporter = new CusEntryHeaderFRVisualizableDocumentSupporter(header);
			var commands = supporter.GetCustomCommands(DataContext.FRPortsCustomsCheckCAED);
			AssertArrayEqualsByElements(new Type[] { typeof(DisabledCommand), typeof(SendNativePortMessageOriginal) }, commands.Select(x => x.GetType()).ToArray());
		}

		public void TestGetMessageLogCreator()
		{
			var document = new Mock<IDocument>();
			document.SetupGet(d => d.DataContext).Returns("FRPortsTrackingRequestTRC");

			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var supporter = new CusEntryHeaderFRVisualizableDocumentSupporter(entryHeader);

			var logCreator = supporter.GetMessageLogCreator(document.Object);
			AssertType<Freight.Forwarding.Documents.DocDataObjects.FR.DemandeDeTracingMessageLogCreator>(logCreator);
		}

		public void TestGetDocDataObjectForFRPortsCustomsCheckCAED()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryInstructions.AddNew();
			var header = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();

			var supporter = new CusEntryHeaderFRVisualizableDocumentSupporter(header);
			var docDataObject = supporter.GetDocDataObject(header, DataContext.FRPortsCustomsCheckCAED, new CustomsDocDataObjectProviderParametersTest());
			AssertNotNull(nameof(docDataObject.Right), docDataObject.Right);
			AssertType<CAEDDataObject>($"{nameof(docDataObject.Right)} type", docDataObject.Right);

			var nctsheader = Factory.New<NctsHeader>();
			var supporterNcts = new NctsHeaderVisualizableDocumentSupporter(nctsheader);
			docDataObject = supporterNcts.GetDocDataObject(Factory.New<NctsHeader>(), DataContext.FRPortsCustomsCheckCAED, new CustomsDocDataObjectProviderParametersTest());
			AssertNotNull(nameof(docDataObject.Right), docDataObject.Right);
			AssertType<CAEDDataObject>($"{nameof(docDataObject.Right)} type", docDataObject.Right);
		}
	}

	class CusEntryHeaderFRVisualizableDocumentSupporterForTest : CusEntryHeaderFRVisualizableDocumentSupporter
	{
		public CusEntryHeaderFRVisualizableDocumentSupporterForTest(CusEntryHeader entryHeader) : base(entryHeader)
		{
		}

		public new string GetDocProviderKey()
		{
			return base.GetDocProviderKey();
		}
	}
}
