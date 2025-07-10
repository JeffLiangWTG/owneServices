using CargoWise.Types;
using Enterprise.Customs.Business.Extensions;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class IncoTermChargesConfigurationKey
	{
		public IncoTermChargesConfigurationKey(JobComInvoiceHeader invoice)
		{
			IncoTerm = invoice.JZ_IncoTerm;
			EffectiveAgreedPlaceCode = GetEffectiveAgreedPlaceCode(invoice);
			ModeOfTransport = invoice.JobDeclaration.JE_TransportMode;
			AirRouteType = invoice.JobDeclaration.JE_TransportMode == Core.Constants.TransportModes.Air ? invoice.JobDeclaration.JE_AirRouteType : ZString.Empty;
		}

		string GetEffectiveAgreedPlaceCode(JobComInvoiceHeader invoice)
		{
			if (invoice.JobDeclaration.IsUCC6)
			{
				var agreedPlaceCountry = invoice.ZG_AgreedPlaceCode.Left(2);
				if (agreedPlaceCountry == Core.Constants.CountryCodes.France)
				{
					return FRConstants.IncoTermKeys.ThisMemberState;
				}
				else if (invoice.Factory.IsMemberOfEU(agreedPlaceCountry))
				{
					return FRConstants.IncoTermKeys.AnotherMemberState;
				}
				else
				{
					return FRConstants.IncoTermKeys.OutsideUnion;
				}
			}
			else
			{
				return invoice.ZG_AgreedPlaceCode;
			}
		}

		public IncoTermChargesConfigurationKey(string incoTerm, string agreedPlaceCode, string modeOfTransport, string airRouteType)
		{
			IncoTerm = incoTerm;
			EffectiveAgreedPlaceCode = agreedPlaceCode;
			ModeOfTransport = modeOfTransport;
			AirRouteType = airRouteType;
		}

		public readonly string IncoTerm;
		public readonly string EffectiveAgreedPlaceCode;
		public readonly string ModeOfTransport;
		public readonly string AirRouteType;

		public override bool Equals(object obj)
		{
			var key = obj as IncoTermChargesConfigurationKey;

			return key != null
				&& IncoTerm == key.IncoTerm
				&& EffectiveAgreedPlaceCode == key.EffectiveAgreedPlaceCode
				&& ModeOfTransport == key.ModeOfTransport
				&& AirRouteType == key.AirRouteType;
		}

		public override int GetHashCode() => (IncoTerm, AgreedPlaceCode: EffectiveAgreedPlaceCode, ModeOfTransport, AirRouteType).GetHashCode();
	}
}
