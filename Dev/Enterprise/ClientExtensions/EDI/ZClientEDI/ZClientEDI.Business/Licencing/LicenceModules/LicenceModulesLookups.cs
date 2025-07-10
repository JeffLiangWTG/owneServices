using System.Linq;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class LicenceModulesLookups : AutoLicenceModulesLookups
	{
		public LicenceModulesLookups(AutoLicenceModules parent)
			: base(parent)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public CodeDescriptionPairList LicenceTypesList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair(LicenceTypes.Codes.NON, LicenceTypes.Descriptions.NON);

				string headerLevelLicenceType = (Parent != null && Parent.LicHeader != null) ? Parent.LicHeader.LA_LicenceAdvStdOth : ZString.Empty;

				var checkpoint = Env.Licence.GetCheckpointFromCode(Parent.LM_GroupModuleCode);
				bool isLanguagePack = Parent != null && (checkpoint is LanguageLicenceCheckpoint || checkpoint is LanguageLicenceChildCheckpoint);
				if (isLanguagePack)
				{
					result.AddPair(LicenceTypes.Codes.ODM, LicenceTypes.Descriptions.ODM);

					bool isPurchasableLanguagePack = Parent != null && Env.Licence.LanguagePackLookup.Values.Any(s => s.Name == Parent.LM_GroupModuleCode);
					if (isPurchasableLanguagePack)
					{
						switch (headerLevelLicenceType)
						{
							case LicenceAdvStdOthList.Codes.Global:
							case LicenceAdvStdOthList.Codes.OtherLegacyApplication:
							case LicenceAdvStdOthList.Codes.Advanced:
							case LicenceAdvStdOthList.Codes.Standard:
							case LicenceAdvStdOthList.Codes.ConcurrentExpress:
							case LicenceAdvStdOthList.Codes.ConcurrentRegional:
							case LicenceAdvStdOthList.Codes.ConcurrentCountry:
							case LicenceAdvStdOthList.Codes.ConcurrentUniversal:
								result.AddPair(LicenceTypes.Codes.PUR, LicenceTypes.Descriptions.PUR);
								break;
						}
					}
				}
				else
				{
					result.AddPair(LicenceTypes.Codes.CPT, LicenceTypes.Descriptions.CPT);

					switch (headerLevelLicenceType)
					{
						case LicenceAdvStdOthList.Codes.Global:
						case LicenceAdvStdOthList.Codes.OtherLegacyApplication:
							result.AddPair(LicenceTypes.Codes.PUR, LicenceTypes.Descriptions.PUR);
							result.AddPair(LicenceTypes.Codes.REN, LicenceTypes.Descriptions.REN);
							result.AddPair(LicenceTypes.Codes.TRI, LicenceTypes.Descriptions.TRI);
							break;

						case LicenceAdvStdOthList.Codes.Advanced:
						case LicenceAdvStdOthList.Codes.Standard:
							result.AddPair(LicenceTypes.Codes.PUR, LicenceTypes.Descriptions.PUR);
							result.AddPair(LicenceTypes.Codes.REN, LicenceTypes.Descriptions.REN);
							result.AddPair(LicenceTypes.Codes.TRI, LicenceTypes.Descriptions.TRI);
							result.AddPair(LicenceTypes.Codes.SRU, LicenceTypes.Descriptions.SRU);
							break;

						case LicenceAdvStdOthList.Codes.SeatTransaction:
						case LicenceAdvStdOthList.Codes.ConversionToODPL:
						case LicenceAdvStdOthList.Codes.OnDemand:
							result.AddPair(LicenceTypes.Codes.ODM, LicenceTypes.Descriptions.ODM);
							break;

						case LicenceAdvStdOthList.Codes.Hybrid:
							result.AddPair(LicenceTypes.Codes.ODM, LicenceTypes.Descriptions.ODM);
							result.AddPair(LicenceTypes.Codes.OPN, LicenceTypes.Descriptions.OPN);
							result.AddPair(LicenceTypes.Codes.OTM, LicenceTypes.Descriptions.OTM);
							result.AddPair(LicenceTypes.Codes.SRU, LicenceTypes.Descriptions.SRU);
							break;

						case LicenceAdvStdOthList.Codes.ConcurrentExpress:
						case LicenceAdvStdOthList.Codes.ConcurrentRegional:
						case LicenceAdvStdOthList.Codes.ConcurrentCountry:
						case LicenceAdvStdOthList.Codes.ConcurrentUniversal:
							result.AddPair(LicenceTypes.Codes.PUR, LicenceTypes.Descriptions.PUR);
							result.AddPair(LicenceTypes.Codes.REN, LicenceTypes.Descriptions.REN);
							result.AddPair(LicenceTypes.Codes.TRI, LicenceTypes.Descriptions.TRI);
							result.AddPair(LicenceTypes.Codes.OPN, LicenceTypes.Descriptions.OPN);
							result.AddPair(LicenceTypes.Codes.SRU, LicenceTypes.Descriptions.SRU);
							break;
					}
				}

				return result;
			}
		}

		protected new LicenceModules Parent
		{
			get { return (LicenceModules)base.Parent; }
		}
	}
}
