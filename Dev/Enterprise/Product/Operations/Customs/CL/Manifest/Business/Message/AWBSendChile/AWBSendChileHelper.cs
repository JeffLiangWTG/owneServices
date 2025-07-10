using CargoWise.Types;

namespace Enterprise.Customs.CL.Manifest.Business
{
	static class AWBSendChileHelper
	{
		internal static string ProdItemUnitCodeCalculator(ZString uQ)
		{
			string res;
			switch (uQ)
			{
				case Core.Constants.Weight.Grams:
					res = WrappersConstants.WeightUnitCode.Grm;
					break;
				case Core.Constants.Weight.Hectograms:
					res = WrappersConstants.WeightUnitCode.Hgm;
					break;
				case Core.Constants.Weight.Kilotonnes:
					res = WrappersConstants.WeightUnitCode.Ktn;
					break;
				case Core.Constants.Weight.Ounces:
					res = WrappersConstants.WeightUnitCode.Onz;
					break;
				case Core.Constants.Weight.Pounds:
					res = WrappersConstants.WeightUnitCode.Lbr;
					break;
				case Core.Constants.Weight.Tonnes:
					res =  WrappersConstants.WeightUnitCode.Tne;
					break;
				case Core.Constants.Weight.Kilograms:
					res =  WrappersConstants.WeightUnitCode.Kgm;
					break;
				case Core.Constants.Weight.ShortTons:
					res =  WrappersConstants.WeightUnitCode.Stn;
					break;

				case Core.Constants.Volume.CubicCentimeters:
					res =  WrappersConstants.VolumeUnitCode.Cmq;
					break;
				case Core.Constants.Volume.CubicFeet:
					res =  WrappersConstants.VolumeUnitCode.Ftq;
					break;
				case Core.Constants.Volume.CubicInches:
					res =  WrappersConstants.VolumeUnitCode.Inq;
					break;
				case Core.Constants.Volume.Litre:
					res =  WrappersConstants.VolumeUnitCode.Ltr;
					break;
				case Core.Constants.Volume.CubicMetres:
					res =  WrappersConstants.VolumeUnitCode.Mtq;
					break;
				case Core.Constants.Volume.USGallons:
					res =  WrappersConstants.VolumeUnitCode.Gli;
					break;

				case Core.Constants.Length.Centimetres:
					res =  WrappersConstants.LengthUnitCode.Cmt;
					break;
				case Core.Constants.Length.Feet:
					res =  WrappersConstants.LengthUnitCode.Fot;
					break;
				case Core.Constants.Length.Inches:
					res =  WrappersConstants.LengthUnitCode.Inh;
					break;
				case Core.Constants.Length.Kilometres:
					res =  WrappersConstants.LengthUnitCode.Ktn;
					break;
				case Core.Constants.Length.Metres:
					res =  WrappersConstants.LengthUnitCode.Mtr;
					break;
				default:
					res =  WrappersConstants.LengthUnitCode.Uni;
					break;
			}
			return res;
		}
	}
}
