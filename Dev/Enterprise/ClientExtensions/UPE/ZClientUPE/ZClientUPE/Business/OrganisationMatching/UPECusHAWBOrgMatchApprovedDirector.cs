using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	public class UPECusHAWBOrgMatchApprovedDirector
	{
		public UPECusHAWBOrgMatchApprovedDirector(UPECusHAWB airCargo)
		{
			this.AirCargo = airCargo;
		}

		public readonly UPECusHAWB AirCargo;

		public void RunMatchApprovedActivities()
		{
			if (IsConsignorMatched && IsImporterOrConsigneeMatched)
			{
				if (IsSplitShipment)
				{
					RunMatchApprovedActivitiesForSplitShipment();
				}
				RunMatchApprovedActivitiesForOtherShipments();
			}
		}

		internal bool matchApprovedForSplitShipment;
		void RunMatchApprovedActivitiesForSplitShipment()
		{
			ZString masterbillNumber = AirCargo.MAWB != null ? AirCargo.MAWB.CM_MAWB : ZString.Empty;
			if (!masterbillNumber.IsEmpty)
			{
				if (!FirstDuplicateAirCargo.CS_JE_CustomsFormalEntry.IsEmpty)
				{
					AirCargo.CS_JE_CustomsFormalEntry = FirstDuplicateAirCargo.CS_JE_CustomsFormalEntry;

					DeliverAlternateBrokerSplitNotificationAfterFactorySave();

					if (AirCargo.Declaration.TryProcessSplitShipments() && (AirCargo.Declaration.CurrentQueue.P4_CustomsQueue == DeclarationQueueCodeDescriptionPairList.Codes.Completed || AirCargo.Declaration.CurrentQueue.P4_CustomsQueue == DeclarationQueueCodeDescriptionPairList.Codes.Submitted))
					{
						AirCargo.Declaration.MoveToQueue(
							DeclarationQueueCodeDescriptionPairList.Codes.BCO,
							ReasonCodeDescriptionPairList.Codes.DN_SplitShipment,
							ZString.Empty,
							"Match Approved for Split Shipment");
						matchApprovedForSplitShipment = true; 
					}
				}
			}
		}

		#region DeliverAlternateBrokerSplitNotificationAfterFactorySave

		void DeliverAlternateBrokerSplitNotificationAfterFactorySave()
		{
			AirCargo.Factory.Saved += new BusinessObjectFactory.SavedEventHandler(OnFactorySaved_DeliverAlternateBrokerSplitNotification);
		}

		void OnFactorySaved_DeliverAlternateBrokerSplitNotification(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			AirCargo.Factory.Saved -= new BusinessObjectFactory.SavedEventHandler(OnFactorySaved_DeliverAlternateBrokerSplitNotification);
			if (savedSuccessfully)
			{
				DeliverAlternateBrokerSplitNotification();
			}
		}

		void DeliverAlternateBrokerSplitNotification()
		{
			try
			{
				if (AirCargo.Declaration.AlternateBroker != null)
				{
					new UPEAlternateBrokerSplitNotificationAutoDelivery(AirCargo).Deliver();
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("AlternateBrokerSplitShipmentDeliveryFailure_" + ex.Message, ex.Message, ex);
			}
		}

		#endregion

		internal BaseJobDeclaration newlyCreatedJobDeclaration;
		internal bool freightRateIsCalculated;
		void RunMatchApprovedActivitiesForOtherShipments()
		{
			UPEDeclarationFromAirCargoCreator uPEDeclarationFromAirCargoCreator = new UPEDeclarationFromAirCargoCreator(AirCargo);

			if (!AirCargo.HasFormalDec)
			{
				newlyCreatedJobDeclaration = uPEDeclarationFromAirCargoCreator.CreateIgnoreWarnings();
			}

			if (AirCargo.HasFormalDec)
			{
				new FreightRateCalculator().CalculateFreightRateOnDeclaration(AirCargo.Declaration);
				freightRateIsCalculated = true;
			}
		}

		bool IsSplitShipment
		{
			get { return FirstDuplicateAirCargo != null; }
		}

		UPECusHAWB FirstDuplicateAirCargo
		{
			get
			{
				if (fFirstDuplicateAirCargo == null && !AirCargo.CS_HAWB.IsEmpty)
				{
					var housebillFilter = new ZDBOnlyQuery(typeof(UPECusHAWB));

					var branches = UPETools.Instance.UPECustomisationBranches(true).ToArray();
					ZDBOnlySubQuery mawbSubQuery = new ZDBOnlySubQuery(typeof(UPECusMAWB), CusMAWBSchema.PK);
					mawbSubQuery.FilterByForeignKey(CusMAWBSchema.CM_GB, branches);

					ZDBOnlySubQuery cusDecSubQuery = new ZDBOnlySubQuery(typeof(UPEJobDeclaration), JobDeclarationSchema.PK);
					cusDecSubQuery.FilterByForeignKey(JobDeclarationSchema.JE_GB, branches);

					housebillFilter.AddSubQuery(CusHAWBSchema.CS_CM, mawbSubQuery, JoinCondition.And);
					housebillFilter.AddSubQuery(CusHAWBSchema.CS_JE_CustomsFormalEntry, cusDecSubQuery, JoinCondition.Or);

					housebillFilter.AddToFilter(CusHAWBSchema.CS_HAWB, AirCargo.CS_HAWB);
					housebillFilter.AddToFilter(CusHAWBSchema.PK, SQLComparisonOperator.NotEqual, AirCargo.PK);
					fFirstDuplicateAirCargo = (UPECusHAWB)AirCargo.Factory.LoadTop1(typeof(UPECusHAWB), housebillFilter);
				}

				return fFirstDuplicateAirCargo;
			}
		}
		UPECusHAWB fFirstDuplicateAirCargo;

		bool IsImporterOrConsigneeMatched
		{
			get { return !AirCargo.ImporterOrConsigneeMatchedOrgPK.IsEmpty; }
		}

		bool IsConsignorMatched
		{
			get { return !AirCargo.CS_OA_ConsignorAddress.IsEmpty; }
		}
	}
}
