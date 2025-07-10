using CargoWise.ComponentModel;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business
{
	public class UPERefLocoMapValidation : RefLocoMapValidation
	{
		public UPERefLocoMapValidation(RefLocoMap parent)
			: base(parent)
		{
		}

		protected new UPERefLocoMap Parent
		{
			get { return (UPERefLocoMap)base.Parent; }
		}

		protected override INotificationType GetDuplicateCountryAndSystemUsageCombinationsNotificationType()
		{
			return Parent.RY_SystemUsage == UPEOtherLocoMapSystemUsageList.Codes.Ups || Parent.RY_RN == Core.Constants.CountryGuids.UnitedStates ? NotificationType.Warning : NotificationType.Error;
		}
	}
}
