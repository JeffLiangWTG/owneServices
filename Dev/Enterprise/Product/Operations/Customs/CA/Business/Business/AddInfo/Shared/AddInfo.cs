using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public abstract class AddInfo : AutoCAAddInfo
	{
		#region Constructor

		protected AddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		#endregion

		[List(nameof(Lookups) + "." + nameof(CAAddInfoLookups.CBSAOffices))]
		public override ZString CA_PlaceOfReport
		{
			get { return base.CA_PlaceOfReport; }
			set { base.CA_PlaceOfReport = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CAAddInfoLookups.CBSAOffices))]
		public override ZString CA_PortOfExit
		{
			get { return base.CA_PortOfExit; }
			set { base.CA_PortOfExit = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CAAddInfoLookups.ReasonForExportCodes))]
		public override ZString CA_ReasonForExportCode
		{
			get { return base.CA_ReasonForExportCode; }
			set { base.CA_ReasonForExportCode = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CAAddInfoLookups.DeclaredCurrencies))]
		public override ZGuid CA_RX_DeclaredCurr
		{
			get { return base.CA_RX_DeclaredCurr; }
			set { base.CA_RX_DeclaredCurr = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CAAddInfoLookups.OGDStatusCodes))]
		public override ZString CA_OGDStatus
		{
			get { return base.CA_OGDStatus; }
			set { base.CA_OGDStatus = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CAAddInfoLookups.ModelYearList))]
		public override ZString CA_ModelYear
		{
			get => base.CA_ModelYear;
			set => base.CA_ModelYear = value;
		}

		public override bool HasChanges
		{
			get { return base.HasChanges; }
			set
			{
				base.HasChanges = value;
				if (HasChanges && Parent != null && !Parent.IsMarkingAsNeedingValidationSuspended)
				{
					Parent.MarkAsNeedingValidation();
				}
			}
		}
	}
}
