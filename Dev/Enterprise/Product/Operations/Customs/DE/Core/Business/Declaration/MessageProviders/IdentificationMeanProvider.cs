using CargoWise.Customs.DE.MessageContracts;

namespace Enterprise.Customs.DE.Business
{
	public class IdentificationMeanProvider : IIdentificationMeans
	{
		public static IdentificationMeanProvider NewOrNull(IdentificationMeansCode identificationMeansCode) => identificationMeansCode == null ? null : new IdentificationMeanProvider(identificationMeansCode);

		public IdentificationMeanProvider(string type, string description)
		{
			Type = type;
			Description = description;
		}

		IdentificationMeanProvider(IdentificationMeansCode identificationMeansCode)
		{
			Type = identificationMeansCode.CY_Code;
			Description = identificationMeansCode.CY_Data;
		}

		public string Type { get; }

		public string Description { get; }
	}
}
