using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.BE.NCTS.Business;

public class NctsGuarantee : EU.NCTS.Business.NctsGuarantee
{
	public NctsGuarantee(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new NctsHeader NctsHeader => (NctsHeader)base.NctsHeader;

	public new NctsGuaranteeValidation Validation => (NctsGuaranteeValidation)base.Validation;

	public override ZString PW_BondNumber
	{
		get => base.PW_BondNumber;
		set
		{
			var oldValue = PW_BondNumber;
			base.PW_BondNumber = value;
			if (!IsCopying && oldValue != PW_BondNumber)
			{
				if (PW_BondNumber.IsEmpty)
				{
					PW_BondAmount = ZDecimal.Zero;
				}
			}
		}
	}

	protected override EU.NCTS.Business.NctsGuaranteePhase5Validation GetNewPhase5Validation() => new NctsGuaranteeValidation(this);

	protected override ZDecimal Phase5DefaultLiabilityAmount => 10000m;

	public override ZString PW_BondFiledPort
	{
		get => base.PW_BondFiledPort;
		set
		{
			if (!IsCopying)
			{
				base.PW_BondFiledPort = value;
			}
		}
	}

	protected override bool CopyCustomsOfficeFromGuaranteeHeader => false;
}
