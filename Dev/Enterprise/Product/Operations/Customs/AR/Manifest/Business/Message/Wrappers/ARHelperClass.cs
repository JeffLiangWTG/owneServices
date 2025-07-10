using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AR.Manifest.Business
{
	internal class ARHelperClass
	{
		internal static decimal WeightConvertion(string uQ, decimal weight)
		{
			string res;
			switch (uQ)
			{
				case Core.Constants.Weight.Grams:
				case Core.Constants.Weight.Hectograms:
				case Core.Constants.Weight.Kilograms:
				case Core.Constants.Weight.Kilotonnes:
				case Core.Constants.Weight.Ounces:
				case Core.Constants.Weight.Pounds:
				case Core.Constants.Weight.Tonnes:
				case Core.Constants.Weight.LongTons:
				case Core.Constants.Weight.ShortTons:
				case Core.Constants.Weight.Decitons:
				case Core.Constants.Weight.MetricCarat:
				case Core.Constants.Weight.Milligrams:
					res = weight.ToString("0.00");
					break;
				default:
					res = (Core.Constants.Weight.ConvertSafe(weight, uQ, Core.Constants.Weight.Kilograms)).ToString("0.00");
					break;
			}
			return Convert.ToDecimal(res);
		}

		internal static (ZString, ZString) AirportInfo(BusinessObjectFactory factory, ZString codePort)
		{
			var port = factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, codePort));

			return (port?.RL_IATA ?? ZString.Empty, port?.Description ?? ZString.Empty);
		}

		internal static string WeightUnitCodeCalculator(ZString uQ)
		{
			string res;
			switch (uQ)
			{
				case Core.Constants.Weight.Pounds:
					res = WeightUnitCode.Lbr;
					break;
				case Core.Constants.Weight.Tonnes:
					res = WeightUnitCode.Tne;
					break;
				case Core.Constants.Weight.Grams:
					res = WeightUnitCode.Grm;
					break;
				case Core.Constants.Weight.Hectograms:
					res = WeightUnitCode.Hgm;
					break;
				case Core.Constants.Weight.Kilotonnes:
					res = WeightUnitCode.Ktn;
					break;
				case Core.Constants.Weight.Ounces:
					res = WeightUnitCode.Onz;
					break;
				case Core.Constants.Weight.Decitons:
					res = WeightUnitCode.Dtn;
					break;
				case Core.Constants.Weight.LongTons:
					res = WeightUnitCode.Ltn;
					break;
				case Core.Constants.Weight.Milligrams:
					res = WeightUnitCode.Mgm;
					break;
				case Core.Constants.Weight.MetricCarat:
					res = WeightUnitCode.Ctm;
					break;
				case Core.Constants.Weight.ShortTons:
					res = WeightUnitCode.Stn;
					break;
				default:
					res = WeightUnitCode.Kgm;
					break;
			}
			return res;
		}

		internal static ZDecimal VolumeConvertion(ZString uQ, ZDecimal volume)
		{
			ZString res;
			switch (uQ)
			{
				case Core.Constants.Volume.CubicCentimeters:
				case Core.Constants.Volume.CubicFeet:
				case Core.Constants.Volume.CubicInches:
				case Core.Constants.Volume.CubicMetres:
				case Core.Constants.Volume.Litre:
				case Core.Constants.Volume.USGallons:
				case Core.Constants.Volume.CubicDecimetres:
				case Core.Constants.Volume.MegaLitre:
					res = volume.ToString("0.00");
					break;
				default:
					res = (Core.Constants.Volume.ConvertSafe(volume, uQ, Core.Constants.Volume.CubicMetres)).ToString("0.00");
					break;
			}
			return Convert.ToDecimal(res);
		}

		internal static string VolumeUnitCodeCalculator(ZString uQ)
		{
			string res;
			switch (uQ)
			{
				case Core.Constants.Volume.CubicCentimeters:
					res = VolumeUnitCode.Cmq;
					break;
				case Core.Constants.Volume.CubicFeet:
					res = VolumeUnitCode.Ftq;
					break;
				case Core.Constants.Volume.CubicInches:
					res = VolumeUnitCode.Inq;
					break;
				case Core.Constants.Volume.Litre:
					res = VolumeUnitCode.Ltr;
					break;
				case Core.Constants.Volume.CubicDecimetres:
					res = VolumeUnitCode.Dmq;
					break;
				case Core.Constants.Volume.CubicYards:
					res = VolumeUnitCode.Ydq;
					break;
				case Core.Constants.Volume.MegaLitre:
					res = VolumeUnitCode.Mal;
					break;
				case Core.Constants.Volume.USGallons:
					res = VolumeUnitCode.Gll;
					break;
				default:
					res = VolumeUnitCode.Mtq;
					break;
			}
			return res;
		}

		internal static ZString GetStaffAssignmentName(OrgStaffAssignmentsCollection staffAssignments) => staffAssignments?.Cast<OrgStaffAssignments>().FirstOrDefault(c => c.O8_Role == StaffAssignmentRoles.Codes.CustomsAgent)?.ResponsiblePersonName ?? ZString.Empty;

		internal static DateTime SafeDateTime(ZDateTime dateTime) => dateTime.IsValid ? Convert.ToDateTime(dateTime.ToString("yyyy-MM-ddTHH:mm:ss")) : default;
	}
}
