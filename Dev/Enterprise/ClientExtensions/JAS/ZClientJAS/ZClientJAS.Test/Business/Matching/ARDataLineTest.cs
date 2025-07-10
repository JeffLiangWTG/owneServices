using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.Matching.Testing
{
	public class ARDataLineTest : PreMatchedDataLineTest
	{
		protected ARDataLine ARLine
		{
			get
			{
				return (ARDataLine)Line;
			}
		}

		protected override ZString Ledger
		{
			get
			{
				return ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			}
		}

		protected override PreMatchedDataLine NewPreMatchedDataLine(AccTransactionHeaderWithJobInfo transaction)
		{
			return new ARDataLine(transaction);
		}

		#region Expected Exported Transaction Strings
		protected override string ExpectedNormalTransactionString
		{
			get
			{
				return "         303.60-11/14/71USDTRAN00A AUSYDIDJKT                        CRD           11/01/04MBILL         TESTHB        TRAN0012      R";
			}
		}

		protected override string ExpectedTransactionWithMultipleMawbsV3_3i2
		{
			get
			{
				return "         101.00-11/14/71AUDTRAN10A AUSYDAUMEL                        CRD           01/01/04MB101         HB101         TRAN101       R";
			}
		}

		protected override string ExpectedTransactionWithMultipleHawbsV3_3i4
		{
			get
			{
				return "         201.00+11/14/71AUDTRAN20M AUSYDAUMELQF2012004010120040101   INV           01/01/04MB201         HB201         TRAN201       R";
			}
		}

		protected override string ExpectedTransactionWithCategoryOorTV3_3i6
		{
			get
			{
				return "         301.00+11/14/71AUDTRAN30T AUSYDAUBNE                        INV           01/01/04TRAN301       TRAN301       TRAN301       R";
			}
		}

		protected override string ExpectedTransactionWithCategorySV3_3i7
		{
			get
			{
				return "         401.00-11/14/71AUDTRAN40S AUSYDAUBNE                        CRD           01/01/04CN401         HB401         TRAN401       R";
			}
		}

		protected override string ExpectedTransactionWithCategorySButNoContainerNum
		{
			get
			{
				return "         401.00-11/14/71AUDTRAN40S AUSYDAUBNE                        CRD           01/01/04MB401         HB401         TRAN401       R";
			}
		}

		protected override string ExpectedTransactionWithDirectConsolV3_3i9
		{
			get
			{
				return "         551.10+11/14/71AUDTRAN50A AUSYDAUBNE                        ADJ           01/01/04MB501         MB501         TRAN501       R";
			}
		}

		protected override string ExpectedTransactionWithCoLoadConsolV3_3i10
		{
			get
			{
				return "         594.99+11/14/71AUDTRAN60A AUSYDAUBNEJV22222004020220040202  INV           01/01/04MB601         HB601         TRAN601       R";
			}
		}

		protected override string ExpectedTransactionWithCoLoadConsolButNoVoyageNumber
		{
			get
			{
				return "         315.45+11/14/71AUDTRAN70A AUSYDAUBNE                        INV           01/01/04MB701         HB701         TRAN701       R";
			}
		}

		string fNettingCodes;
		protected override string NettingCodes
		{
			get
			{
				if (fNettingCodes == null)
				{
					fNettingCodes = string.Format("{0,-5}{1,-5}", CounterpartOrg.CustomsCodes.GetUNC().Left(5), CurrentOrg.CustomsCodes.GetUNC().Left(5));
				}

				return fNettingCodes;
			}
		}
		#endregion
	}
}
