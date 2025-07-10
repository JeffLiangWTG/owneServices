using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class NonPersistentStandAloneFsrEnquiryForNewValidation : AutoNonPersistentStandAloneFsrEnquiryForNewValidation
	{
		public NonPersistentStandAloneFsrEnquiryForNewValidation(AutoNonPersistentStandAloneFsrEnquiryForNew parent)
			: base(parent)
		{ }

		protected override void CheckMAWB()
		{
			base.CheckMAWB();
			MandatoryValidation.CheckEntered(Parent.MAWBInfo);
			CheckExactLength(11, Parent.MAWBInfo);
		}

		protected override void CheckHAWB()
		{
			base.CheckHAWB();
			if (Parent.HAWB.Length > 0)
			{
				CheckExactLength(8, Parent.HAWBInfo);
			}
		}

		protected override void CheckDatabaseToQuery()
		{
			base.CheckDatabaseToQuery();
			ListValidation.ErrorIfInvalidCode(Parent.DatabaseToQueryInfo);
			MandatoryValidation.CheckEntered(Parent.DatabaseToQueryInfo);
			EnsureAirportAndShedForFsnQuery(Parent.DatabaseToQueryInfo);
			ValidateAirport();
			ValidateShed();
		}

		void EnsureAirportAndShedForFsnQuery(ZPropertyInfo zPropertyInfo)
		{
			if (Parent.DatabaseToQuery == FsrRequestType.Codes.FsnRetransmission)
			{
				if (Parent.Airport.IsEmpty || Parent.Shed.IsEmpty)
				{
					zPropertyInfo.AddError("FSN retransmission requires airport and shed");
				}
			}
		}

		protected override void CheckSRF()
		{
			base.CheckSRF();
			if (Parent.SRF.Length > 0)
			{
				CheckExactLength(2, Parent.SRFInfo);
			}
		}

		protected override void CheckShed()
		{
			base.CheckShed();
			if (Parent.Shed.Length > 0)
			{
				CheckExactLength(3, Parent.ShedInfo);
			}
			CheckPairBothPresentOrBothAbsent(Parent.ShedInfo, Parent.AirportInfo);
			ValidateAirport();
			ValidatePIMA();
		}

		protected override void CheckAirport()
		{
			base.CheckAirport();
			if (Parent.Airport.Length > 0)
			{
				CheckExactLength(3, Parent.AirportInfo);
			}
			CheckPairBothPresentOrBothAbsent(Parent.AirportInfo, Parent.ShedInfo);
			ValidateShed();
			ValidatePIMA();
		}

		protected override void CheckPIMA()
		{
			base.CheckPIMA();
			MandatoryValidation.CheckEntered(Parent.PIMAInfo);
			ListValidation.ErrorIfInvalidCode(Parent.PIMAInfo);
			if (Parent.PIMA.StartsWith(LicenceAndPimaHelper.ShedProfilePrefix))
			{
				CusHAWBValidation.EnsureSecurityRightForShedPima(Parent.PIMAInfo);
			}
		}

		void CheckPairBothPresentOrBothAbsent(ZPropertyInfo propertyBeingChecked, ZPropertyInfo brotherProperty)
		{
			if ((propertyBeingChecked.Value.IsEmpty && !brotherProperty.Value.IsEmpty)
				||
				(!propertyBeingChecked.Value.IsEmpty && brotherProperty.Value.IsEmpty))
			{
				propertyBeingChecked.AddError("Airport and shed must be both present or both missing");
			}
		}

		void CheckExactLength(int length, ZPropertyInfo zPropertyInfo)
		{
			var value = (ZString)zPropertyInfo.Value.ToString();
			if (value.KeepAlphanumericCharacters().Length != length)
			{
				zPropertyInfo.AddError(string.Format("{0} must be exactly {1} characters long", zPropertyInfo.HumanReadableName, length));
			}
		}
	}
}
