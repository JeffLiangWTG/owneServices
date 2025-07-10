using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Accounting.Business.AccountingConstants;
using ResString = Enterprise.Accounting.Business.ResString;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class EnforceZeroBalanceDisbursementsConfiguration : RegistryBusinessObjectTemplate
	{
		public EnforceZeroBalanceDisbursementsConfiguration(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public EnforceZeroBalanceDisbursementsConfiguration()
			: base()
		{
		}

		[List("EnforceZeroBalanceDisbursementsValidationTypes")]
		public ZString EnforceZeroBalanceDisbursementsValidationType
		{
			get { return enforceZeroBalanceDisbursementsValidationType; }
			set
			{
				SetNonPersistentPropertyValue(EnforceZeroBalanceDisbursementsValidationTypeInfo, ref enforceZeroBalanceDisbursementsValidationType, value);
				if (IsMaximumVarianceApplicable)
				{
					if (MaximumVariance == 0)
					{
						DefaultMaxVariance();
					}
				}
				else
				{
					UnitDecimals = Core.Constants.DecimalPlaces.DefaultNumberOfDecimalsForIndividualUnits;
					MaximumVariance = 0;
				}
			}
		}

		ZString enforceZeroBalanceDisbursementsValidationType = EnforceZeroBalanceDisbursementsOption.Default.Code;

		public ZPropertyInfo EnforceZeroBalanceDisbursementsValidationTypeInfo
		{
			get { return GetZPropertyInfo(nameof(EnforceZeroBalanceDisbursementsValidationType)); }
		}

		public CodeDescriptionPairList EnforceZeroBalanceDisbursementsValidationTypes
		{
			get
			{
				return EnforceZeroBalanceDisbursementsOption.CodeList;
			}
		}

		public ZBool IsMaximumVarianceApplicable
		{
			get
			{
				return maximumVarianceApplicableForValidationTypes.Contains(enforceZeroBalanceDisbursementsValidationType);
			}
		}

		readonly List<ZString> maximumVarianceApplicableForValidationTypes = new List<ZString> { EnforceZeroBalanceDisbursementsOption.LocalAmount.Code,
																						EnforceZeroBalanceDisbursementsOption.Either.Code,
																						EnforceZeroBalanceDisbursementsOption.Both.Code };

		public ZInt UnitDecimals { get; private set; }

		[DecimalPlaces(nameof(UnitDecimals))]
		public ZDecimal MaximumVariance
		{
			get
			{
				return maximumVariance;
			}
			set
			{
				SetNonPersistentPropertyValue(MaximumVarianceInfo, ref maximumVariance, value);
				ValidateMaxVariance();
			}
		}

		ZDecimal maximumVariance;

		public ZPropertyInfo MaximumVarianceInfo
		{
			get { return GetZPropertyInfo(nameof(MaximumVariance)); }
		}

		public void DefaultMaxVariance()
		{
			if (CurrentFallbackLevel != null && CurrentFactory != null)
			{
				var company = CurrentFactory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, CurrentFallbackLevel.CompanyPK(false)));
				var currency = company?.LocalCurrency;
				var subUnitMultiplier = 10.0;
				if (currency != null && currency.RX_SubUnitRatio > 0)
				{
					UnitDecimals = (ZInt)Math.Log10(currency.RX_SubUnitRatio);
					MaximumVariance = new ZDecimal(subUnitMultiplier / currency.RX_SubUnitRatio);
				}
			}
			else
			{
				UnitDecimals = Core.Constants.DecimalPlaces.DefaultNumberOfDecimalsForIndividualUnits;
				MaximumVariance = 0;
			}
		}

		ZString maximumVarianceErrorMessage => ResString.GetMultilingualString("D6A770A6-E356-48F0-837B-D5967298B5FB", "Enter a numeric value greater than or equal to zero for Maximum Variance.");

		public void ValidateMaxVariance()
		{
			MaximumVarianceInfo.ClearAllNotifications();
			if (MaximumVariance < 0.0)
			{
				MaximumVarianceInfo.AddError(maximumVarianceErrorMessage);
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateMaxVariance();
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(nameof(EnforceZeroBalanceDisbursementsValidationType), enforceZeroBalanceDisbursementsValidationType);
			writer.WriteElementString(nameof(UnitDecimals), UnitDecimals.ToString());
			writer.WriteElementString(nameof(MaximumVariance), MaximumVariance.ToString());
		}
		protected override void ReadElements(XmlReaderWrapper reader)
		{
			EnforceZeroBalanceDisbursementsValidationType = new ZString(reader.ReadElementString(nameof(EnforceZeroBalanceDisbursementsValidationType)));
			UnitDecimals = new ZInt(reader.ReadElementString(nameof(UnitDecimals)));
			MaximumVariance = new ZDecimal(reader.ReadElementString(nameof(MaximumVariance)));
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EnforceZeroBalanceDisbursementsConfiguration(fallbackLevel, factory);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			EnforceZeroBalanceDisbursementsConfiguration castedClone = (EnforceZeroBalanceDisbursementsConfiguration)clone;
			castedClone.EnforceZeroBalanceDisbursementsValidationType = EnforceZeroBalanceDisbursementsValidationType;
			castedClone.UnitDecimals = UnitDecimals;
			castedClone.MaximumVariance = MaximumVariance;
		}
	}
}