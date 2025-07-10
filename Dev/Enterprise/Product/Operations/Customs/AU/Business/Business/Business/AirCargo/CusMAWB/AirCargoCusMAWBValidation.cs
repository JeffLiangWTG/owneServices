using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class AirCargoCusMAWBValidation : CusMAWBValidation
	{
		protected AirCargoCusMAWBValidation(CusMAWB parent)
			: base(parent)
		{
			mHBValidator = new DuplicateMAWBAndMasterHouseBillValidator(MAWB);
		}

		protected override void CheckCM_FlightNo()
		{
			base.CheckCM_FlightNo();
			if (MAWB.Consol != null)
			{
				MessageValidation.ValidateAirCargoDataDifferentFromFreight(MAWB.CM_FlightNoInfo, MAWB.Consol.JK_VoyageFlightForLastImportTransport);
			}
		}

		protected override void CheckCM_ArrivalDate()
		{
			base.CheckCM_ArrivalDate();
			if (MAWB.Consol != null)
			{
				MessageValidation.ValidateAirCargoDataDifferentFromFreight(MAWB.CM_ArrivalDateInfo, MAWB.Consol.JK_ArrivalForLastImportTransport);
			}
		}

		protected override void CheckCM_RL_NKDischargePort()
		{
			base.CheckCM_RL_NKDischargePort();
			if (MAWB.Consol != null)
			{
				MessageValidation.ValidateAirCargoDataDifferentFromFreight(MAWB.CM_RL_NKDischargePortInfo, MAWB.Consol.JK_RL_NKDiscForFirstImportTransport);
			}
		}

		protected override void CheckCM_RL_NKLoadPort()
		{
			base.CheckCM_RL_NKLoadPort();
			if (MAWB.LoadPort == null)
			{
				MAWB.CM_RL_NKLoadPortInfo.AddMessageError("Valid loading port is required.");
			}
			else
			{
				ZString portWarning = MessageValidation.ValidatePortType(MAWB.CM_RL_NKLoadPort, true, false);
				if (!portWarning.IsEmpty)
				{
					MAWB.CM_RL_NKLoadPortInfo.AddWarning(portWarning);
				}

				if (MAWB.CM_RL_NKLoadPort.EndsWith("ZZZ"))
				{
					MAWB.CM_RL_NKLoadPortInfo.AddMessageError("Load port is invalid. Please check the job detail and enter a correct port.");
				}
				else if (MAWB.LoadPort.RL_RN_NKCountryCode == "AU")
				{
					MAWB.CM_RL_NKLoadPortInfo.AddMessageError("Port of loading must be an overseas one.");
				}
			}

			if (MAWB.Consol != null)
			{
				MessageValidation.ValidateAirCargoDataDifferentFromFreight(MAWB.CM_RL_NKLoadPortInfo, MAWB.Consol.JK_RL_NKLoadForFirstImportTransport);
			}
		}

		protected override void CheckCM_MAWB()
		{
			base.CheckCM_MAWB();
			if (MAWB.CM_MAWB.IsEmpty)
			{
				MAWB.CM_MAWBInfo.AddError("Master bill number is required for air cargo messaging.");
			}
			else
			{
				new AirWayBillValidator().ValidateAndAddMessageError(MAWB.CM_MAWBInfo);
			}

			mHBValidator.Validate(MAWB.CM_MAWB, MAWB.CM_MasterHouseBill);
			if (!mHBValidator.ErrorMessage.IsEmpty)
			{
				MAWB.CM_MAWBInfo.AddError(mHBValidator.ErrorMessage);
			}

			ValidateCM_MasterHouseBill();

			if (MAWB.Consol != null)
			{
				MessageValidation.ValidateAirCargoDataDifferentFromFreight(MAWB.CM_MAWBInfo, MAWB.Consol.JK_MasterBillNum);
			}
		}

		protected override void CheckCM_MasterHouseBill()
		{
			base.CheckCM_MasterHouseBill();
			mHBValidator.Validate(MAWB.CM_MAWB, MAWB.CM_MasterHouseBill);
			if (!mHBValidator.ErrorMessage.IsEmpty)
			{
				MAWB.CM_MasterHouseBillInfo.AddError(mHBValidator.ErrorMessage);
			}
			ValidateCM_MAWB();
		}

		readonly DuplicateMAWBAndMasterHouseBillValidator mHBValidator;
		class DuplicateMAWBAndMasterHouseBillValidator
		{
			public DuplicateMAWBAndMasterHouseBillValidator(CusMAWB mAWB)
			{
				this.mAWB = mAWB;
			}

			public ZString ErrorMessage = "";

			public void Validate(ZString mAWBNo, ZString coLoadMaster)
			{
				if ((mAWBNo != lastMAWBNo || coLoadMaster != lastCoLoadMaster))
				{
					if (IsHVLV && coLoadMaster.IsEmpty)
					{
						return;
					}
					else
					{
						lastMAWBNo = mAWBNo;
						lastCoLoadMaster = coLoadMaster;
						ErrorMessage = "";
						Customs.Business.CusMAWB[] mAWBs = mAWB.CusMAWBLoader.FindMatchingActiveMAWBsWithParent(mAWBNo, true, coLoadMaster, true, true);
						if (mAWBs.Length > 0)
						{
							if (coLoadMaster.IsEmpty)
							{
								ErrorMessage = "MAWB " + mAWBNo + " already exists in database.";
							}
							else
							{
								ErrorMessage = "MAWB " + mAWBNo + "and Co-load Master" + coLoadMaster + " already exist in database.";
							}
						}
					}
				}
			}

			readonly CusMAWB mAWB;
			ZString lastMAWBNo = "";
			ZString lastCoLoadMaster = "";

			bool IsHVLV => mAWB.ChildBills.Cast<CusHAWBBase>().Any(hawb => hawb.CS_IsHVLV);
		}

		#region Implementation

		public new CusMAWB MAWB
		{
			get { return (CusMAWB)Parent; }
		}

		#endregion
	}
}
