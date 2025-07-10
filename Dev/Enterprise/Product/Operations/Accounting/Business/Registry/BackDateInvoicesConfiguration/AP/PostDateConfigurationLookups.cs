using System;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Res = Enterprise.Accounting.Business.Res;
using ResString = Enterprise.Accounting.Business.ResString;

namespace Enterprise.Accounting.Registry.Business
{
	public class PostDateConfigurationLookups : ZLookups
	{
		public PostDateConfigurationLookups(PostDateConfiguration parent)
			: base(parent)
		{
			this.Parent = parent;
		}
		new readonly PostDateConfiguration Parent;

		#region Job Type List

		CodeDescriptionPairList fJobTypeList;
		public CodeDescriptionPairList JobTypeList
		{
			get
			{
				if (fJobTypeList == null)
				{
					fJobTypeList = JobInvoicingConsumerTypes.NewOnlyJobInvoicingTypes();
					fJobTypeList.AddPair(JobTypeAdditionalCodes.All, JobTypeAdditionalDescriptions.All);
				}
				return fJobTypeList;
			}
		}

		public static class JobTypeAdditionalCodes
		{
			public const string All = "ALL";
		}

		public static class JobTypeAdditionalDescriptions
		{
			public static MultilingualString All
			{
				get { return ResString.GetMultilingualString("6a626605-31b5-4ad1-97a9-345066ca0824", "Any Job Type"); }
			}
		}

		#endregion

		#region Direction List

		public CodeDescriptionPairList DirectionList
		{
			get
			{
				if (fDirectionList == null)
				{
					fDirectionList = new CodeDescriptionPairList();
					fDirectionList.AddPair(Constants.FreightShipmentDirection.Code.All, Constants.FreightShipmentDirection.Description.All);
					fDirectionList.AddPair(Constants.FreightShipmentDirection.Code.Export, Constants.FreightShipmentDirection.Description.Export);
					fDirectionList.AddPair(Constants.FreightShipmentDirection.Code.Import, Constants.FreightShipmentDirection.Description.Import);
					fDirectionList.AddPair(Constants.FreightShipmentDirection.Code.Domestic, Constants.FreightShipmentDirection.Description.Domestic);
					fDirectionList.AddPair(Constants.FreightShipmentDirection.Code.Other, Constants.FreightShipmentDirection.Description.Other);
				}

				return fDirectionList;
			}
		}
		CodeDescriptionPairList fDirectionList;

		#endregion

		#region Mode List

		public CodeDescriptionPairList ModeList
		{
			get
			{
				if (fModeList == null)
				{
					fModeList = new CodeDescriptionPairList(OLookUpEditType.TransportType);
					fModeList.AddPair(ModeAdditionalCodes.All, ModeAdditionalDescriptions.All);
				}
				return fModeList;
			}
		}

		CodeDescriptionPairList fModeList;

		public static class ModeAdditionalCodes
		{
			public const string All = "ALL";
		}

		public static class ModeAdditionalDescriptions
		{
			public static string All
			{
				get { return Res.GetString("89503e3d-88d5-4f46-bf43-3cf7618539c4", "Any Transport Mode"); }
			}
		}

		#endregion

		#region Significant Date List

		public CodeDescriptionPairList SignificantDateList
		{
			get
			{
				if (Parent.JobType == String.Empty)
				{
					return CompleteSignificantDateList;
				}
				if (Parent.JobType == JobInvoicingConsumerTypes.Shipment.Code)
				{
					return SignificantDatePermittedForShipmentsList;
				}
				if (Parent.JobType == JobInvoicingConsumerTypes.ForwardingConsol.Code
					|| Parent.JobType == JobInvoicingConsumerTypes.GatewayConsol.Code)
				{
					return SignificantDatePermittedForConsolsList;
				}
				if (Parent.JobType == JobInvoicingConsumerTypes.Brokerage.Code)
				{
					return SignificantDatePermittedForDeclarationsList;
				}
				if (Parent.JobType == JobInvoicingConsumerTypes.ImporterSecurityFiling.Code)
				{
					return SignificantDatePermittedForISFList;
				}
				if (Parent.JobType == JobTypeAdditionalCodes.All)
				{
					return SignificantDatePermittedForAllList;
				}

				return SignificantDatePermittedForOthersList;
			}
		}

		public static class SignificantDateCodes
		{
			public const string ActualArrivalDate = AccountingMasterFilesConstants.SignificantDateCodes.ActualArrivalDate;
			public const string ActualDepartureDate = AccountingMasterFilesConstants.SignificantDateCodes.ActualDepartureDate;
			public const string CustomsClearanceDate = AccountingMasterFilesConstants.SignificantDateCodes.CustomsClearanceDate;
			public const string PickupDate = AccountingMasterFilesConstants.SignificantDateCodes.PickupDate;
			public const string DeliveryDate = AccountingMasterFilesConstants.SignificantDateCodes.DeliveryDate;
			public const string JobOpenDate = "JOP";
			public const string AWBIssueDate = AccountingMasterFilesConstants.SignificantDateCodes.AWBIssueDate;
			public const string InvoiceAddDate = "ADD";
			public const string InvoiceDate = "INV";
			public const string HouseBillIssueDate = AccountingMasterFilesConstants.SignificantDateCodes.HouseBillIssueDate;
		}

		public static class SignificantDateDescriptions
		{
			public static MultilingualString ActualArrivalDate
			{
				get { return ResString.GetMultilingualString("f818b4f7-a8dd-42ed-949d-e37a5d4956e6", "Actual Arrival Date"); }
			}
			public static MultilingualString ActualDepartureDate
			{
				get { return ResString.GetMultilingualString("745e4e5b-0e34-4709-8ae2-49c5d831e95e", "Actual Departure Date"); }
			}
			public static MultilingualString CustomsClearanceDate
			{
				get { return ResString.GetMultilingualString("839c4bee-d167-4b99-be06-72ac6234d89e", "Customs Clearance Date"); }
			}
			public static MultilingualString PickupDate
			{
				get { return ResString.GetMultilingualString("38adc404-da49-4d9f-86a3-f2fd915a6b99", "Pickup Date"); }
			}
			public static MultilingualString DeliveryDate
			{
				get { return ResString.GetMultilingualString("b29e6577-9226-4048-a767-9513eb725914", "Delivery Date"); }
			}
			public static MultilingualString JobOpenDate
			{
				get { return ResString.GetMultilingualString("0e33dce8-4286-4ae2-85be-39d83cdbbc0b", "Job Open Date"); }
			}
			public static MultilingualString AWBIssueDate
			{
				get { return ResString.GetMultilingualString("86bc577d-759f-4656-84ef-06f3ab0d2a2d", "AWB Issue Date"); }
			}

			public static MultilingualString InvoiceAddDate
			{
				get { return ResString.GetMultilingualString("1425b168-8b0e-452b-849c-01ce97c70bdc", "Invoice Add Date"); }
			}

			public static MultilingualString InvoiceDate
			{
				get { return ResString.GetMultilingualString("633836f9-b4ca-4086-8fb0-2ca4ac1a31bb", "Invoice Date"); }
			}

			public static MultilingualString HouseBillIssueDate
			{
				get { return ResString.GetMultilingualString("095666ae-89ba-41ec-9ce4-c87140b480ae", "House Bill Issue Date"); }
			}
		}

		public static CodeDescriptionPairList CompleteSignificantDateList
		{
			get
			{
				if (fCompleteSignificantDateList == null)
				{
					fCompleteSignificantDateList = new CodeDescriptionPairList();
					fCompleteSignificantDateList.AddPair(SignificantDateCodes.ActualArrivalDate, SignificantDateDescriptions.ActualArrivalDate);
					fCompleteSignificantDateList.AddPair(SignificantDateCodes.ActualDepartureDate, SignificantDateDescriptions.ActualDepartureDate);
					fCompleteSignificantDateList.AddPair(SignificantDateCodes.PickupDate, SignificantDateDescriptions.PickupDate);
					fCompleteSignificantDateList.AddPair(SignificantDateCodes.DeliveryDate, SignificantDateDescriptions.DeliveryDate);
					fCompleteSignificantDateList.AddPair(SignificantDateCodes.JobOpenDate, SignificantDateDescriptions.JobOpenDate);
					fCompleteSignificantDateList.AddPair(SignificantDateCodes.CustomsClearanceDate, SignificantDateDescriptions.CustomsClearanceDate);
					fCompleteSignificantDateList.AddPair(SignificantDateCodes.AWBIssueDate, SignificantDateDescriptions.AWBIssueDate);
					fCompleteSignificantDateList.AddPair(SignificantDateCodes.InvoiceAddDate, SignificantDateDescriptions.InvoiceAddDate);
					fCompleteSignificantDateList.AddPair(SignificantDateCodes.InvoiceDate, SignificantDateDescriptions.InvoiceDate);
				}
				return fCompleteSignificantDateList;
			}
		}
		[ThreadStatic]
		static CodeDescriptionPairList fCompleteSignificantDateList;

		public CodeDescriptionPairList SignificantDatePermittedForShipmentsList
		{
			get
			{
				if (fSignificantDatePermittedForShipmentsList == null)
				{
					fSignificantDatePermittedForShipmentsList = new CodeDescriptionPairList();
					fSignificantDatePermittedForShipmentsList.AddPair(SignificantDateCodes.ActualArrivalDate, SignificantDateDescriptions.ActualArrivalDate);
					fSignificantDatePermittedForShipmentsList.AddPair(SignificantDateCodes.ActualDepartureDate, SignificantDateDescriptions.ActualDepartureDate);
					fSignificantDatePermittedForShipmentsList.AddPair(SignificantDateCodes.PickupDate, SignificantDateDescriptions.PickupDate);
					fSignificantDatePermittedForShipmentsList.AddPair(SignificantDateCodes.DeliveryDate, SignificantDateDescriptions.DeliveryDate);
					fSignificantDatePermittedForShipmentsList.AddPair(SignificantDateCodes.JobOpenDate, SignificantDateDescriptions.JobOpenDate);
					fSignificantDatePermittedForShipmentsList.AddPair(SignificantDateCodes.CustomsClearanceDate, SignificantDateDescriptions.CustomsClearanceDate);
					fSignificantDatePermittedForShipmentsList.AddPair(SignificantDateCodes.HouseBillIssueDate, SignificantDateDescriptions.HouseBillIssueDate);
					fSignificantDatePermittedForShipmentsList.AddPair(SignificantDateCodes.AWBIssueDate, SignificantDateDescriptions.AWBIssueDate);
					fSignificantDatePermittedForShipmentsList.AddPair(SignificantDateCodes.InvoiceAddDate, SignificantDateDescriptions.InvoiceAddDate);
					fSignificantDatePermittedForShipmentsList.AddPair(SignificantDateCodes.InvoiceDate, SignificantDateDescriptions.InvoiceDate);
				}
				return fSignificantDatePermittedForShipmentsList;
			}
		}
		CodeDescriptionPairList fSignificantDatePermittedForShipmentsList;

		public CodeDescriptionPairList SignificantDatePermittedForConsolsList
		{
			get
			{
				if (fSignificantDatePermittedForConsolsList == null)
				{
					fSignificantDatePermittedForConsolsList = new CodeDescriptionPairList();
					fSignificantDatePermittedForConsolsList.AddPair(SignificantDateCodes.ActualArrivalDate, SignificantDateDescriptions.ActualArrivalDate);
					fSignificantDatePermittedForConsolsList.AddPair(SignificantDateCodes.ActualDepartureDate, SignificantDateDescriptions.ActualDepartureDate);
					fSignificantDatePermittedForConsolsList.AddPair(SignificantDateCodes.InvoiceAddDate, SignificantDateDescriptions.InvoiceAddDate);
					fSignificantDatePermittedForConsolsList.AddPair(SignificantDateCodes.InvoiceDate, SignificantDateDescriptions.InvoiceDate);
				}
				return fSignificantDatePermittedForConsolsList;
			}
		}
		CodeDescriptionPairList fSignificantDatePermittedForConsolsList;

		public CodeDescriptionPairList SignificantDatePermittedForDeclarationsList
		{
			get
			{
				if (fSignificantDatePermittedForDeclarationsList == null)
				{
					fSignificantDatePermittedForDeclarationsList = new CodeDescriptionPairList();
					fSignificantDatePermittedForDeclarationsList.AddPair(SignificantDateCodes.JobOpenDate, SignificantDateDescriptions.JobOpenDate);
					fSignificantDatePermittedForDeclarationsList.AddPair(SignificantDateCodes.CustomsClearanceDate, SignificantDateDescriptions.CustomsClearanceDate);
					fSignificantDatePermittedForDeclarationsList.AddPair(SignificantDateCodes.InvoiceAddDate, SignificantDateDescriptions.InvoiceAddDate);
					fSignificantDatePermittedForDeclarationsList.AddPair(SignificantDateCodes.InvoiceDate, SignificantDateDescriptions.InvoiceDate);
				}
				return fSignificantDatePermittedForDeclarationsList;
			}
		}
		CodeDescriptionPairList fSignificantDatePermittedForDeclarationsList;

		public CodeDescriptionPairList SignificantDatePermittedForISFList
		{
			get
			{
				if (fSignificantDatePermittedForISFList == null)
				{
					fSignificantDatePermittedForISFList = new CodeDescriptionPairList();
					fSignificantDatePermittedForISFList.AddPair(SignificantDateCodes.JobOpenDate, SignificantDateDescriptions.JobOpenDate);
					fSignificantDatePermittedForISFList.AddPair(SignificantDateCodes.InvoiceAddDate, SignificantDateDescriptions.InvoiceAddDate);
					fSignificantDatePermittedForISFList.AddPair(SignificantDateCodes.InvoiceDate, SignificantDateDescriptions.InvoiceDate);
				}
				return fSignificantDatePermittedForISFList;
			}
		}
		CodeDescriptionPairList fSignificantDatePermittedForISFList;

		public CodeDescriptionPairList SignificantDatePermittedForAllList
		{
			get
			{
				if (fSignificantDatePermittedForAllList == null)
				{
					fSignificantDatePermittedForAllList = new CodeDescriptionPairList();
					fSignificantDatePermittedForAllList.AddPair(SignificantDateCodes.InvoiceAddDate, SignificantDateDescriptions.InvoiceAddDate);
				}
				return fSignificantDatePermittedForAllList;
			}
		}
		CodeDescriptionPairList fSignificantDatePermittedForAllList;

		public CodeDescriptionPairList SignificantDatePermittedForOthersList
		{
			get
			{
				if (fSignificantDatePermittedForOthersList == null)
				{
					fSignificantDatePermittedForOthersList = new CodeDescriptionPairList();
					fSignificantDatePermittedForOthersList.AddPair(SignificantDateCodes.JobOpenDate, SignificantDateDescriptions.JobOpenDate);
					fSignificantDatePermittedForOthersList.AddPair(SignificantDateCodes.InvoiceAddDate, SignificantDateDescriptions.InvoiceAddDate);
					fSignificantDatePermittedForOthersList.AddPair(SignificantDateCodes.InvoiceDate, SignificantDateDescriptions.InvoiceDate);
				}
				return fSignificantDatePermittedForOthersList;
			}
		}
		CodeDescriptionPairList fSignificantDatePermittedForOthersList;

		#endregion

		#region Default Post Date If Significant Date in Prior Closed Period / Prior Open Period / Current Period / Future Period

		public static class SignificantDatePeriodCodes
		{
			public const string InvoiceAddDate = "ADD";
			public const string EndOfPriorMonth = "EPM";
			public const string EndOfPriorPeriod = "EPP";
			public const string SignificantDate = "SGN";
			public const string InvoiceDate = "INV";
			public const string FirstDayOfFirstOpenPeriodAfterSignificantDate = "FDO";
		}

		public static class SignificantDatePeriodDescriptions
		{
			public static MultilingualString InvoiceAddDate
			{
				get { return ResString.GetMultilingualString("1425b168-8b0e-452b-849c-01ce97c70bdc", "Invoice Add Date"); }
			}

			public static MultilingualString EndOfPriorMonth
			{
				get { return ResString.GetMultilingualString("bcd0331d-dd44-4f80-8d4a-0cf3002827e2", "End of Prior Month"); }
			}

			public static MultilingualString EndOfPriorPeriod
			{
				get { return ResString.GetMultilingualString("50ea8ad3-9ee9-459c-a39b-10d808b935ee", "End of Prior Period"); }
			}

			public static MultilingualString SignificantDate
			{
				get { return ResString.GetMultilingualString("b79664fd-5ef7-4a0d-9ccd-1929fcc09d75", "Significant Date"); }
			}

			public static MultilingualString InvoiceDate
			{
				get { return ResString.GetMultilingualString("633836f9-b4ca-4086-8fb0-2ca4ac1a31bb", "Invoice Date"); }
			}

			public static MultilingualString FirstDayOfFirstOpenPeriodAfterSignificantDate
			{
				get { return ResString.GetMultilingualString("66d7eaea-863f-4197-aed8-f29d4db676a4", "First Day of First Open Period After Significant Date"); }
			}
		}

		public CodeDescriptionPairList PriorClosedPeriodList
		{
			get
			{
				if (priorClosedPeriodList == null)
				{
					priorClosedPeriodList = new CodeDescriptionPairList();
					priorClosedPeriodList.AddPair(SignificantDatePeriodCodes.InvoiceAddDate, SignificantDatePeriodDescriptions.InvoiceAddDate);
					priorClosedPeriodList.AddPair(SignificantDatePeriodCodes.EndOfPriorMonth, SignificantDatePeriodDescriptions.EndOfPriorMonth);
					priorClosedPeriodList.AddPair(SignificantDatePeriodCodes.EndOfPriorPeriod, SignificantDatePeriodDescriptions.EndOfPriorPeriod);
					priorClosedPeriodList.AddPair(SignificantDatePeriodCodes.InvoiceDate, SignificantDatePeriodDescriptions.InvoiceDate);
					priorClosedPeriodList.AddPair(SignificantDatePeriodCodes.FirstDayOfFirstOpenPeriodAfterSignificantDate, SignificantDatePeriodDescriptions.FirstDayOfFirstOpenPeriodAfterSignificantDate);
				}
				return priorClosedPeriodList;
			}
		}
		CodeDescriptionPairList priorClosedPeriodList;

		public CodeDescriptionPairList PriorOpenPeriodList
		{
			get
			{
				if (priorOpenPeriodList == null)
				{
					priorOpenPeriodList = new CodeDescriptionPairList();
					priorOpenPeriodList.AddPair(SignificantDatePeriodCodes.InvoiceAddDate, SignificantDatePeriodDescriptions.InvoiceAddDate);
					priorOpenPeriodList.AddPair(SignificantDatePeriodCodes.EndOfPriorMonth, SignificantDatePeriodDescriptions.EndOfPriorMonth);
					priorOpenPeriodList.AddPair(SignificantDatePeriodCodes.EndOfPriorPeriod, SignificantDatePeriodDescriptions.EndOfPriorPeriod);
					priorOpenPeriodList.AddPair(SignificantDatePeriodCodes.SignificantDate, SignificantDatePeriodDescriptions.SignificantDate);
					priorOpenPeriodList.AddPair(SignificantDatePeriodCodes.InvoiceDate, SignificantDatePeriodDescriptions.InvoiceDate);
				}
				return priorOpenPeriodList;
			}
		}
		CodeDescriptionPairList priorOpenPeriodList;

		public CodeDescriptionPairList CurrentPeriodList
		{
			get
			{
				if (currentPeriodList == null)
				{
					currentPeriodList = new CodeDescriptionPairList();
					currentPeriodList.AddPair(SignificantDatePeriodCodes.InvoiceAddDate, SignificantDatePeriodDescriptions.InvoiceAddDate);
					currentPeriodList.AddPair(SignificantDatePeriodCodes.EndOfPriorMonth, SignificantDatePeriodDescriptions.EndOfPriorMonth);
					currentPeriodList.AddPair(SignificantDatePeriodCodes.EndOfPriorPeriod, SignificantDatePeriodDescriptions.EndOfPriorPeriod);
					currentPeriodList.AddPair(SignificantDatePeriodCodes.SignificantDate, SignificantDatePeriodDescriptions.SignificantDate);
					currentPeriodList.AddPair(SignificantDatePeriodCodes.InvoiceDate, SignificantDatePeriodDescriptions.InvoiceDate);
				}
				return currentPeriodList;
			}
		}
		CodeDescriptionPairList currentPeriodList;

		public CodeDescriptionPairList FuturePeriodList
		{
			get
			{
				if (futurePeriodList == null)
				{
					futurePeriodList = new CodeDescriptionPairList();
					futurePeriodList.AddPair(SignificantDatePeriodCodes.InvoiceAddDate, SignificantDatePeriodDescriptions.InvoiceAddDate);
					futurePeriodList.AddPair(SignificantDatePeriodCodes.InvoiceDate, SignificantDatePeriodDescriptions.InvoiceDate);
				}
				return futurePeriodList;
			}
		}
		CodeDescriptionPairList futurePeriodList;

		#endregion

		#region Broker List

		public CodeDescriptionPairList BrokerList
		{
			get
			{
				if (fBrokerList == null)
				{
					fBrokerList = new CodeDescriptionPairList();
					fBrokerList.AddPair(BrokerCodes.All, BrokerDescriptions.All);
					fBrokerList.AddPair(BrokerCodes.Internal, BrokerDescriptions.Internal);
					fBrokerList.AddPair(BrokerCodes.External, BrokerDescriptions.External);
				}

				return fBrokerList;
			}
		}
		CodeDescriptionPairList fBrokerList;

		public static class BrokerCodes
		{
			public const string All = "ALL";
			public const string Internal = "INT";
			public const string External = "EXT";
		}

		public static class BrokerDescriptions
		{
			public static string All
			{
				get { return Res.GetString("0c6a01bc-a729-4753-8081-de35878a20f6", "All Brokers"); }
			}
			public static string Internal
			{
				get { return Res.GetString("e1da73ba-e253-43ed-b92d-ccb7d96c0dfd", "Broker is OrgProxy of Login Company/Branches"); }
			}
			public static string External
			{
				get { return Res.GetString("539cb814-32c1-4239-b3dd-84f9f0060d1f", "Broker is not OrgProxy of Login Company/Branches"); }
			}
		}

		#endregion

		#region Reversal Rule List

		public CodeDescriptionPairList ReversalRuleList
		{
			get
			{
				if (reversalRuleList == null)
				{
					reversalRuleList = new CodeDescriptionPairList();
					reversalRuleList.AddPair(ReversalRuleCodes.StandardPostingRules, ReversalRuleDescriptions.StandardPostingRules);
					reversalRuleList.AddPair(ReversalRuleCodes.DefaultFromOriginalTransactionPostDateOrCurrentDate, ReversalRuleDescriptions.DefaultFromOriginalTransactionPostDateOrCurrentDate);
					reversalRuleList.AddPair(ReversalRuleCodes.DefaultFromOriginalTransactionPostDateOrFirstDayOfFirstOpenPeriod, ReversalRuleDescriptions.DefaultFromOriginalTransactionPostDateOrFirstDayOfFirstOpenPeriod);
				}

				return reversalRuleList;
			}
		}
		CodeDescriptionPairList reversalRuleList;

		public static class ReversalRuleCodes
		{
			public const string StandardPostingRules = "STD";
			public const string DefaultFromOriginalTransactionPostDateOrCurrentDate = "OCD";
			public const string DefaultFromOriginalTransactionPostDateOrFirstDayOfFirstOpenPeriod = "OFD";
		}

		public static class ReversalRuleDescriptions
		{
			public static string StandardPostingRules
			{
				get { return Res.GetString("2bb10ac5-46e2-4296-8981-3c38359c38c3", "Use Standard Posting Rules"); }
			}
			public static string DefaultFromOriginalTransactionPostDateOrCurrentDate
			{
				get { return Res.GetString("dbb6e4f7-e14c-43eb-bf39-3598c1ed6711", "Override Standard Posting Rules and Default from Original Transaction Post Date or current date if original date is in closed period"); }
			}
			public static string DefaultFromOriginalTransactionPostDateOrFirstDayOfFirstOpenPeriod
			{
				get { return Res.GetString("80783cd1-77df-4c69-a392-fac15fe5cdc9", "Override Standard Posting Rules and Default from Original Transaction Post Date or first day of first open period if original date is in closed period"); }
			}
		}

		#endregion

	}
}