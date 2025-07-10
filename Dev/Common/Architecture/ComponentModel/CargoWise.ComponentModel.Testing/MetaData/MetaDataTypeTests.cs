#if DEBUG
using System;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public class MetaDataTypeTests : TestCase
	{
		[ExpectException(typeof(ArgumentException))]
		public void TestRegisterMetaDataTypeTwice()
		{
			MetaDataType.RegisterMetaDataType(new MetaDataType("MetaDataType.Test", typeof(int), 0));
			MetaDataType.RegisterMetaDataType(new MetaDataType("MetaDataType.Test", typeof(int), 0));
			MetaDataType.RegisterMetaDataType(new MetaDataType("MetaDataType.Test", typeof(string), ""));
		}

		public void TestRegisterMetaDataType_DisableModifiable()
		{
			var metaDataTypes = MetaDataType.GetRegisteredMetaDataTypes();
			var metaDataType = metaDataTypes.FirstOrDefault(m => m.Id == "DisableModifiable");
			AssertNotNull("MetaDataType DisableModifiable was found", metaDataType);
			AssertEquals("The default value is false", false, metaDataType?.DefaultValue);
		}
	}
}
#endif
