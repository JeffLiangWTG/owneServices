using System.Collections.Generic;

namespace Enterprise.ZArchitecture.DataMapping
{
	public interface ISettingsStorage
	{
		IEnumerable<string> GetSavedSettings();
		void SaveSettings(string name, string settings);
		string LoadSettings(string name);
		void RemoveSettings(string name);
		bool HasSecurityRight();
		void ShowSecurityError();
	}
}
