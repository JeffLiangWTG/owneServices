using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.AU;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.Wow
{
	public class WowDocDeclaration : DocDeclaration
	{
		#region Constructors and Type Overriding

		protected WowDocDeclaration(JobDeclaration declaration, BusinessObjectFactory factoryToWrap) : base(declaration, factoryToWrap)
		{
		}

		public new static DocDeclaration New(JobDeclaration declaration, BusinessObjectFactory factoryToWrap)
		{
			if (declaration == null)
			{
				return null;
			}
			else
			{
				return new WowDocDeclaration(declaration, factoryToWrap);
			}
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(New);
		}

		#endregion

		#region Constants

		public const string AccountsDocumentDataNoteDescription = "Accounts Document Data";

		#endregion

		protected override Enterprise.DocumentWrappers.Customs.Base.DocBaseCusContainerCollection CreateCusContainerCollection(ICusContainerCollection<BaseCusContainer> collectionToWrap)
		{
			return new WowDocCusContainerCollection(collectionToWrap, Factory);
		}

		public ZString SingleContainerNumber
		{
			get
			{
				if (Containers.Count > 0)
				{
					return Containers[0].ContainerNumber;
				}
				else
				{
					return null;
				}
			}
		}

		public new WowDocJobComInvoiceLineCollection AllInvoiceLines
		{
			get
			{
				WowDocJobComInvoiceLineCollection result = new WowDocJobComInvoiceLineCollection(Factory);
				result.AddRange(base.AllInvoiceLines);
				return result;
			}
		}

		public ZDecimal TotalInvoiceLinesVolume
		{
			get { return AllInvoiceLines.Sum(line => ((DocJobComInvoiceLine)line).Volume); }
		}

		#region Invoicing Charges

		public ZString ConsolCharges { get { return CalculateChargeFormatted("CONSOL"); } }
		public ZString PSCCharges { get { return CalculateChargeFormatted("PSC"); } }
		public ZString PreCarriageCharges { get { return CalculateChargeFormatted("HANDLING?"); } }
		public ZString TransportCharges { get { return CalculateChargeFormatted("TRAN"); } }
		public ZString FumigationCharges { get { return CalculateChargeFormatted("FUMO"); } }
		public ZString StorageCharges { get { return CalculateChargeFormatted("STO"); } }
		public ZString BankCharges { get { return CalculateChargeFormatted("BANK"); } }
		public ZString TravelExpenseCharges { get { return CalculateChargeFormatted("TE"); } }
		public ZString AssistsCharges { get { return CalculateChargeFormatted("ASTS"); } }
		public ZString DestuffingCharges { get { return CalculateChargeFormatted("DESTUF"); } }

		public ZDecimal OriginCharges
		{
			get { return TotalLocalSellAmtByChargeGroup(ChargeCodeGroupList.Codes.Origin, false); }
		}

		public ZDecimal DestinationCharges
		{
			get { return TotalLocalSellAmtByChargeGroup(ChargeCodeGroupList.Codes.Destination, false); }
		}

		public ZDecimal OtherImportCharges
		{
			get { return TotalLocalSellAmtByChargeGroup(ChargeCodeGroupList.Codes.NotGrouped, false); }
		}

		public ZDecimal QuarantineDetentionFumigationCharges
		{
			get { return TotalLocalSellAmtByChargeGroup(ChargeCodeGroupList.Codes.Brokerage, false); }
		}

		public ZDecimal DetentionCharges
		{
			get { return TotalLocalSellAmtByChargeGroup("DET", true); }
		}

		ZDecimal TotalLocalSellAmtByChargeGroup(ZString chargeGroupOrCode, bool isChargeCode)
		{
			ZDecimal result = ZDecimal.Zero;

			if (JobInvoicingJob != null)
			{
				foreach (DocJobInvoicingJobCharge current in JobInvoicingJob.Charges)
				{
					if (current.ChargeCode != null)
					{
						if ((isChargeCode && current.ChargeCode.Code == chargeGroupOrCode) ||
							(!isChargeCode && current.ChargeCode.ChargeGroup == chargeGroupOrCode))
						{
							result += current.LocalSellAmt;
						}
					}
				}
			}

			return result;
		}

		public ZString AdminFeeCharges
		{
			get
			{
				ZString result;
				JobCharge charge1 = FindJobCharge("AFADSE");
				JobCharge charge2 = FindJobCharge("AFAENT");
				JobCharge charge3 = FindJobCharge("AFAINV");
				JobCharge charge4 = FindJobCharge("AFSDSE");
				JobCharge charge5 = FindJobCharge("AFSENT");
				JobCharge charge6 = FindJobCharge("AFSINV");
				if (charge1 == null)
				{
					result = "Could not find charge AFADSE";
				}
				else if (charge2 == null)
				{
					result = "Could not find charge AFAENT";
				}
				else if (charge3 == null)
				{
					result = "Could not find charge AFAINV";
				}
				else if (charge4 == null)
				{
					result = "Could not find charge AFSDSE";
				}
				else if (charge5 == null)
				{
					result = "Could not find charge AFSENT";
				}
				else if (charge6 == null)
				{
					result = "Could not find charge AFSINV";
				}
				else
				{
					result =
						(charge1.JR_LocalCostAmt +
						charge2.JR_LocalCostAmt +
						charge3.JR_LocalCostAmt +
						charge4.JR_LocalCostAmt +
						charge5.JR_LocalCostAmt +
						charge6.JR_LocalCostAmt).ToString();
				}
				return result;
			}
		}

		protected ZString CalculateChargeFormatted(ZString chargeCode)
		{
			JobCharge charge = FindJobCharge(chargeCode);
			return (charge == null) ? ("Could not find charge " + chargeCode) : charge.JR_LocalCostAmt.ToString();
		}

		protected JobCharge FindJobCharge(ZString chargeCode)
		{
			ZQuery jobFilter = new ZQuery();
			jobFilter.AddToFilter(JobHeaderSchema.JH_ParentID, SQLComparisonOperator.Equal, JobDeclaration.PK);
			jobFilter.AddToFilter(JobHeaderSchema.JH_GC, SQLComparisonOperator.Equal, JobDeclaration.CompanyPK);
			Enterprise.Accounting.Business.JobInvoicing.Job job = (Enterprise.Accounting.Business.JobInvoicing.Job)Factory.LoadTop1(typeof(Enterprise.Accounting.Business.JobInvoicing.Job), jobFilter);

			ZQuery chargeCodeFilter = new ZQuery();
			chargeCodeFilter.AddToFilter(AccChargeCodeSchema.AC_Code, SQLComparisonOperator.Equal, chargeCode);
			chargeCodeFilter.AddToFilter(AccChargeCodeSchema.AC_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);

			AccChargeCode chargeCodeBus = (AccChargeCode)Factory.LoadTop1(typeof(AccChargeCode), chargeCodeFilter);
			JobCharge result = null;

			if (chargeCodeBus != null)
			{
				JobCharge[] charges = (JobCharge[])job.Charges.Find(new ZQuery(JobChargeSchema.JR_AC, SQLComparisonOperator.Equal, chargeCodeBus.PK));
				if (charges.Length > 1)
				{
					ErrorReporter.ReportOnce(
						"ExpectedOnly1ChargeFromSingleChargeCodeInWowDocDeclaration",
						"Expected only 1 charge from a single charge code (code " + chargeCodeBus.AC_Code + ")");
				}
				result = charges[0];
			}

			return result;
		}

		#endregion

		#region Implementation

		protected new WoolworthsJobDeclaration JobDeclaration
		{
			get { return (WoolworthsJobDeclaration)base.JobDeclaration; }
		}

		#endregion
	}
}
