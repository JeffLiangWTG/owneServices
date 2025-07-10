using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.NCTS.GUI;

abstract class ClickableContext : IClickableContext
{
	public ClickableContext(NctsHeader header)
	{
		Header = Argument.NotNull(header, nameof(header));
	}

	#region IClickableContext

	public abstract ResourceString Caption { get; }

	public abstract string Name { get; }

	public abstract bool Enabled { get; }

	public abstract bool Visible { get; }

	public abstract void Execute(IClickableItem clickableItem);

	#endregion

	#region Implementation

	protected bool IsPhase5DepartureHeader() => Header.IsPhase5Departure;

	protected NctsDepartureMovementHeader MovementHeader => Header.MovementHeader;

	protected NctsHeader Header { get; }

	#endregion
}
