using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class ImportSADNumberValidation : Customs.Business.CusSupportingInfoValidation
	{
		public ImportSADNumberValidation(ImportSADNumber bizObj)
			: base(bizObj)
		{
		}

		protected new ImportSADNumber Parent => (ImportSADNumber)base.Parent;

		protected override void CheckCSI_Description()
		{
			base.CheckCSI_Description();
			var emcsDeclaration = Parent.Declaration;
			if (emcsDeclaration != null)
			{
				if (emcsDeclaration.ZG_OriginType != EMCSOriginTypeList.Codes.Import)
				{
					Parent.CSI_DescriptionInfo.AddMessageError(Res.GetString("ba976460-c561-49ac-99f9-84b451bfdbc9", "Import SAD Numbers only required when Origin Type is Import."));
				}
				else
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_DescriptionInfo);
				}
				if (emcsDeclaration.ImportSADNumbers.Cast<ImportSADNumber>().Any(x => x.PK != Parent.PK && x.CSI_Description == Parent.CSI_Description))
				{
					Parent.CSI_DescriptionInfo.AddMessageError(Res.GetString("0e158e9c-62ea-4ba0-9301-490ff60516ac", "Import SAD Number should not be duplicated."));
				}
			}
		}
	}
}
