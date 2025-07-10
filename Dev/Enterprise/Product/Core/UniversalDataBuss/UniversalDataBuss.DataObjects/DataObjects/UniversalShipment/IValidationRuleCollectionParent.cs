using System.Collections.Generic;

namespace Enterprise.UniversalDataBuss.DataObjects
{
	public interface IValidationRuleCollectionParent
	{
		List<ValidationRule> ValidationRuleCollection { get; }
	}
}
