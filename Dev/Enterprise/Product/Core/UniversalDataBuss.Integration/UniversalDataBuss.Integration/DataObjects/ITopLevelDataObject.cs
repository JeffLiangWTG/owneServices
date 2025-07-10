
using System;
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface ITopLevelDataObject : IDataObject, IDisposable, ISettableWriterStrategy
	{
		IDataContextDataObject DataContext { get; set; }

		IEnumerable<IMessageNumber> MessageNumberCollection { get; }

		void SetMessageNumber(MessageNumberType type, ZString value);
	}
}
