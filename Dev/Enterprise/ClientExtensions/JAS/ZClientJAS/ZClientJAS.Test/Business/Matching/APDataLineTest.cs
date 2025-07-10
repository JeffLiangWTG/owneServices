using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.Matching.Testing
{
	public class APDataLineTest : PreMatchedDataLineTest
	{
		protected APDataLine APLine
		{
			get
			{
				return (APDataLine)Line;
			}
		}

		protected override ZString Ledger
		{
			get
			{
				return ZArchitecture.Core.LedgerTypes.AccountsPayable;
			}
		}

		protected override PreMatchedDataLine NewPreMatchedDataLine(AccTransactionHeaderWithJobInfo transaction)
		{
			return new APDataLine(transaction, new ZDateTime(2004, 12, 1));
		}

		#region Expected Exported Transaction Strings
		protected override string ExpectedNormalTransactionString
		{
			get
			{
				return "200411011971111420041201MBILL               TESTHB              TRAN0012            TRAN00              A                            303.600USDTThis is the description           CRD             ";
			}
		}

		protected override string ExpectedTransactionWithMultipleMawbsV3_3i2
		{
			get
			{
				return "200401011971111420041201MB101               HB101               TRAN101             TRAN10              A                            101.000AUDTDescription for 101               CRD             ";
			}
		}

		protected override string ExpectedTransactionWithMultipleHawbsV3_3i4
		{
			get
			{
				return "200401011971111420041201MB201               HB201               TRAN201             TRAN20              M                            201.000AUDFDescription for 201               INV             ";
			}
		}

		protected override string ExpectedTransactionWithCategoryOorTV3_3i6
		{
			get
			{
				return "200401011971111420041201TRAN301             TRAN301             TRAN301             TRAN30              T                            301.000AUDFDescription for 301               INV             ";
			}
		}

		protected override string ExpectedTransactionWithCategorySV3_3i7
		{
			get
			{
				return "200401011971111420041201CN401               HB401               TRAN401             TRAN40              S                            401.000AUDTDescription for 401               CRD             ";
			}
		}

		protected override string ExpectedTransactionWithCategorySButNoContainerNum
		{
			get
			{
				return "200401011971111420041201MB401               HB401               TRAN401             TRAN40              S                            401.000AUDTDescription for 401               CRD             ";
			}
		}

		protected override string ExpectedTransactionWithDirectConsolV3_3i9
		{
			get
			{
				return "200401011971111420041201MB501               MB501               TRAN501             TRAN50              A                            551.100AUDTDescription for 501               ADJ             ";
			}
		}

		protected override string ExpectedTransactionWithCoLoadConsolV3_3i10
		{
			get
			{
				return "200401011971111420041201MB601               HB601               TRAN601             TRAN60              A                            594.990AUDFDescription for 601               INV             ";
			}
		}

		protected override string ExpectedTransactionWithCoLoadConsolButNoVoyageNumber
		{
			get
			{
				return "200401011971111420041201MB701               HB701               TRAN701             TRAN70              A                            315.450AUDFDescription for 701               INV             ";
			}
		}

		string fNettingCodes;
		protected override string NettingCodes
		{
			get
			{
				if (fNettingCodes == null)
				{
					fNettingCodes = string.Format("{0,-8}{1,-8}", CurrentOrg.CustomsCodes.GetUNC().Left(8), CounterpartOrg.CustomsCodes.GetUNC().Left(8));
				}

				return fNettingCodes;
			}
		}
		#endregion
	}
}
