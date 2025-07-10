using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class ElectronicProcessingChargeProvider : IElectronicProcessingChargeProvider
	{
		void IElectronicProcessingChargeProvider.CreateElectronicProcessingCharge(Job job)
		{
			if (AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.GetFallBackValueAtAllLevels(job.JH_GC.ToGuid(), Guid.Empty, Guid.Empty)
				&& ObjectFactory.Get<IAccounting>().IsIncludedInElectronicProcessingChargeConfiguration(job.JH_A_JOP, job.JobType.Code))
			{
				if (AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCode.Value != Guid.Empty
				&& AccountingConfigurationRegistry.Instance.ElectronicProcessingChargePayableClearingAccount.Value != Guid.Empty)
				{
					var (currency, price) = GetDisbursementFee();
					if (price == decimal.Zero)
					{
						return;
					}

					var jobFactory = job.Factory;
					var globalCharge = jobFactory.Load<AccChargeCode>(AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCode.Value);
					var chargeInCurrenctCompany = globalCharge.ChildChargeCodes.FirstOrDefault(c => c.AC_GC == job.JH_GC);
					var jobRevenueJournal = jobFactory.New<JobRevenueJournal>();
					jobRevenueJournal.AH_InvoiceDate = ZDateTime.Now;
					jobRevenueJournal.AH_PostDate = ZDateTime.Now;
					jobRevenueJournal.MarkAsAlreadyTransformed();

					job.JH_Direction = job.Direction;

					var charge = AddChargeForJRJLine();
					var lineWithCharge = AddJRJLine(true);
					charge.JR_AL_APLine = lineWithCharge.PK;
					AddJRJLine(false);

					if (AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.GetFallBackValueAtAllLevels(job.JH_GC.ToGuid(), job.JH_GB.ToGuid(), job.JH_GE.ToGuid())
						&& Charge.IsRevenueRecognized(job, charge.SellRecognition))
					{
						var wip = jobFactory.New<WIP>();

						using (wip.GetValidationSuspender())
						{
							wip.SetValues(job, charge);
							charge.JR_AL_ARLine = wip.PK;
							wip.UpdateAL_PostDate();
						}
					}

					JobRevenueJournalLine AddJRJLine(bool withCharge)
					{
						var line = jobRevenueJournal.JournalLines.AddNew();
						line.CostRevenueType = TransactionLineTypes.Cost;
						line.AL_LineType = TransactionLineTypes.Cost;
						line.AL_JH = withCharge ? job.PK : Guid.Empty;
						line.AL_GC = job.JH_GC;
						line.AL_GB = job.JH_GB;
						line.AL_GE = job.JH_GE;
						line.AL_AC = withCharge ? chargeInCurrenctCompany.PK : Guid.Empty;
						line.AL_AG = withCharge ? line.ChargeCode.CostAccount.PK : AccountingConfigurationRegistry.Instance.ElectronicProcessingChargePayableClearingAccount.Value;
						line.AL_Desc = GetLineDescription(withCharge);
						line.AL_RX_NKTransactionCurrency = currency;
						line.AL_ExchangeRate = charge.JR_OSCostExRate;
						line.AL_OSExTaxAmount = price * (withCharge ? -1 : 1);
						line.CalculateHighPrecisionExchangeRate();
						line.AL_PostDate = jobRevenueJournal.AH_PostDate;
						if (withCharge)
						{
							job.ApplyRevenueRecognitionDate(line);
						}
						else
						{
							line.UpdateAL_ReverseDate();
						}

						return line;
					}

					string GetLineDescription(bool withCharge)
					{
						var description = chargeInCurrenctCompany.AC_DescMultilingual;
						return withCharge ? description : $"{description} {job.JH_JobNum}";
					}

					Charge AddChargeForJRJLine()
					{
						var charge = job.Charges.AddNew();
						charge.JR_AC = chargeInCurrenctCompany.PK;
						charge.JR_Desc = GetChargeDescription(charge.JR_Desc, job);
						charge.JR_GC = job.JH_GC;
						charge.JR_GB = job.JH_GB;
						charge.JR_GE = job.JH_GE;
						charge.JR_RX_NKCostCurrency = currency;
						charge.JR_RX_NKSellCurrency = currency;
						charge.JR_OSCostAmt = price;

						return charge;
					}

					(string, decimal) GetDisbursementFee()
					{
						var currencyCode = AccountingUtils.GetElectronicProcessingChargeCurrency(job);

						if (!string.IsNullOrEmpty(currencyCode))
						{
							var electronicProcessingFeeQuery = new ZQuery();
							if (job.JobType == JobInvoicingConsumerTypes.Shipment)
							{
								electronicProcessingFeeQuery.AddToFilter(RefAccElectronicProcessingFeeSchema.EPF_Code, RefAccElectronicProcessingFeeLookups.ElectronicProcessingFeeCodes.SHD);
							}
							else if (job.JobType == JobInvoicingConsumerTypes.Brokerage)
							{
								electronicProcessingFeeQuery.AddToFilter(RefAccElectronicProcessingFeeSchema.EPF_Code, RefAccElectronicProcessingFeeLookups.ElectronicProcessingFeeCodes.BRD);
							}
							else
							{
								return (string.Empty, decimal.Zero);
							}

							electronicProcessingFeeQuery.AddToFilter(RefAccElectronicProcessingFeeSchema.EPF_JobDirection, new List<string> { job.Direction, AccountingMasterFilesConstants.ApportionmentMethod.AllCode });
							electronicProcessingFeeQuery.AddToFilter(RefAccElectronicProcessingFeeSchema.EPF_SystemCode, RefAccElectronicProcessingFeeLookups.SystemCodes.CWN);
							electronicProcessingFeeQuery.AddToFilter(RefAccElectronicProcessingFeeSchema.EPF_Category, RefAccElectronicProcessingFeeLookups.CategoryCodes.STL);
							electronicProcessingFeeQuery.AddToFilter(RefAccElectronicProcessingFeeSchema.EPF_Currency, currencyCode);
							electronicProcessingFeeQuery.AddToFilter(RefAccElectronicProcessingFeeSchema.EPF_CountryCode, new List<string> { GlbCompany.CurrentCompany.Country.Code, string.Empty });

							var disbursementFees = Factory.Load<RefAccElectronicProcessingFee>(electronicProcessingFeeQuery);
							var price = disbursementFees
								.Where(x => x.EPF_ValidFrom <= job.JH_A_JOP)
								.OrderBy(x => new ElectronicProcessingFeeCompare(x.EPF_CountryCode, x.EPF_JobDirection, x.EPF_ValidFrom))
								.FirstOrDefault()?.EPF_Price ?? decimal.Zero;

							return (currencyCode, price);
						}

						return (string.Empty, decimal.Zero);
					}
				}
			}
		}

		string GetChargeDescription(string originalDesc, Job job)
		{
			var result = originalDesc;
			var shipment = job.Parent as ForwardingShipment;
			if (shipment != null)
			{
				var electronicProcessingChargeDescriptionOverrides = AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeDescriptionOverride.GetFallBackValueAtAllLevels(job.Company.PK.ToGuid(), Guid.Empty, Guid.Empty);
				var matchElectronicProcessingChargeDescriptionOverride = electronicProcessingChargeDescriptionOverrides
					.Cast<ElectronicProcessingChargeDescriptionOverride>()
					.Where(x => (x.Transport == shipment.TransportMode || x.Transport == AccountingMasterFilesConstants.ApportionmentMethod.AllCode)
						&& (x.Container == shipment.JS_PackingMode || x.Container == Core.Constants.ContainerModes.All)
						&& (x.ShipmentType == shipment.JS_ShipmentType || x.ShipmentType == AccountingMasterFilesConstants.ApportionmentMethod.AllCode)
						&& CheckOriginDestination(x.Origin, job.Branch, shipment.Origin, shipment.IsDomesticFreight)
						&& CheckOriginDestination(x.Destination, job.Branch, shipment.Destination, shipment.IsDomesticFreight))
					.OrderBy(x => new ElectronicProcessingChargeDescriptionOverrideCompare(x.Transport, x.Container, x.ShipmentType, x.Origin, shipment.IsDomesticFreight))
					.FirstOrDefault();

				if (matchElectronicProcessingChargeDescriptionOverride != null)
				{
					var splitString = " ";

					switch (matchElectronicProcessingChargeDescriptionOverride.PrefixSuffix)
					{
						case AccountingConstants.ElectronicProcessingChargeDescriptionOverridePrefixSuffixCodes.Prefix:
							result = matchElectronicProcessingChargeDescriptionOverride.Text + splitString + result;
							break;
						case AccountingConstants.ElectronicProcessingChargeDescriptionOverridePrefixSuffixCodes.Suffix:
							result = result + splitString + matchElectronicProcessingChargeDescriptionOverride.Text;
							break;
					}

					if (matchElectronicProcessingChargeDescriptionOverride.IncludeShipmentNumber)
					{
						result = result + splitString + job.JH_JobNum;
					}
				}
			}

			return result;
		}

		bool CheckOriginDestination(string originDestinationCodes, GlbBranch jobBranch, RefUNLOCO originDestination, bool isDomestic)
		{
			var result = false;
			var communityRegionsForDirectionCalculation = FreightDataRegistry.Instance.CommunityRegionsForDirectionCalculation.GetFallBackValueAtAllLevels(jobBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty);

			switch (originDestinationCodes)
			{
				case AccountingConstants.ElectronicProcessingChargeDescriptionOverrideOriginDestinationCodes.InSameCountryRegionAsCurrentCompany:
					result = originDestination.Country.PK == Env.CurrentCompany.Country.PK
							|| (communityRegionsForDirectionCalculation.Contains(originDestination.Country.PK.ToGuid()) && communityRegionsForDirectionCalculation.Contains(Env.CurrentCompany.Country.PK));
					break;
				case AccountingConstants.ElectronicProcessingChargeDescriptionOverrideOriginDestinationCodes.SameUnlocoAsJobBranch:
					result = isDomestic && originDestination.PK == jobBranch.HomePort.PK;
					break;
				case AccountingConstants.ElectronicProcessingChargeDescriptionOverrideOriginDestinationCodes.NotSameCountryRegionAsCurrentCompany:
					result = originDestination.Country.PK != Env.CurrentCompany.Country.PK
						&& (!communityRegionsForDirectionCalculation.Contains(originDestination.Country.PK.ToGuid()) || !communityRegionsForDirectionCalculation.Contains(jobBranch.Country.PK.ToGuid()));
					break;
				case AccountingConstants.ElectronicProcessingChargeDescriptionOverrideOriginDestinationCodes.NotSameUnlocoAsJobBranch:
					result = isDomestic && originDestination.PK != jobBranch.HomePort.PK;
					break;
			}

			return result;
		}

		void IElectronicProcessingChargeProvider.InsertAndSetElectronicProcessingChargeRegistry()
		{
			var featureData = ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingElectronicProcessingChargeFeature);
			if (featureData != null && featureData.TryDeserializeParameterAsJson<ElectronicProcessingChargeFeatureControlModel>(out var electronicProcessingChargeFeatureControl)
				&& electronicProcessingChargeFeatureControl != null && electronicProcessingChargeFeatureControl.EnableDisbursementLicenseFee != null)
			{
				var enableDisbursementLicenseFeeModel = electronicProcessingChargeFeatureControl.EnableDisbursementLicenseFee;
				using (var manager = Db.Connection.BeginTransactionWithManager())
				{
					try
					{
						var allCompanies = Factory.Load<GlbCompany>(new ZQuery());
						var unConfiguredCompanies = enableDisbursementLicenseFeeModel.CompanyLevel != null && enableDisbursementLicenseFeeModel.CompanyLevel.Any() ? GetUnConfiguredCompanies(enableDisbursementLicenseFeeModel, allCompanies) : null;
						UpdateElectronicProcessingChargeCodeAndAccount(enableDisbursementLicenseFeeModel, unConfiguredCompanies);
						UpdateSystemLevelConfig(enableDisbursementLicenseFeeModel);
						if (enableDisbursementLicenseFeeModel.CompanyLevel != null && enableDisbursementLicenseFeeModel.CompanyLevel.Any())
						{
							UpdateSpecificCompaniesConfig(enableDisbursementLicenseFeeModel.CompanyLevel);
							UpdateOtherConpaniesConfig(unConfiguredCompanies);
						}
						else
						{
							RemoveOverridedValueAtCompanyLevel(allCompanies.ToList());
						}

						manager.CommitTransaction();
					}
					catch (Exception)
					{
						manager.RollbackTransaction();
						throw;
					}
				}
				Factory.Save();
			}
		}

		void UpdateSystemLevelConfig(EnableDisbursementLicenseFeeModel enableDisbursementLicenseFeeModel)
		{
			var enableSystemLevel = enableDisbursementLicenseFeeModel.EnableSystemLevel;
			if (enableSystemLevel && !AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, enableSystemLevel);
			}
			else if (!enableSystemLevel && AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.Inner.HasActualValue(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.Inner.DeleteValue(Guid.Empty, Guid.Empty, Guid.Empty);
			}
		}

		void UpdateSpecificCompaniesConfig(List<CompanySecurityModel> enableDisbursementLicenseFee)
		{
			var groupedCompaniesCodesDictionay = enableDisbursementLicenseFee.GroupBy(x => x.Enable).ToDictionary(x => x.Key, x => x.AsEnumerable());
			foreach (var isEnabled in groupedCompaniesCodesDictionay.Keys.Cast<bool>())
			{
				var enabledCompaniesCodes = groupedCompaniesCodesDictionay[isEnabled];
				var enabledCompanies = Factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, enabledCompaniesCodes.Select(x => x.CompanyCode.ToUpper()))).ToList();
				UpdateEnableElectronicProcessingChargeFunctionality(isEnabled, enabledCompanies);
			}
		}

		void UpdateOtherConpaniesConfig(List<GlbCompany> unConfiguredCompanies)
		{
			RemoveOverridedValueAtCompanyLevel(unConfiguredCompanies);
		}

		List<GlbCompany> GetUnConfiguredCompanies(EnableDisbursementLicenseFeeModel model, GlbCompany[] allCompanies)
		{
			var specificCompanies = model.CompanyLevel.Select(x => x.CompanyCode.ToUpper()).ToList();
			var allCompaniesSet = allCompanies.ToHashSet();
			allCompaniesSet.RemoveWhere(x => specificCompanies.Contains(x.GC_Code));
			return allCompaniesSet.ToList();
		}

		void RemoveOverridedValueAtCompanyLevel(List<GlbCompany> companies)
		{
			companies.ForEach(x =>
			{
				var isOverridedAtCompanyLevel = AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.Inner.HasActualValue(x.PK.ToGuid(), Guid.Empty, Guid.Empty);
				if (isOverridedAtCompanyLevel)
				{
					AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.Inner.DeleteValue(x.PK.ToGuid(), Guid.Empty, Guid.Empty);
				}
			});
		}

		void UpdateElectronicProcessingChargeCodeAndAccount(EnableDisbursementLicenseFeeModel enableDisbursementLicenseFeeModel, List<GlbCompany> unConfiguredCompanies)
		{
			var shouldUpdateElectronicProcessingChargeCodeAndAccount = false;
			if (enableDisbursementLicenseFeeModel.CompanyLevel != null)
			{
				if (enableDisbursementLicenseFeeModel.EnableSystemLevel && unConfiguredCompanies.Any() || enableDisbursementLicenseFeeModel.CompanyLevel.Any(x => x.Enable))
				{
					shouldUpdateElectronicProcessingChargeCodeAndAccount = true;
				}
			}
			else
			{
				shouldUpdateElectronicProcessingChargeCodeAndAccount = enableDisbursementLicenseFeeModel.EnableSystemLevel;
			}

			if (shouldUpdateElectronicProcessingChargeCodeAndAccount)
			{
				if (AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeDisbursementClearingAccount.Value == Guid.Empty)
				{
					AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeDisbursementClearingAccount.Inner.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CreateNewGLHeader(Res.GetString("6A20F37A-BB66-4507-9AB5-083B8757E339", "ELECTRONIC PROCESSING FEE CLEARING"), true));
				}

				if (AccountingConfigurationRegistry.Instance.ElectronicProcessingChargePayableClearingAccount.Value == Guid.Empty)
				{
					AccountingConfigurationRegistry.Instance.ElectronicProcessingChargePayableClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CreateNewGLHeader(Res.GetString("27834D00-A2B9-4270-B94E-58FFF03ECEA6", "ELECTRONIC PROCESSING FEE PAYABLE"), false));
				}

				if (AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCode.Value == Guid.Empty)
				{
					AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GetNewChargeCode());
				}
			}
		}

		void UpdateEnableElectronicProcessingChargeFunctionality(bool isEnabled, List<GlbCompany> companies)
		{
			companies.ForEach(x =>
			{
				var isOverridedAtCompanyLevel = AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.Inner.HasActualValue(x.PK.ToGuid(), Guid.Empty, Guid.Empty);
				var companyLevelRegistryValue = AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.GetValueWithoutFallback(x.PK.ToGuid(), Guid.Empty, Guid.Empty);
				if ((isOverridedAtCompanyLevel && isEnabled != companyLevelRegistryValue) || !isOverridedAtCompanyLevel)
				{
					AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.SetValue(x.PK.ToGuid(), Guid.Empty, Guid.Empty, isEnabled);
				}
			});
		}

		Guid CreateNewGLHeader(string description, bool disallowDirectPosting)
		{
			var glHeader = Factory.New<AccGLHeader>();
			glHeader.AG_AccountNum = GetNotUseGLAccountNumber();
			glHeader.AG_Description = description;
			glHeader.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			glHeader.AG_DebitCredit = Core.Constants.DebitCredit.Credit;
			glHeader.AG_Column = AccGLHeader.Constants.SectionTypes.Codes.Liabilities;
			glHeader.AG_ControlAccount = false;
			glHeader.AG_CashFlowType = CashFlowCodeLists.Codes.XXX;
			glHeader.AG_SystemCreateUser = User.ServiceUserCode;
			glHeader.AG_SystemLastEditUser = User.ServiceUserCode;
			glHeader.AG_DisallowDirectPosting = disallowDirectPosting;

			Factory.Save();

			return glHeader.PK.ToGuid();
		}

		Guid GetNewChargeCode()
		{
			var chargeCode = AccChargeCode.CreateGlobalChargeCode(Factory);
			chargeCode.AC_Code = GetUnusedGlobalChargeCode();
			chargeCode.AC_Desc = Res.GetString("01257FC1-D38D-4DFD-8859-FB0DA696E694", "ELECTRONIC PROCESSING FEE");
			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			chargeCode.AC_MarginPercentage = 100;
			chargeCode.AC_AG_RevenueAccount = AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeDisbursementClearingAccount.Value;
			chargeCode.AC_AG_WIPAccount = AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeDisbursementClearingAccount.Value;
			chargeCode.AC_AG_CostAccount = AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeDisbursementClearingAccount.Value;
			chargeCode.AC_AG_AccrualAccount = AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeDisbursementClearingAccount.Value;
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			chargeCode.AC_SystemCreateUser = User.ServiceUserCode;
			chargeCode.AC_SystemLastEditUser = User.ServiceUserCode;

			var revenueRecOverride = chargeCode.RevenueRecOverrides.AddNew();
			revenueRecOverride.AE_JobType = JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All;
			revenueRecOverride.AE_RecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;

			Factory.Save();
			return chargeCode.PK.ToGuid();
		}

		string GetNotUseGLAccountNumber()
		{
			(var dotPos1, var dotPos2, var dotPos3) = GetDotPositionForCurrentGLAccountFormat();
			var result = TransformIntToGLAccountFormat(dotPos1, dotPos2, dotPos3);

			while (ExistGLAccountNumber.Contains(result))
			{
				InsertGLAccountWithoutSeparatorNumber++;
				result = TransformIntToGLAccountFormat(dotPos1, dotPos2, dotPos3);
			}

			ExistGLAccountNumber.Add(result);

			return result;
		}

		(int dotPos1, int dotPos2, int dotPos3) GetDotPositionForCurrentGLAccountFormat()
		{
			string currentGLAccountFormat = AccGLHeader.CurrentGLAccountFormat;
			var pos1 = currentGLAccountFormat.IndexOf(".");
			var pos2 = -1;
			var pos3 = -1;

			if (pos1 > -1)
			{
				pos2 = currentGLAccountFormat.IndexOf(".", pos1 + 1);

				if (pos2 > -1)
				{
					pos3 = currentGLAccountFormat.IndexOf(".", pos2 + 1);
				}
			}

			return (pos1, pos2, pos3);
		}

		string TransformIntToGLAccountFormat(int dotPos1, int dotPos2, int dotPos3)
		{
			var result = InsertGLAccountWithoutSeparatorNumber.ToString();
			AddDot(dotPos1);
			AddDot(dotPos2);
			AddDot(dotPos3);

			return result;

			void AddDot(int dotPosition)
			{
				if (dotPosition > -1)
				{
					result = result.Insert(dotPosition, ".");
				}
			}
		}

		List<ZString> existGLAccountNumber;
		List<ZString> ExistGLAccountNumber =>
			existGLAccountNumber != null && existGLAccountNumber.Any()
			? existGLAccountNumber
			: existGLAccountNumber = Factory.Load<AccGLHeader>(new ZQuery()).Select(x => x.AG_AccountNum).ToList();

		int insertGLAccountWithoutSeparatorNumber;
		int InsertGLAccountWithoutSeparatorNumber
		{
			get
			{
				if (insertGLAccountWithoutSeparatorNumber == 0)
				{
					if (AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.Value != Guid.Empty)
					{
						var accruedCostControlAccount = Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.Value);
						insertGLAccountWithoutSeparatorNumber = int.Parse(accruedCostControlAccount.AG_AccountNum.Replace(".", ""));
					}
					else
					{
						var glHeaderQuery = new ZQuery();
						glHeaderQuery.AddToFilter(AccGLHeaderSchema.AG_AccountType, Core.Constants.AccountType.BalanceSheetAccount);
						glHeaderQuery.AddToFilter(AccGLHeaderSchema.AG_Column, AccGLHeader.Constants.SectionTypes.Codes.Liabilities);
						glHeaderQuery.AddToFilter(AccGLHeaderSchema.AG_ControlAccount, true);

						var glHeader = Factory.LoadTop1<AccGLHeader>(glHeaderQuery);
						insertGLAccountWithoutSeparatorNumber = int.Parse(glHeader.AG_AccountNum.Replace(".", ""));
					}
				}
				return insertGLAccountWithoutSeparatorNumber;
			}
			set
			{
				insertGLAccountWithoutSeparatorNumber = value;
			}
		}

		string GetUnusedGlobalChargeCode()
		{
			var result = string.Empty;
			foreach (var chargeCode in DesireChargeCodes)
			{
				if (!ExistChargeCode.Contains(chargeCode))
				{
					result = chargeCode;
					break;
				}
			}

			if (string.IsNullOrEmpty(result))
			{
				for (var index = 1; index < 100; index++)
				{
					result = "EFEE" + index.ToString();
					if (!ExistChargeCode.Contains(result))
					{
						break;
					}
				}
			}

			return result;
		}

		ZString[] existChargeCode;
		ZString[] ExistChargeCode =>
			existChargeCode != null && existChargeCode.Any()
			? existChargeCode
			: existChargeCode = Factory.Load<AccChargeCode>(new ZQuery()).Select(x => x.AC_Code).ToArray();

		BusinessObjectFactory factory;
		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());

		string[] DesireChargeCodes => new string[]
		{
			"EFEE",
			"SYSEFEE",
			"E-FEE",
			"SYS-EFEE",
			"SYS-E-FEE",
			"EPFEE",
			"E-PFEE",
			"SYSEPFEE",
			"SYS-EPFEE",
			"SYS-E-PFEE",
			"EPROCEFEE",
			"E-PROCEFEE",
		};

		public class ElectronicProcessingFeeCompare : IComparable
		{
			public ElectronicProcessingFeeCompare(string country, string direction, ZDateTime validFromDate)
			{
				Country = country;
				Direction = direction;
				ValidFromDate = validFromDate;
			}

			public readonly ZString Country;
			public readonly ZString Direction;
			public readonly ZDateTime ValidFromDate;

			public int CompareTo(object obj)
			{
				var electronicProcessingFeeCompareCompare = (ElectronicProcessingFeeCompare)obj;

				if (Country != electronicProcessingFeeCompareCompare.Country)
				{
					if (string.IsNullOrEmpty(Country))
					{
						return 1;
					}
					else if (string.IsNullOrEmpty(electronicProcessingFeeCompareCompare.Country))
					{
						return -1;
					}
				}

				if (Direction != electronicProcessingFeeCompareCompare.Direction)
				{
					if (Direction == AccountingMasterFilesConstants.ApportionmentMethod.AllCode)
					{
						return 1;
					}
					else if (electronicProcessingFeeCompareCompare.Direction == AccountingMasterFilesConstants.ApportionmentMethod.AllCode)
					{
						return -1;
					}
				}

				return ValidFromDate > electronicProcessingFeeCompareCompare.ValidFromDate
					? -1
					: ValidFromDate < electronicProcessingFeeCompareCompare.ValidFromDate
						? 1
						: 0;
			}
		}

		bool IElectronicProcessingChargeProvider.HasElectronicProcessingChargeCurrencyExchangeRate(Job job)
		{
			var result = true;
			var currencyCode = AccountingUtils.GetElectronicProcessingChargeCurrency(job);

			if (!job.IsInDatabase
				&& AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.GetFallBackValueAtAllLevels(job.JH_GC.ToGuid(), Guid.Empty, Guid.Empty)
				&& AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCode.Value != Guid.Empty
				&& ObjectFactory.Get<IAccounting>().IsIncludedInElectronicProcessingChargeConfiguration(job.JH_A_JOP, job.JobType.Code)
				&& !string.IsNullOrEmpty(currencyCode))
			{
				var currency = job.Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currencyCode);
				var invoiceCurrencyType = AccExchangeRateConfigurationRateFinder.GetInvoiceCurrencyType(job.Company, ExchangeRateValidLedgerEnum.AR, currencyCode);
				var globalElectronicProcessingChargeCode = job.Factory.Load<AccChargeCode>(AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCode.Value);
				var electronicProcessingChargeCode = globalElectronicProcessingChargeCode.ChildChargeCodes.FirstOrDefault(c => c.AC_GC == job.JH_GC);

				var debtorPK = job.GetDebtorPK(electronicProcessingChargeCode, ZString.Empty);
				var defaultDebtor = job.Factory.Load<OrgHeader>(debtorPK);

				if (defaultDebtor != null
					&& (!defaultDebtor.OH_IsDebtor || defaultDebtor.IsCancelled))
				{
					debtorPK = Guid.Empty;
					defaultDebtor = null;
				}

				if (job.Company.GC_RX_NKLocalCurrency != currencyCode
					&& (!HasExchangeRate(true)
						|| !HasExchangeRate(false)))
				{
					result = false;
				}

				bool HasExchangeRate(bool isAR)
				{
					return job.GetExchangeRate(currencyCode, isAR ? debtorPK : Guid.Empty, isAR ? ExchangeRateOrgTypeEnum.Debtor : ExchangeRateOrgTypeEnum.Creditor, isAR ? ExchangeRateType.Sell : ExchangeRateType.Buy, isAR ? invoiceCurrencyType : InvoiceCurrencyType.NotApplicable) != 0
						|| AccExchangeRateConfigurationRateFinder.GetExchangeRate(job.ExchangeRateConfigurationRateConsumer, currency, isAR ? defaultDebtor : null, isAR ? ExchangeRateValidLedgerEnum.AR : ExchangeRateValidLedgerEnum.AP, isAR ? invoiceCurrencyType : InvoiceCurrencyType.NotApplicable) != 0m;
				}
			}

			return result;
		}

		bool IElectronicProcessingChargeProvider.ShouldCarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePosting(ZGuid chargeCodePK, ZGuid companyPK)
		{
			var electronicProcessingChargeCode = AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCode.Value;

			return AccountingConfigurationRegistry.Instance.CarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePosting.GetFallBackValueAtAllLevels(companyPK.ToGuid(), Guid.Empty, Guid.Empty)
				&& chargeCodePK == ReadOnlyBusinessObjectFactory.Load<AccChargeCode>(electronicProcessingChargeCode)?.ChildChargeCodes.FirstOrDefault(x => x.AC_GC == companyPK)?.PK;
		}

		ReadOnlyBusinessObjectFactory readOnlyBusinessObjectFactory;
		ReadOnlyBusinessObjectFactory ReadOnlyBusinessObjectFactory
		{
			get { return readOnlyBusinessObjectFactory ?? (readOnlyBusinessObjectFactory = new ReadOnlyBusinessObjectFactory()); }
		}
	}

	public class ElectronicProcessingChargeDescriptionOverrideCompare : IComparable
	{
		public ElectronicProcessingChargeDescriptionOverrideCompare(string transport, string container, string shipmentType, string origin, bool isDomesticShipment)
		{
			Transport = transport;
			Container = container;
			ShipmentType = shipmentType;
			Origin = origin;
			IsDomesticShipment = isDomesticShipment;
		}

		public readonly ZString Transport;
		public readonly ZString Container;
		public readonly ZString ShipmentType;
		public readonly ZString Origin;
		public readonly bool IsDomesticShipment;

		public int CompareTo(object obj)
		{
			var electronicProcessingChargeDescriptionOverrideCompare = (ElectronicProcessingChargeDescriptionOverrideCompare)obj;

			if (Transport != electronicProcessingChargeDescriptionOverrideCompare.Transport)
			{
				if (Transport == AccountingMasterFilesConstants.ApportionmentMethod.AllCode)
				{
					return 1;
				}
				else if (electronicProcessingChargeDescriptionOverrideCompare.Transport == AccountingMasterFilesConstants.ApportionmentMethod.AllCode)
				{
					return -1;
				}
			}

			if (Container != electronicProcessingChargeDescriptionOverrideCompare.Container)
			{
				if (Container == Core.Constants.ContainerModes.All)
				{
					return 1;
				}
				else if (electronicProcessingChargeDescriptionOverrideCompare.Container == Core.Constants.ContainerModes.All)
				{
					return -1;
				}
			}

			if (ShipmentType != electronicProcessingChargeDescriptionOverrideCompare.ShipmentType)
			{
				if (ShipmentType == AccountingMasterFilesConstants.ApportionmentMethod.AllCode)
				{
					return 1;
				}
				else if (electronicProcessingChargeDescriptionOverrideCompare.ShipmentType == AccountingMasterFilesConstants.ApportionmentMethod.AllCode)
				{
					return -1;
				}
			}

			if (IsDomesticShipment)
			{
				if (AccountingConstants.ElectronicProcessingChargeDescriptionOverrideOriginDestinationCompareJobBranchList.ContainsCode(Origin))
				{
					return -1;
				}
				else if (AccountingConstants.ElectronicProcessingChargeDescriptionOverrideOriginDestinationCompareJobBranchList.ContainsCode(electronicProcessingChargeDescriptionOverrideCompare.Origin))
				{
					return 1;
				}
			}
			else
			{
				if (AccountingConstants.ElectronicProcessingChargeDescriptionOverrideOriginDestinationCompareCurrentCompanyList.ContainsCode(Origin))
				{
					return -1;
				}
				else if (AccountingConstants.ElectronicProcessingChargeDescriptionOverrideOriginDestinationCompareCurrentCompanyList.ContainsCode(electronicProcessingChargeDescriptionOverrideCompare.Origin))
				{
					return 1;
				}
			}

			return 0;
		}
	}
}
