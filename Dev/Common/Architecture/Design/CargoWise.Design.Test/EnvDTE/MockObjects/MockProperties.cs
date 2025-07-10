using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace CargoWise.Design.DTE.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class MockProperties : List<EnvDTE.Property>, EnvDTE.Properties
	{
		public MockProperties(MockDTE dte)
		{ this.dte = dte; }

		public MockDTE DTE
		{ get { return dte; } }

		readonly MockDTE dte;

		EnvDTE.DTE EnvDTE.Properties.DTE
		{ get { return DTE; } }

		public MockProperty AddNew(string name)
		{
			MockProperty result = new MockProperty(this);
			result.Name = name;
			Add(result);
			return result;
		}

		EnvDTE.Property EnvDTE.Properties.Item(object index)
		{
			if (index is string)
			{
				foreach (EnvDTE.Property property in this)
				{
					if (property.Name == (string)index)
					{
						return property;
					}
				}
				return null;
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		System.Collections.IEnumerator EnvDTE.Properties.GetEnumerator()
		{
			return GetEnumerator();
		}

		#region Unsupported Members

		object EnvDTE.Properties.Application
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		object EnvDTE.Properties.Parent
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		#endregion
	}
}
