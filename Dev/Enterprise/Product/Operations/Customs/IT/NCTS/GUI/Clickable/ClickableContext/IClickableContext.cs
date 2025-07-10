using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.NCTS.GUI;

public interface IClickableContext
{
	ResourceString Caption { get; }

	string Name { get; }

	bool Enabled { get; }

	bool Visible { get; }

	void Execute(IClickableItem clickableItem);
}
