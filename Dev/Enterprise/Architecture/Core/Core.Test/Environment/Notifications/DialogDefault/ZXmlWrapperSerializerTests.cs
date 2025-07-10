using System;
using System.Text;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Environment.DialogDefault.Testing
{
	public class ZXmlWrapperSerializerTests : XmlSerializerTestCase<ZXmlWrapperSerializerTests.DummyObj>
	{
		protected override DummyObj GetObjectForSerialization()
		{
			return new DummyObj
			{
				SomeCSharpObj = new DateTime(1994, 12, 23),
				SomePrimitive = 23,
				SomeZString = new ZString("Hello World")
			};
		}

		protected override ISerializer<DummyObj> GetNewSerializer()
		{
			return new ZXmlSerializerWrapper<DummyObj>();
		}

		#region Classes for testing

		public class DummyObj
		{
			public int SomePrimitive { get; set; }
			public DateTime SomeCSharpObj { get; set; }
			public ZString SomeZString { get; set; }

			public override int GetHashCode()
			{
				var sb = new StringBuilder();
				sb.Append(SomePrimitive);
				sb.Append(SomeCSharpObj);
				sb.Append(SomeZString.ToString());

				return sb.ToString().GetHashCode();
			}

			public override bool Equals(object obj)
			{
				var dObj = obj as DummyObj;
				return dObj != null && Equals(SomePrimitive, dObj.SomePrimitive) && Equals(SomeCSharpObj, dObj.SomeCSharpObj) && Equals(SomeZString, dObj.SomeZString);
			}

			public DummyObj Copy()
			{
				return new DummyObj
				{
					SomeZString = SomeZString,
					SomePrimitive = SomePrimitive,
					SomeCSharpObj = SomeCSharpObj
				};
			}

			public override string ToString()
			{
				return string.Format("DummyObj {{{0}, {1}, {2}}}", SomePrimitive, SomeCSharpObj, SomeZString);
			}
		}

		#endregion
	}
}
