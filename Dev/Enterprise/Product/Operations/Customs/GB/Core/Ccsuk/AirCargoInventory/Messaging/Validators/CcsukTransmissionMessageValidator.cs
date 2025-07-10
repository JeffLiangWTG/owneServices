using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using ccsukBO = Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Validators
{
	public abstract class CcsukTransmissionMessageValidator
	{
		public CcsukTransmissionMessageValidator(IBusiness bizO)
		{
			var splitHouse = bizO as ccsukBO.SplitHouse;
			var splitBasic = bizO as ccsukBO.SplitBasic;
			if (splitHouse != null)
			{
				this.hawb = (ccsukBO.CusHAWB)splitHouse.AWB;
			}
			else if (splitBasic != null)
			{
				this.mawb = (ccsukBO.CusMAWB)splitBasic.AWB;
			}
			else if (bizO is ccsukBO.CusUnderbond)
			{
				var awb = ((ccsukBO.CusUnderbond)bizO).WholeAwb;
				if (awb is ccsukBO.CusMAWB)
				{
					mawb = (ccsukBO.CusMAWB)awb;
				}
				else if (awb is ccsukBO.CusHAWB)
				{
					hawb = (ccsukBO.CusHAWB)awb;
					mawb = hawb.MAWB;
				}
			}
			else
			{
				this.mawb = bizO as ccsukBO.CusMAWB;
				this.hawb = bizO as ccsukBO.CusHAWB;
			}
		}

		protected abstract Dictionary<ZPropertyInfo, Action> GetDictionaryOfFieldsToValidate();

		internal MessageSendingNotificationCollection Validate()
		{
			Dictionary<ZPropertyInfo, Action> fieldsToValidate = this.GetDictionaryOfFieldsToValidate();

			foreach (var validateMethod in fieldsToValidate.Values)
			{
				validateMethod();
			}

			var notificationCollection = new MessageSendingNotificationCollection();
			foreach (var propertyInfo in fieldsToValidate.Keys)
			{
				foreach (var notification in propertyInfo.Notifications)
				{
					if (notification.Type != CargoWise.ComponentModel.NotificationType.Warning)
					{
						notificationCollection.AddError(propertyInfo.HumanReadableName + ": " + notification.Message);
					}
				}
			}
			PerformAdditionalValidation(notificationCollection);
			return notificationCollection;
		}

		protected virtual void PerformAdditionalValidation(MessageSendingNotificationCollection notificationCollection)
		{
		}

		protected Dictionary<ZPropertyInfo, Action> GetCommonFieldsToValidate()
		{
			var fieldsToValidate = new Dictionary<ZPropertyInfo, Action>();
			if (hawb != null)
			{
				fieldsToValidate.Add(hawb.CS_HAWBInfo, hawb.Validation.ValidateCS_HAWB);
				if (hawb.MAWB != null)
				{
					fieldsToValidate.Add(hawb.MAWB.CM_MAWBInfo, hawb.MAWB.Validation.ValidateCM_MAWB);
				}

				fieldsToValidate.Add(hawb.CargoTerminalOperatorAirportAndShedInfo, hawb.Validation.ValidateCargoTerminalOperatorAirportAndShed);
			}
			else
			{
				fieldsToValidate.Add(mawb.CM_MAWBInfo, mawb.Validation.ValidateCM_MAWB);
				fieldsToValidate.Add(mawb.CargoTerminalOperatorAirportAndShedInfo, mawb.MasterLevelHouseHelper.Validation.ValidateCargoTerminalOperatorAirportAndShed);
			}
			return fieldsToValidate;
		}

		protected ccsukBO.CusMAWB mawb;
		protected ccsukBO.CusHAWB hawb;
	}
}
