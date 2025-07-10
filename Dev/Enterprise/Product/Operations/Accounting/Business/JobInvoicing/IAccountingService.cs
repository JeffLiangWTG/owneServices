using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
#if NETFRAMEWORK
using System.ServiceModel;
using System.ServiceModel.Web;
#elif NETCOREAPP
using CoreWCF.Web;
#endif
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
#if NETFRAMEWORK
	[ServiceContract(Namespace = "http://cargowise.com/Accounting")]
	public interface IAccountingService
	{
		[WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]
		[OperationContract]
		string PostTransactions(Guid jobPK, JobInvoicingPostingOption postingOption, bool shouldPerformBackDating, DateTime postDate, DateTime invoiceDate);

		[WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]
		[OperationContract]
		PreviewInvoicesData PreviewTransactions(Guid jobPK, bool shouldPerformBackDating, DateTime postDate, DateTime invoiceDate);

		[WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]
		[OperationContract]
		BackPostingData GetBackPostingData(Guid jobPK, JobInvoicingPostingOption postingOption);

		[OperationContract]
		string RecognizeRevenue(Guid jobPK);

		[OperationContract]
		string CreateProfitShareCharges(Guid jobPK);

		[WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]
		[OperationContract]
		string ReverseInvoiceRedoBilling(Guid jobPK, string reversingReason);

		[OperationContract]
		JobCalcPropertiesData GetJobCalcPropertiesData(Guid jobPK);

		[OperationContract]
		JobChargeCalcPropertiesData GetJobChargeCalcPropertiesData(Guid chargePK);

		[WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]
		[OperationContract]
		JobHeaderDefaultFields GetJobHeaderDefaultFields(Guid shipmentPK, string loginName, Guid branchPK, Guid departmentPK);
	}
#elif NETCOREAPP

	[CoreWCF.ServiceContract(Namespace = "http://cargowise.com/Accounting")]
	public interface IAccountingService
	{
		[WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]
		[CoreWCF.OperationContract]
		string PostTransactions(Guid jobPK, JobInvoicingPostingOption postingOption, bool shouldPerformBackDating, DateTime postDate, DateTime invoiceDate);

		[WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]
		[CoreWCF.OperationContract]
		PreviewInvoicesData PreviewTransactions(Guid jobPK, bool shouldPerformBackDating, DateTime postDate, DateTime invoiceDate);

		[WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]
		[CoreWCF.OperationContract]
		BackPostingData GetBackPostingData(Guid jobPK, JobInvoicingPostingOption postingOption);

		[CoreWCF.OperationContract]
		string RecognizeRevenue(Guid jobPK);

		[CoreWCF.OperationContract]
		string CreateProfitShareCharges(Guid jobPK);

		[WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]
		[CoreWCF.OperationContract]
		string ReverseInvoiceRedoBilling(Guid jobPK, string reversingReason);

		[CoreWCF.OperationContract]
		JobCalcPropertiesData GetJobCalcPropertiesData(Guid jobPK);

		[CoreWCF.OperationContract]
		JobChargeCalcPropertiesData GetJobChargeCalcPropertiesData(Guid chargePK);

		[WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]
		[CoreWCF.OperationContract]
		JobHeaderDefaultFields GetJobHeaderDefaultFields(Guid shipmentPK, string loginName, Guid branchPK, Guid departmentPK);
	}
#endif

	[DataContract(Namespace = "")]
	[Serializable]
	public class BackPostingData
	{
		[DataMember(Order = 0)]
		public virtual DateTime InvoiceDate { get; set; }
		[DataMember(Order = 1)]
		public virtual DateTime PostDate { get; set; }
		[DataMember(Order = 2)]
		public virtual string RevenueRecognitionDates { get; set; }
		[DataMember(Order = 3)]
		public string ErrorMessage { get; set; }
	}

	[DataContract(Namespace = "")]
	[Serializable]
	public class PreviewInvoicesData
	{
		[DataMember(Order = 0)]
		public IEnumerable<PreviewInvoiceData> PreviewInvoices { get; set; }
		[DataMember(Order = 1)]
		public string ErrorMessage { get; set; }
	}

	[DataContract(Namespace = "")]
	[Serializable]
	public class PreviewInvoiceData
	{
		[DataMember(Order = 0)]
		public string Account { get; set; }
		[DataMember(Order = 1)]
		public string Description { get; set; }
		[DataMember(Order = 2)]
		public string Category { get; set; }
		[DataMember(Order = 3)]
		public string InvoiceType { get; set; }
		[DataMember(Order = 4)]
		public string Currency { get; set; }
		[DataMember(Order = 5)]
		public decimal Amount { get; set; }
		[DataMember(Order = 6)]
		public byte[] PDFDocument { get; set; }
	}

	[DataContract(Namespace = "")]
	[Serializable]
	public class JobCalcPropertiesData
	{
		[DataMember(Order = 0)]
		public string LocalCurrency { get; set; }
		[DataMember(Order = 1)]
		public decimal ProfitRevenueMargin { get; set; }
		[DataMember(Order = 2)]
		public decimal ChargeableWgtVol { get; set; }
	}

	[DataContract(Namespace = "")]
	[Serializable]
	public class JobChargeCalcPropertiesData
	{
		[DataMember(Order = 0)]
		public bool IsRevenuePosted { get; set; }
		[DataMember(Order = 1)]
		public bool IsApportioned { get; set; }
		[DataMember(Order = 2)]
		public bool IsApproved { get; set; }
		[DataMember(Order = 3)]
		public bool IsCostPosted { get; set; }
		[DataMember(Order = 4)]
		public string ChargeType { get; set; }
		[DataMember(Order = 5)]
		public string SellRecognition { get; set; }
		[DataMember(Order = 6)]
		public string CostRecognition { get; set; }
		[DataMember(Order = 7)]
		public decimal MarginPercentage { get; set; }
		[DataMember(Order = 8)]
		public decimal CFXAmtReverseSign { get; set; }
		[DataMember(Order = 9)]
		public decimal OSSellAmtWithGST { get; set; }
		[DataMember(Order = 10)]
		public decimal CFXAmt { get; set; }
		[DataMember(Order = 11)]
		public decimal AgentDeclaredRevenueLocal { get; set; }
		[DataMember(Order = 12)]
		public decimal AgentDeclaredCostLocal { get; set; }
	}

	[DataContract(Namespace = "")]
	[Serializable]
	public class JobHeaderDefaultFields
	{
		[DataMember(Order = 0)]
		public byte JH_JobBufferPercentOverride { get; set; }
		[DataMember(Order = 1)]
		public DateTime? JH_A_JCL { get; set; }
		[DataMember(Order = 2)]
		public DateTime? JH_A_JOP { get; set; }
		[DataMember(Order = 3)]
		public DateTime? JH_RevenueRecognizedDate { get; set; }
		[DataMember(Order = 4)]
		public DateTime? JH_SystemLastEditTimeUtc { get; set; }
		[DataMember(Order = 5)]
		public DateTime? JH_JobPlannedStartDate { get; set; }
		[DataMember(Order = 6)]
		public DateTime? JH_SystemCreateTimeUtc { get; set; }
		[DataMember(Order = 7)]
		public short JH_UniqueJobInvoiceNumber { get; set; }
		[DataMember(Order = 8)]
		public decimal JH_AgentChargesCFX { get; set; }
		[DataMember(Order = 9)]
		public decimal JH_LocalChargesCFX { get; set; }
		[DataMember(Order = 10)]
		public Guid JH_GB { get; set; }
		[DataMember(Order = 11)]
		public Guid? JH_GC { get; set; }
		[DataMember(Order = 12)]
		public Guid JH_GE { get; set; }
		[DataMember(Order = 13)]
		public Guid? JH_JH_ParentJob { get; set; }
		[DataMember(Order = 14)]
		public Guid? JH_OA_AgentCollectAddr { get; set; }
		[DataMember(Order = 15)]
		public Guid? JH_OA_LocalChargesAddr { get; set; }
		[DataMember(Order = 16)]
		public Guid? JH_OC_LocalBillingContact { get; set; }
		[DataMember(Order = 17)]
		public Guid? JH_ProfitShareInvoice { get; set; }
		[DataMember(Order = 18)]
		public string JH_Description { get; set; }
		[DataMember(Order = 19)]
		public string JH_ExcludeFromPeriodicRating { get; set; }
		[DataMember(Order = 20)]
		public string JH_GS_NKRepOps { get; set; }
		[DataMember(Order = 21)]
		public string JH_GS_NKRepSales { get; set; }
		[DataMember(Order = 22)]
		public string JH_HeaderType { get; set; }
		[DataMember(Order = 23)]
		public string JH_HoldReason { get; set; }
		[DataMember(Order = 24)]
		public string JH_IsProfitSharePosted { get; set; }
		[DataMember(Order = 25)]
		public string JH_JobLocalReference { get; set; }
		[DataMember(Order = 26)]
		public string JH_LocalClientInvoicingStyle { get; set; }
		[DataMember(Order = 27)]
		public string JH_Name { get; set; }
		[DataMember(Order = 28)]
		public string JH_PaymentCollectionStatus { get; set; }
		[DataMember(Order = 29)]
		public string JH_ProfitLossReasonCode { get; set; }
		[DataMember(Order = 30)]
		public string JH_RatingHasBeenRun { get; set; }
		[DataMember(Order = 31)]
		public string JH_SingleAgentsInvoicePerConsol { get; set; }
		[DataMember(Order = 32)]
		public string JH_Status { get; set; }
		[DataMember(Order = 33)]
		public string JH_SystemCreateUser { get; set; }
		[DataMember(Order = 34)]
		public string JH_SystemLastEditUser { get; set; }
		[DataMember(Order = 35)]
		public string JH_TH_NKQuoteNumber { get; set; }
	}
}
