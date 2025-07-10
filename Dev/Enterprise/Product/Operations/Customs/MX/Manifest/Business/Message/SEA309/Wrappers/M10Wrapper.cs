using CargoWise.Customs.MX.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.MX.Manifest.Business
{
	internal class M10Wrapper : IM10ManifestIdentifyingInformation
	{
		public M10Wrapper(string manifestNature, string action)
		{
			this.manifestNature = manifestNature;
			this.action = action;
		}

		readonly string manifestNature;
		readonly string action;

		string IM10ManifestIdentifyingInformation.StandCarrierAlphaCode => ZString.Empty;

		string IM10ManifestIdentifyingInformation.TypeCode => ZString.Empty;

		string IM10ManifestIdentifyingInformation.CountryCode => ZString.Empty;

		string IM10ManifestIdentifyingInformation.VesselCode => ZString.Empty;

		string IM10ManifestIdentifyingInformation.VesselName => ZString.Empty;

		string IM10ManifestIdentifyingInformation.VoyageNumber => ZString.Empty;

		string IM10ManifestIdentifyingInformation.Quantity => ZString.Empty;

		string IM10ManifestIdentifyingInformation.ReferenceIdentification => ZString.Empty;

		string IM10ManifestIdentifyingInformation.ManifestTypeCode => action;

		string IM10ManifestIdentifyingInformation.VesselCodeQualifier => ZString.Empty;

		string IM10ManifestIdentifyingInformation.ConditionResponseCode => ZString.Empty;

		string IM10ManifestIdentifyingInformation.ManifestReferenceIdentification => ZString.Empty;

		string IM10ManifestIdentifyingInformation.TransactionSetPurposeCode => ZString.Empty;

		string IM10ManifestIdentifyingInformation.ApplicationType
		{
			get
			{
				switch (manifestNature)
				{
					case ShipmentTypeList.Codes.Import23:
						return SEA309Constants.NatureImpo;
					case ShipmentTypeList.Codes.Export22:
						return SEA309Constants.NatureExpo;
					default:
						return ZString.Empty;
				}
			}
		}

		string IM10ManifestIdentifyingInformation.ApplicationTypeCode => ZString.Empty;

		string IM10ManifestIdentifyingInformation.AmendmentCode => ZString.Empty;

		string IM10ManifestIdentifyingInformation.ManifestTypeCode2 => ZString.Empty;
	}
}
