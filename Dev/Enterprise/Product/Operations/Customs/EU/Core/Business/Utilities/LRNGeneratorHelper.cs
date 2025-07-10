using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business
{
	public static class LRNGeneratorHelper
	{
		public static ZString GenerateLocalReferenceNumber(this ILRNGenerator header, string eoriForLrn = "")
		{
			var result = ZString.Empty;

			eoriForLrn = eoriForLrn.IsNullOrEmpty() ? GetEORIForLRNGeneration(header) : eoriForLrn;

			if (!eoriForLrn.IsNullOrEmpty())
			{
				var sequenceNr = header.LrnNumberFountain.GetNext(header.Factory).ToString();
				var lengthSequenceNr = 20 - eoriForLrn.Length;
				sequenceNr = sequenceNr.PadLeft(lengthSequenceNr, '0');
				result = string.Format("{0}{1}{2}", ZDateTime.Now.Year.ToString().Substring(2), eoriForLrn, sequenceNr);
			}
			return result;
		}

		public static ZString GetEORIForLRNGeneration(this ILRNGenerator header)
		{
			var eoriForLrn = GetEoriForLrnNumber(header.Branch.OrgProxy);
			if (eoriForLrn.IsEmpty)
			{
				eoriForLrn = GetEoriForLrnNumber(header.Branch.Company.OrgProxy);
			}
			return eoriForLrn;
		}

		static ZString GetEoriForLrnNumber(OrgHeader party)
		{
			var (eoriCountry, eoriNr) = party.GetEuIdentificationNumberComponents();

			if (!eoriCountry.IsEmpty && !eoriNr.IsEmpty)
			{
				if (eoriNr.StartsWith(eoriCountry)
					|| (eoriCountry == Core.Constants.CountryCodes.Greece && eoriNr.StartsWith("EL")))
				{
					eoriNr = eoriNr.SubstringSafe(2);
				}
				if (eoriNr.Length > 15)
				{
					eoriNr = ZString.Empty;
				}
			}

			return eoriNr;
		}
	}
}
