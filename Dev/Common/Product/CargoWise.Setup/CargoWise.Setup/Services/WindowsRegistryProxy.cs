using Microsoft.Win32;

namespace CargoWise.Setup.Services;

public interface IWindowsRegistryProxy
{
	object? GetValue(string keyPath, string keyName, object? defaultValue = null);
	void SetLocalMachineValue(RegistryView view, string registryKeyPath, string valueName, object value);
}

internal class WindowsRegistryProxy : IWindowsRegistryProxy
{
	public object? GetValue(string keyPath, string keyName, object? defaultValue = null)
	{
		// GetValue only uses defaultValue if the key exists but doesn't contain the value, if the key doesn't exist it returns null
		return Registry.GetValue(keyPath, keyName, defaultValue) ?? defaultValue;
	}

	public void SetLocalMachineValue(RegistryView view, string registryKeyPath, string valueName, object value)
	{
		using var rootKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view);
		var subKey = rootKey.CreateSubKey(registryKeyPath);
		subKey.SetValue(valueName, value);
	}
}
