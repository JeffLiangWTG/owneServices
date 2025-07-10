using System;
using System.ComponentModel;

namespace CargoWise.Design.DTE.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class MockConfigurationManager : MarshalByRefObject, EnvDTE.ConfigurationManager
	{
		public MockConfiguration ActiveConfiguration
		{
			get
			{
				if (activeConfiguration == null)
				{
					activeConfiguration = new MockConfiguration();
				}
				return activeConfiguration;
			}
		}
		MockConfiguration activeConfiguration;

		EnvDTE.Configuration EnvDTE.ConfigurationManager.ActiveConfiguration
		{ get { return ActiveConfiguration; } }

		#region ConfigurationManager Members

		public System.Collections.IEnumerator GetEnumerator()
		{
			// TODO:  Add MockConfigurationManager.GetEnumerator implementation
			return null;
		}

		public EnvDTE.Configuration Item(object index, string Platform)
		{
			// TODO:  Add MockConfigurationManager.Item implementation
			return null;
		}

		public EnvDTE.DTE DTE
		{
			get
			{
				// TODO:  Add MockConfigurationManager.DTE getter implementation
				return null;
			}
		}

		public int Count
		{
			get
			{
				// TODO:  Add MockConfigurationManager.Count getter implementation
				return 0;
			}
		}

		public void DeletePlatform(string Name)
		{
			// TODO:  Add MockConfigurationManager.DeletePlatform implementation
		}

		public object ConfigurationRowNames
		{
			get
			{
				// TODO:  Add MockConfigurationManager.ConfigurationRowNames getter implementation
				return null;
			}
		}

		public object SupportedPlatforms
		{
			get
			{
				// TODO:  Add MockConfigurationManager.SupportedPlatforms getter implementation
				return null;
			}
		}

		public EnvDTE.Configurations AddPlatform(string NewName, string ExistingName, bool Propagate)
		{
			// TODO:  Add MockConfigurationManager.AddPlatform implementation
			return null;
		}

		public void DeleteConfigurationRow(string Name)
		{
			// TODO:  Add MockConfigurationManager.DeleteConfigurationRow implementation
		}

		public EnvDTE.Configurations AddConfigurationRow(string NewName, string ExistingName, bool Propagate)
		{
			// TODO:  Add MockConfigurationManager.AddConfigurationRow implementation
			return null;
		}

		public EnvDTE.Configurations ConfigurationRow(string Name)
		{
			// TODO:  Add MockConfigurationManager.ConfigurationRow implementation
			return null;
		}

		public object Parent
		{
			get
			{
				// TODO:  Add MockConfigurationManager.Parent getter implementation
				return null;
			}
		}

		public EnvDTE.Configurations Platform(string Name)
		{
			// TODO:  Add MockConfigurationManager.Platform implementation
			return null;
		}

		public object PlatformNames
		{
			get
			{
				// TODO:  Add MockConfigurationManager.PlatformNames getter implementation
				return null;
			}
		}

		#endregion
	}
}
