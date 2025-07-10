using System;
using System.Drawing;
using System.Linq;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CN.Business
{
	public sealed class CNCustomsDataRegistry : RegistryItemSet
	{
		public override bool IsForProductivityWise => false;

		CNCustomsDataRegistry()
		{
		}

		[ThreadStatic] static CNCustomsDataRegistry fInstance;

		public static CNCustomsDataRegistry Instance => fInstance ?? (fInstance = new CNCustomsDataRegistry());

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Customs_China => CombineCategories(Customs_CountryOrRegion, ResString.GetMultilingualString("BC1BC8E3-24DC-4B0A-9F68-524C62B34BB3", "China"));
		}

		public BooleanRegistryItem TwoStepDeclarationActive => GetItem(
			"TwoStepDeclarationActive",
			() => new BooleanRegistryItem(
				"TwoStepDeclarationActive",
				Categories.Customs_China,
				ResString.GetMultilingualString("1BC51888-1929-40C5-BE8B-BCF05C2EF544", "Enable Two-step Declaration"),
				ResString.GetMultilingualString("74284655-F458-4554-B510-6594ABB08535", "Turning this on will show the Two-step Declaration option on CN Customs Declaration."),
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers,
				false
			)
		);

		public BooleanRegistryItem CNBTHFunctionActive =>
			GetItem("CNBTHFunctionActive",
				() => new BooleanRegistryItem(
					"CNBTHFunctionActive",
					Categories.Customs_China,
					ResString.GetMultilingualString("0C9CF0F7-9838-41E0-9099-0DCF2BE24C31", "Enable generate Import & Export entry on one Declaration"),
					ResString.GetMultilingualString("A2859275-71D6-433A-9FF7-47322B6250F5", "If set to ‘Yes’, the system will show option BTH for Declaration Type on Declaration."),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForDevelopers,
					false
				)
			);

		public CNSWClientSettingRegistryItem CNSWClientSetting =>
			GetItem("CNSWClientApplicationSetting",
				() => new CNSWClientSettingRegistryItem(
					"CNSWClientApplicationSetting",
					Categories.Customs_China,
					ResString.GetMultilingualString("5B6A959A-E48A-4C67-AD85-AA1C11A9FC48", "Single Window Client Application Settings"),
					ResString.GetMultilingualString("8ABB09B7-B8AF-4552-B805-6C26318CC111", "The settings are for Single Window Client Application which is a standalone tool installed on the client’s local machine. The tool sends and receives messages through the China Customs Single Window interface."),
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					new CNSWClientSetting() { RunningIntervalInSeconds = 60 }
				)
				{
					CountryFilterPKs = CountryFilterPKs.China
				}
			);

		public CNDocTemplateForAttachmentRegistryItem CNDocTemplateForAttachment =>
			GetItem("CNDocTemplateForAttachment",
				() => new CNDocTemplateForAttachmentRegistryItem(
					"CNDocTemplateForAttachment",
					Categories.Customs_China,
					ResString.GetMultilingualString("CF5865EF-72ED-45FA-860E-DEEFF8D93AA5", "Document Templates for Generating Attachments"),
					ResString.GetMultilingualString("234E3AAD-65DF-43CB-B21F-74A5AC19F073", "This setting specifies which document templates will be used for generating attachments for entry. You can find the feature on the context menu of Entries grid on Declaration form."),
					RegistryStorageFlags.Company,
					CNDocTemplateForAttachmentCollection.GetDefault()
				)
				{
					CountryFilterPKs = CountryFilterPKs.China
				}
				);

		public CNDeclarationDeadlineWarningThresholdRegistryItem CNDeclarationDeadlineWarningThresholdSetting =>
			GetItem("CNDeclarationDeadlineWarningThreshold",
				() => new CNDeclarationDeadlineWarningThresholdRegistryItem(
					"CNDeclarationDeadlineWarningThreshold",
					Categories.Customs_China,
					ResString.GetMultilingualString("87319134-B0D4-4DF4-9067-69435972035E", "Declaration Deadline Warning Threshold"),
					ResString.GetMultilingualString("E6CFE41A-9AA5-44C5-B99F-AD50A4663C66", "Use this registry setting to set three level thresholds for declaration deadline warning. The jobs on Customs Declarations and Customs Entries module will be displayed in different row colors according to the thresholds (the number of days before its declaration deadline). And those jobs have already been delayed for declaration will be displayed in Delayed Warning Color."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					CNDeclarationDeadlineWarningThresholdCollection.GetDefault()
					)
				{
					CountryFilterPKs = CountryFilterPKs.China
				}
				);

		public BooleanRegistryItem DefaultTradeUnitPriceOnProduct =>
			GetItem("DefaultTradeUnitPriceOnProduct",
				() => new BooleanRegistryItem(
					"DefaultTradeUnitPriceOnProduct",
					Categories.Customs_China,
					ResString.GetMultilingualString("DF92A3E9-DCCD-4A18-ABFE-CE0D5194F470", "Default Trade Unit Price on Product"),
					ResString.GetMultilingualString("BC121CC8-829B-4219-BCC9-CD1041687DC8", "Should copy Trade Unit Price when creating a new Product from Invoice Line?"),
					RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					true
					)
				{
					CountryFilterPKs = CountryFilterPKs.China
				}
			);

		public static Color GetColorByRemainingDays(ZInt remainingDays, string transportMode)
		{
			var result = Color.Empty;
			var cNDeclarationDeadlineWarningThresholds = Instance.CNDeclarationDeadlineWarningThresholdSetting.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).Cast<CNDeclarationDeadlineWarningThreshold>();
			var matchedDeclarationDeadlineWarningThreshold = cNDeclarationDeadlineWarningThresholds.FirstOrDefault(x => x.TransportMode == transportMode) ??
				cNDeclarationDeadlineWarningThresholds.FirstOrDefault(x => x.TransportMode == CNDeclarationDeadlineWarningThreshold.ALL);
			if (matchedDeclarationDeadlineWarningThreshold != null)
			{
				if (remainingDays < 0)
				{
					result = ColorTranslator.FromHtml(matchedDeclarationDeadlineWarningThreshold.DelayedWarningColor);
				}
				else if (remainingDays > 0)
				{
					if (matchedDeclarationDeadlineWarningThreshold.FirstLevelThreshold > 0 && remainingDays <= matchedDeclarationDeadlineWarningThreshold.FirstLevelThreshold)
					{
						result = ColorTranslator.FromHtml(matchedDeclarationDeadlineWarningThreshold.FirstLevelWarningColor);
					}
					else if (matchedDeclarationDeadlineWarningThreshold.SecondLevelThreshold > 0 && remainingDays <= matchedDeclarationDeadlineWarningThreshold.SecondLevelThreshold)
					{
						result = ColorTranslator.FromHtml(matchedDeclarationDeadlineWarningThreshold.SecondLevelWarningColor);
					}
					else if (matchedDeclarationDeadlineWarningThreshold.ThirdLevelThreshold > 0 && remainingDays <= matchedDeclarationDeadlineWarningThreshold.ThirdLevelThreshold)
					{
						result = ColorTranslator.FromHtml(matchedDeclarationDeadlineWarningThreshold.ThirdLevelWarningColor);
					}
				}
			}
			return result;
		}
	}
}
