using System;
using System.ComponentModel;

namespace CargoWise.Design.DTE.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class MockProperty : MarshalByRefObject, EnvDTE.Property
	{
		public MockProperty(MockProperties collection)
		{ this.collection = collection; }

		EnvDTE.DTE EnvDTE.Property.DTE
		{ get { return Collection.DTE; } }

		public MockProperties Collection
		{ get { return collection; } }

		readonly MockProperties collection;

		EnvDTE.Properties EnvDTE.Property.Collection
		{ get { return Collection; } }

		public string Name
		{
			get { return name; }
			set { name = value; }
		}
		string name;

		string EnvDTE.Property.Name
		{ get { return Name; } }

		public object Value
		{
			get { return fValue; }
			set { fValue = value; }
		}
		object fValue;

		#region Unsupported Members

		object EnvDTE.Property.Application
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		short EnvDTE.Property.NumIndices
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		object EnvDTE.Property.Object
		{
			get
			{
				throw new Exception("The method or operation is not implemented.");
			}
			set
			{
				throw new Exception("The method or operation is not implemented.");
			}
		}

		EnvDTE.Properties EnvDTE.Property.Parent
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		object EnvDTE.Property.get_IndexedValue(object index1, object index2, object index3, object index4)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.Property.let_Value(object lppvReturn)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.Property.set_IndexedValue(object index1, object index2, object index3, object index4, object val)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		#endregion
	}
}
