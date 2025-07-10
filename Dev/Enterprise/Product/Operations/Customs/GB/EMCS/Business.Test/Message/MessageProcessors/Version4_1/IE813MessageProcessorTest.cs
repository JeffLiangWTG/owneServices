using System;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie813;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.tms;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing.Version4_1
{
	[TestedType(typeof(IE813MessageProcessor))]
	sealed class IE813MessageProcessorTest : IE813MessageProcessorAbstractTest<Ie813Type>
	{
		protected override Ie813Type CreateMissingOrEmptyElementsMessage()
		{
			return ie813 = new Ie813Type
			{
				Header = new HeaderType
				{
					MessageSender = "NDEA.GB",
					MessageRecipient = "NDEA.GB",
					DateOfPreparation = new DateTime(2022, 07, 14),
					TimeOfPreparation = new DateTime(2022, 07, 14, 13, 29, 08, 000),
					MessageIdentifier = "1F157879-4820-4207-B936-91D54E34AC4F"
				},
				Body = new BodyType()
				{
					ChangeOfDestination = new ChangeOfDestinationType()
					{
						UpdateEadEsad = new UpdateEadEsadType()
						{
							SequenceNumber = "8888",
						},
						DestinationChanged = new DestinationChangedType
						{
						},
					},
				},
			};
		}

		protected override Ie813Type CreateDefaultIE813Type()
		{
			return EMCSMessageProcessorTestHelper.GetStandardIE813Type();
		}

		protected override void UpdateTraderIdEmpty()
		{
			ie813.Body.ChangeOfDestination.DestinationChanged.NewConsigneeTrader.Traderid = "";
		}

		protected override void UpdateTraderIdNotMatchImporter()
		{
			ie813.Body.ChangeOfDestination.DestinationChanged.NewConsigneeTrader.Traderid = "DETI123";
		}

		protected override void UpdateTransportMode2Zero()
		{
			ie813.Body.ChangeOfDestination.UpdateEadEsad.TransportModeCode = "0";
		}

		protected override void UpdateTransportMode2Invalid()
		{
			ie813.Body.ChangeOfDestination.UpdateEadEsad.TransportModeCode = "-1";
		}

		protected override void UpdateGuarantor2Null()
		{
			ie813.Body.ChangeOfDestination.DestinationChanged.MovementGuarantee.GuarantorTrader = null;
		}

		protected override void UpdateTransportArranger2Null()
		{
			ie813.Body.ChangeOfDestination.NewTransportArrangerTrader = null;
		}
	}
}
