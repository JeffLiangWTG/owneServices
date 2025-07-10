using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Environment.DialogDefault.Testing
{
	public abstract class XmlSerializerTestCase<T> : TestCaseWithFactory
	{
		protected abstract T GetObjectForSerialization();
		protected abstract ISerializer<T> GetNewSerializer();

		public void TestGetObjectIsntReturningTheDefaultValue()
		{
			AssertNotEquals("If you are testing the default value then we cannot tell if changes were correctly saved", default(T), GetNewSerializer());
		}

		public void TestGetNewSerializerGetsNEWSerializer()
		{
			Assert("You cannot recycle a serializer for tests since defaults will be saved and restored by different instances of CW1", !ReferenceEquals(GetNewSerializer(), GetNewSerializer()));
		}

		public void TestSerializeDeserialize()
		{
			var serializer = GetNewSerializer();

			var originalObject = GetObjectForSerialization();
			var deserializedValue = serializer.Deserialize(serializer.Serialize(originalObject));

			AssertEquals(originalObject, deserializedValue);
		}

		public void TestSerialiseDeserialise_DifferentSerializerObject()
		{
			var originalObject = GetObjectForSerialization();
			var deserializedObject = GetNewSerializer().Deserialize(GetNewSerializer().Serialize(originalObject));

			AssertEquals(originalObject, deserializedObject);
		}

		public void TestSerializeHasRootNode()
		{
			var serialized = GetNewSerializer().Serialize(GetObjectForSerialization());
			var siblings = serialized.ElementsAfterSelf().Concat(serialized.ElementsBeforeSelf());
			AssertEquals("You must have a single root node. This is required by DefaultDialogHandler", 0, siblings.Count());
		}
	}
}
