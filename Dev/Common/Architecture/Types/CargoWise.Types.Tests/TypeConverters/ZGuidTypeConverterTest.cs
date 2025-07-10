using System;
using System.ComponentModel.Design.Serialization;

namespace CargoWise.Types.Tests
{
	class ZGuidTypeConverterTest : TypeConverterTest
	{
		protected override Type GetZTypeImplementingTypeConverter()
		{
			return typeof(ZGuid);
		}
		protected override ValueMapping[] GetConvertibleFromValues()
		{
			Guid guid = Guid.NewGuid();
			return new ValueMapping[] {
				new ValueMapping(ZGuid.Empty, ZGuid.Empty),
				new ValueMapping(DBNull.Value, ZGuid.Empty),
				new ValueMapping(new ZString(guid.ToString()), new ZGuid(guid)),
				new ValueMapping(guid.ToString(), new ZGuid(guid)),
				new ValueMapping("", ZGuid.Empty),
				new ValueMapping(ZString.Empty, ZGuid.Empty),
				new ValueMapping(guid, new ZGuid(guid))
			};
		}

		protected override ValueMapping[] GetConvertibleToValues()
		{
			ZGuid zGuid = ZGuid.NewZGuid();
			return new ValueMapping[] {
				new ValueMapping(zGuid, zGuid),
				new ValueMapping(zGuid, zGuid.ToGuid()),
				new ValueMapping(ZGuid.Empty, Guid.Empty),
			};
		}

		protected override Type[] GetNonConvertibleFromTypes()
		{
			return new Type[] { typeof(int), typeof(DateTime) };
		}

		protected override Type[] GetNonConvertibleToTypes()
		{
			return new Type[] { typeof(int), typeof(DateTime) };
		}

		public void TestConvertToInstanceDescriptor()
		{
			ZGuid guid = ZGuid.NewZGuid();
			ZGuidTypeConverter typeConverter = ZGuidTypeConverter.Instance;
			Assert(typeConverter.CanConvertTo(typeof(InstanceDescriptor)));
			InstanceDescriptor id = typeConverter.ConvertTo(guid, typeof(InstanceDescriptor)) as InstanceDescriptor;
			AssertNotNull(id);
			Assert(id.IsComplete);
			AssertEquals(".ctor", id.MemberInfo.Name);
			Assert(id.Arguments is object[]);
			AssertEquals(1, ((object[])id.Arguments).Length);
			AssertEquals(guid.ToString(), ((object[])id.Arguments)[0]);
		}
	}
}
