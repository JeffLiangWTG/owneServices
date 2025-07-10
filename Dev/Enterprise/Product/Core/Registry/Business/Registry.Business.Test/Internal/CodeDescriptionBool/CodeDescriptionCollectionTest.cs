using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	public class CodeDescriptionCollectionTest : TestCase
	{
		public class GetDescriptionTest : TestCase
		{
			public abstract class Test<T, P> : CodeDescriptionCollectionTest
				where T : CodeDescription<P>, ICodeDescription<P>, new()
				where P : IZType
			{
				protected override void SetUp()
				{
					base.SetUp();
					collection = new Mock<CodeDescriptionCollection<T, P>>(null, null, null, default(P), 10) { CallBase = true };
				}

				public void TestNoItemReturnsEmptyString()
				{
					// Arrange

					// Act
					var result = collection.Object.GetDescriptionFromCode("asd");

					// Assert
					AssertEquals(string.Empty, result);
				}

				public void TestReturnsDescription()
				{
					Test("ABC", "ABC Description");
					Test("XYZ", "XYZ Description");

					void Test(string code, string description)
					{
						// Arrange
						var element = collection.Object.AddNew();
						element.Code = code;
						element.Description = (NoResString)description;

						// Act
						var result = collection.Object.GetDescriptionFromCode(code);

						// Assert
						AssertEquals(description, result);
					}
				}

				Mock<CodeDescriptionCollection<T, P>> collection;
			}

			public class ZByteTest : Test<CodeDescriptionForTest, ZByte>
			{
			}
		}

		public class SetTest : TestCase
		{
			public abstract class Test<T, P> : CodeDescriptionCollectionTest
				where T : CodeDescription<P>, ICodeDescription<P>, new()
				where P : IZType
			{
				protected override void SetUp()
				{
					base.SetUp();
					collection = new Mock<CodeDescriptionCollection<T, P>>(null, null, null, default(P), 10) { CallBase = true };
				}

				public void TestNoItemThrowsException()
				{
					// Arrange

					// Act
					var result = AssertExceptionThrown<Exception>(() => collection.Object.Set("asd", default));

					// Assert
					AssertType<ArgumentException>(result);
					AssertContains("asd", result.Message);
				}

				public void TestSetsValue()
				{
					foreach (var value in Values)
					{
						Test(value.code, value.value);
					}

					void Test(string code, P value)
					{
						// Arrange
						var element = collection.Object.AddNew();
						element.Code = code;

						// Act
						collection.Object.Set(code, value);

						// Assert
						var result = ((T)collection.Object.FindByCode(code)).Value;
						AssertEquals(value, result);
					}
				}

				protected abstract IEnumerable<(string code, P value)> Values { get; }

				Mock<CodeDescriptionCollection<T, P>> collection;
			}

			public class ZByteTest : Test<CodeDescriptionForTest, ZByte>
			{
				protected override IEnumerable<(string code, ZByte value)> Values => new (string code, ZByte value)[]
				{
					("asd", ZByte.Zero),
					("qwe", 1),
					("zxc", 45),
				};
			}
		}

		public class CreateDataTableTest : TestCase
		{
			public abstract class Test<T, P> : CodeDescriptionCollectionTest
				where T : CodeDescription<P>, ICodeDescription<P>, new()
				where P : IZType
			{
				protected override void SetUp()
				{
					base.SetUp();
					collection = new Mock<CodeDescriptionCollection<T, P>>(null, null, null, default(P), 10) { CallBase = true };
				}

				public void TestColumns()
				{
					// Arrange

					// Act
					var result = collection.Object.CreateDataTable();

					// Assert
					AssertEquals(true, result.Columns.Contains("code"));
					AssertEquals(typeof(string), result.Columns["code"].DataType);
					AssertEquals(true, result.Columns.Contains("description"));
					AssertEquals(typeof(string), result.Columns["description"].DataType);
				}

				public void TestValues()
				{
					// Arrange
					var values = new (string code, string description)[]
					{
						("ABC", "ABC Description"),
						("XYZ", "XYZ Description"),
						("qwe", "qwe Description"),
					};

					foreach (var (code, description) in values)
					{
						var element = collection.Object.AddNew();
						element.Code = code;
						element.Description = (NoResString)description;
					}

					// Act
					var result = collection.Object.CreateDataTable()
						.Rows
						.Cast<DataRow>()
						.Select(row => (row["code"].ToString(), row["description"].ToString()))
						.ToArray();

					// Assert
					AssertContainsExactElementsInAnyOrder(values, result);
				}

				Mock<CodeDescriptionCollection<T, P>> collection;
			}

			public class ZByteTest : Test<CodeDescriptionForTest, ZByte>
			{
			}
		}

		public class CodeDescriptionForTest : CodeDescription<ZByte>
		{
			protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			{
				return new CodeDescriptionForTest();
			}

			protected override ZByte ValueFromString(string value)
			{
				return new ZByte(value);
			}
		}
	}
}
