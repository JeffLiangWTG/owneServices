using System;
using System.Collections.Generic;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	public interface ICustomsReferenceCollectionParent
	{
		List<CustomsReference> CustomsReferenceCollection { get; }

		bool SetCustomsReferenceCollection(Func<List<CustomsReference>> getter);
	}
}
