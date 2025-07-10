using CargoWise.EntityFramework;
using Enterprise.Customs.Common.DE;

namespace Enterprise.Customs.DE.ExitControl.Business
{
	public class CusExitReportValidation : EU.ExitControl.Business.CusExitReportValidation
	{
		public CusExitReportValidation(CusExitReport parent)
			: base(parent)
		{
		}

		new CusExitReport Parent => (CusExitReport)base.Parent;

		protected override void CheckCER_OfficeOfExport()
		{
			base.CheckCER_OfficeOfExport();

			var propertyInfo = Parent.CER_OfficeOfExportInfo;
			ListValidation.MessageErrorIfInvalidCode(propertyInfo);
			var officeOfExport = Parent.CER_OfficeOfExport;
			var officeOfExit = Parent.CER_OfficeOfExit;
			if (!officeOfExport.IsEmpty)
			{
				if (officeOfExport == officeOfExit)
				{
					propertyInfo.AddMessageError(Res.GetString("E8A57E1F-B969-4050-A4F0-6EDAF506CDAB", "The entered value must be different to the ‘Office of Exit’ if you want to forward the declaration to another customs office of exit."));
				}
				else if(!officeOfExit.IsEmpty)
				{
					propertyInfo.AddWarning(Res.GetString("089934B2-A03B-4256-A777-5740A46BF7D3", "This field should only be filled if you want to forward the declaration to another customs office of exit."));
				}
			}
		}

		protected override bool IsCER_DateTimeRequired()
		{
			return Parent.CER_Type == DEExitReportTypeList.Codes.ExitNotification
					|| Parent.CER_Type == DEExitReportTypeList.Codes.Transfer
					|| Parent.CER_Type == DEExitReportTypeList.Codes.Presentation && Parent.IsAutomatedValidationEnabledTRA;
		}

		protected override void CheckCER_TransportMode()
		{
			base.CheckCER_TransportMode();
			switch (Parent.CER_Type)
			{
				case DEExitReportTypeList.Codes.Transfer:
				case DEExitReportTypeList.Codes.Presentation when Parent.IsAutomatedValidationEnabledTRA:
					MandatoryValidation.MessageErrorIfNotEntered(Parent.CER_TransportModeInfo);
					break;
			}
		}

		protected override void CheckCER_TransportType()
		{
			base.CheckCER_TransportType();
			switch (Parent.CER_Type)
			{
				case DEExitReportTypeList.Codes.Transfer:
				case DEExitReportTypeList.Codes.Presentation when Parent.IsAutomatedValidationEnabledTRA:
					MandatoryValidation.MessageErrorIfNotEntered(Parent.CER_TransportTypeInfo);
					break;
			}
		}

		protected override void CheckCER_Location()
		{
			base.CheckCER_Location();
			switch (Parent.CER_Type)
			{
				case DEExitReportTypeList.Codes.Transfer:
				case DEExitReportTypeList.Codes.Presentation when Parent.IsAutomatedValidationEnabledTRA:
					MandatoryValidation.MessageErrorIfNotEntered(Parent.CER_LocationInfo);
					break;
			}
		}
	}
}
