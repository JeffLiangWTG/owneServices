using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.MX.Business
{
	public partial class MXDeclarationTypeList
	{
		public static CodeDescriptionPairList GetMessageSubTypeListForExport(BusinessObjectFactory factory)
		{
			var cachekey = "MXCustomRegimeList_EXP";
			return factory.GetCachedValue(cachekey, () =>
			{
				var result = new MXDeclarationTypeList();
				result.RemoveCode(Codes.A3);
				result.RemoveCode(Codes.A5);
				result.RemoveCode(Codes.AF);
				result.RemoveCode(Codes.BE);
				result.RemoveCode(Codes.BH);
				result.RemoveCode(Codes.BI);
				result.RemoveCode(Codes.C1);
				result.RemoveCode(Codes.C3);
				result.RemoveCode(Codes.E1);
				result.RemoveCode(Codes.E2);
				result.RemoveCode(Codes.E3);
				result.RemoveCode(Codes.E4);
				result.RemoveCode(Codes.F2);
				result.RemoveCode(Codes.F3);
				result.RemoveCode(Codes.F5);
				result.RemoveCode(Codes.G2);
				result.RemoveCode(Codes.G8);
				result.RemoveCode(Codes.IN);
				result.RemoveCode(Codes.M1);
				result.RemoveCode(Codes.M2);
				result.RemoveCode(Codes.P1);
				result.RemoveCode(Codes.VF);
				result.RemoveCode(Codes.VU);
				return result;
			});
		}

		public static CodeDescriptionPairList GetMessageSubTypeListForImport(BusinessObjectFactory factory)
		{
			var cachekey = "MXCustomRegimeList_IMP";
			return factory.GetCachedValue(cachekey, () =>
			{
				var result = new MXDeclarationTypeList();
				result.RemoveCode(Codes.BF);
				result.RemoveCode(Codes.BM);
				result.RemoveCode(Codes.CT);
				result.RemoveCode(Codes.J3);
				result.RemoveCode(Codes.K3);
				result.RemoveCode(Codes.M5);
				result.RemoveCode(Codes.RT);
				result.RemoveCode(Codes.V4);
				return result;
			});
		}

		public static string GetDefaultCustomsRegimeForImport(ZString code)
		{
			switch (code)
			{
				case Codes.A1:
				case Codes.A3:
				case Codes.BB:
				case Codes.C1:
				case Codes.C3:
				case Codes.D1:
				case Codes.F3:
				case Codes.F4:
				case Codes.F5:
				case Codes.G1:
				case Codes.G2:
				case Codes.G9:
				case Codes.GC:
				case Codes.H1:
				case Codes.H8:
				case Codes.I1:
				case Codes.K1:
				case Codes.L1:
				case Codes.P1:
				case Codes.S2:
				case Codes.T1:
				case Codes.V2:
				case Codes.V5:
				case Codes.V6:
				case Codes.V7:
				case Codes.V9:
				case Codes.VD:
				case Codes.VF:
				case Codes.VU:
					return CustomsRegimeList.Codes.IMD;
				case Codes.AD:
				case Codes.AF:
				case Codes.AJ:
				case Codes.BA:
				case Codes.BC:
				case Codes.BD:
				case Codes.BE:
				case Codes.BH:
				case Codes.BI:
				case Codes.BO:
				case Codes.BP:
				case Codes.E2:
				case Codes.E4:
					return CustomsRegimeList.Codes.ITR;
				case Codes.E1:
				case Codes.E3:
				case Codes.IN:
				case Codes.V1:
					return CustomsRegimeList.Codes.ITE;
				case Codes.A4:
				case Codes.A5:
				case Codes.F2:
				case Codes.F8:
				case Codes.F9:
				case Codes.V3:
					return CustomsRegimeList.Codes.DFI;
				case Codes.G8:
				case Codes.M1:
				case Codes.M2:
					return CustomsRegimeList.Codes.RFE;
				case Codes.T3:
				case Codes.T6:
				case Codes.T7:
				case Codes.T9:
					return CustomsRegimeList.Codes.TRA;
				case Codes.J4:
				case Codes.M3:
				case Codes.M4:
					return CustomsRegimeList.Codes.RFS;
				default:
					return string.Empty;
			}
		}

		public static string GetDefaultCustomsRegimeForExport(ZString code)
		{
			switch (code)
			{
				case Codes.A1:
				case Codes.BB:
				case Codes.D1:
				case Codes.F4:
				case Codes.G1:
				case Codes.G6:
				case Codes.G7:
				case Codes.G9:
				case Codes.H1:
				case Codes.H8:
				case Codes.I1:
				case Codes.K1:
				case Codes.K2:
				case Codes.K3:
				case Codes.L1:
				case Codes.M3:
				case Codes.RT:
				case Codes.S2:
				case Codes.T1:
				case Codes.V1:
				case Codes.V2:
				case Codes.V5:
				case Codes.V6:
				case Codes.V7:
				case Codes.V8:
				case Codes.V9:
				case Codes.VD:
					return CustomsRegimeList.Codes.EXD;
				case Codes.AJ:
				case Codes.BA:
				case Codes.BF:
				case Codes.BR:
				case Codes.V4:
					return CustomsRegimeList.Codes.ETR;
				case Codes.BM:
				case Codes.BO:
				case Codes.CT:
					return CustomsRegimeList.Codes.ETE;
				case Codes.A4:
				case Codes.F8:
				case Codes.F9:
				case Codes.V3:
					return CustomsRegimeList.Codes.DFI;
				case Codes.J3:
				case Codes.M5:
					return CustomsRegimeList.Codes.RFE;
				case Codes.T3:
				case Codes.T6:
				case Codes.T7:
				case Codes.T9:
					return CustomsRegimeList.Codes.TRA;
				case Codes.J4:
				case Codes.M4:
					return CustomsRegimeList.Codes.RFS;
				default:
					return string.Empty;
			}
		}
	}
}
