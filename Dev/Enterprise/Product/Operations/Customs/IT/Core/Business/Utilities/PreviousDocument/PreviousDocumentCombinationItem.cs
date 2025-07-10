using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public class PreviousDocumentCombinationItem
{
	public PreviousDocumentCombinationItem(BusinessObjectFactory factory, string procedure, string code, string subType, PreviousDocumentCombinationTemplate template)
	{
		this.factory = Argument.NotNull(factory, nameof(factory));
		Procedure = Argument.NotNull(procedure, nameof(procedure));
		this.code = Argument.NotNull(code, nameof(code));
		this.subType = Argument.NotNull(subType, nameof(subType));
		Template = template;
	}
	readonly BusinessObjectFactory factory;
	readonly ZString code;
	readonly ZString subType;

	public ZString Procedure { get; }
	public ICodeDescription Code => factory.GetCachedValue<PreviousDocumentCodeList>()?[code, StringComparison.OrdinalIgnoreCase];
	public ICodeDescription SubType => factory.GetCachedValue<PreviousDocumentClassList>()?[subType, StringComparison.OrdinalIgnoreCase];
	public PreviousDocumentCombinationTemplate Template { get; }
}
