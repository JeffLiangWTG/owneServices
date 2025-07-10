using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Common;

namespace CargoWise.Main.Navigation;

public class SearchResultSection
{
	public SearchResultSection(string displayName, string name, IEnumerable<MenuItem> items, SectionType sectionType)
	{
		Argument.NotNull(name, nameof(name));
		Argument.NotNull(items, nameof(items));

		DisplayName = RemoveAmpersand(displayName);
		Name = name;
		Items = [.. items];
		SectionType = sectionType;
	}

	string RemoveAmpersand(string displayNameCleanerWithMnemonic) => displayNameCleanerWithMnemonic.Replace(" && ", " & ");

	public string DisplayName { get; private set; }

	public string Name { get; private set; }

	public ObservableCollection<MenuItem> Items { get; private set; }

	public SectionType SectionType { get; private set; }

	public string ResultSectionKey
	{
		get
		{
			return $"{SectionType}{Name}-{ResultOrder}: {DisplayName}";
		}
	}

	/// <summary>
	/// The order in which the results should be displayed
	/// </summary>
	public int ResultOrder { get; internal set; }

	public override string ToString()
	{
		return $"({Items.Count}) {Name}: {DisplayName}";
	}
}
