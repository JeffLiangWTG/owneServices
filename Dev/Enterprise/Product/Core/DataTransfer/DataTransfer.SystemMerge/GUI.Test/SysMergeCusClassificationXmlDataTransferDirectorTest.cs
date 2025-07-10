using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.SystemMerge.DataAdapters;
using Enterprise.DataTransfer.SystemMerge.Xml;

namespace Enterprise.DataTransfer.SystemMerge.GUI.Testing
{
	sealed class SysMergeCusClassificationXmlDataTransferDirectorTest : TestCaseWithFactory
	{
		public void TestAdapterAndSerializer()
		{
			SysMergeCusClassificationXmlDataTransferDirectorForTest director = new SysMergeCusClassificationXmlDataTransferDirectorForTest();
			AssertEquals("Adapter", typeof(SysMergeClassificationValueObjectDataAdapter), director.Adapter_Exposed.GetType());
			AssertEquals("serializer", typeof(SysMergeClassificationObjectSerializer), director.Serializer_Exposed);
		}

		class SysMergeCusClassificationXmlDataTransferDirectorForTest : SysMergeCusClassificationXmlDataTransferDirector
		{
			public IValueObjectDataAdapter Adapter_Exposed
			{
				get { return Adapter; }
			}

			public Type Serializer_Exposed
			{
				get { return Serializer.GetType(); }
			}
		}
	}
}
