using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CCF15A;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.TCL;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.MessageSending;

namespace Enterprise.Customs.FR.NCTS.Messaging
{
	public class CCF15AMessageBuilder : NctsXmlMessageBuilder<ICCF15ADeclaration, FRNctsMessageFunctionSet.PrelodgeValidationMessage, Ccf15AType>
	{
		public CCF15AMessageBuilder(ICCF15ADeclaration wrapper, FRNctsMessageFunctionSet.PrelodgeValidationMessage messageFunction, ErrorCollector errorCollector)
			: base(wrapper, messageFunction, errorCollector)
		{
		}

		protected override void PopulateMessageHeader(Ccf15AType message)
		{
			base.PopulateMessageHeader(message);
			message.MesTypMes20 = MessageTypes.Ccf15A;
		}

		protected override void PopulateMessageBody(Ccf15AType message)
		{
			base.PopulateMessageBody(message);
			message.Heahea = PopulateHeaheaType();
			message.Conresers = PopulateConresersType();
			message.Cusoffdepept = PopulateCusoffdepeptType();
			message.Trapripc1 = PopulateTrapripc1Type();
		}

		HeaheaType PopulateHeaheaType()
		{
			return new HeaheaType
			{
				NumAgrHea1005 = wrapper.AgreementNumber,
				TinOpeBenAgrHea1022 = wrapper.PrincipalTIN,
				RefNumHea4 = wrapper.LocalReferenceNumber,
				DocNumHea5 = wrapper.MovementReferenceNumber,
				DateValHea1021 = wrapper.ValidationDate,
				AutLocOfGooCodHea41 = wrapper.AuthorisedLocationOfGoodsCode,
				AgrLocOfGooHea39 = wrapper.AgreedLocationOfGoods,
				AgrLocOfGooHea39Lng = wrapper.AgreedLocationOfGoodsLanguage,
				IdeOfMeaOfTraAtDhea78 = wrapper.IdentityOfMeansOfTransportAtDeparture,
				IdeOfMeaOfTraAtDhea78Lng = wrapper.IdentityOfMeansOfTransportAtDepartureLanguage,
				NatOfMeaOfTraAtDhea80 = wrapper.NationalityOfMeansOfTransportAtDeparture
			};
		}

		ConresersType PopulateConresersType()
		{
			return new ConresersType
			{
				DatLimErs69 = wrapper.ControlResultDateLimit
			};
		}

		CusoffdepeptType PopulateCusoffdepeptType()
		{
			return new CusoffdepeptType
			{
				RefNumEpt1 = wrapper.DepartureCustomsOfficeReferenceNumber
			};
		}

		Trapripc1Type PopulateTrapripc1Type()
		{
			return new Trapripc1Type
			{
				Tinpc159 = wrapper.PrincipalTIN
			};
		}
	}
}
