using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business
{
	public struct AlternateGLAccountWithAttributeSetDetails
	{
		public AlternateGLAccountWithAttributeSetDetails(ZGuid parentGLAccountPK, ZGuid chartPK, int sequence, ZString accountType, ZString cashFlowCategory, ZString unit)
		{
			ParentGLAccountPK = parentGLAccountPK;
			ChartPK = chartPK;
			Sequence = sequence;
			AccountType = accountType;
			CashFlowCategory = cashFlowCategory;
			Unit = unit;
		}

		public ZGuid ParentGLAccountPK { get; set; }
		public ZGuid ChartPK { get; set; }
		public int Sequence { get; set; }
		public ZString AccountType { get; set; }
		public ZString ReportSection { get; set; }
		public ZString DebitCredit { get; set; }
		public ZString CashFlowCategory { get; set; }
		public ZString Unit { get; set; }
	}

	public class AlternateGLAccountWithAttributeSet : NonPersistentBusinessObject
	{
		public AlternateGLAccountWithAttributeSet(AlternateGLAccountWithAttributeSetDetails alternateGLAccountWithAttributeSetDetails, BusinessObjectFactory factory) : base(factory)
		{
			ParentGLAccountPK = alternateGLAccountWithAttributeSetDetails.ParentGLAccountPK;
			Chart = factory.Load<AccAlternateChart>(alternateGLAccountWithAttributeSetDetails.ChartPK);
			Sequence = alternateGLAccountWithAttributeSetDetails.Sequence;
			AccountType = alternateGLAccountWithAttributeSetDetails.AccountType;
			CashFlowCategory = alternateGLAccountWithAttributeSetDetails.CashFlowCategory;
			Unit = alternateGLAccountWithAttributeSetDetails.Unit;
		}

		ZString alternateGLAccountNum;
		[MaxLength(20)]
		public ZString AlternateGLAccountNum
		{
			get
			{
				return alternateGLAccountNum;
			}
			set
			{
				if (alternateGLAccountNum != value)
				{
					SetNonPersistentPropertyValue(AlternateGLAccountNumInfo, ref alternateGLAccountNum, value);

					if (AlternateGLAccount == null || alternateGLAccountNum != AlternateGLAccount.AGA_AccountNum)
					{
						if (Chart != null && !AccountingMasterFilesConstants.WithoutParentAccountTypeList.ContainsCode(AccountType))
						{
							var query = new ZQuery();
							query.AddToFilter(AccAlternateGLAccountSchema.AGA_AccountNum, alternateGLAccountNum);
							query.AddToFilter(AccAlternateGLAccountSchema.AGA_AAC_AlternateChart, Chart.PK);

							var existAlternateGLAccount = Factory.LoadTop1<AccAlternateGLAccount>(query);
							if (existAlternateGLAccount != null && !string.IsNullOrEmpty(alternateGLAccountNum))
							{
								UndoOriginalAlternateGLAccount();
								AlternateGLAccount = existAlternateGLAccount;
							}
							else
							{
								SetAlternateGLAccountNum();
							}
						}
						else
						{
							SetAlternateGLAccountNum();
						}

						void SetAlternateGLAccountNum()
						{
							var attributes = AlternateGLAccount?.AlternateGLAccountAttributes.Cast<AccAlternateGLAccountAttribute>();
							if ((AlternateGLAccount?.IsInDatabase ?? false)
								&& ((attributes.Any() && !(attributes.FirstOrDefault(x => x.AAA_AG_GLHeader == ParentGLAccountPK)?.IsInDatabase ?? false))
								|| AlternateGLAccount.AGA_AccountNumInfo.HasErrors()))
							{
								UndoOriginalAlternateGLAccount();
								AlternateGLAccount = Factory.New<AccAlternateGLAccount>();
							}
							else if (AlternateGLAccount == null || attributes.Where(x => x.AAA_AG_GLHeader == ParentGLAccountPK).Select(x => x.AAA_Sequence).Distinct().Count() > 1)
							{
								AlternateGLAccount?.AlternateGLAccountAttributes.RemoveRange(Attributes);
								AlternateGLAccount = Factory.New<AccAlternateGLAccount>();
							}
							AlternateGLAccount.AGA_AccountNum = alternateGLAccountNum;
						}

						void UndoOriginalAlternateGLAccount()
						{
							if (AlternateGLAccount != null)
							{
								AlternateGLAccount.AlternateGLAccountAttributes.RemoveRange(Attributes);
								var queryForAlternateGLAccountInSameGLAccount = new ZQuery();
								queryForAlternateGLAccountInSameGLAccount.AddToFilter(AccAlternateGLAccountAttributeSchema.AAA_AGA_AlternateGLAccount, AlternateGLAccount.PK);
								queryForAlternateGLAccountInSameGLAccount.AddToFilter(AccAlternateGLAccountAttributeSchema.AAA_Sequence, SQLComparisonOperator.NotEqual, Sequence);

								if ((!AlternateGLAccount.IsInDatabase && !Factory.Exists(typeof(AccAlternateGLAccountAttribute), queryForAlternateGLAccountInSameGLAccount))
									|| (!OriginalAlternateGLAccountHasOtherParent() && !Attributes.Cast<AccAlternateGLAccountAttribute>().Any(x => !string.IsNullOrEmpty(x.AAA_Attribute))))
								{
									AlternateGLAccount.Delete();
								}
								else if (AlternateGLAccount.IsInDatabase)
								{
									AlternateGLAccount.Reload();
								}
							}
						}

						if (!IsValidationSuspended)
						{
							Validation.ValidateAlternateGLAccountNum();
						}
					}
				}
			}
		}

		public bool AlternateGLAccountNumExistsInDatabase(string alternateAccountNum)
		{
			var result = false;
			if (Chart != null)
			{
				var query = new ZDBOnlyQuery(typeof(AccAlternateGLAccount));
				query.AddToFilter(AccAlternateGLAccountSchema.AGA_AccountNum, alternateAccountNum);
				query.AddToFilter(AccAlternateGLAccountSchema.AGA_AAC_AlternateChart, Chart.PK);

				result = Factory.ExistsInDatabase(AccAlternateGLAccountSchema.Constants.TableName, query);
			}

			return result;
		}

		public bool OriginalAlternateGLAccountHasOtherParent()
		{
			var result = true;
			if (!AlternateGLAccount.Lookups.WithoutParentAccountTypeList.ContainsCode(AccountType))
			{
				var query = new ZQuery();
				query.AddToFilter(AccAlternateGLAccountAttributeSchema.AAA_AGA_AlternateGLAccount, AlternateGLAccount.PK);
				query.AddToFilter(AccAlternateGLAccountAttributeSchema.AAA_AG_GLHeader, SQLComparisonOperator.NotEqual, ParentGLAccountPK);

				result = Factory.Exists(typeof(AccAlternateGLAccountAttribute), query);
			}

			return result;
		}

		public void ResetAlternateGLAccountNum()
		{
			alternateGLAccountNum = AlternateGLAccount.AGA_AccountNum;
		}

		public ZPropertyInfo AlternateGLAccountNumInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(AlternateGLAccountNum));
			}
		}

		AccAlternateGLAccount alternateGLAccount;

		[ChildEditable(true)]
		public AccAlternateGLAccount AlternateGLAccount
		{
			get
			{
				return alternateGLAccount;
			}
			set
			{
				alternateGLAccount = value;

				if (!alternateGLAccount.IsInDatabase)
				{
					alternateGLAccount.AGA_AccountType = AccountType;
					alternateGLAccount.AGA_DebitCredit = ParentGLAccount?.AG_DebitCredit ?? ZString.Empty;
					alternateGLAccount.AGA_ReportSection = ParentGLAccount?.AG_Column ?? ZString.Empty;
					if (Chart != null)
					{
						alternateGLAccount.AGA_AAC_AlternateChart = Chart.PK;
					}
				}
				if (!Attributes.Any() && parentGLAccountPK.IsValid)
				{
					if (ORG.IsValid)
					{
						CreateAttributeValue(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG, string.Empty, ORG);
					}
					if (!string.IsNullOrEmpty(OCG))
					{
						CreateAttributeValue(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, OCG);
					}
					if (!string.IsNullOrEmpty(LFE))
					{
						CreateAttributeValue(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE, LFE);
					}
					if (!string.IsNullOrEmpty(LFO))
					{
						CreateAttributeValue(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO, LFO);
					}
					if (!string.IsNullOrEmpty(TIC))
					{
						CreateAttributeValue(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC, TIC);
					}
					if (!string.IsNullOrEmpty(SPR))
					{
						CreateAttributeValue(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR, SPR);
					}
					if (!Attributes.Any())
					{
						CreateAttributeValue(string.Empty, string.Empty);
					}
				}
				else
				{
					alternateGLAccount.AlternateGLAccountAttributes.AddRange(Attributes);
				}

				RegisterEditableChildObject(alternateGLAccount);
			}
		}

		#region Attributes

		ZGuid oRG;
		public ZGuid ORG
		{
			get
			{
				return oRG;
			}
			set
			{
				oRG = value;
				SetAttributeValue(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG, string.Empty, oRG);
			}
		}

		OrgHeader oRGOrganization => Factory.Load<OrgHeader>(ORG);
		public ZString OrganizationCode => oRGOrganization?.OH_Code ?? ZString.Empty;

		ZString oCG;
		public ZString OCG
		{
			get
			{
				return oCG;
			}
			set
			{
				oCG = value;
				SetAttributeValue(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, oCG);
			}
		}

		public ZString OCGDescription => string.IsNullOrEmpty(OCG) ? string.Empty : AccountingMasterFilesConstants.OCGList.GetDescriptionFromCode(OCG);

		ZString lFO;
		public ZString LFO
		{
			get
			{
				return lFO;
			}
			set
			{
				lFO = value;
				SetAttributeValue(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO, lFO);
			}
		}

		public ZString LFODescription => string.IsNullOrEmpty(LFO) ? string.Empty : AccountingMasterFilesConstants.LFOList.GetDescriptionFromCode(LFO);

		ZString lFE;
		public ZString LFE
		{
			get
			{
				return lFE;
			}
			set
			{
				lFE = value;
				SetAttributeValue(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE, lFE);
			}
		}

		public ZString LFEDescription => string.IsNullOrEmpty(LFE) ? string.Empty : AccountingMasterFilesConstants.LFEList.GetDescriptionFromCode(LFE);

		ZString tIC;
		public ZString TIC
		{
			get
			{
				return tIC;
			}
			set
			{
				tIC = value;
				SetAttributeValue(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC, tIC);
			}
		}

		public ZString TICDescription => string.IsNullOrEmpty(TIC) ? string.Empty : AccountingMasterFilesConstants.TICList.GetDescriptionFromCode(TIC);

		ZString sPR;
		public ZString SPR
		{
			get
			{
				return sPR;
			}
			set
			{
				sPR = value;
				SetAttributeValue(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR, sPR);
			}
		}

		public ZString SPRDescription => string.IsNullOrEmpty(SPR) ? string.Empty : AccountingMasterFilesConstants.SPRList.GetDescriptionFromCode(SPR);

		AccAlternateGLAccountAttributeCollection attributes;

		[BusinessObjectTestExclude]
		public AccAlternateGLAccountAttributeCollection Attributes
		{
			get
			{
				attributes = attributes ?? new AccAlternateGLAccountAttributeCollection(Factory);
				return attributes;
			}
			set
			{
				attributes = value;
				foreach (AccAlternateGLAccountAttribute alternateGLAccountAttribute in attributes)
				{
					switch (alternateGLAccountAttribute.AAA_Attribute)
					{
						case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG:
							oRG = alternateGLAccountAttribute.AAA_AttributeValueID;
							break;
						case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG:
							oCG = alternateGLAccountAttribute.AAA_Value;
							break;
						case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE:
							lFE = alternateGLAccountAttribute.AAA_Value;
							break;
						case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO:
							lFO = alternateGLAccountAttribute.AAA_Value;
							break;
						case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC:
							tIC = alternateGLAccountAttribute.AAA_Value;
							break;
						case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR:
							sPR = alternateGLAccountAttribute.AAA_Value;
							break;
					}
				}
			}
		}

		#endregion

		#region DebitCredit

		[MaxLength(2)]
		[List("Lookups.DebitCreditList")]
		public ZString DebitCredit
		{
			get => AlternateGLAccount?.AGA_DebitCredit ?? ZString.Empty;
			set
			{
				AlternateGLAccount.AGA_DebitCredit = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateDebitCredit();
				}
				DebitCreditInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo DebitCreditInfo
		{
			get { return GetZPropertyInfo(nameof(DebitCredit)); }
		}

		#endregion

		#region Description

		[MaxLength(128)]
		public ZString Description
		{
			get => AlternateGLAccount?.AGA_Description ?? ZString.Empty;
			set
			{
				AlternateGLAccount.AGA_Description = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateDescription();
				}
				DescriptionInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(Description)); }
		}

		#endregion

		#region Report Section

		[MaxLength(2)]
		[List("Lookups.ReportSectionList")]
		public ZString ReportSection
		{
			get => AlternateGLAccount?.AGA_ReportSection ?? ZString.Empty;
			set
			{
				AlternateGLAccount.AGA_ReportSection = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateReportSection();
				}
				ReportSectionInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo ReportSectionInfo
		{
			get { return GetZPropertyInfo(nameof(ReportSection)); }
		}

		#endregion

		#region Total Level

		public ZInt TotalLevel
		{
			get => AlternateGLAccount?.AGA_TotalLevel ?? ZInt.Zero;
			set
			{
				AlternateGLAccount.AGA_TotalLevel = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateTotalLevel();
				}
				TotalLevelInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo TotalLevelInfo
		{
			get { return GetZPropertyInfo(nameof(TotalLevel)); }
		}

		protected bool TotalLevel_ReadOnly
		{
			get { return AccountType != Core.Constants.AccountType.Total; }
		}

		#endregion

		#region Print Sequence

		public ZInt PrintSequence
		{
			get => AlternateGLAccount?.AGA_PrintSequence ?? ZInt.Zero;
			set
			{
				AlternateGLAccount.AGA_PrintSequence = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidatePrintSequence();
				}
				PrintSequenceInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo PrintSequenceInfo
		{
			get { return GetZPropertyInfo(nameof(PrintSequence)); }
		}

		#endregion

		#region Percent Number

		[List("Lookups.PercentNums")]
		public ZGuid PercentNum
		{
			get => AlternateGLAccount?.AGA_AGA_PercentNum ?? ZGuid.Empty;
			set
			{
				AlternateGLAccount.AGA_AGA_PercentNum = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidatePercentNum();
				}
				PercentNumInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo PercentNumInfo
		{
			get { return GetZPropertyInfo(nameof(PercentNum)); }
		}

		protected bool PercentNum_ReadOnly
		{
			get { return AlternateGLAccount?.AccountTypeWithoutReferenceAlteranteGLAccountForPercentNum.Contains(AccountType) ?? false; }
		}

		#endregion

		#region Consolidation Number

		[List("Lookups.ConsolidationNums")]
		public ZGuid ConsolidationNum
		{
			get => AlternateGLAccount?.AGA_AGA_ConsolidationNum ?? ZGuid.Empty;
			set
			{
				AlternateGLAccount.AGA_AGA_ConsolidationNum = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateConsolidationNum();
				}
				ConsolidationNumInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo ConsolidationNumInfo
		{
			get { return GetZPropertyInfo(nameof(ConsolidationNum)); }
		}

		protected bool ConsolidationNum_ReadOnly
		{
			get { return AlternateGLAccount?.AccountTypeWithoutReferenceAlteranteGLAccountForConsolidationNum.Contains(AccountType) ?? false; }
		}

		#endregion

		#region Alternate Number

		[List("Lookups.AlternateNums")]
		public ZGuid AlternateNum
		{
			get => AlternateGLAccount?.AGA_AGA_AlternateNum ?? ZGuid.Empty;
			set
			{
				AlternateGLAccount.AGA_AGA_AlternateNum = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateAlternateNum();
				}
				AlternateNumInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo AlternateNumInfo
		{
			get { return GetZPropertyInfo(nameof(AlternateNum)); }
		}

		protected bool AlternateNum_ReadOnly
		{
			get { return AccountType != Core.Constants.AccountType.BalanceSheetAccount; }
		}

		#endregion

		#region Totala Reference

		[List("Lookups.HeaderDependsOnTotals")]
		public ZGuid HeaderDependsOnTotal
		{
			get => AlternateGLAccount?.AGA_AGA_HeaderDependsOnTotal ?? ZGuid.Empty;
			set
			{
				AlternateGLAccount.AGA_AGA_HeaderDependsOnTotal = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateHeaderDependsOnTotal();
				}
				HeaderDependsOnTotalInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo HeaderDependsOnTotalInfo
		{
			get { return GetZPropertyInfo(nameof(HeaderDependsOnTotal)); }
		}

		protected bool HeaderDependsOnTotal_ReadOnly
		{
			get { return AccountType != Core.Constants.AccountType.Header; }
		}

		#endregion

		#region Lookups

		public AlternateGLAccountWithAttributeSetLookups Lookups
		{
			get
			{
				if (fLookups == null)
				{
					fLookups = new AlternateGLAccountWithAttributeSetLookups(this);
				}

				return fLookups;
			}
		}

		AlternateGLAccountWithAttributeSetLookups fLookups;

		#endregion

		ZGuid parentGLAccountPK;
		public ZGuid ParentGLAccountPK
		{
			get
			{
				return parentGLAccountPK;
			}
			set
			{
				parentGLAccountPK = value;
				Attributes.Cast<AccAlternateGLAccountAttribute>().ForEach(x => x.AAA_AG_GLHeader = parentGLAccountPK);
			}
		}

		AccGLHeader parentGLAccount;
		AccGLHeader ParentGLAccount => parentGLAccount = parentGLAccount == null || parentGLAccount.PK != parentGLAccountPK ? Factory.Load<AccGLHeader>(parentGLAccountPK) : parentGLAccount;

		AccAlternateChart chart;
		public AccAlternateChart Chart
		{
			get
			{
				return chart;
			}
			set
			{
				chart = value;
				if (AlternateGLAccount != null)
				{
					AlternateGLAccount.AGA_AAC_AlternateChart = chart.PK;
				}
				Attributes.Cast<AccAlternateGLAccountAttribute>().ForEach(x =>
				{
					x.AAA_AAC_AlternateChart = chart.PK;
				});
			}
		}

		public int Sequence;

		ZString accountType;
		public ZString AccountType
		{
			get
			{
				return accountType;
			}
			set
			{
				accountType = value;
				if (AlternateGLAccount != null)
				{
					AlternateGLAccount.AGA_AccountType = accountType;
					SetReadonlyDependentValues();
				}
			}
		}

		protected void SetReadonlyDependentValues()
		{
			if (PercentNumInfo.ReadOnly)
			{
				PercentNum = ZGuid.Empty;
			}

			if (ConsolidationNumInfo.ReadOnly)
			{
				ConsolidationNum = ZGuid.Empty;
			}

			if (AlternateNumInfo.ReadOnly)
			{
				AlternateNum = ZGuid.Empty;
			}

			if (HeaderDependsOnTotalInfo.ReadOnly)
			{
				HeaderDependsOnTotal = ZGuid.Empty;
			}

			if (TotalLevelInfo.ReadOnly)
			{
				TotalLevel = 0;
			}
		}

		ZString cashFlowCategory;
		public ZString CashFlowCategory
		{
			get
			{
				return cashFlowCategory;
			}
			set
			{
				cashFlowCategory = value;
			}
		}

		ZString unit;
		public ZString Unit
		{
			get
			{
				return unit;
			}
			set
			{
				unit = value;
			}
		}

		public override void Delete()
		{
			if (AlternateGLAccount != null)
			{
				AlternateGLAccount.Delete();
				Attributes.RemoveAndDeleteAll();
			}

			base.Delete();
		}

		void SetAttributeValue(ZString attributeName, ZString value, ZGuid? valuePK = null)
		{
			if (Attributes.Cast<AccAlternateGLAccountAttribute>().Any(x => x.AAA_Attribute == attributeName))
			{
				var attribute = Attributes.Cast<AccAlternateGLAccountAttribute>().FirstOrDefault(x => x.AAA_Attribute == attributeName);
				attribute.AAA_Value = value;
			}
			else
			{
				CreateAttributeValue(attributeName, value, valuePK);
			}
		}

		void CreateAttributeValue(string attributeName, string value, ZGuid? valuePK = null)
		{
			var attribute = Attributes.AddNew();
			attribute.AAA_Attribute = attributeName;
			attribute.AAA_AG_GLHeader = ParentGLAccountPK;
			attribute.AAA_AAC_AlternateChart = Chart?.PK ?? ZGuid.Empty;
			attribute.AAA_AGA_AlternateGLAccount = AlternateGLAccount?.PK ?? ZGuid.Empty;
			attribute.AAA_Sequence = Sequence;

			if (!string.IsNullOrEmpty(value))
			{
				attribute.AAA_Value = value;
			}
			else if (valuePK?.IsValid ?? false)
			{
				attribute.AAA_AttributeValueID = valuePK.Value;
			}
		}

		public AlternateGLAccountWithAttributeSetValidation Validation
		{
			get { return new AlternateGLAccountWithAttributeSetValidation(this, Factory); }
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			Validation.ValidateAll();
		}

		public override bool IsInDatabase => (AlternateGLAccount?.IsInDatabase ?? false) && Attributes.All(y => y.IsInDatabase);
	}
}
