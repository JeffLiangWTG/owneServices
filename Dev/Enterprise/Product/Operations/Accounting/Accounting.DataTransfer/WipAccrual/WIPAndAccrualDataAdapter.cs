using System;
using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.GenericJob;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.DataTransfer.XmlMapping;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.WipsAndAccruals
{
	public class WIPAndAccrualDataAdapter : BaseAccountingDataAdapter<WIPAccrualPRBusinessObject, Xsd.WipOrAccrual>
	{
		#region Data Adapter Overrides

		public override string RootCollectionElementName
		{
			get { return "FinancialTransactions"; }
		}

		public override string RootElementName
		{
			get { return "FinancialWIPOrAccrual"; }
		}

		public override XmlSchema Schema
		{
			get { return AccountingXmlSchemaDefinitions.Instance.FinancialWipOrAccrualSchema; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return AccountingXmlSchemaDefinitions.Instance.FinancialWipOrAccrualSchema; }
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(WIPAccrualPRBusinessObject bizObj, Xsd.WipOrAccrual constructedValueObject, IValueObjectExportContext context)
		{
			PopulateValuesForXmlWIPOrAccrual(bizObj, constructedValueObject, context);
		}

		protected void PopulateValuesForXmlWIPOrAccrual(WIPAccrualPRBusinessObject wIPWrapper, Xsd.WipOrAccrual xmlWIPOrAccrual, IValueObjectExportContext context)
		{
			BusinessObjectFactory exportFactory = wIPWrapper.Factory;

			BaseWIPAccrual wIPOrAccrualBizObj = wIPWrapper.WIP;
			xmlWIPOrAccrual.PostOrReverse = wIPWrapper.PostedOrReverseStatus;

			xmlWIPOrAccrual.LocalInvoiceAmtExclTax = GetXmlFinancialValue(wIPOrAccrualBizObj.AL_LocalExTaxAmount, wIPOrAccrualBizObj.Branch.Company.LocalCurrency, wIPOrAccrualBizObj.GetType(), xmlWIPOrAccrual.PostOrReverse);

			ZDateTime postOrReverseDate = ZDateTime.Empty;
			int batchNumber = 0;
			int sequenceNumber = 0;
			if (xmlWIPOrAccrual.PostOrReverse == Xsd.WipOrAccrualPostOrReverse.P)
			{
				postOrReverseDate = wIPOrAccrualBizObj.AL_PostDate;

				if (wIPOrAccrualBizObj.ExportBatchSequencePostedObject != null)
				{
					batchNumber = wIPOrAccrualBizObj.ExportBatchSequencePostedObject.XB_BatchNumber;
					sequenceNumber = wIPOrAccrualBizObj.ExportBatchSequencePostedObject.XB_Sequence;
				}
				else
				{
					sequenceNumber = 1;
				}
			}
			else if (xmlWIPOrAccrual.PostOrReverse == Xsd.WipOrAccrualPostOrReverse.R)
			{
				postOrReverseDate = wIPOrAccrualBizObj.AL_ReverseDate;

				if (wIPOrAccrualBizObj.ExportBatchSequenceReversedObject != null)
				{
					batchNumber = wIPOrAccrualBizObj.ExportBatchSequenceReversedObject.XB_BatchNumber;
					sequenceNumber = wIPOrAccrualBizObj.ExportBatchSequenceReversedObject.XB_Sequence;
				}
			}
			if (!postOrReverseDate.IsEmpty)
			{
				xmlWIPOrAccrual.PostOrReverseDate = postOrReverseDate;
				xmlWIPOrAccrual.PostOrReversePeriod = BusinessObjectRetriever.GetGLPeriodFromDate(wIPOrAccrualBizObj.Factory, postOrReverseDate).ToString();
			}

			xmlWIPOrAccrual.TransactionReference = batchNumber.ToString().PadLeft(7, '0') + sequenceNumber.ToString().PadLeft(5, '0');

			xmlWIPOrAccrual.Branch = wIPOrAccrualBizObj.Branch.GB_Code;

			if (wIPOrAccrualBizObj.ChargeCode != null)
			{
				xmlWIPOrAccrual.ChargeCode = wIPOrAccrualBizObj.ChargeCode.AC_Code;
				xmlWIPOrAccrual.ChargeGroup = wIPOrAccrualBizObj.ChargeCode.AC_ChargeGroup;
				xmlWIPOrAccrual.ChargeSubGroup = wIPOrAccrualBizObj.ChargeCode.AC_ChargeSubGroup;

				if (wIPOrAccrualBizObj.ChargeCode.SalesGroup != null)
				{
					xmlWIPOrAccrual.ChargeCodeSalesGroup = wIPOrAccrualBizObj.ChargeCode.SalesGroup.AR_Code;
				}

				if (wIPOrAccrualBizObj.ChargeCode.ExpenseGroup != null)
				{
					xmlWIPOrAccrual.ChargeCodeExpenseGroup = wIPOrAccrualBizObj.ChargeCode.ExpenseGroup.AR_Code;
				}

				if (wIPOrAccrualBizObj.GLHeader != null)
				{
					xmlWIPOrAccrual.GLAccount = wIPOrAccrualBizObj.GLHeader.AG_AccountNum;
				}
			}

			xmlWIPOrAccrual.Department = wIPOrAccrualBizObj.Department.GE_Code;

			xmlWIPOrAccrual.CreatedUserId = wIPOrAccrualBizObj.CreatingUserID;
			xmlWIPOrAccrual.Description = wIPOrAccrualBizObj.AL_Desc;
			xmlWIPOrAccrual.LineType = GetWIPOrAccrualLineType(wIPOrAccrualBizObj.AL_LineType);
#if DEBUG
			xmlWIPOrAccrual.WipOrAccrualGUID = "GUID";
#else
			xmlWIPOrAccrual.WipOrAccrualGUID = wIPOrAccrualBizObj.PK.ToString();
#endif

			if (wIPOrAccrualBizObj.Header != null)
			{
				xmlWIPOrAccrual.DebtorOrCreditor = new OrganisationValueObjectDataAdapter().ExportToValueObject(wIPOrAccrualBizObj.Header, context);
			}

			GenericJob jobDetails = null;

			if (wIPOrAccrualBizObj.AL_JH.IsValid)
			{
				jobDetails = wIPOrAccrualBizObj.Job.LoadGenericJob<GenericJob>();
			}

			if (jobDetails != null)
			{
				if (jobDetails.InvoicingSupporter.Origin != null)
				{
					xmlWIPOrAccrual.OriginPortCode = Xsd.UNLOCO.FromPortCode(exportFactory, jobDetails.InvoicingSupporter.Origin.RL_Code);
				}

				if (jobDetails.InvoicingSupporter.Destination != null)
				{
					xmlWIPOrAccrual.DestinationPortCode = Xsd.UNLOCO.FromPortCode(exportFactory, jobDetails.InvoicingSupporter.Destination.RL_Code);
				}

				xmlWIPOrAccrual.MasterBillNo = jobDetails.InvoicingSupporter.MasterBillNumber;
				xmlWIPOrAccrual.HouseBIllNo = jobDetails.InvoicingSupporter.HouseBillNumber;

				var paymentTermInfo = jobDetails.InvoicingSupporter.PaymentTerm.GetPaymentTermInfo(CostSell.Revenue);
				xmlWIPOrAccrual.Incoterm = paymentTermInfo != null && paymentTermInfo.InfoType == PaymentTermType.Incoterm ? paymentTermInfo.Value : string.Empty;

				xmlWIPOrAccrual.JobNo = jobDetails.JobNumber;
				xmlWIPOrAccrual.JobType = WipOrAccrualJobTypeXmlMapping.Instance.GetExternalCode(jobDetails.VJ_JobType, Res.GetString("a83deb11-a972-49d4-acc3-c87cb756729f", "Job Type Code mapping"), context);
				xmlWIPOrAccrual.JobTypeSpecified = true;

				ZDateTime revenueRecognitionDate = wIPOrAccrualBizObj.InvoicingJob.GetRevenueRecognitionDate(wIPOrAccrualBizObj.AL_RevRecognitionType);
				xmlWIPOrAccrual.JobRevenueRecognitionDate = revenueRecognitionDate.IsEmpty || revenueRecognitionDate == AccountingConstants.RevenueRecognitionDateConstants.Immediate
															? xmlWIPOrAccrual.PostOrReverseDate
															: revenueRecognitionDate;
			}

			xmlWIPOrAccrual.DepartmentActivitySpecified = true;
			xmlWIPOrAccrual.DepartmentActivity = DepartmentActivityXmlMapping.Instance.GetExternalCode(wIPOrAccrualBizObj.Department.GE_Activity, Res.GetString("54e8a33d-d3fd-47c8-b632-5464ba243660", "Setting Department Activity"), context);
			SetAgentsReference(xmlWIPOrAccrual, wIPOrAccrualBizObj, context);
		}

		protected void SetAgentsReference(Xsd.WipOrAccrual xmlWIPAccrualLine, BaseWIPAccrual wIPAccrualLine, INotifications notify)
		{
			BusinessObjectFactory factory = wIPAccrualLine.Factory;

			if (wIPAccrualLine.Job != null)
			{
				BaseJobDeclaration jobDec = null;
				if (IsDeclarationJob(wIPAccrualLine.Job))
				{
					jobDec = factory.Load<BaseJobDeclaration>(wIPAccrualLine.Job.JH_ParentID);
				}
				else if (IsShipmentJob(wIPAccrualLine.Job))
				{
					ZQuery filter = new ZQuery(JobDeclarationSchema.JE_JS, wIPAccrualLine.Job.JH_ParentID);
					filter.AddToFilter(JobDeclarationSchema.JE_GB, GlbCompany.CurrentCompany.Branches.GetPKs());
					jobDec = factory.LoadTop1<BaseJobDeclaration>(filter);
				}

				if (jobDec != null)
				{
					xmlWIPAccrualLine.AgentsReference = jobDec.JE_AgentsReference;
				}
			}
		}

		#region Helpers

		#region Line Type

		protected Xsd.WipOrAccrualLineType GetWIPOrAccrualLineType(ZString lineType)
		{
			Xsd.WipOrAccrualLineType result;

			if (lineType == TransactionLineTypes.WIP)
			{
				result = Xsd.WipOrAccrualLineType.REV;
			}
			else if (lineType == TransactionLineTypes.Accrual)
			{
				result = Xsd.WipOrAccrualLineType.CST;
			}
			else
			{
				throw new InvalidOperationException("Only WIPs and Accruals can be exported from this DataAdapter");
			}

			return result;
		}

		#endregion

		#region Financial Value

		protected Xsd.FinancialValue GetXmlFinancialValue(ZDecimal value, RefCurrency currency, Type transactionType, Xsd.WipOrAccrualPostOrReverse postOrReverse)
		{
			ZDecimal result = ZArchitecture.Core.Utilities.Round(value * MultiplierForExport(transactionType, postOrReverse), GlbCompany.CurrentCompany.LocalCurrency.Decimals);
			return Xsd.FinancialValue.FromAmountAndCurrency(result, currency);
		}

		protected ZDecimal MultiplierForExport(Type type, Xsd.WipOrAccrualPostOrReverse postOrReverse)
		{
			if ((type == typeof(WIP) && postOrReverse == Xsd.WipOrAccrualPostOrReverse.R) ||
				(type == typeof(Accrual) && postOrReverse == Xsd.WipOrAccrualPostOrReverse.P))
			{
				return -1;
			}
			else
			{
				return 1;
			}
		}

		#endregion

		#endregion

		#endregion

		#region Import

		protected override void ImportFromValueObjectCore(WIPAccrualPRBusinessObject bizObj, Xsd.WipOrAccrual value, IValueObjectImportContext context)
		{
			throw new NotSupportedException("Importing WIPs and Accruals is currently not supported");
		}

		#endregion
	}
}
