using System;
using System.Collections.Generic;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public interface IAdHocValidationProvider
	{
		Func<IEnumerable<string>> Validator { get; }
	}
}
