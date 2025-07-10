using System;
using System.ComponentModel;

namespace CargoWise.Design.DTE.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class MockConfiguration : MarshalByRefObject, EnvDTE.Configuration
	{
		public MockOutputGroups OutputGroups
		{
			get
			{
				if (outputGroups == null)
				{
					outputGroups = new MockOutputGroups();
				}
				return outputGroups;
			}
		}
		MockOutputGroups outputGroups;

		EnvDTE.OutputGroups EnvDTE.Configuration.OutputGroups
		{ get { return OutputGroups; } }

		#region Configuration Members

		public object Owner
		{
			get
			{
				// TODO:  Add MockConfiguration.Owner getter implementation
				return null;
			}
		}

		public string ConfigurationName
		{
			get
			{
				// TODO:  Add MockConfiguration.ConfigurationName getter implementation
				return null;
			}
		}

		public bool IsBuildable
		{
			get
			{
				// TODO:  Add MockConfiguration.IsBuildable getter implementation
				return false;
			}
		}

		public EnvDTE.DTE DTE
		{
			get
			{
				// TODO:  Add MockConfiguration.DTE getter implementation
				return null;
			}
		}

		public object get_Extender(string ExtenderName)
		{
			// TODO:  Add MockConfiguration.get_Extender implementation
			return null;
		}

		public bool IsRunable
		{
			get
			{
				// TODO:  Add MockConfiguration.IsRunable getter implementation
				return false;
			}
		}

		public string PlatformName
		{
			get
			{
				// TODO:  Add MockConfiguration.PlatformName getter implementation
				return null;
			}
		}

		public EnvDTE.ConfigurationManager Collection
		{
			get
			{
				// TODO:  Add MockConfiguration.Collection getter implementation
				return null;
			}
		}

		public object Object
		{
			get
			{
				// TODO:  Add MockConfiguration.Object getter implementation
				return null;
			}
		}

		public object ExtenderNames
		{
			get
			{
				// TODO:  Add MockConfiguration.ExtenderNames getter implementation
				return null;
			}
		}

		public EnvDTE.vsConfigurationType Type
		{
			get
			{
				// TODO:  Add MockConfiguration.Type getter implementation
				return new EnvDTE.vsConfigurationType();
			}
		}

		public EnvDTE.Properties Properties
		{
			get
			{
				// TODO:  Add MockConfiguration.Properties getter implementation
				return null;
			}
		}

		public string ExtenderCATID
		{
			get
			{
				// TODO:  Add MockConfiguration.ExtenderCATID getter implementation
				return null;
			}
		}

		public bool IsDeployable
		{
			get
			{
				// TODO:  Add MockConfiguration.IsDeployable getter implementation
				return false;
			}
		}

		#endregion
	}
}
