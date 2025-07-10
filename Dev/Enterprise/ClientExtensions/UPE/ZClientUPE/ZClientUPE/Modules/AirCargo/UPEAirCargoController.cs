using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.UPE.Module
{
	public class UPEAirCargoController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ClientControllerRegistration.AirCargo; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ClientModuleRegistration.HouseAirCargo; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(UPECusHAWB); }
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			IZForm result = null;
			if (Mutex == null)
			{
				Mutex = new ZGlobalMutex(new MutexID("Air Cargo House", "Only one user at a time is allowed to edit an Air Cargo House"), sourceEntity.PK.ToString());
			}

			if (Mutex.HasLock || Mutex.Lock())
			{
				result = base.ShowEditForm(sourceEntity);
				if (result != null)
				{
					result.Closed += new EventHandler(Result_Closed);
				}
				else
				{
					Mutex.Unlock();
				}
			}
			else
			{
				LockInfo lockInfo = Mutex.GetLockInfo();
				string hAWB = sourceEntity != null ? ((UPECusHAWB)sourceEntity).CS_HAWB.ToString() : "Unknown";
				string lockStartTime = lockInfo != null ? lockInfo.LockStartTime.ToString() : ZDateTime.Now.ToString();
				string lockUser = lockInfo != null && lockInfo.UserWithLock != null ? lockInfo.UserWithLock.GS_FullName.ToString() : "Unknown";

				string infoMessage = string.Format("Access to Air Cargo House : '{0}' is denied.\n" +
						"The record has been locked since '{1}' by '{2}'.\n" +
						"Please wait until the lock has been released before trying to edit the record.\n",
						hAWB, lockStartTime, lockUser);

				Globals.Message.ShowInformation(infoMessage, "Air Cargo House Locked");
			}

			return result;
		}

		ZGlobalMutex Mutex;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new UPEAirCargoHouseForm(businessEntity as UPECusHAWB);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ACAHouseModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ACAHouseModify; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ACAHouseModify; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ACAHouse; }
		}

		void Result_Closed(object sender, EventArgs e)
		{
			if (sender != null)
			{
				((IZForm)sender).Closed -= new EventHandler(Result_Closed);
			}
			if (Mutex != null && Mutex.HasLock)
			{
				Mutex.Unlock();
			}
		}
	}
}