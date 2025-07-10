using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class ClassificationDataLoad : ClassificationDataLoad<CusClassification>, IExposeMethodsForPGADataLoad
	{
		public static class CAFieldNames
		{
			public const string VFDCode = "VFDCode";
			public const string TariffTreatment = "TariffTreatment";
			public const string Tariff99Code = "Tariff99Code";
			public const string AuthorityNum = "AuthorityNum";
			public const string TRSNum = "TRSNum";
			public const string Manufacturer = "Manufacturer";
			public const string GSTCode = "GSTCode";
			public const string ExciseExemptCode = "ExciseExemptCode";
			public const string ExciseRateCode = "ExciseRateCode";
			public const string SIMACode = "SIMACode";
			public const string SIMADumpingNumber = "SIMADumpingNumber";
			public const string OriginCountry = "OriginCountry";
		}

		protected override void ProcessCountrySpecificData(ClassificationDataToLoad dataToLoad, CusClassification classification)
		{
			if (dataToLoad is ClassificationDataToLoadForCA data)
			{
				SetValue(classification.CCA_ValueForDutyCodeInfo, data.VFDCode);
				SetValue(classification.CCA_TreatmentCodeInfo, data.TariffTreatment);
				SetValue(classification.CCA_99TariffCodeInfo, data.Tariff99Code);
				SetValue(classification.CCA_AuthorityNumberInfo, data.AuthorityNum);
				SetValue(classification.CCA_TRSNumberInfo, data.TRSNum);
				SetValue(classification.CCA_GSTStatusCodeInfo, data.GSTCode);
				SetValue(classification.CCA_ETExemptionInfo, data.ExciseExemptCode);
				SetValue(classification.CCA_ETRateCodeInfo, data.ExciseRateCode);
				SetValue(classification.CCA_RN_NKOriginInfo, data.OriginCountry);

				if (classification.IsHTS)
				{
					if (data.Manufacturer.HasValue)
					{
						using (classification.SetterSuspender.ResumeSetting(CusClassification.Schema.CCA_OA_Manufacturer))
						{
							classification.CCA_OA_Manufacturer_ZAddress.OrgPK = OrgHeader.LoadFromCode(Factory, data.Manufacturer.Value)?.PK ?? ZGuid.Empty;
						}
					}
					if (data is IPGADataToLoad pgaDataToLoad)
					{
						var helper = new CAOrgSupplierPartAndClassificationDataLoad_PGAHelper(Factory, this);
						helper.SetPGAIndicator(classification, pgaDataToLoad);
					}

					if (!data.SIMACode.IsEmpty)
					{
						classification.CCA_SIMADumpingNumber = data.SIMADumpingNumber.Left(classification.CCA_SIMADumpingNumberInfo.MaxLength);

						var simaTax = classification.DutiesAndTaxes.Cast<DutyAndTax>().FirstOrDefault(x => DutyAndTaxTypes.IsSIMATaxCode(x.C1_TaxType));
						if (simaTax != null)
						{
							var simaExemptCode = data.SIMACode.Left(simaTax.C1_ExemptCodeInfo.MaxLength);
							if (!classification.HasMultipleADDs || simaExemptCode.EndsWith("0", System.StringComparison.OrdinalIgnoreCase))
							{
								simaTax.C1_ExemptCode = simaExemptCode;
							}
							else if (classification.HasMultipleADDs)
							{
								DisplayLogMessage(Res.GetString("F4BA4977-9794-4BF4-AAE7-B828735D5C79", "Warning – Classification code {0} has multiple matching SIMA dumping codes. SIMA rates could not be loaded.", classification.CC_LookupCode));
								classification.DutiesAndTaxes.DeleteAll();
							}
						}
					}
				}
			}
		}

		protected override IEnumerable<ZPropertyInfo> GetCountrySpecificClassificationFieldsUponWhichToRunFriendlyValidation(CusClassification classification)
		{
			yield return classification.CCA_ValueForDutyCodeInfo;
			yield return classification.CCA_TreatmentCodeInfo;
			yield return classification.CCA_99TariffCodeInfo;
			yield return classification.CCA_AuthorityNumberInfo;
			yield return classification.CCA_TRSNumberInfo;
			yield return classification.CCA_OA_ManufacturerInfo;
			yield return classification.CCA_GSTStatusCodeInfo;
			yield return classification.CCA_ETExemptionInfo;
			yield return classification.CCA_ETRateCodeInfo;
			yield return classification.CCA_SIMADumpingNumberInfo;
			yield return classification.CCA_RN_NKOriginInfo;
		}

		protected override IEnumerable<string> GetCountrySpecificFieldNames()
		{
			yield return CAFieldNames.VFDCode;
			yield return CAFieldNames.TariffTreatment;
			yield return CAFieldNames.Tariff99Code;
			yield return CAFieldNames.AuthorityNum;
			yield return CAFieldNames.TRSNum;
			yield return CAFieldNames.Manufacturer;
			yield return CAFieldNames.GSTCode;
			yield return CAFieldNames.ExciseExemptCode;
			yield return CAFieldNames.ExciseRateCode;
			yield return CAFieldNames.SIMACode;
			yield return CAFieldNames.SIMADumpingNumber;
			yield return CAFieldNames.OriginCountry;

			foreach (var property in CAOrgSupplierPartAndClassificationDataLoad_PGAHelper.PGAFieldNames.GetPGAFieldNames())
			{
				yield return property;
			}
		}

		protected override ZString GetTariffDescription(ZString tariff, ZString type)
		{
			return TariffDescriptionHelper.GetTariffDescription(Factory, tariff, type == ClassificationType.EXP);
		}

		protected override CusClassification LoadClassificationIfItExists(ZString lookupCode, ZString type)
		{
			return new CusClassification.Loader(Factory).Load(lookupCode, type);
		}

		protected override bool IsClassificationTypeValid(string type)
		{
			var upperType = type.ToUpper(CultureInfo.InvariantCulture);
			return upperType.Equals(ClassificationType.IMP) || upperType.Equals(ClassificationType.EXP);
		}

		protected override void DisplayInvalidClassificationTypeMessage()
		{
			DisplayExcludedRecordMessage(Res.GetString("B539D810-DEFD-461B-AC14-E53FA116BC25", ": Record Excluded - Classification type should only be '{0}' or '{1}'", ClassificationType.IMP, ClassificationType.EXP));
		}

		protected override ClassificationDataToLoad GetClassificationDataToLoad()
		{
			return new ClassificationDataToLoadForCA();
		}

		#region IAddToDisposableList

		void IExposeMethodsForPGADataLoad.AddToDisposableList(IDisposable disposable)
		{
			base.AddToDisposableList(disposable);
		}

		bool IExposeMethodsForPGADataLoad.HasColumn(string columnName)
		{
			return base.HasColumn(columnName);
		}

		void IExposeMethodsForPGADataLoad.DisplayLogMessage(string message)
		{
			base.DisplayLogMessage(message);
		}

		#endregion

		protected override IDisposable GetCusClassificationPropertiesToSuspendSetting(BaseCusClassification classification)
		{
			IDisposable disposable = null;
			if (classification is CusClassification classification1)
			{
				var propertiesList = new List<ZString>();
				if (HasColumn(CAFieldNames.Manufacturer))
				{
					propertiesList.Add(CusClassPartPivot.Schema.CCA_OA_Manufacturer);
				}
				propertiesList.AddRange(CAOrgSupplierPartAndClassificationDataLoad_PGAHelper.PGAFieldNames.GetPGAPropertiesToSuspendSetting(this));
				if (propertiesList.Any())
				{
					disposable = classification1.SetterSuspender.SuspendSetting(propertiesList.ToArray());
				}
			}
			return disposable;
		}
	}
}
