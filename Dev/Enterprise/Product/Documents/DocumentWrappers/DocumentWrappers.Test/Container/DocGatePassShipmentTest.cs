using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Container
{
	[TestedType(typeof(DocGatePassShipment))]
	sealed class DocGatePassShipmentTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocGatePassShipment.New(Shipment, Factory), DocGatePassShipment.New(Factory, Shipment.PK) };
		}

		protected override void SetUp()
		{
			Shipment = Factory.New<GatePassShipment>();
			GatePass = DocGatePassShipment.New(Shipment, Factory);
			base.SetUp();
		}

		public void TestGatePassNotes()
		{
			StmNote agentNotes = AddNotes(Shipment, PredefinedNoteTypes.Instance.GatePassNotes.Description, "Gate Pass Notes Stuff\nLine Two");
			StmNote otherNotes = AddNotes(Shipment, PredefinedNoteTypes.Instance.InternalWorkNotes.Description, "This is other notes and should not be included.");

			AssertEquals("Detailed goods description", "Gate Pass Notes Stuff\nLine Two", GatePass.GatePassNotes);
		}

		DocGatePassShipment GatePass;
		GatePassShipment Shipment;
	}
}
