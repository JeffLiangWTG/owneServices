using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Common;

public class EntryStatusFilterValidation : ModuleTextFilterValidation
{
	public EntryStatusFilterValidation(EntryStatusFilter parent) : base(parent)
	{
	}

	protected override void CheckProperty()
	{
		base.CheckProperty();
		ListValidation.WarnIfInvalidCode(Parent.PropertyInfo);
	}

	public void ValidateFilterType()
	{
		ValidateCalculatedProperty(Parent.FilterTypeInfo);
	}

	protected void CheckFilterType()
	{
		ListValidation.ErrorIfInvalidCodeOrEmpty(Parent.FilterTypeInfo);
	}

	new EntryStatusFilter Parent => (EntryStatusFilter)base.Parent;
}
