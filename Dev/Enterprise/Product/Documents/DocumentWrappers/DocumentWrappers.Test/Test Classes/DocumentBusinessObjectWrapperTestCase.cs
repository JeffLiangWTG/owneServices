using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappersCore;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public abstract class DocumentWrapperTestCase : NonPersistentBusinessObjectTestCase
	{
		#region Get New Business Object

		protected sealed override BusinessObject GetNewBusinessObject()
		{
			DocumentWrapper result = CreateDocumentWrapperFromStaticNewMethod();

			AssertNotNull("Unable to create DocumentWrapper using the public static New() method: [" + Wrappers[0].GetType().FullName + "]. Please ensure you have a New constructor that takes a BusinessObject and a BusinessObjectFactory only", result);

			return result;
		}

		protected virtual DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			DocumentWrapper result = null;
			MethodInfo[] methods = Wrappers[0].GetType().GetMethods(BindingFlags.Static | BindingFlags.Public);

			foreach (MethodInfo method in methods)
			{
				if (method.Name == "New" && method.GetParameters().Length <= 2)
				{
					ParameterInfo[] parameters = method.GetParameters();

					if (parameters.Length == 0)
					{
						result = (DocumentWrapper)method.Invoke(null, null);
					}
					else if (parameters.Length == 1)
					{
						result = (DocumentWrapper)method.Invoke(null, new object[] { Factory });
					}
					else if (typeof(BusinessObject).IsAssignableFrom(parameters[0].ParameterType) && (parameters[1].ParameterType == typeof(BusinessObjectFactory)))
					{
						BusinessObject wrappedObject = null;
						Type bizoType = parameters[0].ParameterType;

						if (!bizoType.IsAbstract)
						{
							if (bizoType.IsSubclassOf(typeof(NonPersistentBusinessObject)))
							{
								ConstructorInfo constructor = bizoType.GetConstructor(System.Type.EmptyTypes);
								if (constructor != null)
								{
									wrappedObject = (BusinessObject)constructor.Invoke(null);
								}
								else
								{
									constructor = bizoType.GetConstructor(new Type[] { typeof(BusinessObjectFactory) });
									if (constructor != null)
									{
										wrappedObject = (BusinessObject)constructor.Invoke(new object[] { Factory });
									}
								}
							}
							else
							{
								wrappedObject = Factory.New(parameters[0].ParameterType);
							}
						}
						else
						{
							ArrayList assemblyList = new ArrayList();
							foreach (Type type in bizoType.Assembly.GetTypes())
							{
								if (type.IsSubclassOf(bizoType) && !type.IsAbstract)
								{
									if (bizoType.IsSubclassOf(typeof(NonPersistentBusinessObject)))
									{
										ConstructorInfo constructor = bizoType.GetConstructor(System.Type.EmptyTypes);
										if (constructor != null)
										{
											wrappedObject = (BusinessObject)constructor.Invoke(null);
										}
									}
									else
									{
										wrappedObject = Factory.New(type);
									}
								}
							}
						}
						result = (DocumentWrapper)method.Invoke(null, new object[] { wrappedObject, Factory });
					}

					if (result != null)
					{
						break;
					}
				}
			}

			return result;
		}

		#endregion

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		[ExpectNoExceptions]
		public void TestNewUsingNullObject()
		{
			Type wrapperType = Wrappers[0].GetType();
			foreach (MethodInfo hopefullyNewInfo in wrapperType.GetMethods(BindingFlags.Static | BindingFlags.Public))
			{
				if (hopefullyNewInfo.Name == "New" && hopefullyNewInfo.GetParameters().Length == 1)
				{
					if (hopefullyNewInfo.GetParameters()[0].ParameterType.IsSubclassOf(typeof(BusinessObject)))
					{
						AssertNull(hopefullyNewInfo.Invoke(null, new object[] { null }));
					}
				}
			}
		}

		[ExpectNoExceptions]
		public void TestNewUsingInvalidPKWhenNewStaticMethodExists()
		{
			Type wrapperType = Wrappers[0].GetType();
			foreach (MethodInfo hopefullyNewInfo in wrapperType.GetMethods(BindingFlags.Static | BindingFlags.Public))
			{
				if (hopefullyNewInfo.Name == "New" && hopefullyNewInfo.GetParameters().Length == 2)
				{
					if (hopefullyNewInfo.GetParameters()[0].ParameterType == typeof(BusinessObjectFactory) && hopefullyNewInfo.GetParameters()[1].ParameterType == typeof(ZGuid))
					{
						AssertNull(hopefullyNewInfo.Invoke(null, new object[] { Factory, ZGuid.NewZGuid() }));
					}
				}
			}
		}

		public void TestAllPropertiesAreOfTheCorrectType()
		{
			StringBuilder results = new StringBuilder();

			if (Wrappers.Length > 0)
			{
				DocumentWrapper wrapper = Wrappers[0];
				PropertyInfo[] publicProperties = wrapper.GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
				foreach (PropertyInfo info in publicProperties)
				{
					if (info.GetGetMethod().GetParameters().Length == 0)
					{
						if (!IsOfCorrectType(info))
						{
							//This is to suppress the Decimal property in DocCurrency that is used for ICurrency
							if (info.Name != "Decimals")
							{
								if (info.GetGetMethod().GetBaseDefinition().DeclaringType == info.GetGetMethod().DeclaringType)
								{
									results.Append(string.Format("Property: {1}.{2} >> Return Type: {0}" + System.Environment.NewLine, info.PropertyType, info.ReflectedType.Name, info.Name));
								}
							}
						}
					}
				}
			}

			if (results.Length > 0)
			{
				results.Insert(0, System.Environment.NewLine + "These properties have unusual return types" + System.Environment.NewLine);
			}

			PrintResults(results);
		}

		public void TestAllPropertiesDontThrowExceptions()
		{
			StringBuilder results = new StringBuilder();
			for (int i = 0; i < Wrappers.Length; i++)
			{
				DocumentWrapper wrapper = Wrappers[i];
				PropertyInfo[] publicProperties = wrapper.GetType().GetProperties();
				foreach (PropertyInfo info in publicProperties)
				{
					if (info.GetGetMethod().GetParameters().Length == 0
						&& info.DeclaringType == wrapper.GetType())
					{
						if (!typeof(DocumentWrapper).IsAssignableFrom(info.PropertyType) && (!typeof(System.Drawing.Image).IsAssignableFrom(info.PropertyType)) && !info.PropertyType.IsInterface)
						{
							try
							{
								bool returnsNull = info.GetValue(wrapper, null) == null;
								if (returnsNull)
								{
									results.Append("Instance " + i.ToString() + ", " + info.Name + " << NULL" + System.Environment.NewLine);
								}
							}
							catch (Exception e)
							{
								if (!(e.InnerException is NotSupportedException))
								{
									results.Append("Instance " + i.ToString() + ", " + info.Name + "  << Exception = " + e.InnerException.Message + System.Environment.NewLine);
								}
							}
						}
					}
				}
			}
			if (results.Length > 0)
			{
				results.Insert(0, System.Environment.NewLine + "These properties return null or throw exceptions" + System.Environment.NewLine);
			}

			PrintResults(results);
		}

		#region DocManagerTests

		public void TestDocManagerBarcodeProperties()
		{
			StringBuilder results = new StringBuilder();

			foreach (DocumentWrapper wrapper in Wrappers)
			{
				if (wrapper is DocBaseWrapper)
				{
					PropertyInfo docManagerUniqueIDInfo = wrapper.GetType().GetProperty("DocManagerUniqueID", BindingFlags.NonPublic | BindingFlags.Instance);
					if (docManagerUniqueIDInfo.GetValue(wrapper, null) == null)
					{
						results.Append(">> DocManagerUniqueID property should not return null. Please check your override implementation." + System.Environment.NewLine);
					}

					string[] propertyNames = new string[] { "BarcodeTextForFont", "BarcodeText", "BarcodeTextForFontPlaceholder", "BarcodeTextPlaceholder" };

					foreach (string propertyName in propertyNames)
					{
						try
						{
							PropertyInfo info = wrapper.GetType().GetProperty(propertyName);
							if (info.GetValue(wrapper, null) == null)
							{
								results.Append(string.Format(@">> {0} should not return null. Please check the overrides for DocManagerBarcode (should return TextBarcode.Empty if not valid) 
								and DocManagerUniqueID (should return ZString.Empty if not valid) in {1}", propertyName, wrapper.GetType()) + System.Environment.NewLine);
							}
						}
						catch
						{
							results.Append(string.Format(@">> {0} - exception caught accessing this property.  Please check the overrides for DocManagerBarcode (should return TextBarcode.Empty if not valid) 
								and DocManagerUniqueID (should return ZString.Empty if not valid) in {1}", propertyName, wrapper.GetType()) + System.Environment.NewLine);
						}
					}
				}
			}
			if (results.Length > 0)
			{
				results.Insert(0, System.Environment.NewLine);
			}

			PrintResults(results);
		}

		#endregion

		public abstract DocumentWrapper[] GetDocumentWrappers();

		#region Implementation

		protected void PrintResults(StringBuilder results)
		{
			if (results.Length > 0)
			{
				Fail(results.ToString());
			}
			else
			{
				Assert(true);
			}
		}

		class DummyWrapperWithPublicConstructor : DocBaseWrapperBaseWithImageSupport
		{
			public DummyWrapperWithPublicConstructor(BusinessObjectFactory factory) : base(factory.New<DummyBusinessObject>(), factory) { }
		}

		protected void WriteSimpleContext(BusinessObjectFactory factory)
		{
			new DummyWrapperWithPublicConstructor(factory).SetTemplateConstants(new Dictionary<string, object> { { DocumentEngineIntegration.Constants.TemplateDefined.ContactType, ContactType.Consignor.Code } });
		}

		protected override void SetUp()
		{
			base.SetUp();
			if (!string.IsNullOrEmpty(TestingCountry))
			{
				StoredCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				GlbCompany.CurrentCompany.SetCountry(TestingCountry);
			}

			Wrappers = GetDocumentWrappers();
			WriteSimpleContext(Factory);
		}
		protected string StoredCountry;

		protected override void TearDown()
		{
			if (!string.IsNullOrEmpty(StoredCountry) && StoredCountry != GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
			{
				GlbCompany.CurrentCompany.SetCountry(StoredCountry);
			}

			base.TearDown();
		}

		protected virtual string TestingCountry
		{
			get { return Enterprise.Core.Constants.CountryCodes.Australia; }
		}

		protected bool IsOfCorrectType(PropertyInfo info)
		{
			return
				typeof(IZType).IsAssignableFrom(info.PropertyType)
				|| typeof(System.Drawing.Image).IsAssignableFrom(info.PropertyType)
				|| BODocDataProvider.IsBODocDataProvider(info.PropertyType)
				|| typeof(IBODocDataProviderCollection).IsAssignableFrom(info.PropertyType)
				|| typeof(ZString[]).IsAssignableFrom(info.PropertyType)
				|| info.PropertyType.IsInterface
				|| typeof(Enum).IsAssignableFrom(info.PropertyType);
		}

		protected StmNote AddNotes(EnterpriseBusinessObject bizObj, ZString noteDesc, ZString noteText)
		{
			var note = bizObj.Notes.AddNew();
			note.ST_Description = noteDesc;
			note.ST_ParentID = bizObj.PK;
			note.ST_Table = bizObj.TableName;
			note.ST_NoteDataAsText = noteText;
			return note;
		}

		protected DocumentWrapper[] Wrappers;

		#endregion

	}
}
