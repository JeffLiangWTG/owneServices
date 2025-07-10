using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class CFRCollectionSummaryWriterTest : TestCaseWithFactory
	{
		public void TestGetSummary_StatementOfApproval()
		{
			var cfrSubstance = Factory.New<UNDGSubstanceCFR>();
			cfrSubstance.CFR_UNNO = "1234";

			var undgDataItem = Factory.New<ForwardingUNDGDataItem>();
			undgDataItem.DI_DG = cfrSubstance.PK;
			undgDataItem.LinkDefault(cfrSubstance);

			Factory.Save();

			var wrappers = new List<UNDGSubstanceWrapper>
			{
				new UNDGSubstanceWrapper(undgDataItem, Factory)
			};

			var writer = new CFRCollectionSummaryWriter() as IUNDGSubstanceCollectionSummaryWriter;
			var summary = writer.GetSummary(wrappers);

			AssertContains("Statement is included", "This is to certify that the above-named/herein-named materials are properly", summary);
		}

		public void TestGetSummary_ShipmentAdditionalHandlingInformation()
		{
			var cfrSubstance = Factory.New<UNDGSubstanceCFR>();
			cfrSubstance.CFR_UNNO = "1234";

			var shipment = Factory.New<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			var undgDataItem = packline.UNDGs.AddNew();
			undgDataItem.DI_DG = cfrSubstance.PK;
			undgDataItem.LinkDefault(cfrSubstance);

			var dangerousGoodsDescription = shipment.Notes.AddNew();
			dangerousGoodsDescription.ST_Description = PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description;
			dangerousGoodsDescription.ST_NoteText = "Hey it's some stuff";

			Factory.Save();

			var wrappers = new List<UNDGSubstanceWrapper>
			{
				new UNDGSubstanceWrapper(undgDataItem, Factory)
			};

			var writer = new CFRCollectionSummaryWriter() as IUNDGSubstanceCollectionSummaryWriter;
			var summary = writer.GetSummary(wrappers);

			AssertContains("DG handling information is included", "Hey it's some stuff", summary);
		}
	}
}
