using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.MessageStructure;

public abstract class CustomsInterchange
{
	public CustomsInterchangeHeader Header { get; set; }

	protected IEnumerable<ZString> Lines { get; private set; }

	protected virtual void Load(ZString content)
	{
		Lines = content.SplitByNewLine(StringSplitOptions.RemoveEmptyEntries);
		Header = new CustomsInterchangeHeader();
		Header.Load(Lines.ElementAt(0));
	}

	[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
	public static CustomsInterchangeReadResult<T> LoadSafe<T>(ZString content) where T : CustomsInterchange, new()
	{
		var parseResult = new CustomsInterchangeReadResult<T>();
		try
		{
			var interchangeObject = new T();
			interchangeObject.Load(content);
			parseResult.Interchange = interchangeObject;
		}
		catch (Exception ex) when (!ex.IsCriticalException())
		{
			parseResult.Exception = ex;
		}
		return parseResult;
	}
}
