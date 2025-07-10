using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.ZArchitecture.Business;
using CusEntryInstruction = Enterprise.Customs.FR.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.FR.Business
{
	public class DeltaIECusAuthorizationUsageValueSetStrategy : CusAuthorizationUsageValueSetStrategy
	{
		public DeltaIECusAuthorizationUsageValueSetStrategy(CusAuthorizationUsage header) : base(header)
		{
		}

		protected override void ValueSetCore(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			base.ValueSetCore(valueThatHasChanged, oldValue);

			switch (valueThatHasChanged.Name)
			{
				case CusAuthorizationUsage.Schema.AGC_CPH_Authorization:
					SetSimplifiedAuthorisationAdditionalInfo();
					break;
			}
		}

		protected void SetSimplifiedAuthorisationAdditionalInfo()
		{
			var header = Header;
			if (header.AuthorisationHeader is CusAuthorisationHeader authorisationHeader && header.Instruction is CusEntryInstruction entryInstruction)
			{
				if (authorisationHeader.CPH_IsAdHoc)
				{
					if(!entryInstruction.HasSimplifiedAuthorisationAdditionalInfo)
					{
						var additionalInfo = header.Instruction.AdditionalInfos.AddNew();
						additionalInfo.CSI_Code = EU.Business.UniversalReferenceConstants.AdditionalDocumentTypes._00100;
						additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation;
					}
				}
				else
				{
					entryInstruction.AdditionalInfos.Cast<AdditionalInfo>().Where(x => x.CSI_SubType == EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation && x.CSI_Code == EU.Business.UniversalReferenceConstants.AdditionalDocumentTypes._00100).DeleteAll();
				}
			}
		}
	}
}
