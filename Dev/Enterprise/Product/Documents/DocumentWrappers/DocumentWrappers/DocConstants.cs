using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public static class DocConstants
	{
		public static class Resources
		{
			const string BasePath = "Enterprise.DocumentWrappers.Specialised_Classes.Resources.Raw.";
			public const string TestImage = BasePath + "TestImage.gif";

			public static class FIATA
			{
				const string FIATABasePath = Resources.BasePath + "FIATA.";

				public static class TextLogoCodes
				{
					public const string HBL = "HBL";
					public const string ICC = "ICC";
					public const string SWB = "SWB";
					public const string FWB = "FWB";
				}

				public static ReadOnlyCodeDescriptionPairList GetTextLogoPaths()
				{
					CodeDescriptionPairList result = new CodeDescriptionPairList();

					result.AddPair(TextLogoCodes.HBL, FIATABasePath + "FIATATextLogo-HBL.gif");
					result.AddPair(TextLogoCodes.ICC, FIATABasePath + "FIATATextLogo-ICC.gif");
					result.AddPair(TextLogoCodes.SWB, FIATABasePath + "FIATATextLogo-SWB.gif");
					result.AddPair(TextLogoCodes.FWB, FIATABasePath + "FIATATextLogo-FWB.gif");

					return new ReadOnlyCodeDescriptionPairList(result);
				}

				public static ReadOnlyCodeDescriptionPairList GetGraphicLogoPaths()
				{
					CodeDescriptionPairList result = new CodeDescriptionPairList();

					result.AddPair(Core.Constants.CountryCodes.Australia, FIATABasePath + "FIATAlogo-AFIF.gif");
					result.AddPair(Core.Constants.CountryCodes.UnitedKingdom, FIATABasePath + "FIATAlogo-BIFA.gif");
					result.AddPair(Core.Constants.CountryCodes.NewZealand, FIATABasePath + "FIATAlogo-CBAFF.gif");
					result.AddPair(Core.Constants.CountryCodes.China, FIATABasePath + "FIATAlogo-CIFA.gif");
					result.AddPair(Core.Constants.CountryCodes.Canada, FIATABasePath + "FIATAlogo-CIFFA.gif");
					result.AddPair(Core.Constants.CountryCodes.India, FIATABasePath + "FIATAlogo-FFI.gif");
					result.AddPair(Core.Constants.CountryCodes.Malaysia, FIATABasePath + "FIATAlogo-FMFF.gif");
					result.AddPair(Core.Constants.CountryCodes.HongKong, FIATABasePath + "FIATAlogo-HAFFA.gif");
					result.AddPair(Core.Constants.CountryCodes.Indonesia, FIATABasePath + "FIATAlogo-INFA.gif");
					result.AddPair(Core.Constants.CountryCodes.Taiwan, FIATABasePath + "FIATAlogo-IOFFLAT.gif");
					result.AddPair(Core.Constants.CountryCodes.Japan, FIATABasePath + "FIATAlogo-JAFA.gif");
					result.AddPair(Core.Constants.CountryCodes.KoreaSouth, FIATABasePath + "FIATAlogo-KIFFA.gif");
					result.AddPair(Core.Constants.CountryCodes.Myanmar, FIATABasePath + "FIATAlogo-MIFFA.gif");
					result.AddPair(Core.Constants.CountryCodes.SouthAfrica, FIATABasePath + "FIATAlogo-SAAFF.gif");
					result.AddPair(Core.Constants.CountryCodes.Singapore, FIATABasePath + "FIATAlogo-SLA.gif");
					result.AddPair(Core.Constants.CountryCodes.Switzerland, FIATABasePath + "FIATAlogo-SSV.gif");
					result.AddPair(Core.Constants.CountryCodes.UnitedStates, FIATABasePath + "FIATAlogo-TIA.gif");
					result.AddPair(Core.Constants.CountryCodes.Thailand, FIATABasePath + "FIATAlogo-TIFFA.gif");
					result.AddPair(Core.Constants.CountryCodes.VietNam, FIATABasePath + "FIATAlogo-VLA.gif");

					return new ReadOnlyCodeDescriptionPairList(result);
				}
			}

			public static class BillTitles
			{
				public static string HBL => (NoResString)"Bill of Lading";
				public static string SWB => (NoResString)"Sea Waybill";
			}

			public static class BillTerms
			{
				public const string StandardBillTermsBasePath = Resources.BasePath + "Standard_Bill_Terms.";

				public static string DeliveryAgentHeading => (NoResString)"Delivery Agent";

				public static string BillSurrenderedToHeading => (NoResString)"Bill of Lading must be surrendered to:";

				public static string NotNegotiableText
				{
					get { return Res.GetString("fd8b185d-3440-465c-ba90-59558555c92b", "Not Negotiable Unless Consigned 'To Order'"); }
				}

#if DEBUG
				public const string TestHBLImage = StandardBillTermsBasePath + "Test_HBL.gif";
				public const string TestSWBImage = StandardBillTermsBasePath + "Test_SWB.gif";
#endif
			}
		}

		public static class USDestinationControlStatement
		{
			public static ZDateTime EffectiveDate
			{
				get { return new ZDateTime(2016, 11, 15); }
			}
		}
	}
}