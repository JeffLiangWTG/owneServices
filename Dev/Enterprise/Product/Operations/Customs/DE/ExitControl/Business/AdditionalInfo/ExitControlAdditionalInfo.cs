using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.ExitControl.Business
{
	public class ExitControlAdditionalInfo : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo
	{
		public ExitControlAdditionalInfo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ResourceStringData("A6CD0B53-428C-406E-8273-D7BD6A7CA9DB", Caption = "Full Type")]
		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set => base.CSI_Code = value;
		}

		public CusExitConsignment ExitConsignment => (CusExitConsignment)Parent;

		public new ExitControlAdditionalInfoValidation Validation => (ExitControlAdditionalInfoValidation)base.Validation;

		public new ExitControlAdditionalInfoLookups Lookups => (ExitControlAdditionalInfoLookups)base.Lookups;

		protected override CusSupportingInfoLookups GetNewLookups() => new ExitControlAdditionalInfoLookups(this);

		protected override CusSupportingInfoValidation GetNewValidation() => new ExitControlAdditionalInfoValidation(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
		}
	}
}
