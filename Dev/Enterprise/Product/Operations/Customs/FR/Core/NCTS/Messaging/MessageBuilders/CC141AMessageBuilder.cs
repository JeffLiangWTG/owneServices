using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC141A;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.COMPLEX_NCTS;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.TCL;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Customs.FR.Messaging.MessageBuilders;

namespace Enterprise.Customs.FR.NCTS.Messaging
{
	public class CC141AMessageBuilder : NctsXmlMessageBuilder<ICC141ADeclaration, NctsMessageFunctionSet.InformationAboutNonArrivedMovementMessage, Cc141AType>
	{
		public CC141AMessageBuilder(ICC141ADeclaration wrapper, NctsMessageFunctionSet.InformationAboutNonArrivedMovementMessage messageFunction, ErrorCollector errorCollector) : base(wrapper, messageFunction, errorCollector)
		{
		}

		protected override void PopulateMessageHeader(Cc141AType message)
		{
			base.PopulateMessageHeader(message);
			message.MesTypMes20 = MessageTypes.Cc141A;
		}

		protected override void PopulateMessageBody(Cc141AType message)
		{
			base.PopulateMessageBody(message);
			message.Heahea = PopulateHEAHEA();
			message.Trapripc1 = PopulateTrapripc1();
			message.Cusoffcomaut = PopulateCusoffcomaut();
			message.Cusoffpreoffres = PopulateCusoffpreoffres();
			message.Enqenq = PopulateEnqenq();
			message.Cnecne = PopulateCnecne();
		}

		HeaheaType PopulateHEAHEA()
		{
			return new HeaheaType
			{
				DocNumHea5 = wrapper.MovementReferenceNumber
			};
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
				errorCollector.AddError(Res.GetString("96854205-80A1-47B5-9D63-F75C62EBC6F1", "Principal is empty"));
			}
			return result;
		}

		CusoffcomautType PopulateCusoffcomaut()
		{
			return new CusoffcomautType
			{
				RefNumAut1 = wrapper.DepartureCustomsOfficeReferenceNumber
			};
		}

		CusoffpreoffresType PopulateCusoffpreoffres()
		{
			return new CusoffpreoffresType
			{
				RefNumRes1 = wrapper.DestinationCustomsOfficeReferenceNumber
			};
		}

		EnqenqType PopulateEnqenq()
		{
			return new EnqenqType
			{
				Tc11DelEnq155 = wrapper.IsTC11DeliveredByCustoms ? Flag.Item1 : Flag.Item0,
				Tc11DelDatEnq143 = wrapper.TC11Date,
				InfoEnq148 = wrapper.QueryInformation,
				InfoEnq148Lng = Core.Constants.CountryCodes.France,
				InfOnPapAvaEnq790 = wrapper.IsQueryAvailableOnPaper ? Flag.Item1 : Flag.Item0,
			};
		}

		CnecneType PopulateCnecne()
		{
			CnecneType result = null;
			var consignee = wrapper.Consignee;
			if (consignee != null)
			{
				result = new CnecneType
				{
					Tincne59 = MessageBuilderHelper.GetOptionalString(consignee.TIN),
					NamCne17 = MessageBuilderHelper.GetOptionalString(consignee.Name),
					StrAndNumCne122 = MessageBuilderHelper.GetOptionalString(consignee.StreetAndNumber),
					PosCodCne123 = MessageBuilderHelper.GetOptionalString(consignee.PostalCode),
					CouCne125 = MessageBuilderHelper.GetOptionalString(consignee.CountryCode),
					CitCne124 = MessageBuilderHelper.GetOptionalString(consignee.City),
					Nadlngact = MessageBuilderHelper.GetOptionalString(consignee.NameAndAddressLanguage.Left(2))
				};
			}
			else
			{
				errorCollector.AddError(Res.GetString("A662F6D0-83EA-464C-95A9-5A946A70DE6D", "Consignee is empty"));
			}
			return result;
		}
	}
}
