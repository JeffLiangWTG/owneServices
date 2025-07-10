using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AE.Business;

public class JobComInvoiceLineLookups : Customs.Business.JobComInvoiceLineLookups
{
	public JobComInvoiceLineLookups(JobComInvoiceLine parent)
		: base(parent)
	{
	}

	public new JobComInvoiceLine InvoiceLine
	{
		get { return (JobComInvoiceLine)base.InvoiceLine; }
	}

	public override ICodeDescriptionPairList Procedures
	{
		get
		{
			var parent = InvoiceLine;
			ZString dataGroupingCode = parent.Declaration?.IsInterface ?? true ? Core.Constants.CountryCodes.UnitedArabEmirates : parent.Declaration.JE_ApplicationCode;
			return new RefCusProcedureCollection(Factory, dataGroupingCode, parent.Declaration?.DateOfValuation ?? ZDateTime.Today);
		}
	}

	public override CodeDescriptionPairList InvoiceUQList
	{
		get { return new InvoiceUQList(); }
	}

	public CodeDescriptionPairList GoodsConditionList => Factory.GetCachedValue<AEGoodsConditionList>();
}
