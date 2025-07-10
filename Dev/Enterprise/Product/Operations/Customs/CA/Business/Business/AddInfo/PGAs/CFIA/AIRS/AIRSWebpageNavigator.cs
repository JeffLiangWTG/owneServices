using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.CA.Business
{
	public class AIRSWebpageNavigator : NonPersistentBusinessObject, IObsoleteValidation
	{
		public AIRSWebpageNavigator(BusinessObjectFactory factory, ZString tariffCodePassedIn)
			: base(factory)
		{
			this.TariffCodePassedIn = tariffCodePassedIn;
		}

		#region Schema
		public static class Schema
		{
			public const string AG_EndUseCode = "AG_EndUseCode";
			public const int AG_EndUseCodeMaxLength = 3;
			public const string AG_ExtensionCode = "AG_ExtensionCode";
			public const int AG_ExtensionCodeMaxLength = 6;
			public const string AG_Miscellaneous = "AG_Miscellaneous";
			public const int AG_MiscellaneousMaxLength = 3;
		}
		#endregion

		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.Customs.CA.Business.AIRSWebpageNavigator|AG_EndUseCode", Caption = "End Use")]
		[MaxLength(Schema.AG_EndUseCodeMaxLength)]
		public ZString AG_EndUseCode { get; set; }

		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.Customs.CA.Business.AIRSWebpageNavigator|AG_ExtensionCode", Caption = "OGD Extension")]
		[MaxLength(Schema.AG_ExtensionCodeMaxLength)]
		public ZString AG_ExtensionCode { get; set; }

		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.Customs.CA.Business.AIRSWebpageNavigator|AG_Miscellaneous", Caption = "Miscellaneous")]
		[MaxLength(Schema.AG_MiscellaneousMaxLength)]
		public ZString AG_Miscellaneous { get; set; }

		public ZString TariffCodePassedIn { get; }

		[CodeAlive("Used Code")]
		enum GridTitleText { Materialized, DeMaterialized, AIRSRegistration, OR, UnDefined }

		public AIRSWebpageConfiguration WebPageConfiguration
		{
			get
			{
				return webPageConfiguration ?? (webPageConfiguration = new AIRSWebpageConfiguration(Factory));
			}
		}
		AIRSWebpageConfiguration webPageConfiguration;

		public List<AIRSLPCOSelection> LPCOList => lPCOList ?? (lPCOList = new List<AIRSLPCOSelection>());
		List<AIRSLPCOSelection> lPCOList;
	}
}
