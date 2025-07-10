using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CAHTSTariffBulkChange : TariffBulkChange, IObsoleteValidation
	{
		public CAHTSTariffBulkChange(BusinessObjectFactory factory)
			: base(factory, BaseCusClassification.ClassificationType.IMP)
		{
		}

		#region Overrides

		protected override bool IsPivotTariffNumSupported
		{
			get { return true; }
		}

		protected override ZString[] ValidPivotTypes
		{
			get { return new ZString[] { ClassificationTypeList.Codes.HTI, ClassificationTypeList.Codes.HTE }; }
		}

		public override ZString ReferenceKey
		{
			get { return "HS2022 HS " + lookupType; }
		}

		public override ZGuid CountryPK
		{
			get { return Core.Constants.CountryGuids.Canada; }
		}

		public override ZString CountryCode
		{
			get { return Enterprise.Core.Constants.CountryCodes.Canada; }
		}

		public override string SaveNotAllowedMessage
		{
			get
			{
				return Res.GetString("C4159C51-E62E-4D3A-BC0D-81B12D2E18ED", "The HS2022 concordance is not available yet. Save is not allowed.");
			}
		}

		public override string ApplyTariffChangesMessage
		{
			get
			{
				return Res.GetString("F78C9E7D-1D86-41EF-AA9C-101506219660", @"Your Data Base will now be updated with Pending Tariff Changes.
NOTE: If you are applying pending changes for the HS2022 change then you should only apply these changes on, or after, the 1st January 2022.
Do you wish to continue?");
			}
		}

		#endregion

	}
}
