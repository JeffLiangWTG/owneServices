using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.ES.Business;

public class CusAuthorizationUsage(BusinessObjectFactory factory, DataRow row) : EU.Business.CusAuthorizationUsage(factory, row)
{
	public new CusAuthorizationUsageValidation Validation => (CusAuthorizationUsageValidation)base.Validation;

	protected override EU.Business.CusAuthorizationUsageValidation GetNewValidation() => new CusAuthorizationUsageValidation(this);

	[ResourceStringData("4DC05B52-7E44-4EDE-BBC1-6F14385A64E8", Caption = "Type", MediumCaption = "Type", ShortCaption = "Type", FullDescription = "[12 12 002 000] Type", MultipleKey = "IMPUCC63-26F2-41DA-8AFD-1F49A93B7BDF")]
	public override ZString AGC_Code { get => base.AGC_Code; set => base.AGC_Code = value; }

	[ResourceStringData("DDD7DF94-E3E3-4AA7-8831-760565BA837D", Caption = "Reference", MediumCaption = "Refer.", ShortCaption = "Ref.", FullDescription = "[12 12 001 000] Reference", MultipleKey = "IMPUCC63-26F2-41DA-8AFD-1F49A93B7BDF")]
	public override ZString AGC_Number { get => base.AGC_Number; set => base.AGC_Number = value; }
}
