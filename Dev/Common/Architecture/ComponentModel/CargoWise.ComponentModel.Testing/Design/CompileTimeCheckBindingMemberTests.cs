#if DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using CargoWise.ComponentModel.Design;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public class CompileTimeCheckBindingMemberTests : TestCase
	{
		public void TestNavigateProperties()
		{
			PropertyDescriptor[] properties;
			properties = CompileTimeCheckBindingMember.Empty.NavigateProperties(typeof(MockMasterEntity), "ChildEntities.ChildChildEntities.SomeChildChildColumn");
			AssertEquals("ChildEntities", properties[0].Name);
			AssertEquals("ChildChildEntities", properties[1].Name);
			AssertEquals("SomeChildChildColumn", properties[2].Name);

			properties = CompileTimeCheckBindingMember.Empty.NavigateProperties(typeof(MockMasterEntity), "ChildEntities.ChildChildEntities.somenonexistantprop");
			AssertNull("Error finding property", properties);

			properties = CompileTimeCheckBindingMember.Empty.NavigateProperties(typeof(MockMasterEntity), "");
			AssertEquals("Empty navigation path", 0, properties.Length);
		}

		public void TestNavigateProperties_WithCollectionAsRoot()
		{
			PropertyDescriptor[] properties;
			properties = CompileTimeCheckBindingMember.Empty.NavigateProperties(typeof(MockMasterEntityCollection), "ChildEntities.ChildChildEntities.SomeChildChildColumn");
			AssertEquals("ChildEntities", properties[0].Name);
			AssertEquals("ChildChildEntities", properties[1].Name);
			AssertEquals("SomeChildChildColumn", properties[2].Name);
		}

		public void TestIsList()
		{
			AssertEquals(true, CompileTimeCheckBindingMember.IsList(typeof(IList)));
			AssertEquals(true, CompileTimeCheckBindingMember.IsList(typeof(List<int>)));
			AssertEquals(false, CompileTimeCheckBindingMember.IsList(typeof(BusinessObject)));
			AssertEquals(false, CompileTimeCheckBindingMember.IsList(typeof(object)));
			AssertEquals(false, CompileTimeCheckBindingMember.IsList(typeof(int)));
		}

		class BusinessObject : IList // gai
		{
			#region IList Members

			int IList.Add(object value)
			{
				throw new NotImplementedException();
			}

			void IList.Clear()
			{
				throw new NotImplementedException();
			}

			bool IList.Contains(object value)
			{
				throw new NotImplementedException();
			}

			int IList.IndexOf(object value)
			{
				throw new NotImplementedException();
			}

			void IList.Insert(int index, object value)
			{
				throw new NotImplementedException();
			}

			bool IList.IsFixedSize
			{
				get { throw new NotImplementedException(); }
			}

			bool IList.IsReadOnly
			{
				get { throw new NotImplementedException(); }
			}

			void IList.Remove(object value)
			{
				throw new NotImplementedException();
			}

			void IList.RemoveAt(int index)
			{
				throw new NotImplementedException();
			}

			object IList.this[int index]
			{
				get
				{
					throw new NotImplementedException();
				}
				set
				{
					throw new NotImplementedException();
				}
			}

			#endregion

			#region ICollection Members

			void ICollection.CopyTo(Array array, int index)
			{
				throw new NotImplementedException();
			}

			int ICollection.Count
			{
				get { throw new NotImplementedException(); }
			}

			bool ICollection.IsSynchronized
			{
				get { throw new NotImplementedException(); }
			}

			object ICollection.SyncRoot
			{
				get { throw new NotImplementedException(); }
			}

			#endregion

			#region IEnumerable Members

			IEnumerator IEnumerable.GetEnumerator()
			{
				throw new NotImplementedException();
			}

			#endregion
		}

		public void TestAppliedCorrectlyInArchitectureAssemblies()
		{
			foreach (Assembly assembly in AssembliesToCheckAttributesOn.GetAssemblies())
			{
				foreach (Type type in assembly.GetExportedTypes())
				{
					if (!type.IsGenericTypeDefinition)
					{
						foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(type))
						{
							if (property.PropertyType == typeof(CompileTimeCheckBindingMember))
							{
								DesignerSerializationVisibilityAttribute attr = (DesignerSerializationVisibilityAttribute)property.Attributes[typeof(DesignerSerializationVisibilityAttribute)];
								AssertEquals("Should serialize property " + property.Name + " on component " + type.FullName + " with DesignerSerializationVisibility.Content", DesignerSerializationVisibility.Content, attr.Visibility);
							}
						}
					}
				}
			}
			Assert(true);
		}

		#region Mock Objects

		public class MockMasterEntityCollection : ArrayList
		{
			public new MockMasterEntity this[int i]
			{ get { return null; } }
		}

		public abstract class MockMasterEntity
		{
			public abstract int PK { get; set; }

			public MockDetailEntityCollection ChildEntities
			{ get { return null; } }
		}

		public class MockDetailEntityCollection : ArrayList
		{
			public new MockDetailEntity this[int i]
			{ get { return null; } }
		}

		public abstract class MockDetailEntity
		{
			public abstract MockDetailDetailEntityCollection ChildChildEntities { get; }
		}

		public class MockDetailDetailEntityCollection : ArrayList
		{
			public new MockDetailDetailEntity this[int i]
			{ get { return null; } }
		}

		public abstract class MockDetailDetailEntity
		{
			public abstract string SomeChildChildColumn { get; set; }
		}

		#endregion
	}
}
#endif
