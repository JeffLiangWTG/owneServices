using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.ES.ExitControl.Business
{
	public class AdditionalInfo : EU.ExitControl.Business.AdditionalInfo
	{
		public AdditionalInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new AdditionalInfoValidation Validation => (AdditionalInfoValidation)base.Validation;

		protected override Customs.Business.CusSupportingInfoValidation GetNewValidation() => new AdditionalInfoValidation(this);

		protected override Customs.Business.CusSupportingInfoLookups GetNewLookups() => new AdditionalInfoLookups(this);

		[ResourceStringData("EEE30069-83C2-4498-BBAC-E072BE061CB9", Caption = "Sequence Number", MediumCaption = "Seq Number", ShortCaption = "Seq Num.")]
		public override ZInt CSI_ItemNumber
		{
			get => base.CSI_ItemNumber;
			set => base.CSI_ItemNumber = value;
		}

		protected override ZString DefaultStatus => ZString.Empty;

		protected override bool IsUCC6Core => Ucc6ValueProvider?.IsUCC6 ?? true;
	}
}
