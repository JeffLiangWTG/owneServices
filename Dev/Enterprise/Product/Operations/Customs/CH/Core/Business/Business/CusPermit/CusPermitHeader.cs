using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class CusPermitHeader : BaseCusPermitHeader, Integration.Customs.CH.ICusPermitHeader
{
	public CusPermitHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : BaseCusPermitHeader.Schema
	{
		public new const int CPH_TypeMaxLength = 3;
	}

	#region Properties

	[MaxLength(Schema.CPH_TypeMaxLength)]
	public override ZString CPH_Type { get => base.CPH_Type; set => base.CPH_Type = value; }

	[List(nameof(Lookups) + "." + nameof(CusPermitHeaderLookups.UnitOfQuantityList))]
	public override ZString CPH_UnitOfMeasure { get => base.CPH_UnitOfMeasure; set => base.CPH_UnitOfMeasure = value; }

	#endregion

	protected override ZBool AllowNewLineTransactions => IsCUM && IsTransactionsApplicable();

	public new CusPermitHeaderLookups Lookups => (CusPermitHeaderLookups)base.Lookups;

	protected override Customs.Business.CusPermitHeaderLookups GetNewLookups() => new CusPermitHeaderLookups(this);

	protected override Customs.Business.CusPermitHeaderValidation GetNewValidation() => new CusPermitHeaderValidation(this);
}
