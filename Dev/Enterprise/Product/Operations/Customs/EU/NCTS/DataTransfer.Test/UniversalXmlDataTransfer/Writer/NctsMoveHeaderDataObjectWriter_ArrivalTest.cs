using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.EU.NCTS.DataTransfer.Phase4.Testing
{
	class NctsMoveHeaderDataObjectWriter_ArrivalTest : TestCaseWithFactory
	{
		public void TestPopulateArrivalAddInfosData()
		{
			AssertEquals("Should only populate if arrival", "0", dataObject.AddInfoCollection.First(x => x.Key.Value == "SimplifiedArrivalProcedureFlag").Value);
		}

		public void TestPopulateDataObject()
		{
			AssertEquals("A", dataObject.CommercialInfo.CommercialInvoiceCollection[0].RelatedIndicator.Code);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				CommercialInfo = new CommercialInfo() { CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>() }
			};
			shipment.SetAddInfoCollection(() => new List<AddInfo>());

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);

			movementHeader = nctsHeader.ArrivalMovementHeader;

			var helper = new UniversalDataObjectWriterHelper(Factory, Core.Constants.CountryCodes.Germany);
			var manager = new DataWritingManager(new ActionInfo(RecipientRoleType.ACT, movementHeader));
			writer = new NctsMoveHeaderDataObjectWriter(manager, helper, shipment);

			dataObject = writer.GetDataObject(movementHeader);
		}
		Shipment dataObject;
		NctsHeader nctsHeader;
		NctsCommonMovementHeader movementHeader;
		NctsMoveHeaderDataObjectWriter writer;
	}
}
