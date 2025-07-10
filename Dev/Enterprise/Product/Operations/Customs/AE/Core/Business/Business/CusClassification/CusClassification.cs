using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AE.Business;

public class CusClassification : BaseCusClassification, Integration.Customs.AE.ICusClassification
{
	public CusClassification(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		CC_ClassificationType = BaseCusClassification.ClassificationType.Both;
		HasChanges = false;
	}

	public new CusClassificationLookups Lookups
	{
		get { return (CusClassificationLookups)base.Lookups; }
	}

	public new CusClassificationValidation Validation
	{
		get { return base.Validation; }
	}

	protected override Customs.Business.CusClassificationLookups GetNewLookups()
	{
		return new CusClassificationLookups(this);
	}

	protected override CusClassificationValidation GetNewValidation()
	{
		return new CusClassificationValidation(this);
	}
}
