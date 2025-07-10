using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.H7.Business
{
	public class AdditionalInfo : EU.H7.Business.AdditionalInfo
	{
		public AdditionalInfo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new AdditionalInfoLookups Lookups => (AdditionalInfoLookups)base.Lookups;

		protected override CusSupportingInfoLookups GetNewLookups() => new AdditionalInfoLookups(this);

		public new AdditionalInfoValidation Validation => (AdditionalInfoValidation)base.Validation;

		protected override CusSupportingInfoValidation GetNewValidation()
		{
			return new AdditionalInfoValidation(this);
		}

		[MaxLength(5)]
		[List(nameof(Lookups) + "." + nameof(AdditionalInfoLookups.CodeList))]
		[ResourceStringData("GB.H7.AdditionalInfo.CSI_Code", Caption = "Code", MediumCaption = "Code", ShortCaption = "Code", FullDescription = "Code within relevant additional information list.")]
		public override ZString CSI_Code
		{
			get
			{
				return base.CSI_Code;
			}
			set
			{
				base.CSI_Code = value;
			}
		}
	}
}
