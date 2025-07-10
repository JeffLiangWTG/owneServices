using System.Data;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class ResourceStringHelperTest : TestCase
	{
		public void TestGetData()
		{
			var getter = new Mock<ResourceStringGetter>();
			var data = new ResourceStringData("DummyBizo|Z0_Description", "Test");
			PropertyInfo info = GetInfo<DummyEnterpriseBusinessObject>(DummyEnterpriseBusinessObject.Schema.Z0_Description);
			using (IMockResourceStringCache mockCache = Res.UseMockData())
			{
				mockCache.SetResourceGetter(getter.Object);
				getter.Setup(m => m("DummyBizo|Z0_Description")).Returns(data);
				AssertSame(data, ResourceStringHelper.GetData(info));
			}
		}

		public void TestGetData_WithOverride()
		{
			var getter = new Mock<ResourceStringGetter>();
			var data = new ResourceStringData("ClassName|FieldName", "Test");
			PropertyInfo info = GetInfo<Dummy>("Text");
			using (IMockResourceStringCache mockCache = Res.UseMockData())
			{
				mockCache.SetResourceGetter(getter.Object);
				getter.Setup(m => m("ClassName|FieldName")).Returns(data);
				AssertSame(data, ResourceStringHelper.GetData(info));
			}
		}

		public void TestGetData_Unknown()
		{
			var getter = new Mock<ResourceStringGetter>();
			var data = new Mock<ResourceStringData>();
			PropertyInfo info = GetInfo<Unknown>("Text");
			using (IMockResourceStringCache mockCache = Res.UseMockData())
			{
				mockCache.SetResourceGetter(getter.Object);
				AssertSame(null, ResourceStringHelper.GetData(info));
			}
		}

		#region Implementation
		PropertyInfo GetInfo<T>(string name)
		{
			PropertyInfo info = typeof(T).GetProperty(name);
			AssertNotNull(string.Format("precondition: {0}.{1}", typeof(T).Name, name, info));
			return info;
		}

		public class Dummy : DummyEnterpriseBusinessObject
		{
			public Dummy(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			[ResourceStringData("ClassName|FieldName")]
			public ZString Text
			{
				get
				{
					return "";
				}

				set
				{
				}
			}
		}

		public class Unknown : NonPersistentBusinessObject
		{
			public ZString Text
			{
				get
				{
					return "";
				}

				set
				{
				}
			}
		}
		#endregion
	}
}
