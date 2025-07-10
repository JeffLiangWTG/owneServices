using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.GUI;
using Enterprise.Customs.AU.Module;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.UPE.Module
{
	public class UPEJobDeclarationController : JobDeclarationController
	{
		protected override IZForm GetFormCore(IBusiness businessEntity)
		{
			return new UPEAUCustomsDeclarationForm(businessEntity as UPEJobDeclaration);
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get
			{
				return typeof(UPEJobDeclaration);
			}
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			IZForm result = null;
			if (Mutex == null)
			{
				Mutex = new ZGlobalMutex(new MutexID("CustomsDeclaration", "Only one user at a time is allowed to edit a Customs Declaration"), sourceEntity.PK.ToString());
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
				string declarationReference = sourceEntity != null ? ((BaseJobDeclaration)sourceEntity).JE_DeclarationReference.ToString() : "Unknown";
				string lockStartTime = lockInfo != null ? EnvProxy.Instance.Time.GetLocalTimeFromUtc(lockInfo.LockStartTime.ToDateTime()).ToString(CultureInfo.CurrentCulture) : ZDateTime.Now.ToString();
				string lockUser = lockInfo != null && lockInfo.UserWithLock != null ? lockInfo.UserWithLock.GS_FullName.ToString() : "Unknown";

				string infoMessage = string.Format("Access to Customs Declaration : '{0}' is denied.\n" +
						"The record has been locked since '{1}' by '{2}'.\n" +
						"Please wait until the lock has been released before trying to edit the record.\n",
						declarationReference, lockStartTime, lockUser);

				Globals.Message.ShowInformation(infoMessage, "Customs Declaration Locked");
			}

			return result;
		}

		ZGlobalMutex Mutex;

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