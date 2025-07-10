using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class NctsPreviousDocument : EU.NCTS.Business.NctsPreviousDocument
	{
		public NctsPreviousDocument(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
		{ }

		public new NctsDepartureCargoDesc Parent => (NctsDepartureCargoDesc)base.Parent;

		protected override CusSupportingInfoValidation GetNewPhase5Validation() => new NctsPreviousDocumentPhase5Validation(this);

		protected override CusSupportingInfoValidation GetNewPhase4Validation() => new NctsPreviousDocumentValidation(this);

		protected override CusSupportingInfoLookups GetNewPhase4Lookups() => new NctsPreviousDocumentPhase4Lookups(this);

		protected override CusSupportingInfoLookups GetNewPhase5Lookups() => new NctsPreviousDocumentPhase5Lookups(this);

		[MaxLength(26)]
		[ResourceStringData("FRNctsPreviousDocument|CSI_Description", Caption = "Description")]
		public override ZString CSI_Description
		{
			get => base.CSI_Description;
			set => base.CSI_Description = value;
		}

		[List(nameof(Lookups) + "." + nameof(NctsPreviousDocumentPhase4Lookups.ReferenceList))]
		public override ZString CSI_ReferenceNumber
		{
			get => base.CSI_ReferenceNumber;
			set => base.CSI_ReferenceNumber = value;
		}

		public override ZBool ShowCodeFindBoxForReferenceNumber => (Parent.Header?.IsPhase4 ?? false) ? CSI_Code == PreviousDocumentCodeList.Codes._337 : CSI_Code == UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage;
	}
}
