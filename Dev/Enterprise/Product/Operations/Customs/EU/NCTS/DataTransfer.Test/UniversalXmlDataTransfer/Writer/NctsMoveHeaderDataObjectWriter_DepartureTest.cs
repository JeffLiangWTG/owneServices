using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.EU.NCTS.DataTransfer.Phase4.Testing
{
	class NctsMoveHeaderDataObjectWriter_DepartureTest : TestCaseWithFactory
	{
		public void TestCountryOfDispatch()
		{
			var dataObject = SetupData(setupAdditionalHeaderData: (header) =>
			{
				((INeedRow)header).Row[CusInBondHeader.Schema.BH_RL_NKImportLoadPort] = "AUSYD"; // Need to bypass max length
			});
			AssertEquals("dataObject.PortOfOrigin.Code", "AUSYD", dataObject.PortOfOrigin.Code);
			AssertNull("dataObject.PortOfOrigin.Name", dataObject.PortOfOrigin.Name);
		}

		public void TestCountryOfDestination()
		{
			var dataObject = SetupData(setupAdditionalMovementHeaderData: (movementHeader) =>
			{
				((INeedRow)movementHeader).Row[CusInBondMoveHeader.Schema.BM_RL_NKDestinationPort] = "AUSYD"; // Need to bypass max length
			});
			AssertEquals("dataObject.PortOfDestination.Code", "AUSYD", dataObject.PortOfDestination.Code);
			AssertNull("dataObject.PortOfDestination.Name", dataObject.PortOfDestination.Name);
		}

		public void TestPopulateDataObject_Departure()
		{
			var dataObject = SetupData();
			AssertEquals(Common.EU.NctsMoveHeaderType.Codes.Departure, dataObject.CommercialInfo.CommercialInvoiceCollection[0].RelatedIndicator.Code);
		}

		Shipment SetupData(Action<NctsHeader> setupAdditionalHeaderData = null, Action<NctsDepartureMovementHeader> setupAdditionalMovementHeaderData = null)
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				CommercialInfo = new CommercialInfo() { CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>() }
			};
			shipment.SetAddInfoCollection(() => new List<AddInfo>());

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			setupAdditionalHeaderData?.Invoke(header);
			var movementHeader = header.MovementHeader;
			setupAdditionalMovementHeaderData?.Invoke(movementHeader);
			var helper = new UniversalDataObjectWriterHelper(Factory, Core.Constants.CountryCodes.Germany);
			var manager = new DataWritingManager(new ActionInfo(RecipientRoleType.ACT, movementHeader));
			var writer = new NctsMoveHeaderDataObjectWriter(manager, helper, shipment);

			return writer.GetDataObject(movementHeader);
		}
	}
}
