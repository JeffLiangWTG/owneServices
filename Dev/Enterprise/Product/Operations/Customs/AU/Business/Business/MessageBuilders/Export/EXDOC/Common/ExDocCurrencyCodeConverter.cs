namespace Enterprise.Customs.AU.Declaration.Business
{
	static class ExDocCurrencyCodeConverter
	{
		/// <summary>
		/// Converts from ISO4217 currency to bodgy ExDoc two character currency (except for AUD and EUR)
		/// </summary>
		/// <param name="threeCharacterCurrency"></param>
		/// <returns></returns>
		public static string ConvertToExDoc(string iso4217Currency)
		{
			switch (iso4217Currency)
			{
				//AT AUSTRIAN SCHILLING
				case "AUD":
					return "AUD";//	AUSTRALIAN DOLLAR
								 //BE 	BELGIAN/LUXEMBOURG FRANC
				case "BRL":
					return "BR";// 	BRAZILLIAN CRUZEIRO
				case "CAD":
					return "CA";// 	CANADIAN DOLLAR
				case "CHF":
					return "CH";// 	SWISS FRANC
				case "CNY":
					return "CN";// 	CHINESE RENMINBI
								//DE 	DEUTSCHE MARK
				case "DKK":
					return "DK";// 	DANISH KRONE
								//ES 	SPANISH PESETA
				case "EUR":
					return "EUR";//	EURO DOLLAR
								 //FI 	FINNISH MARKKA
				case "FJD":
					return "FJ";// 	FIJI DOLLAR
								//FR 	FRENCH FRANC
				case "GBP":
					return "GB";// 	UK POUND
								//GR 	GREEK DRACHMA
				case "HKD":
					return "HK";// 	HONG KONG DOLLAR
				case "IDR":
					return "ID";// 	INDONESIAN RUPIAH
								//IE 	IRISH POUND
				case "ILS":
					return "IL";// 	ISRAELI SHEKEL
				case "INR":
					return "IN";// 	INDIAN RUPEE
								//IQ 	IRAQI DINAR
								//IT 	ITALIAN LIRA
				case "JPY":
					return "JP";//	JAPANESE YEN
				case "KRW":
					return "KR";// 	KOREA, REP OF, WON
								//LK 	SRI LANKA RUPEE
				case "MXR":
					return "MX";// 	MEXICAN PESO
				case "MYR":
					return "MY";// 	MALAYSIAN RINGGIT
				case "ANG":
					return "NL";// 	NETHERLANDS GUILDER
				case "NOK":
					return "NO";// 	NORWEGIAN KRONE
				case "NZD":
					return "NZ";// 	NEW ZEALAND DOLLAR
				case "PGK":
					return "PG";// 	PNG KINA
				case "PHP":
					return "PH";//	PHILIPPINE PESO
				case "PKR":
					return "PK";// 	PAKISTAN RUPEE
								//PT 	PORTUGUESE ESCUDO
				case "SAR":
					return "SA";// 	SAUDI RIYAL
				case "SBD":
					return "SB";// 	SOLOMON ISLANDS DOLLAR
				case "SEK":
					return "SE";// 	SWEDISH KRONA
				case "SGD":
					return "SG";// 	SINGAPORE DOLLAR
				case "THB":
					return "TH";// 	THAI BAHT
				case "TWD":
					return "TW";// 	NEW TAIWAN DOLLAR
				case "USD":
					return "US";// 	US DOLLAR
				case "ZAR":
					return "ZA";// 	SOUTH AFRICAN RAND
				default:
					return iso4217Currency;
			}
		}

		/// <summary>
		/// Converts from ISO4217 currency to bodgy ExDoc two character currency (except for AUD and EUR)
		/// </summary>
		/// <param name="threeCharacterCurrency"></param>
		/// <returns></returns>
		public static string ConvertToIso4217(string exDocCurrency)
		{
			switch (exDocCurrency)
			{
				//AT AUSTRIAN SCHILLING
				case "AUD":
					return "AUD";//	AUSTRALIAN DOLLAR
								 //BE 	BELGIAN/LUXEMBOURG FRANC
				case "BR":
					return "BRL";// 	BRAZILLIAN CRUZEIRO
				case "CA":
					return "CAE";// 	CANADIAN DOLLAR
				case "CH":
					return "CHF";// 	SWISS FRANC
				case "CN":
					return "CNY";// 	CHINESE RENMINBI
								 //DE 	DEUTSCHE MARK
				case "DK":
					return "DKK";// 	DANISH KRONE
								 //ES 	SPANISH PESETA
				case "EUR":
					return "EUR";//	EURO DOLLAR
								 //FI 	FINNISH MARKKA
				case "FJ":
					return "FJD";// 	FIJI DOLLAR
								 //FR 	FRENCH FRANC
				case "GB":
					return "GBP";// 	UK POUND
								 //GR 	GREEK DRACHMA
				case "HK":
					return "HKD";// 	HONG KONG DOLLAR
				case "ID":
					return "IDR";// 	INDONESIAN RUPIAH
								 //IE 	IRISH POUND
				case "IL":
					return "ILS";// 	ISRAELI SHEKEL
				case "IN":
					return "INR";// 	INDIAN RUPEE
								 //IQ 	IRAQI DINAR
								 //IT 	ITALIAN LIRA
				case "JP":
					return "JPY";//	JAPANESE YEN
				case "KR":
					return "KRW";// 	KOREA, REP OF, WON
								 //LK 	SRI LANKA RUPEE
				case "MX":
					return "MXR";// 	MEXICAN PESO
				case "MY":
					return "MYR";// 	MALAYSIAN RINGGIT
				case "NL":
					return "ANG";// 	NETHERLANDS GUILDER
				case "NO":
					return "NOK";// 	NORWEGIAN KRONE
				case "NZ":
					return "NZD";// 	NEW ZEALAND DOLLAR
				case "PG":
					return "PGK";// 	PNG KINA
				case "PH":
					return "PHP";//	PHILIPPINE PESO
				case "PK":
					return "PKR";// 	PAKISTAN RUPEE
								 //PT 	PORTUGUESE ESCUDO
				case "SA":
					return "SAR";// 	SAUDI RIYAL
				case "SB":
					return "SBD";// 	SOLOMON ISLANDS DOLLAR
				case "SE":
					return "SEK";// 	SWEDISH KRONA
				case "SG":
					return "SGD";// 	SINGAPORE DOLLAR
				case "TH":
					return "THB";// 	THAI BAHT
				case "TW":
					return "TWD";// 	NEW TAIWAN DOLLAR
				case "US":
					return "USD";// 	US DOLLAR
				case "ZA":
					return "ZAR";// 	SOUTH AFRICAN RAND
				default:
					return exDocCurrency;
			}
		}
	}
}
