using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	[TestedType(typeof(SearchType))]
	public class SearchTypeTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new SearchType(new DummyAssemblyData(), new ArchiveEDocsManager(new DocumentFactoryProvider().GetFactory(Factory)));
		}

		public void TestConstructor()
		{
			IAssemblyData assemblyData = new DummyAssemblyData();
			SearchType searchType = new SearchType(assemblyData, new ArchiveEDocsManager(new DocumentFactoryProvider().GetFactory(Factory)));

			AssertEquals("Name", searchType.Name, "HumanReadableName");
			AssertEquals("IsFilterOn", searchType.IsFilterOn, true);
			AssertEquals("Assembly", searchType.AssemblyData, assemblyData);
		}

		public class DummyAssemblyData : IAssemblyData
		{
			#region IAssemblyData Members

			public Type BusinessObjectType
			{
				get { throw new NotImplementedException(); }
			}

			public string DocManagerCode
			{
				get { throw new NotImplementedException(); }
			}

			public IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
			{
				throw new NotImplementedException();
			}

			public IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory, AssemblyDataParams assemblyDataParams)
			{
				throw new NotImplementedException();
			}

			public ZQuery GetQuery(AssemblyDataParams assemblyDataParams)
			{
				throw new NotImplementedException();
			}

			public IEDocsViaUniversalXmlSupport GetEDocsViaUniversalXmlSupport()
			{
				throw new NotImplementedException();
			}

			public string HumanReadableName
			{
				get { return "HumanReadableName"; }
			}

			public bool IsAllowedForUnallocatedeDocs
			{
				get { throw new NotImplementedException(); }
			}

			public Enterprise.ZArchitecture.Modules.ModuleIdentifier ModuleID
			{
				get { throw new NotImplementedException(); }
			}

			public string ReferenceType
			{
				get { throw new NotImplementedException(); }
			}

			public bool AllowLookupOfBizOFromPk
			{
				get { return false; }
			}

			public ZString GetFriendlyName(BusinessObject businessObject)
			{
				throw new NotImplementedException();
			}
			#endregion
		}
	}
}
