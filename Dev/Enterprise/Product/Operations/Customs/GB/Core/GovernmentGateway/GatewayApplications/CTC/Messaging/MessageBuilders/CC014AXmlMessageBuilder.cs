using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase4.cc014a;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.GovernmentGateway.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.GovernmentGateway.GatewayApplications.CTC.Messaging
{
	public class CC014AXmlMessageBuilder : XmlMessageBuilder<ICC014ADeclaration, Cc014AType>
	{
		public CC014AXmlMessageBuilder(ICC014ADeclaration wrapper, ErrorCollector errorCollector)
			: base(wrapper, errorCollector)
		{
		}

		#region CC014A Message

		protected override void PopulateMessageHeader(Cc014AType message)
		{
			base.PopulateMessageHeader(message);
			message.MesTypMes20 = CTCMessageTypeList.Codes.CC014A;
		}

		protected override void PopulateMessageBody(Cc014AType message)
		{
			base.PopulateMessageBody(message);
			message.Heahea = PopulateHEAHEAType();
			message.Trapripc1 = PopulateTRAPRIPC1Type();
			message.Cusoffdepept = PopulateCUSOFFDEPEPTType();
		}

		HeaheaType PopulateHEAHEAType()
		{
			var data = new HeaheaType();

			data.DocNumHea5 = wrapper.MovementReferenceNumber;
			data.DatOfCanReqHea147 = wrapper.DateOfCancellationRequest;
			data.CanReaHea250 = wrapper.CancellationReason;
			data.CanReaHea250Lng = wrapper.CancellationReasonLanguage.SubstringSafe(0, 2);
			return data;
		}

		Trapripc1Type PopulateTRAPRIPC1Type()
		{
			Trapripc1Type result = null;
			var principal = wrapper.Principal;
			if (principal != null)
			{
				result = new Trapripc1Type();
				if (!principal.TIN.IsEmpty)
				{
					result.Tinpc159 = principal.TIN;
				}
				result.NamPc17 = principal.Name;
				result.StrAndNumPc122 = principal.StreetAndNumber;
				result.PosCodPc123 = principal.PostalCode;
				result.CitPc124 = principal.City;
				result.CouPc125 = principal.CountryCode;
				if (wrapper.IsTIRDeclaration)
				{
					result.Hitpc126 = principal.HolderIDTIR;
				}
			}
			else
			{
				errorCollector.AddError("Principal is empty");
			}
			return result;
		}

		CusoffdepeptType PopulateCUSOFFDEPEPTType()
		{
			return new CusoffdepeptType()
			{
				RefNumEpt1 = wrapper.DepartureCustomsOfficeReferenceNumber
			};
		}

		#endregion
	}
}
