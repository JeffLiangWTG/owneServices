using System.Collections.Generic;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	partial class Event : IValidationRuleCollectionParent
	{
		public List<ValidationRule> ValidationRuleCollection { get; set; }
	}
}
