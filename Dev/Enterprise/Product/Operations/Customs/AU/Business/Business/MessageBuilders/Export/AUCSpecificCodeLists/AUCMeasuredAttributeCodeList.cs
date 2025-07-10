using Enterprise.Edifact.D99B.Elements;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUCMeasuredAttributeCodeList : MeasuredAttributeCodeList
	{
		protected AUCMeasuredAttributeCodeList(string codeValue) : base(codeValue) { }
		public static AUCMeasuredAttributeCodeList GoldGPT
		{
			get
			{
				return new AUCMeasuredAttributeCodeList("AU");
			}
		}

		public static AUCMeasuredAttributeCodeList SilverGPT
		{
			get
			{
				return new AUCMeasuredAttributeCodeList("GPT");
			}
		}

		public static AUCMeasuredAttributeCodeList CopperPER
		{
			get
			{
				return new AUCMeasuredAttributeCodeList("CU");
			}
		}

		public static AUCMeasuredAttributeCodeList LeadPER
		{
			get
			{
				return new AUCMeasuredAttributeCodeList("PB");
			}
		}

		public static AUCMeasuredAttributeCodeList PlatinumPER
		{
			get
			{
				return new AUCMeasuredAttributeCodeList("PT");
			}
		}

		public static AUCMeasuredAttributeCodeList NickelPER
		{
			get
			{
				return new AUCMeasuredAttributeCodeList("NI");
			}
		}

		public static AUCMeasuredAttributeCodeList TinPER
		{
			get
			{
				return new AUCMeasuredAttributeCodeList("SN");
			}
		}

		public static AUCMeasuredAttributeCodeList TungstenPER
		{
			get
			{
				return new AUCMeasuredAttributeCodeList("WO");
			}
		}

		public static AUCMeasuredAttributeCodeList ZincPER
		{
			get
			{
				return new AUCMeasuredAttributeCodeList("ZN");
			}
		}
	}
}
