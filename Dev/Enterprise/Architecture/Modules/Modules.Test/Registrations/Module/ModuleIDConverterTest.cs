using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	public class ModuleIDConverterTest : TestCase
	{
		public void TestConvertTo()
		{
			Assert("Can convert to string", Converter.CanConvertTo(typeof(string)));
			object result = Converter.ConvertTo(DummyModuleID, typeof(string));
			AssertEquals(typeof(string), result.GetType());
			AssertEquals(DummyModuleName, (string)result);
		}

		public void TestConvertFrom()
		{
			Assert("Can convert from string", Converter.CanConvertFrom(typeof(string)));
			object result = Converter.ConvertFrom(DummyModuleName);
			AssertEquals(ModuleIDType, result.GetType());
			AssertEquals(DummyModuleID, Convert.ChangeType(result, ModuleIDType));
		}

		#region Implementation

		protected virtual Type ModuleIDType
		{
			get { return typeof(ModuleIdentifier); }
		}

		protected virtual ModuleIdentifier DummyModuleID
		{
			get { return DummyModuleIDs.Dummy; }
		}

		protected virtual string DummyModuleName
		{
			get { return "Dummy"; }
		}

		protected virtual ModuleIDConverter GetNewConverter()
		{
			return new ModuleIDConverter();
		}

		protected override void SetUp()
		{
			base.SetUp();
			Converter = GetNewConverter();
		}

		ModuleIDConverter Converter;

		#endregion Implementation
	}
}
