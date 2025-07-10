using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;
using Res = Enterprise.Accounting.Business.Res;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class CostVarianceApproval : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string VarianceCalculationStyle = "VarianceCalculationStyle";
			public const string VarianceComparisonOption = "VarianceComparisonOption";
			public const string AutoTickFinalFlag = "AutoTickFinalFlag";
			public const string AuthorisationRequirements = "AuthorisationRequirements";
		}

		#endregion

		public CostVarianceApproval()
			: base()
		{
		}

		public CostVarianceApproval(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CostVarianceApproval(fallbackLevel, null);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			CostVarianceApproval costVarianceApprovalClone = (CostVarianceApproval)clone;
			if (fAuthorisationRequirements != null)
			{
				costVarianceApprovalClone.fAuthorisationRequirements = (CostVarianceApprovalAuthorisationRequirementCollection)fAuthorisationRequirements.Clone(costVarianceApprovalClone.CurrentFallbackLevel, costVarianceApprovalClone.Factory);
				costVarianceApprovalClone.fAuthorisationRequirements.CurrentFallbackLevel = fAuthorisationRequirements.CurrentFallbackLevel;
				costVarianceApprovalClone.fAuthorisationRequirements.Parent = fAuthorisationRequirements.Parent;
				costVarianceApprovalClone.RegisterEditableChildObject(costVarianceApprovalClone.fAuthorisationRequirements);
			}
		}

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();

			VarianceCalculationStyle = Constants.CostVarianceCalculationStyle.LocalExTaxAmount;
			VarianceComparisonOption = Constants.CostVarianceComparisonOption.Job;
		}

		#region Bound Properties

		#region Variance Calculation Style

		ZString fVarianceCalculationStyle;

		[MaxLength(3)]
		[List("VarianceCalculationStylesList")]
		public ZString VarianceCalculationStyle
		{
			get { return fVarianceCalculationStyle; }
			set
			{
				SetNonPersistentPropertyValue(VarianceCalculationStyleInfo, ref fVarianceCalculationStyle, value);
				if (!IsValidationSuspended)
				{
					ValidateVarianceCalculationStyle();
				}
				AuthorisationRequirements.RefreshBinding();
			}
		}

		public ZPropertyInfo VarianceCalculationStyleInfo
		{
			get { return GetZPropertyInfo(Schema.VarianceCalculationStyle, "Variance Calculation Style"); }
		}

		#endregion

		#region Variance Comparison Option

		ZString fVarianceComparisonOption;

		[MaxLength(3)]
		[List("VarianceComparisonOptionsList")]
		public ZString VarianceComparisonOption
		{
			get { return fVarianceComparisonOption; }
			set
			{
				SetNonPersistentPropertyValue(VarianceComparisonOptionInfo, ref fVarianceComparisonOption, value);
				if (!IsValidationSuspended)
				{
					ValidateVarianceComparisonOption();
				}
			}
		}

		public ZPropertyInfo VarianceComparisonOptionInfo
		{
			get { return GetZPropertyInfo(Schema.VarianceComparisonOption, "Variance Comparison Option"); }
		}

		#endregion

		#region Auto Tick Final Flag

		ZBool autoTickFinalFlag;

		public ZBool AutoTickFinalFlag
		{
			get { return autoTickFinalFlag; }
			set
			{
				SetNonPersistentPropertyValue(AutoTickFinalFlagInfo, ref autoTickFinalFlag, value);

				if (!IsValidationSuspended && value)
				{
					ValidateAutoTickFinalFlag();
				}
				AuthorisationRequirements.RefreshBinding();
			}
		}

		public ZPropertyInfo AutoTickFinalFlagInfo
		{
			get { return GetZPropertyInfo(Schema.AutoTickFinalFlag); }
		}

		#endregion

		#region Autorisation Requirements

		CostVarianceApprovalAuthorisationRequirementCollection fAuthorisationRequirements;
		public CostVarianceApprovalAuthorisationRequirementCollection AuthorisationRequirements
		{
			get
			{
				if (fAuthorisationRequirements == null)
				{
					fAuthorisationRequirements = new CostVarianceApprovalAuthorisationRequirementCollection(this, CurrentFallbackLevel, null);
					RegisterEditableChildObject(fAuthorisationRequirements);
				}
				fAuthorisationRequirements.CurrentFallbackLevel = CurrentFallbackLevel;
				fAuthorisationRequirements.Parent = this;
				return fAuthorisationRequirements;
			}
		}

		ZXmlSerializer fAuthorisationRequirementsSerialiser;
		ZXmlSerializer AuthorisationRequirementsSerialiser
		{
			get
			{
				return fAuthorisationRequirementsSerialiser ?? (fAuthorisationRequirementsSerialiser = ZXmlSerializer.New(typeof(CostVarianceApprovalAuthorisationRequirementCollection)));
			}
		}

		#endregion

		#endregion

		#region Lists

		CodeDescriptionPairList fVarianceCalculationStylesList;
		public CodeDescriptionPairList VarianceCalculationStylesList
		{
			get
			{
				if (fVarianceCalculationStylesList == null)
				{
					fVarianceCalculationStylesList = new CodeDescriptionPairList();
					fVarianceCalculationStylesList.AddPair(Constants.CostVarianceCalculationStyle.LocalExTaxAmount, Res.GetString("1840157a-de75-4d14-bec3-b75c920ef292", "Local Cost Amount (Excluding Tax)"));
					fVarianceCalculationStylesList.AddPair(Constants.CostVarianceCalculationStyle.PercentageVariance, Res.GetString("3c421355-162e-43e0-8bc1-c71ee771a41e", "Percentage Variance"));
					fVarianceCalculationStylesList.AddPair(Constants.CostVarianceCalculationStyle.PercentageVarianceAndMaximumLocalExTaxVariance, Res.GetString("24927396-d5e5-48e9-bd85-385eb8f27f8b", "Percentage Variance with Maximum Local Ex Tax Amount"));
					fVarianceCalculationStylesList.AddPair(Constants.CostVarianceCalculationStyle.PercentageVarianceAndLocalCostAmountExTax, Res.GetString("9C4943D3-8C0A-406E-A7FF-8326B0993E03", "Percentage Variance and Local Cost Amount (Excluding Tax)"));
				}
				return fVarianceCalculationStylesList;
			}
		}

		CodeDescriptionPairList fVarianceComparisonOptionsList;
		public CodeDescriptionPairList VarianceComparisonOptionsList
		{
			get
			{
				if (fVarianceComparisonOptionsList == null)
				{
					fVarianceComparisonOptionsList = new CodeDescriptionPairList();
					fVarianceComparisonOptionsList.AddPair(Constants.CostVarianceComparisonOption.Job, Res.GetString("31e7960d-d5f6-4078-a4c3-4cca68bba9cf", "Variance by Job, Br & Dept of Charges being posted"));
					fVarianceComparisonOptionsList.AddPair(Constants.CostVarianceComparisonOption.JobAndChargeCode, Res.GetString("4be93668-3236-465f-8ffd-d6497b81628c", "Variance by Charge Code, Job, Br & Dept being posted"));
					fVarianceComparisonOptionsList.AddPair(Constants.CostVarianceComparisonOption.JobAndCreditor, Res.GetString("E5A1A2F0-9AE6-4586-8054-EF7FA5FD0D87", "Variance by Job, Creditor, Br & Dept of Charges being posted"));
					fVarianceComparisonOptionsList.AddPair(Constants.CostVarianceComparisonOption.ImportedChargeOrJob, Res.GetString("fd4bdd7e-3f75-4633-a407-ecd4245cc171", "Variance by Charge being imported, fallback to {0}", Constants.CostVarianceComparisonOption.Job));
					fVarianceComparisonOptionsList.AddPair(Constants.CostVarianceComparisonOption.ImportedChargeOrJobChargeCode, Res.GetString("3f676799-a690-453d-93c5-b95ae6ccd93f", "Variance by Charge being imported, fallback to {0}", Constants.CostVarianceComparisonOption.JobAndChargeCode));
					fVarianceComparisonOptionsList.AddPair(Constants.CostVarianceComparisonOption.ImportedChargeOrCreditor, Res.GetString("BD404A0A-3132-4794-A4DB-D5A60AB62FCA", "Variance by Charge being imported, fallback to {0}", Constants.CostVarianceComparisonOption.JobAndCreditor));
				}
				return fVarianceComparisonOptionsList;
			}
		}

		#endregion

		#region Validation

		public void ValidateVarianceCalculationStyle()
		{
			VarianceCalculationStyleInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(VarianceCalculationStyleInfo);
			ListValidation.ErrorIfInvalidCode(VarianceCalculationStyleInfo, VarianceCalculationStylesList);
		}

		public void ValidateVarianceComparisonOption()
		{
			VarianceComparisonOptionInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(VarianceComparisonOptionInfo);
			ListValidation.ErrorIfInvalidCode(VarianceComparisonOptionInfo, VarianceComparisonOptionsList);
		}

		public void ValidateAutoTickFinalFlag()
		{
			AutoTickFinalFlagInfo.ClearAllNotifications();

			if (CurrentFallbackLevel != null)
			{
				var payableFinalFlag = AccountingConfigurationRegistry.Instance.PayableFinalFlag.GetFallBackValueAtAllLevels(CurrentFallbackLevel.CompanyPK(false), Guid.Empty, Guid.Empty);
				var payableFinalFlagForConsolCost = AccountingConfigurationRegistry.Instance.PayableFinalFlagForConsolCost.GetFallBackValueAtAllLevels(CurrentFallbackLevel.CompanyPK(false), Guid.Empty, Guid.Empty);
				if (AutoTickFinalFlag && (payableFinalFlag || payableFinalFlagForConsolCost))
				{
					AutoTickFinalFlagInfo.AddError(Res.GetString("2CCAABC1-BFB1-4F1A-A511-FF2FA844EE53", "This flag cannot be ticked unless both 'Final Flag' and 'Final Flag Default for Consol Costs on AP Invoice Screen' registry items under 'Accounting > Payable Defaults > Default Settings' are set to 'No'."));
				}
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateVarianceCalculationStyle();
			ValidateVarianceComparisonOption();
			ValidateAutoTickFinalFlag();
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.VarianceCalculationStyle, VarianceCalculationStyle);
			writer.WriteElementString(Schema.VarianceComparisonOption, VarianceComparisonOption);
			AuthorisationRequirementsSerialiser.Serialize(writer, AuthorisationRequirements);
			writer.WriteElementString(Schema.AutoTickFinalFlag, AutoTickFinalFlag.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			VarianceCalculationStyle = reader.ReadElementString(Schema.VarianceCalculationStyle);
			VarianceComparisonOption = reader.ReadElementString(Schema.VarianceComparisonOption);
			fAuthorisationRequirements = (CostVarianceApprovalAuthorisationRequirementCollection)AuthorisationRequirementsSerialiser.Deserialize(reader);
			fAuthorisationRequirements.Parent = this;
			AutoTickFinalFlag = reader.ReadElementStringAsZBool(Schema.AutoTickFinalFlag);
			RegisterEditableChildObject(fAuthorisationRequirements);
		}

		#endregion
	}
}
