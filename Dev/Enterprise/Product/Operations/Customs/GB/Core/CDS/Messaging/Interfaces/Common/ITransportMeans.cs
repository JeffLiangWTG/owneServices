using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS.Messaging
{
	public interface ITransportMeans
	{
		ZString ID { get; }
		ZString ModeCode { get; }
		ZString IdentificationTypeCode { get; }
		ZString RegistrationNationalityCode { get; }
	}

	public class TransportMeansWrapper : ITransportMeans
	{
		TransportMeansWrapper(ZString id, ZString modeCode, ZString identificationTypeCode, ZString registrationNationalityCode)
		{
			this.id = id;
			this.modeCode = modeCode;
			this.identificationTypeCode = identificationTypeCode;
			this.registrationNationalityCode = registrationNationalityCode;
		}

		public static TransportMeansWrapper New(ZString id, ZString modeCode, ZString identificationTypeCode, ZString registrationNationalityCode)
		{
			return new TransportMeansWrapper(id, modeCode, identificationTypeCode, registrationNationalityCode);
		}

		ZString ITransportMeans.ID => id.StripNewlineCharacters(CDSDataElementsLengths.TransportMeansIdentificationIDMaxLength);

		ZString ITransportMeans.ModeCode => modeCode;

		ZString ITransportMeans.IdentificationTypeCode => identificationTypeCode;

		ZString ITransportMeans.RegistrationNationalityCode => registrationNationalityCode;

		readonly ZString id;
		readonly ZString modeCode;
		readonly ZString identificationTypeCode;
		readonly ZString registrationNationalityCode;
	}
}
