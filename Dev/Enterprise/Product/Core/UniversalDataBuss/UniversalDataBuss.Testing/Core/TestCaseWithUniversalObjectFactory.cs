using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.UniversalDataBuss.Testing.Core
{
	public class TestCaseWithUniversalObjectFactory : TestCaseWithFactory
	{
		public static void AssertCollectionContains(List<AddInfo> addInfoCollection, ZString key, ZString? expectedValue)
		{
			AssertEquals(string.Format("Collection should contain key({0})", key), expectedValue, addInfoCollection.GetZStringValue(key));
		}

		public static void AssertCollectionContains(List<AddInfo> addInfoCollection, ZString key, ZDecimal? expectedValue)
		{
			AssertEquals(string.Format("Collection should contain key({0})", key), expectedValue, addInfoCollection.GetZDecimalValue(key));
		}

		public static void AssertCollectionContains(List<AddInfo> addInfoCollection, ZString key, ZBool? expectedValue)
		{
			AssertEquals(string.Format("Collection should contain key({0})", key), expectedValue, addInfoCollection.GetZBoolValue(key));
		}

		public static void AssertCollectionContains(List<AddInfo> addInfoCollection, ZString key, ZDateTime? expectedValue)
		{
			AssertEquals(string.Format("Collection should contain key({0})", key), expectedValue, addInfoCollection.GetZDateTimeValue(key));
		}

		public static void AssertCollectionContains(List<AddInfo> addInfoCollection, ZString key, ZDateTimeOffset? expectedValue)
		{
			AssertEquals(string.Format("Collection should contain key({0})", key), expectedValue, addInfoCollection.GetZDateTimeOffsetValue(key));
		}

		public static void AssertCollectionContains(List<AddInfo> addInfoCollection, ZString key, ZInt? expectedValue)
		{
			AssertEquals(string.Format("Collection should contain key({0})", key), expectedValue, addInfoCollection.GetZIntValue(key));
		}

		public static void AssertCollectionContains(List<AddInfo> addInfoCollection, ZString key, ZLong? expectedValue)
		{
			AssertEquals(string.Format("Collection should contain key({0})", key), expectedValue, addInfoCollection.GetZIntValue(key));
		}

		public static void AssertCollectionContains(List<AddInfo> addInfoCollection, ZString key, ZShort? expectedValue)
		{
			AssertEquals(string.Format("Collection should contain key({0})", key), expectedValue, addInfoCollection.GetZShortValue(key));
		}

		public static void AssertCollectionContains(List<AddInfo> addInfoCollection, ZString key, ZGuid? expectedValue)
		{
			AssertEquals(string.Format("Collection should contain key({0})", key), expectedValue, addInfoCollection.GetZGuidValue(key));
		}

		protected new UniversalObjectFactory Factory
		{
			get { return factory ?? (factory = NewUniversalObjectFactory()); }
		}
		UniversalObjectFactory factory;

		protected virtual UniversalObjectFactory NewUniversalObjectFactory()
		{
			return new UniversalObjectFactory();
		}

		protected sealed override BusinessObjectFactory NewFactory()
		{
			return Factory.BOFactory;
		}
	}
}
