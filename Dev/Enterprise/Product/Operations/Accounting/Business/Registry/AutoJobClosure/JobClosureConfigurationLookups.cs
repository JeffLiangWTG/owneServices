using System;
using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business
{
	public class JobClosureConfigurationLookups : JobConfigurationSelectorLookups
	{
		public JobClosureConfigurationLookups(JobClosureConfiguration parent)
			: base(parent)
		{
			this.Parent = parent;
		}
		new readonly JobClosureConfiguration Parent;

		#region JobType

		public static CodeDescriptionPairList GetAllJobTypesThatCanBeProcessedByJCS() => GetJobTypes(true);

		protected override CodeDescriptionPairList GetJobTypeList() => GetJobTypes(false);

		static CodeDescriptionPairList GetJobTypes(bool removeJobTypeALL)
		{
			var jobTypeList = GetBaseJobTypeList();

			var invalidJobTypes = new List<string>()
												{
													JobInvoicingConsumerTypes.Project.Code,
													JobInvoicingConsumerTypes.WorkRequest.Code
												};
			if (removeJobTypeALL)
			{
				invalidJobTypes.Add(JobTypeAdditionalCodes.All);
			}

			foreach (string jobTypeCode in invalidJobTypes)
			{
				if (jobTypeList.ContainsCode(jobTypeCode))
				{
					jobTypeList.RemoveCode(jobTypeCode);
				}
			}

			return jobTypeList;
		}

		#endregion

		#region Job Closure Date Option List

		public CodeDescriptionPairList JobClosureDateOptionList
		{
			get
			{
				switch (Parent.JobType)
				{
					case "":
						return CompleteJobClosureDateOptionList;

					case JobInvoicingConsumerTypes.ShipmentCode:
						return JobClosureDateOptionPermittedForShipmentsList;

					case JobInvoicingConsumerTypes.BrokerageCode:
						return JobClosureDateOptionPermittedForDeclarationsList;

					case JobInvoicingConsumerTypes.AgencyBillOfLadingCode:
					case JobInvoicingConsumerTypes.AgencyBookingCode:
						return JobClosureDateOptionPermittedForShippingManagerList;

					case JobInvoicingConsumerTypes.TransportBookingConsignmentCode:
					case JobInvoicingConsumerTypes.TransportConsignmentCode:
						return JobClosureDateOptionPermittedForTransportConsignmentList;

					case JobInvoicingConsumerTypes.ForwardingConsolCode:
					case JobInvoicingConsumerTypes.GatewayConsolCode:
						return JobClosureDateOptionPermittedForGatewayJobList;

					default:
						return JobClosureDateOptionPermittedForOthersList;
				}
			}
		}

		public static CodeDescriptionPairList CompleteJobClosureDateOptionList
		{
			get
			{
				if (fCompleteJobClosureDateOptionList == null)
				{
					fCompleteJobClosureDateOptionList = new CodeDescriptionPairList();
					fCompleteJobClosureDateOptionList.AddPair(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate, RevenueRecognitionLookups.RecognitionDateOptionDescriptions.ActualArrivalDate);
					fCompleteJobClosureDateOptionList.AddPair(RevenueRecognitionLookups.RecognitionDateOptionCodes.AWBIssueDate, RevenueRecognitionLookups.RecognitionDateOptionDescriptions.AWBIssueDate);
					fCompleteJobClosureDateOptionList.AddPair(RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate, RevenueRecognitionLookups.RecognitionDateOptionDescriptions.JobOpenDate);
					fCompleteJobClosureDateOptionList.AddPair(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate, RevenueRecognitionLookups.RecognitionDateOptionDescriptions.ActualDepartureDate);
					fCompleteJobClosureDateOptionList.AddPair(RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate, RevenueRecognitionLookups.RecognitionDateOptionDescriptions.CustomsClearanceDate);
					fCompleteJobClosureDateOptionList.AddPair(RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate, RevenueRecognitionLookups.RecognitionDateOptionDescriptions.PickupDate);
					fCompleteJobClosureDateOptionList.AddPair(RevenueRecognitionLookups.RecognitionDateOptionCodes.DeliveryDate, RevenueRecognitionLookups.RecognitionDateOptionDescriptions.DeliveryDate);
					fCompleteJobClosureDateOptionList.AddPair(RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction, RevenueRecognitionLookups.RecognitionDateOptionDescriptions.PostDateOfFirstARTransaction);
					fCompleteJobClosureDateOptionList.AddPair(RevenueRecognitionLookups.RecognitionDateOptionCodes.VesselArrivalDate, RevenueRecognitionLookups.RecognitionDateOptionDescriptions.VesselArrivalDate);
					fCompleteJobClosureDateOptionList.AddPair(RevenueRecognitionLookups.RecognitionDateOptionCodes.VesselDepartureDate, RevenueRecognitionLookups.RecognitionDateOptionDescriptions.VesselDepartureDate);
					return fCompleteJobClosureDateOptionList;
				}
				return fCompleteJobClosureDateOptionList;
			}
		}
		[ThreadStatic]
		static CodeDescriptionPairList fCompleteJobClosureDateOptionList;

		public CodeDescriptionPairList JobClosureDateOptionPermittedForShipmentsList
		{
			get
			{
				if (fJobClosureDateOptionPermittedForShipmentsList == null)
				{
					fJobClosureDateOptionPermittedForShipmentsList = new CodeDescriptionPairList();
					fJobClosureDateOptionPermittedForShipmentsList.AddPair(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate, RevenueRecognitionLookups.RecognitionDateOptionDescriptions.ActualArrivalDate);
					fJobClosureDateOptionPermittedForShipmentsList.AddPair(RevenueRecognitionLookups.RecognitionDateOptionCodes.AWBIssueDate, RevenueRecognitionLookups.RecognitionDateOptionDescriptions.AWBIssueDate);
					fJobClosureDateOptionPermittedForShipmentsList.AddPair(RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate, RevenueRecognitionLookups.RecognitionDateOptionDescriptions.JobOpenDate);
					fJobClosureDateOptionPermittedForShipmentsList.AddPair(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate, RevenueRecognitionLookups.RecognitionDateOptionDescriptions.ActualDepartureDate);
					fJobClosureDateOptionPermittedForShipmentsList.AddPair(RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate, RevenueRecognitionLookups.RecognitionDateOptionDescriptions.PickupDate);
					fJobClosureDateOptionPermittedForShipmentsList.AddPair(RevenueRecognitionLookups.RecognitionDateOptionCodes.DeliveryDate, RevenueRecognitionLookups.RecognitionDateOptionDescriptions.DeliveryDate);
					fJobClosureDateOptionPermittedForShipmentsList.AddPair(RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate, RevenueRecognitionLookups.RecognitionDateOptionDescriptions.CustomsClearanceDate);
					fJobClosureDateOptionPermittedForShipmentsList.AddPair(RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction, RevenueRecognitionLookups.RecognitionDateOptionDescriptions.PostDateOfFirstARTransaction);
				}
				return fJobClosureDateOptionPermittedForShipmentsList;
			}
		}
		CodeDescriptionPairList fJobClosureDateOptionPermittedForShipmentsList;

		public CodeDescriptionPairList JobClosureDateOptionPermittedForDeclarationsList
		{
			get
			{
				if (fJobClosureDateOptionPermittedForDeclarationsList == null)
				{
					fJobClosureDateOptionPermittedForDeclarationsList = new CodeDescriptionPairList();
					fJobClosureDateOptionPermittedForDeclarationsList.AddPair(RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate, RevenueRecognitionLookups.RecognitionDateOptionDescriptions.JobOpenDate);
					fJobClosureDateOptionPermittedForDeclarationsList.AddPair(RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction, RevenueRecognitionLookups.RecognitionDateOptionDescriptions.PostDateOfFirstARTransaction);
					fJobClosureDateOptionPermittedForDeclarationsList.AddPair(RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate, RevenueRecognitionLookups.RecognitionDateOptionDescriptions.CustomsClearanceDate);
				}
				return fJobClosureDateOptionPermittedForDeclarationsList;
			}
		}
		CodeDescriptionPairList fJobClosureDateOptionPermittedForDeclarationsList;

		public CodeDescriptionPairList JobClosureDateOptionPermittedForOthersList
		{
			get
			{
				if (fJobClosureDateOptionPermittedForOthersList == null)
				{
					fJobClosureDateOptionPermittedForOthersList = new CodeDescriptionPairList();
					fJobClosureDateOptionPermittedForOthersList.AddPair(RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate, RevenueRecognitionLookups.RecognitionDateOptionDescriptions.JobOpenDate);
					fJobClosureDateOptionPermittedForOthersList.AddPair(RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction, RevenueRecognitionLookups.RecognitionDateOptionDescriptions.PostDateOfFirstARTransaction);
				}
				return fJobClosureDateOptionPermittedForOthersList;
			}
		}
		CodeDescriptionPairList fJobClosureDateOptionPermittedForOthersList;

		public CodeDescriptionPairList JobClosureDateOptionPermittedForShippingManagerList
		{
			get
			{
				if (fJobClosureDateOptionPermittedForShippingManagerList == null)
				{
					fJobClosureDateOptionPermittedForShippingManagerList = new CodeDescriptionPairList();
					fJobClosureDateOptionPermittedForShippingManagerList.AddPair(RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction, RevenueRecognitionLookups.RecognitionDateOptionDescriptions.PostDateOfFirstARTransaction);
					fJobClosureDateOptionPermittedForShippingManagerList.AddPair(RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate, RevenueRecognitionLookups.RecognitionDateOptionDescriptions.JobOpenDate);
					fJobClosureDateOptionPermittedForShippingManagerList.AddPair(RevenueRecognitionLookups.RecognitionDateOptionCodes.VesselArrivalDate, RevenueRecognitionLookups.RecognitionDateOptionDescriptions.VesselArrivalDate);
					fJobClosureDateOptionPermittedForShippingManagerList.AddPair(RevenueRecognitionLookups.RecognitionDateOptionCodes.VesselDepartureDate, RevenueRecognitionLookups.RecognitionDateOptionDescriptions.VesselDepartureDate);
				}
				return fJobClosureDateOptionPermittedForShippingManagerList;
			}
		}
		CodeDescriptionPairList fJobClosureDateOptionPermittedForShippingManagerList;

		public CodeDescriptionPairList JobClosureDateOptionPermittedForTransportConsignmentList
		{
			get
			{
				if (fJobClosureDateOptionPermittedForTransportConsignmentList == null)
				{
					fJobClosureDateOptionPermittedForTransportConsignmentList = new CodeDescriptionPairList();
					fJobClosureDateOptionPermittedForTransportConsignmentList.AddPair(RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction, RevenueRecognitionLookups.RecognitionDateOptionDescriptions.PostDateOfFirstARTransaction);
					fJobClosureDateOptionPermittedForTransportConsignmentList.AddPair(RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate, RevenueRecognitionLookups.RecognitionDateOptionDescriptions.JobOpenDate);
					fJobClosureDateOptionPermittedForTransportConsignmentList.AddPair(RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate, RevenueRecognitionLookups.RecognitionDateOptionDescriptions.PickupDate);
					fJobClosureDateOptionPermittedForTransportConsignmentList.AddPair(RevenueRecognitionLookups.RecognitionDateOptionCodes.DeliveryDate, RevenueRecognitionLookups.RecognitionDateOptionDescriptions.DeliveryDate);
				}
				return fJobClosureDateOptionPermittedForTransportConsignmentList;
			}
		}
		CodeDescriptionPairList fJobClosureDateOptionPermittedForTransportConsignmentList;

		public CodeDescriptionPairList JobClosureDateOptionPermittedForGatewayJobList
		{
			get
			{
				if (fJobClosureDateOptionPermittedForGatewayJobList == null)
				{
					fJobClosureDateOptionPermittedForGatewayJobList = new CodeDescriptionPairList();
					fJobClosureDateOptionPermittedForGatewayJobList.AddPair(RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction, RevenueRecognitionLookups.RecognitionDateOptionDescriptions.PostDateOfFirstARTransaction);
					fJobClosureDateOptionPermittedForGatewayJobList.AddPair(RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate, RevenueRecognitionLookups.RecognitionDateOptionDescriptions.JobOpenDate);
					fJobClosureDateOptionPermittedForGatewayJobList.AddPair(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate, RevenueRecognitionLookups.RecognitionDateOptionDescriptions.ActualArrivalDate);
					fJobClosureDateOptionPermittedForGatewayJobList.AddPair(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate, RevenueRecognitionLookups.RecognitionDateOptionDescriptions.ActualDepartureDate);
				}
				return fJobClosureDateOptionPermittedForGatewayJobList;
			}
		}
		CodeDescriptionPairList fJobClosureDateOptionPermittedForGatewayJobList;

		#endregion

		#region Department

		public GlbDepartmentCollection Departments => Parent.Factory.GetCachedValue("AutoJobClosureDepartmentCollection", () => new GlbDepartmentCollection(Parent.Factory));

		#endregion

		#region OffSetTypes

		public CodeDescriptionPairList OffSetTypes => new JobConfigurationSelectorHelper().OffsetTypeListForAutoJobClosure;

		#endregion

		#region JobChargeRecognitionFilter

		public CodeDescriptionPairList JobChargeRecognitionFilterOptions => new ChargeRecognitionFilterOptionList();

		#endregion

		#region Configuration Type List 

		public CodeDescriptionPairList ConfigurationTypeList => new JobConfigurationSelectorHelper().ConfigurationTypeList;

		#endregion
	}
}
