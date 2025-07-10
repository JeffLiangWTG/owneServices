using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers;
using ITNctsDepartureCargoDesc = Enterprise.Customs.IT.NCTS.Business.NctsDepartureCargoDesc;

namespace Enterprise.Customs.IT.NCTS.Business;

public class ITNctsDepCargoDescAttachmentWrapper : DocBaseWrapper
{
	ITNctsDepCargoDescAttachmentWrapper(ITNctsDepartureCargoDesc line, BusinessObjectFactory factory) : base(line, factory)
	{
		this.line = Argument.NotNull(line, nameof(line));
	}

	readonly ITNctsDepartureCargoDesc line;

	public static DocBaseWrapper New(ITNctsDepartureCargoDesc line, BusinessObjectFactory factory)
	{
		return new ITNctsDepCargoDescAttachmentWrapper(line, factory);
	}

	public ZString Remarks => line.Remarks;

	public ZInt ItemNumber => line.LineNumber;

	public ZString HarmonisedTariff => line.BY_HarmonisedTariff;

	public ZString Description => line.BY_Description;
}
