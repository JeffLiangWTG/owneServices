using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Client.EES.Business.Testing
{
	public class EESShipmentDocumentSupporterTest : TestCaseWithFactory
	{
		public void TestGetHBLDocumentTitle()
		{
			ZString menuName = "Bill of Lading";
			Shipment.JS_HouseBillOfLadingType = "bob";
			short expectedNumber = 22;
			Shipment.JS_NoOriginalBills = (ZByte)expectedNumber;
			StmMenuTemplatePivot pivot = Factory.NewWithValidTestData<StmMenuTemplatePivot>();
			pivot.SI_DocumentTitle = "original";
			pivot.SI_PrintCopyType = nameof(Enterprise.ZArchitecture.Core.PrintCopyType.FAX);
			TitleCopyCountPair countPair = DocumentSupporter.GetHBLDocumentTitle(menuName, Shipment, pivot);
			AssertEquals("GetHBLDocumentTitle", (short)1, countPair.CopyCount);
			EESDataRegistry.Instance.NumberOfOriginalBillsToBePrintedOnDotMatrix = 13;
			pivot.SI_PrintCopyType = nameof(Enterprise.ZArchitecture.Core.PrintCopyType.PRN);
			countPair = DocumentSupporter.GetHBLDocumentTitle(menuName, Shipment, pivot);
			AssertEquals("GetHBLDocumentTitle", expectedNumber, countPair.CopyCount);
			expectedNumber = 13;
			Shipment.JS_HouseBillOfLadingType = "EEX";
			pivot.SI_PrintCopyType = nameof(Enterprise.ZArchitecture.Core.PrintCopyType.PRN);
			countPair = DocumentSupporter.GetHBLDocumentTitle(menuName, Shipment, pivot);
			AssertEquals("GetHBLDocumentTitle", expectedNumber, countPair.CopyCount);
		}

#region SetUp
		EESShipmentDocumentSupporterTestClass DocumentSupporter;
		ForwardingShipment Shipment;
		protected override void SetUp()
		{
			base.SetUp();
			Shipment = Factory.New<ForwardingShipment>();
			DocumentSupporter = new EESShipmentDocumentSupporterTestClass(Shipment);
		}

#endregion
#region Inner Test Class
		public class EESShipmentDocumentSupporterTestClass : EESShipmentDocumentSupporter
		{
			public EESShipmentDocumentSupporterTestClass(ForwardingShipment shipment) : base(shipment)
			{
			}

			public new TitleCopyCountPair GetHBLDocumentTitle(ZString parentDocumentMenuName, IDocumentSupportable parentBusinessObject, IStmMenuTemplatePivot pivot)
			{
				return base.GetHBLDocumentTitle(parentDocumentMenuName, parentBusinessObject, pivot);
			}
		}
#endregion
	}
}
