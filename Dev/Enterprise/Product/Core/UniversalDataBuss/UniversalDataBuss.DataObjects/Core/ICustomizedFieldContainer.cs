using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public interface ICustomizedFieldContainer : ISettableWriterStrategy
	{
		List<CustomizedField> CustomizedFieldCollection { get; }
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		bool SetCustomizedFieldCollection(Func<List<CustomizedField>> getter);
	}
}
