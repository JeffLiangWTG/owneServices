using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Web.Business;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	[TestsSubclassesOf(typeof(FilterBusinessObject),
		typeof(TestExcludeBusinessObjectsAllHaveTestCasesAttribute),
		new Type[] { typeof(FilterStripBusinessObject) },
		ExcludeClientDlls = true)]
	public abstract class FilterBusinessObjectTestCase : BusinessObjectBaseTestCase
	{
		public void TestGetExpectedBusinessObjectTypeReturnsFilterBusinessObjectType()
		{
			Assert(GetExpectedBusinessObjectType().IsSubclassOf(typeof(FilterBusinessObject)));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return FilterBusinessObjectFactory.New(GetExpectedBusinessObjectType());
		}

		protected override bool CanTestMaxLength(int maximumLength)
		{
			return maximumLength > -1 && base.CanTestMaxLength(maximumLength);
		}

		protected WebFilterBusinessObjectFactory fFilterBusinessObjectFactory;

		protected WebFilterBusinessObjectFactory FilterBusinessObjectFactory
		{
			get
			{
				if (fFilterBusinessObjectFactory == null)
				{
					BusinessObjectFactory webFactory = new BusinessObjectFactory();
					fFilterBusinessObjectFactory = new WebFilterBusinessObjectFactory(webFactory);
				}

				return fFilterBusinessObjectFactory;
			}
		}

		// This implementation is designed to allow bindable enumerations to be placed on FilterBusinessObject classes.
		protected override IZType GetValueFromBusinessObject(BusinessObject bizO, string name)
		{
			return bizO[name] as IZType;
		}
	}
}
