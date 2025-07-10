using System;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Res = Enterprise.Accounting.Business.Res;
using ResString = Enterprise.Accounting.Business.ResString;

namespace Enterprise.Accounting.Registry.Business
{
	public class InvoiceDateConfigurationLookups : ZLookups
	{
		public InvoiceDateConfigurationLookups(IInvoiceDateConfiguration parent)
			: base((BusinessObject)parent)
		{
			this.Parent = parent;
		}
		new readonly IInvoiceDateConfiguration Parent;

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
				get { return ResString.GetMultilingualString("6bb1f225-6d4c-423e-8f20-f487bdfff7a6", "Any Job Type"); }
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
				get { return Res.GetString("ddbb6578-4618-4c3a-9ca0-e526213a0d01", "Any Transport Mode"); }
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
			public const string HouseBillIssueDate = AccountingMasterFilesConstants.SignificantDateCodes.HouseBillIssueDate;
		}

		public static class SignificantDateDescriptions
		{
			public static MultilingualString ActualArrivalDate
			{
				get { return ResString.GetMultilingualString("c9e50061-5887-4c34-9721-29edf054c9c1", "Actual Arrival Date"); }
			}
			public static MultilingualString ActualDepartureDate
			{
				get { return ResString.GetMultilingualString("09146948-781d-40fc-9e61-aae12493f477", "Actual Departure Date"); }
			}
			public static MultilingualString CustomsClearanceDate
			{
				get { return ResString.GetMultilingualString("a3bdb84d-fb9f-409e-a288-70762bfdc44c", "Customs Clearance Date"); }
			}
			public static MultilingualString PickupDate
			{
				get { return ResString.GetMultilingualString("a8c43817-1c63-4bad-8c75-9962b93bb1ca", "Pickup Date"); }
			}
			public static MultilingualString DeliveryDate
			{
				get { return ResString.GetMultilingualString("4e5066b3-a155-4c61-8d30-2af5d3e72336", "Delivery Date"); }
			}
			public static MultilingualString JobOpenDate
			{
				get { return ResString.GetMultilingualString("77bae0d2-e659-4ae9-9409-e7a61a74b644", "Job Open Date"); }
			}
			public static MultilingualString AWBIssueDate
			{
				get { return ResString.GetMultilingualString("e7fadd72-3169-427d-99d6-9ffc3bd397c7", "AWB Issue Date"); }
			}

			public static MultilingualString InvoiceAddDate
			{
				get { return ResString.GetMultilingualString("61ff62bd-6edc-4c94-ac25-7c8b0f48eb95", "Invoice Add Date"); }
			}

			public static MultilingualString HouseBillIssueDate
			{
				get { return ResString.GetMultilingualString("22e941d4-662c-4725-aac0-4d0ff2fb7d42", "House Bill Issue Date"); }
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
				}
				return fSignificantDatePermittedForOthersList;
			}
		}
		CodeDescriptionPairList fSignificantDatePermittedForOthersList;

		#endregion

		#region Default Invoice Date If Significant Date in Prior Closed Period / Prior Open Period / Current Period / Future Period

		public static class SignificantDatePeriodCodes
		{
			public const string InvoiceAddDate = "ADD";
			public const string EndOfPriorMonth = "EPM";
			public const string EndOfPriorPeriod = "EPP";
			public const string SignificantDate = "SGN";
			public const string FirstDayOfFirstOpenPeriodAfterSignificantDate = "FDO";
		}

		public static class SignificantDatePeriodDescriptions
		{
			public static MultilingualString InvoiceAddDate
			{
				get { return ResString.GetMultilingualString("8398c662-9804-4ee5-8ba1-f14adcf51764", "Invoice Add Date"); }
			}

			public static MultilingualString EndOfPriorMonth
			{
				get { return ResString.GetMultilingualString("07c211de-c3ec-4a50-9867-d6524536307c", "End of Prior Month"); }
			}

			public static MultilingualString EndOfPriorPeriod
			{
				get { return ResString.GetMultilingualString("a9d6550c-1c1c-4e09-b301-6c1674edc729", "End of Prior Period"); }
			}

			public static MultilingualString SignificantDate
			{
				get { return ResString.GetMultilingualString("1ce84a58-5f8b-4636-98e7-5f213df9a3dc", "Significant Date"); }
			}

			public static MultilingualString FirstDayOfFirstOpenPeriodAfterSignificantDate
			{
				get { return ResString.GetMultilingualString("4cb2fef6-380e-4931-9619-828ce677c841", "First Day of First Open Period After Significant Date"); }
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
					priorClosedPeriodList.AddPair(SignificantDatePeriodCodes.SignificantDate, SignificantDatePeriodDescriptions.SignificantDate);
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
					futurePeriodList.AddPair(SignificantDatePeriodCodes.SignificantDate, SignificantDatePeriodDescriptions.SignificantDate);
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
				get { return Res.GetString("26045df6-9ca4-4e0e-84da-6b8562a96587", "All Brokers"); }
			}
			public static string Internal
			{
				get { return Res.GetString("37d5c7c2-0f40-4915-ab60-9a718984e8f5", "Broker is OrgProxy of Login Company/Branches"); }
			}
			public static string External
			{
				get { return Res.GetString("5e388689-2254-4635-a35e-92dac91dc6ea", "Broker is not OrgProxy of Login Company/Branches"); }
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
					reversalRuleList.AddPair(ReversalRuleCodes.DefaultFromOriginalTransactionInvoiceDateOrCurrentDate, ReversalRuleDescriptions.DefaultFromOriginalTransactionInvoiceDateOrCurrentDate);
					reversalRuleList.AddPair(ReversalRuleCodes.DefaultFromOriginalTransactionInvoiceDateOrFirstDayOfFirstOpenPeriod, ReversalRuleDescriptions.DefaultFromOriginalTransactionInvoiceDateOrFirstDayOfFirstOpenPeriod);
				}

				return reversalRuleList;
			}
		}
		CodeDescriptionPairList reversalRuleList;

		public static class ReversalRuleCodes
		{
			public const string StandardPostingRules = "STD";
			public const string DefaultFromOriginalTransactionInvoiceDateOrCurrentDate = "OCD";
			public const string DefaultFromOriginalTransactionInvoiceDateOrFirstDayOfFirstOpenPeriod = "OFD";
		}

		public static class ReversalRuleDescriptions
		{
			public static string StandardPostingRules
			{
				get { return Res.GetString("977f8252-7a17-48dd-ab47-a78e18119027", "Use Standard Posting Rules"); }
			}
			public static string DefaultFromOriginalTransactionInvoiceDateOrCurrentDate
			{
				get { return Res.GetString("7c7cdf6c-2a4e-4e10-a9c5-1c7817ad4752", "Override Standard Posting Rules and Default from Original Transaction Invoice Date or current date if original date is in closed period"); }
			}
			public static string DefaultFromOriginalTransactionInvoiceDateOrFirstDayOfFirstOpenPeriod
			{
				get { return Res.GetString("f43cb034-ec48-4dfb-a07b-4a4a4a86e926", "Override Standard Posting Rules and Default from Original Transaction Invoice Date or first day of first open period if original date is in closed period"); }
			}
		}

		#endregion

	}
}