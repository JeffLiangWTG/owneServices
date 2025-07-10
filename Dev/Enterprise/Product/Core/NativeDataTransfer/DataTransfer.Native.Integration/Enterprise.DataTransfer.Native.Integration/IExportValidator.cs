using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.DataTransfer.Native.Integration
{
	public interface IExportValidator
	{
		bool CanBeExported(Type type);
		void Validate(IEnumerable<IBusiness> businessObject);
	}
}
