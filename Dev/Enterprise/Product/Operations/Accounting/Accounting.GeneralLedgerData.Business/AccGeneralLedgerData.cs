using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.GeneralLedgerData.Business
{
	public class AccGeneralLedgerData : AutoAccGeneralLedgerData, IAccountingNumberFountainDataSource
	{
		public AccGeneralLedgerData(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public int LocalDecimals => GlbCompany.CurrentCompany.GetLocalDecimals();

		public int ExchangeRateDecimalPlaces => GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces;

		protected override ZString HumanReadableNameCore => Res.GetString("98328156-0166-4CE9-8ECE-654B2B19FCFF", "Accounting Journal");

		#region Properties

		AccTransactionHeader TransactionHeaderForColumnDisplay => GLD_ATM_TaxGLMovement.IsValid ? TaxGLMovement?.TaxTransaction?.TransactionHeader : TransactionHeader;

		AccTaxGLMovement TaxGLMovement => fTaxGLMovement ?? (fTaxGLMovement = Factory.Load<AccTaxGLMovement>(GLD_ATM_TaxGLMovement));
		AccTaxGLMovement fTaxGLMovement;

		#region GLD_OSDebitAmount

		[DecimalPlaces(nameof(LocalDecimals))]
		public override ZDecimal GLD_OSDebitAmount
		{
			get { return base.GLD_OSDebitAmount; }
			set { base.GLD_OSDebitAmount = value; }
		}

		#endregion

		#region GLD_OSCreditAmount

		[DecimalPlaces(nameof(LocalDecimals))]
		public override ZDecimal GLD_OSCreditAmount
		{
			get { return base.GLD_OSCreditAmount; }
			set { base.GLD_OSCreditAmount = value; }
		}

		#endregion

		#region GLD_LocalDebitAmount

		[DecimalPlaces(nameof(LocalDecimals))]
		public override ZDecimal GLD_LocalDebitAmount
		{
			get { return base.GLD_LocalDebitAmount; }
			set { base.GLD_LocalDebitAmount = value; }
		}

		#endregion

		#region GLD_LocalCreditAmount

		[DecimalPlaces(nameof(LocalDecimals))]
		public override ZDecimal GLD_LocalCreditAmount
		{
			get { return base.GLD_LocalCreditAmount; }
			set { base.GLD_LocalCreditAmount = value; }
		}

		#endregion

		#region GLD_ExchangeRate

		[DecimalPlaces(nameof(ExchangeRateDecimalPlaces))]
		public override ZDecimal GLD_ExchangeRate
		{
			get { return base.GLD_ExchangeRate; }
			set { base.GLD_ExchangeRate = value; }
		}

		#endregion

		#region Organization

		[List("Lookups.Organizations")]
		public ZGuid OrganizationPK
		{
			get
			{
				if (fOrganizationPK == ZGuid.Empty)
				{
					if ((TransactionHeaderForColumnDisplay?.AH_OH ?? ZGuid.Empty) != ZGuid.Empty)
					{
						fOrganizationPK = TransactionHeaderForColumnDisplay.AH_OH;
					}
					else if ((TransactionLine?.AL_OH ?? ZGuid.Empty) != ZGuid.Empty)
					{
						fOrganizationPK = TransactionLine.AL_OH;
					}
				}

				return fOrganizationPK;
			}
		}

		ZGuid fOrganizationPK = ZGuid.Empty;

		#endregion

		#region Ledger

		public ZString Ledger
		{
			get
			{
				if (IsWipOrAccrual)
				{
					return LedgerTypes.JobCosting;
				}
				else
				{
					return TransactionHeaderForColumnDisplay?.AH_Ledger ?? ZString.Empty;
				}
			}
		}

		public ZPropertyInfo LedgerInfo
		{
			get { return GetZPropertyInfo(nameof(Ledger)); }
		}

		#endregion

		#region TransactionNum

		public ZString TransactionNumber
		{
			get => TransactionHeaderForColumnDisplay?.AH_TransactionNum ?? ZString.Empty;
		}

		public ZPropertyInfo TransactionNumberInfo
		{
			get { return GetZPropertyInfo(nameof(TransactionNumber)); }
		}

		#endregion

		#region DueDate

		public ZDateTime DueDate
		{
			get => TransactionHeaderForColumnDisplay?.AH_DueDate ?? ZDateTime.Empty;
		}

		public ZPropertyInfo DueDateInfo
		{
			get { return GetZPropertyInfo(nameof(DueDate)); }
		}

		#endregion

		#region TransactionDate

		public ZDateTime TransactionDate
		{
			get
			{
				if (IsWipOrAccrual)
				{
					return TransactionLine.AL_PostDate;
				}
				else
				{
					return TransactionHeaderForColumnDisplay?.AH_InvoiceDate ?? ZDateTime.Empty;
				}
			}
		}

		public ZPropertyInfo TransactionDateInfo
		{
			get { return GetZPropertyInfo(nameof(TransactionDate)); }
		}

		#endregion

		#region JobHeaderPK

		[List("Lookups.Jobs")]
		public ZGuid JobHeaderPK
		{
			get
			{
				if (IsWipOrAccrual)
				{
					return TransactionLine.AL_JH;
				}
				else
				{
					return TransactionHeaderForColumnDisplay?.AH_JH ?? ZGuid.Empty;
				}
			}
		}

		public ZPropertyInfo JobHeaderPKInfo
		{
			get { return GetZPropertyInfo(nameof(JobHeaderPK)); }
		}

		#endregion

		#region ChargeCodePK

		[List("Lookups.ChargeCodes")]
		public ZGuid ChargeCodePK
		{
			get
			{
				return TransactionLine?.AL_AC ?? ZGuid.Empty;
			}
		}

		public ZPropertyInfo ChargeCodePKInfo
		{
			get { return GetZPropertyInfo(nameof(ChargeCodePK)); }
		}

		#endregion

		#region GLAccountDesc

		public ZString GLAccountDesc
		{
			get
			{
				return GLAccount?.AG_DescriptionMultilingual ?? ZString.Empty;
			}
		}

		public ZPropertyInfo GLAccountDescInfo
		{
			get { return GetZPropertyInfo(nameof(GLAccountDesc)); }
		}

		#endregion

		#region SubAccount

		public ZString SubAccount
		{
			get
			{
				var subAccountTypeCode = ZString.Empty;

				var transactionPK = ZGuid.Empty;
				if (GLD_AL_TransactionLine.IsValid)
				{
					if (TransactionLine.AL_AG == GLD_AG_GLAccount)
					{
						transactionPK = GLD_AL_TransactionLine;
					}
				}
				else if (GLD_AH_TransactionHeader.IsValid && TransactionHeader.AH_AG == GLD_AG_GLAccount)
				{
					transactionPK = GLD_AH_TransactionHeader;
				}

				if (!transactionPK.IsEmpty)
				{
					var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, $"SELECT SubAccountCodeTypes FROM GetMultiSubAccountDetails ('{transactionPK}','{GLD_AG_GLAccount}')");
					if (dataTable.Rows.Cast<DataRow>().Any())
					{
						var dataRow = dataTable.Rows[0];
						subAccountTypeCode = dataRow["SubAccountCodeTypes"].ToString();
					}
				}

				return subAccountTypeCode;
			}
		}

		public ZPropertyInfo SubAccountInfo
		{
			get { return GetZPropertyInfo(nameof(SubAccount)); }
		}

		#endregion

		#region Header Description

		public ZString HeaderDescription
		{
			get { return TransactionHeaderForColumnDisplay?.AH_Desc ?? ZString.Empty; }
		}

		public ZPropertyInfo HeaderDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(HeaderDescription)); }
		}

		#endregion

		#region Line Description

		public ZString LineDescription
		{
			get { return TransactionLine?.AL_Desc ?? ZString.Empty; }
		}

		public ZPropertyInfo LineDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(LineDescription)); }
		}

		#endregion

		#region Presentation Journal Category

		public ZString PresentationJournalCategory
		{
			get { return TransactionHeaderForColumnDisplay != null && TransactionHeaderForColumnDisplay.AH_Ledger == LedgerTypes.General ? TransactionHeaderForColumnDisplay.AH_TransactionCategory : ZString.Empty; }
		}

		public ZPropertyInfo PresentationJournalCategoryInfo
		{
			get { return GetZPropertyInfo(nameof(PresentationJournalCategory)); }
		}

		#endregion

		#region Units

		public ZString Units
		{
			get
			{
				return GLAccount?.AG_StatisticalUnits ?? ZString.Empty;
			}
		}

		public ZPropertyInfo UnitsInfo
		{
			get { return GetZPropertyInfo(nameof(Units)); }
		}

		#endregion

		#region TransactionType

		public ZString TransactionType
		{
			get
			{
				if (IsWipOrAccrual)
				{
					return TransactionLine.AL_LineType;
				}
				else
				{
					return TransactionHeaderForColumnDisplay?.AH_TransactionType ?? ZString.Empty;
				}
			}
		}

		public ZPropertyInfo TransactionTypeInfo
		{
			get { return GetZPropertyInfo(nameof(TransactionType)); }
		}

		#endregion

		#region IAccountingNumberFountainDataSource members

		IDbConnected IAccountingNumberFountainDataSource.Factory => Factory;

		ZDateTime IAccountingNumberFountainDataSource.PostDate => GLD_PostDate;

		GlbBranch IAccountingNumberFountainDataSource.Branch => Branch;

		GlbDepartment IAccountingNumberFountainDataSource.Department => Department;

		#endregion
		bool IsWipOrAccrual => TransactionLine != null && (TransactionLine.AL_LineType == TransactionLineTypes.WIP || TransactionLine.AL_LineType == TransactionLineTypes.Accrual);

		#endregion

	}
}
