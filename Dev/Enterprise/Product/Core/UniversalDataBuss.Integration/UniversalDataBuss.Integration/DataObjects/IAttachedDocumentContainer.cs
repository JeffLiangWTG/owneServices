using System;
using System.Collections.Generic;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IAttachedDocumentContainer
	{
		IEnumerable<IAttachedDocument> AttachedDocumentCollection { get; }
		bool SetAttachedDocumentCollection(Func<IEnumerable<IAttachedDocument>> value);
	}
}
