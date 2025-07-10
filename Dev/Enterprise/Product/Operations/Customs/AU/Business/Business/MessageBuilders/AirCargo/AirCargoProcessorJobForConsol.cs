using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public sealed class AirCargoProcessorJobForConsol : AirCargoMessageProcessorJobBase, Integration.Customs.AU.IAirCargoMessageProcessorJob
	{
		public AirCargoProcessorJobForConsol(ForwardingConsol consol)
		{
			this.consol = Argument.NotNull(consol, "consol");
			var creator = new Customs.Business.CusMAWBSafeCreator(this.consol);
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode != Core.Constants.CountryCodes.Australia)
			{
				var auBranch = GlbBranch.GetFirstActiveBranch(Core.Constants.CountryCodes.Australia, consol.Factory);
				if (auBranch != null)
				{
					using (DisposableEnvironment.ForBranch(auBranch.PK.ToGuid()))
					{
						disposableActionAfterCreateAirCargo = creator.TryCreateOrUpdateCargoAndChildBills();
					}
				}
			}
			else
			{
				disposableActionAfterCreateAirCargo = creator.TryCreateOrUpdateCargoAndChildBills();
			}

			linkedMAWB = consol.AUCusMAWB as CusMAWB;
			if (linkedMAWB != null)
			{
				using (((IBusinessObjectInternals)linkedMAWB).ResumeValidationForAllDescendantsTemporarily())
				{
					linkedMAWB.LoadChildEditableObjects();
					linkedMAWB.RunPreSaveValidation();
				}
			}
		}
		readonly IDisposable disposableActionAfterCreateAirCargo;

		#region Overrides

		protected override bool IsAir => consol.IsAir;

		protected override bool IsDischargedAtAustralianPort => consol.JK_RL_NKDiscForFirstImportTransport.StartsWith(Core.Constants.CountryCodes.Australia);

		protected override CusMAWB MasterBillCore => linkedMAWB;

		protected override IEnumerable<BusinessObject> ChildrenCore => MasterBill.ChildBills;

		protected override string GetReferenceNumberCore(BusinessObject child) => string.Format("Shipment Number: {0}", ((CusHAWB)child).Shipment.JS_UniqueConsignRef);

		protected override CMRMessageManager GetMessageManagerCore(BusinessObject child) => new CusHAWBAIRCRMessageManager((CusHAWB)child, shouldDelaySending);

		protected override void SetSACIfRequiredCore(BusinessObject child)
		{
			var houseBill = (CusHAWB)child;
			var isSAC = AirCargoProcessorJobForConsol.IsSAC(houseBill.Shipment);
			if (isSAC != houseBill.CS_IsSelfAssessedClearance)
			{
				houseBill.CS_IsSelfAssessedClearance = isSAC;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing & disposableActionAfterCreateAirCargo != null)
			{
				disposableActionAfterCreateAirCargo.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion // Overrides

		#region Implementation

		readonly CusMAWB linkedMAWB;
		readonly ForwardingConsol consol;

		static bool IsSAC(ForwardingShipment shipment)
		{
			bool result = false;

			if (shipment != null)
			{
				var log = shipment.Logs.MostRecentLogByEventTime(Events.DataImport, "AU Declaration Style: SAC");
				result = log != null && !log.IsInDatabase;
			}

			return result;
		}

		#endregion // Implementation
	}
}
