using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public partial class JobDeclaration
	{
		[ReadOnlyMember(nameof(ZG_PartialWriteoff_ReadOnly))]
		public override ZBool ZG_PartialWriteoff
		{
			get => base.ZG_PartialWriteoff;
			set => base.ZG_PartialWriteoff = value;
		}

		public bool ZG_PartialWriteoff_ReadOnly => !(IsImport
													&& (JE_CustomsOffice.StartsWith(CustomsOfficeCodeList.Ceuta, StringComparison.OrdinalIgnoreCase)
														|| JE_CustomsOffice.StartsWith(CustomsOfficeCodeList.Melilla, StringComparison.OrdinalIgnoreCase)));

		public override ZString ZG_OtherEmailAddr
		{
			get => base.ZG_OtherEmailAddr;
			set => base.ZG_OtherEmailAddr = value;
		}

		public override ZString ZG_AgreedPlaceCode
		{
			get => base.ZG_AgreedPlaceCode;
			set
			{
				var oldValue = ZG_AgreedPlaceCode;
				base.ZG_AgreedPlaceCode = value;
				if (!IsCopying && oldValue != ZG_AgreedPlaceCode)
				{
					Invoices.MarkAsNeedingValidation();
				}
			}
		}
	}
}
