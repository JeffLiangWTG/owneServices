using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC014A;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.TCL;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;

namespace Enterprise.Customs.FR.NCTS.Messaging
{
	public class CC014AMessageBuilder : NctsXmlMessageBuilder<ICC014ADeclaration, NctsMessageFunctionSet.DeclarationCancellationRequestMessage, Cc014AType>
	{
		public CC014AMessageBuilder(ICC014ADeclaration wrapper, NctsMessageFunctionSet.DeclarationCancellationRequestMessage messageFunction, ErrorCollector errorCollector) : base(wrapper, messageFunction, errorCollector)
		{
		}

		protected override void PopulateMessageHeader(Cc014AType message)
		{
			base.PopulateMessageHeader(message);
			message.MesTypMes20 = MessageTypes.Cc014A;
		}

		protected override void PopulateMessageBody(Cc014AType message)
		{
			base.PopulateMessageBody(message);
			message.Heahea = PopulateHeaheaType();
			message.Trapripc1 = PopulateTrapripc1();
			message.Cusoffdepept = PopulateCusoffdepept();
		}

		HeaheaType PopulateHeaheaType()
		{
			var data = new HeaheaType();

			data.NumAgrHea1005 = wrapper.AgreementNumber;
			data.TinOpeBenAgrHea1022 = wrapper.PrincipalTIN;
			data.RefNumHea4 = wrapper.LocalReferenceNumber;
			data.CanReaHea250Lng = ZString.Empty;
			data.DatOfCanReqHea147 = wrapper.CancellationDate;
			data.CanReaHea250 = messageFunction.UserReasonForCancellation;
			data.JusRegHea1017 = wrapper.CancellationRegularJustification;
			data.ComHea1018 = messageFunction.CommentOnCancellation;
			return data;
		}

		Trapripc1Type PopulateTrapripc1()
		{
			Trapripc1Type result = null;
			var principal = wrapper.Principal;
			if (principal != null)
			{
				result = new Trapripc1Type();
				if (wrapper.IsTIRDeclaration)
				{
					result.Hitpc126 = principal.HolderIDTIR;
				}

				if (!principal.TIN.IsEmpty)
				{
					result.Tinpc159 = principal.TIN;
					if (wrapper.IsTIRDeclaration)
					{
						result.NamPc17 = principal.CompanyName.TrimEnd();
					}
				}
				else
				{
					result.NamPc17 = principal.CompanyName.TrimEnd();
					result.StrAndNumPc122 = principal.StreetAndNumber.TrimEnd();
					result.PosCodPc123 = principal.PostalCode;
					result.CitPc124 = principal.City;
					result.CouPc125 = principal.CountryCode;
					result.Nadlngpc = principal.NameAndAddressLanguage.Left(2);
				}
			}
			else
			{
				errorCollector.AddError(Res.GetString("7607659A-35E8-4820-BCF4-462B41B5F153", "Principal is empty"));
			}
			return result;
		}

		CusoffdepeptType PopulateCusoffdepept()
		{
			return new CusoffdepeptType()
			{
				RefNumEpt1 = wrapper.DepartureCustomsOfficeReferenceNumber
			};
		}
	}
}
