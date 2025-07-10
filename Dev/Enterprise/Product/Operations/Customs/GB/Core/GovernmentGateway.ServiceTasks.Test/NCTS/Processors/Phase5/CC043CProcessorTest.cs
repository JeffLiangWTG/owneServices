using System.Collections.Generic;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.cc043c;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.Processors.Phase5.Testing
{
	sealed class CC043CProcessorTest : NctsBaseProcessorTest<Cc043CType>
	{
		protected override IEnumerable<MessageProcessorTestCase> ProcessorTestCases
		{
			get
			{
				yield return new CC043CProcessorTestCase(responseHelper.GetEmbeddedResourceFile("CC043C_Message.xml"))
				{
					SetUpHeader = header =>
					{
						header.BH_HeaderType = NctsMovementType.Codes.Arrival;
						var container1 = header.ArrivalHeaderContainers.AddNew();
						container1.BC_ContainerNum = "1111";
						container1.BC_Seal1 = "SEAL1";
						var container2 = header.ArrivalHeaderContainers.AddNew();
						container2.BC_ContainerNum = "2222";
						container2.BC_Seal1 = "SEAL2";
					},
					HeaderAssertion = x =>
					{
						AssertEquals("23XI000081JYRJUVJ9", x.MovementReferenceEntryNumber.CE_EntryNum);

						var arrivalHeader = x.ArrivalMovementHeader;
						AssertEquals("T1", arrivalHeader.BM_InBondEntryType);
						AssertEquals(new ZDateTime(2023, 8, 1), arrivalHeader.BM_EntryDate);
						AssertEquals(NctsTypeOfSecurityList.Codes.NON, arrivalHeader.BM_TypeOfSecurity);
						AssertEquals(false, arrivalHeader.BM_ReducedDatasetIndicator);
						AssertEquals("GB", arrivalHeader.BM_RL_NKDestinationPort);
						AssertEquals(1000m, arrivalHeader.BM_GrossWeight);
						AssertEquals("KG", arrivalHeader.BM_GrossWeightUQ);
						AssertEquals((short)3, arrivalHeader.BM_SealQty);

						var goodsItem1 = x.Bills[0].ArrivalGoodsItems[0];
						AssertEquals("39269078", goodsItem1.BY_HarmonisedTariff);

						var containers = x.ArrivalHeaderContainers;
						AssertEquals(2, containers.Count);
						AssertContainerAndSeals(containers[0], "WGPCGR", NctsUnloadedStateList.Codes.DEC, Core.Constants.ContainerModes.Containerised, 1, "1234", "2345");
						AssertContainerAndSeals(containers[1], ZString.Empty, NctsUnloadedStateList.Codes.DEC, Core.Constants.ContainerModes.NonContainerised, 2, "3456");
					},
					ExpectedMovementType = NctsMovementType.Codes.Arrival,
					ExpectedAcceptMovementType = NctsMovementType.Codes.Arrival,
				};

				yield return new CC043CProcessorTestCase(responseHelper.GetEmbeddedResourceFile("CC043C_Message.xml"))
				{
					HeaderAssertion = x =>
					{
						AssertEquals(1, x.Bills?.Count);
						AssertNotNull(x.Bills[0].Consignee);
						AssertNotNull(x.Bills[0].Consignor);

						var actualConsignor = x.Bills[0].Consignor;
						AssertEquals("XI175521246821", actualConsignor.E2_GovRegNum);
						AssertEquals("1 St", actualConsignor.E2_Address1);
						AssertEquals("12345", actualConsignor.E2_Postcode);
						AssertEquals("City1", actualConsignor.E2_City);
						AssertEquals("AU", actualConsignor.E2_RN_NKCountryCode);
						Assert("Consignor address validation should be suppressed", actualConsignor.E2_SuppressAddressValidationError);

						var actualConsignee = x.Bills[0].Consignee;
						AssertEquals("GB953574106000", actualConsignee.E2_GovRegNum);
						AssertEquals("2 St", actualConsignee.E2_Address1);
						AssertEquals("AB1 2CD", actualConsignee.E2_Postcode);
						AssertEquals("City2", actualConsignee.E2_City);
						AssertEquals("GB", actualConsignee.E2_RN_NKCountryCode);
						Assert("Consignee address validation should be suppressed", actualConsignee.E2_SuppressAddressValidationError);
					},
					ExpectedMovementType = NctsMovementType.Codes.Arrival,
					ExpectedAcceptMovementType = NctsMovementType.Codes.Arrival,
				};

				yield return new CC043CProcessorTestCase(responseHelper.GetEmbeddedResourceFile("CC043C_Message_NoConsignmentPartyAddress.xml"))
				{
					HeaderAssertion = x =>
					{
						AssertEquals(1, x.Bills?.Count);
						AssertNotNull(x.Bills[0].Consignee);
						AssertNotNull(x.Bills[0].Consignor);

						var actualConsignee = x.Bills[0].Consignee;
						AssertEquals(ZString.Empty, actualConsignee.E2_Address1);
						AssertEquals(ZString.Empty, actualConsignee.E2_Postcode);
						AssertEquals(ZString.Empty, actualConsignee.E2_City);

						var actualConsignor = x.Bills[0].Consignor;
						AssertEquals(ZString.Empty, actualConsignor.E2_Address1);
						AssertEquals(ZString.Empty, actualConsignor.E2_Postcode);
						AssertEquals(ZString.Empty, actualConsignor.E2_City);
					},
					ExpectedMovementType = NctsMovementType.Codes.Arrival,
					ExpectedAcceptMovementType = NctsMovementType.Codes.Arrival,
				};

				yield return new CC043CProcessorTestCase(responseHelper.GetEmbeddedResourceFile("CC043C_Message_NoConsignment.xml"))
				{
					HeaderAssertion = x =>
					{
						AssertEquals("Expect no exception", "23XI000081JYRJUVJ9", x.MovementReferenceEntryNumber.CE_EntryNum);
					},
					ExpectedMovementType = NctsMovementType.Codes.Arrival,
					ExpectedAcceptMovementType = NctsMovementType.Codes.Arrival,
				};

				yield return new CC043CProcessorTestCase(responseHelper.GetEmbeddedResourceFile("CC043C_Message_NullElements.xml"))
				{
					HeaderAssertion = x =>
					{
						AssertEquals("Expect no exception", "23XI000081JYRJUVJ9", x.MovementReferenceEntryNumber.CE_EntryNum);
					},
					ExpectedMovementType = NctsMovementType.Codes.Arrival,
					ExpectedAcceptMovementType = NctsMovementType.Codes.Arrival,
				};
			}
		}

		class CC043CProcessorTestCase : MessageProcessorTestCase
		{
			public CC043CProcessorTestCase(ZString messageText)
			{
				MRN = "23XI000081JYRJUVJ9";
				SetUpHeader = header => header.BH_HeaderType = NctsMovementType.Codes.Arrival;
				IncomingMessageText = messageText;
				MessageSubType = "43C";
				ExpectedNewMessageStatus = LogicalStatusList.Codes.Accepted;
				ExpectedNewMessageInterpretation = messageText;
				ExpectedNewArrivalStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
				ExpectedMovementType = NctsMovementType.Codes.Arrival;
			}
		}

		void AssertContainerAndSeals(NctsArrivalHeaderContainer container, ZString containerNum, ZString unloadedState, ZString mode, ZShort containerSequence, params ZString[] seals)
		{
			AssertEquals(containerNum, container.BC_ContainerNum);
			AssertEquals(unloadedState, container.BC_UnloadedState);
			AssertEquals(mode, container.BC_Mode);
			AssertEquals(containerSequence, container.BC_SequenceNumber);
			AssertEquals(seals[0], container.BC_Seal1);
			AssertEquals(seals[0], container.BC_Seal2);

			AssertEquals(seals.Length, container.Seals.Count);
			for (var index = 0; index < seals.Length; index++)
			{
				var actualSeal = container.Seals[index];
				AssertEquals(seals[index], actualSeal.BK_SealNumber);
				AssertEquals((short)index + 1, actualSeal.BK_SequenceNumber);
				AssertEquals(unloadedState, actualSeal.BK_UnloadingState);
			}
		}
	}
}
