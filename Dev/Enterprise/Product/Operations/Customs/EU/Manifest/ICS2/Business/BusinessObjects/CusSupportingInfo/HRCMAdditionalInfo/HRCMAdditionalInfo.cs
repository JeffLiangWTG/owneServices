using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class HRCMAdditionalInfo : CusSupportingInfo
	{
		public HRCMAdditionalInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new HRCMAdditionalInfoLookups Lookups => (HRCMAdditionalInfoLookups)base.Lookups;
		protected override CusSupportingInfoLookups GetNewLookups() => new HRCMAdditionalInfoLookups(this);
		protected override CusSupportingInfoValidation GetNewValidation() => new HRCMAdditionalInfoValidation(this);

		#region Overrides

		[List(nameof(Lookups) + "." + nameof(HRCMAdditionalInfoLookups.CodeList))]
		[MaxLength(17)]
		[ResourceStringData("EUICS2.HRCMAdditionalInfo.CSI_Code", Caption = "Code")]
		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set => base.CSI_Code = value;
		}

		[MaxLength(512)]
		[ResourceStringData("EUICS2.HRCMAdditionalInfo.CSI_Description", Caption = "Text")]
		public override ZString CSI_Description
		{
			get => base.CSI_Description;
			set => base.CSI_Description = value;
		}

		[List(nameof(Lookups) + "." + nameof(HRCMAdditionalInfoLookups.SubTypeList))]
		[MaxLength(3)]
		[ResourceStringData("EUICS2.HRCMAdditionalInfo.CSI_SubType", Caption = "Type")]
		public override ZString CSI_SubType
		{
			get => base.CSI_SubType;
			set => base.CSI_SubType = value;
		}

		[MaxLength(300)]
		[ResourceStringData("EUICS2.HRCMAdditionalInfo.CSI_AdditionalDescription", Caption = "Details")]
		public override ZString CSI_AdditionalDescription
		{
			get => base.CSI_AdditionalDescription;
			set => base.CSI_AdditionalDescription = value;
		}

		#endregion
	}
}
