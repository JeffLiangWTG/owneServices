namespace Enterprise.ZArchitecture.GUI
{
	public interface IReadOnlyAutomationOptional : IReadOnlyToggleControl
	{
		bool ShouldSetReadOnlyWhenSettingIncludingChildren { get; set; }
	}
}
