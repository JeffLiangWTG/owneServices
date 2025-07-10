using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class HouseBillOfLadingType : RegistryBusinessObject
	{
		public HouseBillOfLadingType()
		{
		}

		public HouseBillOfLadingType(FallbackLevel fallbackLevel) : base(fallbackLevel)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new HouseBillOfLadingType(fallbackLevel);
		}

		#region Validation Overrides

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateLogoCode();
			ValidateTermsAndConditionsCode();
			ValidatePrintLogoFormBuilder();
		}

		protected override void ValidateDescriptionCore()
		{
			MandatoryValidation.CheckEntered(DescriptionInfo, Res.GetString("4b9a6c00-b6e8-4b60-811a-b3cc8f63da21", "Description"));
		}

		protected override int MaxDescriptionLength
		{
			get { return 50; }
		}

		#endregion

		#region New Bound Properties

		#region Logo Code

		[CargoWise.ComponentModel.MaxLength(3)]
		[List(nameof(LogoImageCodeList))]
		public ZString LogoCode
		{
			get { return logoCode; }
			set
			{
				CheckMaximumLength(LogoCodeInfo, value);
				SetNonPersistentPropertyValue<ZString>(LogoCodeInfo, ref logoCode, value);
				logoImage = null;
				if (!IsValidationSuspended)
				{
					ValidateLogoCode();
				}
			}
		}

		public ZPropertyInfo LogoCodeInfo => GetZPropertyInfo(nameof(LogoCode));

		public void ValidateLogoCode()
		{
			LogoCodeInfo.ClearAllNotifications();

			if (CurrentFallbackLevel != null)
			{
				ListValidation.ErrorIfInvalidCode(LogoCodeInfo, LogoImageCodeList);
			}
		}

		ZString logoCode;

		#endregion

		#region Terms and Conditions Code

		[CargoWise.ComponentModel.MaxLength(3)]
		[List(nameof(TermsAndConditionImageCodeList))]
		public ZString TermsAndConditionsCode
		{
			get { return termsAndConditionsCode; }
			set
			{
				CheckMaximumLength(TermsAndConditionsCodeInfo, value);
				SetNonPersistentPropertyValue<ZString>(TermsAndConditionsCodeInfo, ref termsAndConditionsCode, value);
				termsAndConditionsImage = null;
				if (!IsValidationSuspended)
				{
					ValidateTermsAndConditionsCode();
				}
			}
		}

		public ZPropertyInfo TermsAndConditionsCodeInfo => GetZPropertyInfo(nameof(TermsAndConditionsCode));

		public void ValidateTermsAndConditionsCode()
		{
			TermsAndConditionsCodeInfo.ClearAllNotifications();

			if (CurrentFallbackLevel != null)
			{
				ListValidation.ErrorIfInvalidCode(TermsAndConditionsCodeInfo, TermsAndConditionImageCodeList);
			}
		}

		ZString termsAndConditionsCode;

		#endregion

		#region Pre Printed

		public ZBool PrePrinted
		{
			get { return prePrinted; }
			set { SetNonPersistentPropertyValue<ZBool>(PrePrintedInfo, ref prePrinted, value); }
		}

		public ZPropertyInfo PrePrintedInfo => GetZPropertyInfo(nameof(PrePrinted));

		ZBool prePrinted;

		#endregion

		#region Print Logo

		public ZBool PrintLogo
		{
			get { return printLogo; }
			set { SetNonPersistentPropertyValue<ZBool>(PrintLogoInfo, ref printLogo, value); }
		}

		public ZPropertyInfo PrintLogoInfo => GetZPropertyInfo(nameof(PrintLogo));

		ZBool printLogo;

		#endregion

		#region Print Logo Form Builder

		[CargoWise.ComponentModel.MaxLength(3)]
		[List(nameof(PrintLogoFormBuilderList))]
		public ZString PrintLogoInFormBuilder
		{
			get { return printLogoInFormBuilder; }
			set
			{
				if (SetNonPersistentPropertyValue(PrintLogoInFormBuilderInfo, ref printLogoInFormBuilder, value)
					&& !IsValidationSuspended)
				{
					ValidatePrintLogoFormBuilder();
				}
			}
		}

		public ZPropertyInfo PrintLogoInFormBuilderInfo => GetZPropertyInfo(nameof(PrintLogoInFormBuilder));

		ZString printLogoInFormBuilder;

		void ValidatePrintLogoFormBuilder()
		{
			PrintLogoInFormBuilderInfo.ClearAllNotifications();

			if (CurrentFallbackLevel != null)
			{
				MandatoryValidation.CheckEntered(PrintLogoInFormBuilderInfo);
				ListValidation.ErrorIfInvalidCode(PrintLogoInFormBuilderInfo, PrintLogoFormBuilderList);
			}
		}

		#endregion

		#endregion

		#region Calculated Properties

		public RegistryImage LogoImage
		{
			get
			{
				if (logoImage == null)
				{
					logoImage = FreightDataRegistry.Instance.HouseBillOfLadingLogoImages.Value.FindByCode(LogoCode);
				}

				return logoImage;
			}
		}

		public HouseBillOfLadingTermsAndConditions TermsAndConditionsImage
		{
			get
			{
				if (termsAndConditionsImage == null)
				{
					termsAndConditionsImage = FreightDataRegistry.Instance.HouseBillOfLadingTermsAndConditionsImages.Value.FindByCodeForDeliveryModeALL(TermsAndConditionsCode);
				}

				return termsAndConditionsImage;
			}
		}

		RegistryImage logoImage;
		HouseBillOfLadingTermsAndConditions termsAndConditionsImage;

		#endregion

		#region Lookups

		public CodeDescriptionPairList LogoImageCodeList
		{
			get
			{
				if (fLogoImageCodeList == null)
				{
					fLogoImageCodeList = FreightDataRegistry.Instance.HouseBillOfLadingLogoImages.LatestImagesCodeDescList(CurrentFallbackLevel);
				}

				return fLogoImageCodeList;
			}
		}

		public CodeDescriptionPairList TermsAndConditionImageCodeList
		{
			get
			{
				if (fTermsAndConditionImageCodeList == null)
				{
					fTermsAndConditionImageCodeList = FreightDataRegistry.Instance.HouseBillOfLadingTermsAndConditionsImages.LatestImagesCodeDescList(CurrentFallbackLevel);
				}

				return fTermsAndConditionImageCodeList;
			}
		}

		public CodeDescriptionPairList PrintLogoFormBuilderList
		{
			get
			{
				if (printLogoFormBuilderList == null)
				{
					printLogoFormBuilderList = new PrintLogoOptions();
				}

				return printLogoFormBuilderList;
			}
		}

		CodeDescriptionPairList fLogoImageCodeList;
		CodeDescriptionPairList fTermsAndConditionImageCodeList;
		CodeDescriptionPairList printLogoFormBuilderList;

		#endregion

		#region Write/Read XML

		protected override void WriteMoreElements(XmlWriter writer)
		{
			writer.WriteElementString(nameof(LogoCode), LogoCode);
			writer.WriteElementString(nameof(TermsAndConditionsCode), TermsAndConditionsCode);
			writer.WriteElementString(nameof(PrePrinted), PrePrinted.ToString());
			writer.WriteElementString(nameof(PrintLogo), PrintLogo.ToString());
			writer.WriteElementString(nameof(PrintLogoInFormBuilder), PrintLogoInFormBuilder.ToString());
		}

		protected override void ReadMoreElements(XmlReader reader)
		{
			LogoCode = reader.ReadElementString(nameof(LogoCode));
			TermsAndConditionsCode = reader.ReadElementString(nameof(TermsAndConditionsCode));
			PrePrinted = new ZBool(reader.ReadElementString(nameof(PrePrinted)));
			PrintLogo = new ZBool(reader.ReadElementString(nameof(PrintLogo)));
			PrintLogoInFormBuilder = new ZString(reader.ReadElementString(nameof(PrintLogoInFormBuilder)));
		}

		#endregion

		public HouseBillOfLadingTermsAndConditions GetTermsAndConditionsImage(string deliveryMode)
		{
			return FreightDataRegistry.Instance.HouseBillOfLadingTermsAndConditionsImages.Value.FindByCodeAndDeliveryMode(TermsAndConditionsCode, deliveryMode);
		}

		protected override FallbackLevel CurrentFallbackLevelCore
		{
			get { return base.CurrentFallbackLevelCore; }
			set
			{
				base.CurrentFallbackLevelCore = value;

				fLogoImageCodeList = null;
				fTermsAndConditionImageCodeList = null;
			}
		}
	}
}
