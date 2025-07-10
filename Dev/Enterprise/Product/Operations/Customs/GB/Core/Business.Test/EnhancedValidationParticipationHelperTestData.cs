using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using static Enterprise.Customs.GB.Business.EnhancedValidationParticipationHelper;

namespace Enterprise.Customs.GB.Business.Testing
{
	static class EnhancedValidationParticipationHelperTestData
	{
		static readonly TestData[] data =
		[
			new("GB358522637000", "ZGBLGK390322513645514837201", true, true, true, true, true),
			new("GB358522637000", "ZGBLGK390322513645514837202", true, true, false, true, false),
			new("GB358522637000", "ZGBLGK390322513645514837203", true, false, false, false, false),
			new("GB358522637000", "ZGBLGK390322513645514837204", true, false, false, false, false),
			new("GB358522637000", "ZGBLGK390322513645514837205", true, true, false, true, false),
			new("GB358522637000", "ZGBLGK390322513645514837206", true, false, false, false, false),
			new("GB358522637000", "ZGBLGK390322513645514837207", true, false, false, false, false),
			new("GB358522637000", "ZGBLGK390322513645514837208", true, false, false, false, false),
			new("GB606091760000", "PZLLGA163004306145012377109", true, true, false, true, false),
			new("GB606091760000", "PZLLGA163004306145012377110", true, true, false, true, false),
			new("GB606091760000", "PZLLGA163004306145012377111", true, true, false, true, false),
			new("GB606091760000", "PZLLGA163004306145012377112", true, true, false, true, false),
			new("GB606091760000", "PZLLGA163004306145012377113", true, true, false, true, false),
			new("GB606091760000", "PZLLGA163004306145012377114", true, true, true, true, true),
			new("GB606091760000", "PZLLGA163004306145012377115", true, true, false, true, false),
			new("GB606091760000", "PZLLGA163004306145012377116", true, true, false, true, false),
			new("GB408111075000", "ZZGWVS225953248795121916317", true, false, false, false, false),
			new("GB408111075000", "ZZGWVS225953248795121916318", true, true, false, true, false),
			new("GB408111075000", "ZZGWVS225953248795121916319", true, false, false, false, false),
			new("GB408111075000", "ZZGWVS225953248795121916320", true, false, false, false, false),
			new("GB408111075000", "ZZGWVS225953248795121916321", true, true, false, true, false),
			new("GB408111075000", "ZZGWVS225953248795121916322", true, true, false, true, false),
			new("GB408111075000", "ZZGWVS225953248795121916323", true, true, false, true, false),
			new("GB408111075000", "ZZGWVS225953248795121916324", true, true, true, true, true),
			new("GB453320830000", "HUDDIN337746705859899503425", false, true, false, false, false),
			new("GB453320830000", "HUDDIN337746705859899503426", false, false, false, false, false),
			new("GB453320830000", "HUDDIN337746705859899503427", false, true, false, false, false),
			new("GB453320830000", "HUDDIN337746705859899503428", false, true, false, false, false),
			new("GB453320830000", "HUDDIN337746705859899503429", false, true, false, false, false),
			new("GB453320830000", "HUDDIN337746705859899503430", false, false, false, false, false),
			new("GB453320830000", "HUDDIN337746705859899503431", false, true, true, false, false),
			new("GB453320830000", "HUDDIN337746705859899503432", false, true, false, false, false),
			new("GB453320830000", "HUDDIN337746705859899503433", false, true, false, false, false),
			new("GB882456430000", "BCUUYS111692705191671850634", true, false, false, false, false),
			new("GB882456430000", "BCUUYS111692705191671850635", true, false, false, false, false),
			new("GB882456430000", "BCUUYS111692705191671850636", true, false, false, false, false),
			new("GB882456430000", "BCUUYS111692705191671850637", true, false, false, false, false),
			new("GB882456430000", "BCUUYS111692705191671850638", true, true, false, true, false),
			new("GB882456430000", "BCUUYS111692705191671850639", true, false, false, false, false),
			new("GB882456430000", "BCUUYS111692705191671850640", true, false, false, false, false),
			new("GB882456430000", "BCUUYS111692705191671850641", true, false, false, false, false),
			new("GB974861116000", "HNHHOZ302861181553453196942", true, false, false, false, false),
			new("GB974861116000", "HNHHOZ302861181553453196943", true, true, true, true, true),
			new("GB974861116000", "HNHHOZ302861181553453196944", true, true, false, true, false),
			new("GB974861116000", "HNHHOZ302861181553453196945", true, true, false, true, false),
			new("GB974861116000", "HNHHOZ302861181553453196946", true, true, false, true, false),
			new("GB974861116000", "HNHHOZ302861181553453196947", true, true, false, true, false),
			new("GB974861116000", "HNHHOZ302861181553453196948", true, false, false, false, false),
			new("GB974861116000", "HNHHOZ302861181553453196949", true, true, false, true, false),
			new("GB974861116000", "HNHHOZ302861181553453196950", true, false, false, false, false),
			new("GB974861116000", "HNHHOZ302861181553453196951", true, true, false, true, false),
			new("GB974861116000", "HNHHOZ302861181553453196952", true, false, false, false, false),
			new("GB128362155000", "UUHDLP683317121351137781353", true, false, false, false, false),
			new("GB128362155000", "UUHDLP683317121351137781354", true, true, false, true, false),
			new("GB128362155000", "UUHDLP683317121351137781355", true, true, false, true, false),
			new("GB128362155000", "UUHDLP683317121351137781356", true, false, false, false, false),
			new("GB128362155000", "UUHDLP683317121351137781357", true, false, false, false, false),
			new("GB128362155000", "UUHDLP683317121351137781358", true, false, false, false, false),
			new("GB128362155000", "UUHDLP683317121351137781359", true, false, false, false, false),
			new("GB128362155000", "UUHDLP683317121351137781360", true, false, false, false, false),
			new("GB128362155000", "UUHDLP683317121351137781361", true, false, false, false, false),
			new("GB128362155000", "UUHDLP683317121351137781362", true, false, false, false, false),
			new("GB128362155000", "UUHDLP683317121351137781363", true, true, false, true, false),
			new("GB326929236000", "JPYGDH679863218450918801464", false, false, false, false, false),
			new("GB326929236000", "JPYGDH679863218450918801465", false, false, false, false, false),
			new("GB326929236000", "JPYGDH679863218450918801466", false, false, false, false, false),
			new("GB326929236000", "JPYGDH679863218450918801467", false, false, false, false, false),
			new("GB326929236000", "JPYGDH679863218450918801468", false, false, false, false, false),
			new("GB326929236000", "JPYGDH679863218450918801469", false, false, false, false, false),
			new("GB457394695000", "ZDMCVD717014558147638984770", false, false, false, false, false),
			new("GB457394695000", "ZDMCVD717014558147638984771", false, false, false, false, false),
			new("GB457394695000", "ZDMCVD717014558147638984772", false, true, false, false, false),
			new("GB457394695000", "ZDMCVD717014558147638984773", false, false, false, false, false),
			new("GB457394695000", "ZDMCVD717014558147638984774", false, false, false, false, false),
			new("GB457394695000", "ZDMCVD717014558147638984775", false, true, false, false, false),
			new("GB457394695000", "ZDMCVD717014558147638984776", false, true, false, false, false),
			new("GB457394695000", "ZDMCVD717014558147638984777", false, true, false, false, false),
			new("GB457394695000", "ZDMCVD717014558147638984778", false, true, false, false, false),
			new("GB457394695000", "ZDMCVD717014558147638984779", false, true, false, false, false),
			new("GB457394695000", "ZDMCVD717014558147638984780", false, false, false, false, false),
			new("GB457394695000", "ZDMCVD717014558147638984781", false, false, false, false, false),
			new("GB435841409000", "JFOWFD71255885297432539982", false, true, false, false, false),
			new("GB435841409000", "JFOWFD71255885297432539983", false, false, false, false, false),
			new("GB435841409000", "JFOWFD71255885297432539984", false, false, false, false, false),
			new("GB435841409000", "JFOWFD71255885297432539985", false, true, true, false, false),
			new("GB435841409000", "JFOWFD71255885297432539986", false, true, false, false, false),
			new("GB435841409000", "JFOWFD71255885297432539987", false, true, false, false, false),
			new("GB435841409000", "JFOWFD71255885297432539988", false, true, false, false, false),
			new("GB435841409000", "JFOWFD71255885297432539989", false, true, false, false, false),
			new("GB435841409000", "JFOWFD71255885297432539990", false, true, false, false, false),
			new("GB435841409000", "JFOWFD71255885297432539991", false, false, false, false, false),
			new("GB435841409000", "JFOWFD71255885297432539992", false, true, false, false, false),
			new("GB435841409000", "JFOWFD71255885297432539993", false, false, false, false, false),
			new("GB504081435000", "XMTZZA541295369155704992594",true, false, false, false, false),
			new("GB504081435000", "XMTZZA541295369155704992595",true, true, true, true, true),
			new("GB504081435000", "XMTZZA541295369155704992596",true, false, false, false, false),
			new("GB888782255000", "DEOMCX462807859992608427297",true, true, false, true, false),
			new("GB888782255000", "DEOMCX462807859992608427298",true, false, false, false, false),
			new("GB888782255000", "DEOMCX462807859992608427299",true, false, false, false, false),
			new("GB888782255000", "DEOMCX4628078599926084272100", true, true, false, true, false),
			new("GB888782255000", "DEOMCX4628078599926084272101", true, true, true, true, true),
			new("GB888782255000", "DEOMCX4628078599926084272102", true, true, false, true, false),
			new("GB888782255000", "DEOMCX4628078599926084272103", true, true, false, true, false),
			new("GB888782255000", "DEOMCX4628078599926084272104", true, false, false, false, false),
			new("GB888782255000", "DEOMCX4628078599926084272105", true, false, false, false, false),
		];

		public static IEnumerable<TestData> GetData() => data;

		public static IEnumerable<(string EORI, bool Treatment)> GetEORIData() => data.Select(x => (x.EORI, x.EORITreatment)).Distinct();

		public static IEnumerable<(string LRN, bool Treatment)> GetLRNData(WarningFactor warningFactor)
		{
			if (!warningFactor.In(WarningFactor.Two, WarningFactor.Sixteen))
			{
				throw new ArgumentException($"No data set available for {warningFactor}");
			}

			return data.Select(x => (x.LRN, warningFactor == WarningFactor.Two ? x.LRNFactorTwoTreatment : x.LRNFactorSixteenTreatment));
		}

		public readonly struct TestData(string eori, string lrn, bool eoriTreatment, bool lrnFactorTwoTreatment, bool lrnFactorSixteenTreatment, bool showNudgeFactorTwoTreatment, bool showNudgeFactorSixteenTreatment)
		{
			public string EORI { get; } = eori;

			public string LRN { get; } = lrn;

			public bool EORITreatment { get; } = eoriTreatment;

			public bool LRNFactorTwoTreatment { get; } = lrnFactorTwoTreatment;

			public bool LRNFactorSixteenTreatment { get; } = lrnFactorSixteenTreatment;

			public bool ShowNudgeFactorTwoTreatment { get; } = showNudgeFactorTwoTreatment;

			public bool ShowNudgeFactorSixteenTreatment { get; } = showNudgeFactorSixteenTreatment;
		}
	}
}
