using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsGuarantee : EU.NCTS.Business.NctsGuarantee
{
	public NctsGuarantee(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	[ResourceStringData("a132f960-47be-4ff7-b368-3c728882b54a", Caption = "Office Desc.", MultipleKey = EU.NCTS.Business.NctsHeader.Phase5CaptionKey)]
	public ZString OfficeDescription => Factory.GetCachedValue(FormattableString.Invariant($"IT_NCTS_P5_OFFICE_{PW_BondFiledPort}"), GetCustomsOfficeDescription);

	public ZPropertyInfo OfficeDescriptionInfo => GetZPropertyInfo(nameof(OfficeDescription));

	protected override EU.NCTS.Business.NctsGuaranteeValidation GetNewPhase4Validation() => new NctsGuaranteePhase4Validation(this);

	protected override CusBondDetailLookups GetNewLookups()
		=> NctsHeader?.IsPhase5 ?? false
			? new NctsPhase5GuaranteeLookups(this)
			: base.GetNewLookups();

	#region Implementation

	ZString GetCustomsOfficeDescription()
	{
		var officeCodeList = Lookups.OfficeCodeList;
		if (!officeCodeList.IsLoaded && !officeCodeList.IsLoading)
		{
			officeCodeList.Load();
		}

		var officeCode = officeCodeList.Cast<ZZRefCusCodeListCombined>()
			.FirstOrDefault(c => c.ZZD_Code == PW_BondFiledPort);
		return officeCode?.ZZD_Description ?? ZString.Empty;
	}

	#endregion
}
