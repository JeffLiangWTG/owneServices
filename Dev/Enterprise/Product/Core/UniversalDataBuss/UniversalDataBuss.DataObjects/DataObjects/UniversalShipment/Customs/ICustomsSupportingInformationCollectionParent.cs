using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	public interface ICustomsSupportingInformationCollectionParent
	{
		void SetWriterStrategy(IDataObjectWriterStrategy strategy);
		List<CustomsSupportingInformation> CustomsSupportingInformationCollection { get; }
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		bool SetCustomsSupportingInformationCollection(Func<List<CustomsSupportingInformation>> value);
	}
}
