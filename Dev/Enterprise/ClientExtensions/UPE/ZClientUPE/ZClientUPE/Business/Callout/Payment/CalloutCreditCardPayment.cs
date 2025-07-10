using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.UPE.Business
{
	public class CalloutCreditCardPayment : CalloutPaymentDetails
	{
		public CalloutCreditCardPayment(Callout callout)
			: base(callout)
		{
		}

		[MaxLength(16)]
		public ZString CardNumber
		{
			get { return fCardNumber; }
			set
			{
				if (fCardNumber != value)
				{
					CheckMaximumLength(CardNumberInfo, value);
					SetNonPersistentPropertyValue(CardNumberInfo, ref fCardNumber, value);
				}
			}
		}

		public ZPropertyInfo CardNumberInfo
		{
			get { return GetZPropertyInfo(nameof(CardNumber)); }
		}

		[MaxLength(35)]
		public ZString NameOnCard
		{
			get { return fNameOnCard; }
			set
			{
				if (fNameOnCard != value)
				{
					CheckMaximumLength(NameOnCardInfo, value);
					SetNonPersistentPropertyValue(NameOnCardInfo, ref fNameOnCard, value);
				}
			}
		}

		public ZPropertyInfo NameOnCardInfo
		{
			get { return GetZPropertyInfo(nameof(NameOnCard)); }
		}

		[MaxLength(6)]
		public ZString SecurityCode
		{
			get { return fSecurityCode; }
			set
			{
				if (fSecurityCode != value)
				{
					CheckMaximumLength(SecurityCodeInfo, value);
					SetNonPersistentPropertyValue(SecurityCodeInfo, ref fSecurityCode, value);
				}
			}
		}

		public ZPropertyInfo SecurityCodeInfo
		{
			get { return GetZPropertyInfo(nameof(SecurityCode)); }
		}

		[List("ExpiryDate_Month_List")]
		[MaxLength(2)]
		public ZString ExpiryDate_Month
		{
			get { return fExpiryDate_Month; }
			set
			{
				SetNonPersistentPropertyValue(ExpiryDate_MonthInfo, ref fExpiryDate_Month, value);
			}
		}

		public ZPropertyInfo ExpiryDate_MonthInfo
		{
			get { return GetZPropertyInfo(nameof(ExpiryDate_Month)); }
		}

		public ReadOnlyCodeDescriptionPairList ExpiryDate_Month_List
		{
			get
			{
				if (fExpiryDate_Month_List == null)
				{
					fExpiryDate_Month_List = new CodeDescriptionPairList();
					fExpiryDate_Month_List.AddPair("01");
					fExpiryDate_Month_List.AddPair("02");
					fExpiryDate_Month_List.AddPair("03");
					fExpiryDate_Month_List.AddPair("04");
					fExpiryDate_Month_List.AddPair("05");
					fExpiryDate_Month_List.AddPair("06");
					fExpiryDate_Month_List.AddPair("07");
					fExpiryDate_Month_List.AddPair("08");
					fExpiryDate_Month_List.AddPair("09");
					fExpiryDate_Month_List.AddPair("10");
					fExpiryDate_Month_List.AddPair("11");
					fExpiryDate_Month_List.AddPair("12");
				}

				return fExpiryDate_Month_List;
			}
		}
		CodeDescriptionPairList fExpiryDate_Month_List;

		[List("ExpiryDate_Year_List")]
		[MaxLength(2)]
		public ZString ExpiryDate_Year
		{
			get { return fExpiryDate_Year; }
			set
			{
				SetNonPersistentPropertyValue(ExpiryDate_YearInfo, ref fExpiryDate_Year, value);
			}
		}

		public ZPropertyInfo ExpiryDate_YearInfo
		{
			get { return GetZPropertyInfo(nameof(ExpiryDate_Year)); }
		}

		public ReadOnlyCodeDescriptionPairList ExpiryDate_Year_List
		{
			get
			{
				if (fExpiryDate_Year_List == null)
				{
					fExpiryDate_Year_List = new CodeDescriptionPairList();
					for (int i = ZDateTime.Now.Year - 2000; i < ZDateTime.Now.Year - 2000 + 20; i++)
					{
						fExpiryDate_Year_List.AddPair(i.ToString().Length != 2 ? "0" + i : i.ToString());
					}
				}

				return fExpiryDate_Year_List;
			}
		}
		CodeDescriptionPairList fExpiryDate_Year_List;

		[MaxLength(10)]
		public ZString VPOSCode
		{
			get { return fVPOSCode; }
			set
			{
				SetNonPersistentPropertyValue(VPOSCodeInfo, ref fVPOSCode, value);
			}
		}

		public ZPropertyInfo VPOSCodeInfo
		{
			get { return GetZPropertyInfo(nameof(VPOSCode)); }
		}

		protected override string Reference
		{
			get
			{
				string format = "Card Number: {0:}\r\nExpiry Date: {1}\r\nSecurity Code: {2}\r\nName: {3}\r\nVPOS: {4}";
				return string.Format(format, FormattedCardNumber, ExpiryDate_Month + "/" + ExpiryDate_Year, FormattedSecurityCode, NameOnCard, VPOSCode);
			}
		}

		protected override string PaymentMethodAsText
		{
			get { return "Credit Card"; }
		}

		string FormattedCardNumber
		{
			get
			{
				string xXXX = "XXXX";
				string fourthPart = CardNumber.SubstringSafe(12, 4);

				return string.Format("{0} {1} {2} {3}", xXXX, xXXX, xXXX, fourthPart).Trim();
			}
		}

		string FormattedSecurityCode
		{
			get
			{
				string firstPart = SecurityCode.SubstringSafe(0, 3);
				string secondPart = SecurityCode.SubstringSafe(3, 3);

				return string.Format("{0} {1}", firstPart, secondPart).Trim();
			}
		}

		ZString fCardNumber;
		ZString fNameOnCard;
		ZString fSecurityCode;
		ZString fExpiryDate_Month;
		ZString fExpiryDate_Year;
		ZString fVPOSCode;
	}
}
